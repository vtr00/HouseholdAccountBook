using HouseholdAccountBook.Others;
using HouseholdAccountBook.Properties;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using static HouseholdAccountBook.Adapters.FileConstants;

namespace HouseholdAccountBook.Adapters.Logger
{
    /// <summary>
    /// トレースログ出力
    /// </summary>
    public class Log : SingletonBase<Log>
    {
        /// <summary>
        /// ログレベル
        /// </summary>
        public enum LogLevel
        {
            Debug = 0,
            Info,
            Warning,
            Error,
        }

        /// <summary>
        /// 出力ログレベル
        /// </summary>
        public static LogLevel OutputLogLevel { get; set; } = LogLevel.Debug;

        private static readonly string mListenerName = "logFileOutput";
        private TextWriterTraceListener mListener;

        static Log() => Register(static () => new Log());

        /// <summary>
        /// <see cref="Log"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        private Log() => Trace.AutoFlush = true;

        ~Log()
        {
            if (this.mListener != null) {
                this.mListener.Close();
                this.mListener.Dispose();
                this.mListener = null;
            }
        }

        /// <summary>
        /// リスナーを生成する
        /// </summary>
        private void CreateListener()
        {
            if (this.mListener == null) {
                if (!Directory.Exists(LogFolderPath)) { _ = Directory.CreateDirectory(LogFolderPath); }
                TextWriter sw = new StreamWriter(LogFilePath, true, Encoding.UTF8);

                this.mListener = new TextWriterTraceListener {
                    Name = mListenerName,
                    Writer = sw,
                    TraceOutputOptions = TraceOptions.DateTime | TraceOptions.Timestamp | TraceOptions.ProcessId | TraceOptions.ThreadId
                };

                _ = Trace.Listeners.Add(this.mListener);
            }
        }

        /// <summary>
        /// ログファイルに関数開始ログを1行出力する
        /// </summary>
        /// <param name="args">引数を含む匿名クラス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void FuncStart(object args = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
            => Vars("func start", args, fileName, methodName, lineNumber);
        /// <summary>
        /// ログファイルに関数終了ログを1行出力する
        /// </summary>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="digit">「-」の出力回数</param>
        public static void FuncEnd([CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, ushort digit = 0)
            => Info("func end", fileName, methodName, -digit);
        /// <summary>
        /// ログファイルに変数ログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="args">引数を含む匿名クラス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void Vars(string message = "", object vars = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            string details = string.Empty;
            if (vars != null) {
                // オブジェクトを文字列化する
                static string ToString(object obj, int depth = 0)
                {
                    // 再帰呼び出しの深さ制限
                    depth++;
                    if (10 < depth) {
                        return "[overflow]";
                    }

                    if (obj is null) { return "null"; }

                    if (obj.GetType().IsPrimitive) { return $"{obj}"; }
                    if (obj.GetType().IsEnum) { return $"{obj}"; }

                    switch (obj) {
                        case string s:
                            return $"\"{s}\"";
                        case DateTime dt:
                            return dt.TimeOfDay == TimeSpan.Zero ? $"{dt:yyyy-MM-dd}" : $"{dt:yyyy-MM-dd HH:mm:ss}";
                        case IEnumerable enumerable:
                            return $"[{string.Join(", ", enumerable.Cast<object>().Select(item => ToString(item, depth)))}]";
                        default:
                            PropertyInfo[] propInfos = obj.GetType().GetProperties();
                            return string.Join(", ", propInfos.Select(propInfo => $"{propInfo.Name}:{ToString(propInfo.GetValue(obj), depth)}"));
                    }
                }
                details += ToString(vars);
            }

            string tmpMessage = string.Join((message != string.Empty && details != string.Empty) ? " - " : "", message, details);
            Info(tmpMessage, fileName, methodName, lineNumber);
        }

        /// <summary>
        /// ログファイルにエラーログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void Error(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
            => Instance.WriteLine(LogLevel.Error, message, fileName, methodName, lineNumber);
        /// <summary>
        /// ログファイルに警告ログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void Warning(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
            => Instance.WriteLine(LogLevel.Warning, message, fileName, methodName, lineNumber);
        /// <summary>
        /// ログファイルに情報ログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void Info(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
            => Instance.WriteLine(LogLevel.Info, message, fileName, methodName, lineNumber);
        /// <summary>
        /// ログファイルにデバッグログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="filePath">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void Debug(string message = "", [CallerFilePath] string filePath = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
            => Instance.WriteLine(LogLevel.Debug, message, filePath, methodName, lineNumber);

        /// <summary>
        /// ログファイルにログを1行出力する
        /// </summary>
        /// <param name="level">ログレベル</param>
        /// <param name="message">出力メッセージ</param>
        /// <param name="filePath">出力元ファイルパス</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数(負の場合は絶対値の数だけ「-」を繰り返す)</param>
        private void WriteLine(LogLevel level, string message, string filePath, string methodName, int lineNumber)
        {
            Settings settings = Settings.Default;
            if (!settings.App_OutputFlag_OperationLog) { return; }
            if (level < OutputLogLevel) { return; }

            this.CreateListener();

            int index = filePath.LastIndexOf('\\');
            string fileName = filePath.Substring(index + 1, filePath.IndexOf('.', index + 1) - index - 1);

            string callerStr = fileName;
            if (methodName != ".ctor") {
                callerStr += $"::{methodName}";
            }
            if (0 <= lineNumber) {
                callerStr += $":{lineNumber}";
            }
            else {
                callerStr += ":" + new string('-', Math.Abs(lineNumber));
            }
            string output = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{Environment.CurrentManagedThreadId:000}] [{level,-5}] [{callerStr}]";

            if (!string.IsNullOrEmpty(message)) {
                output += $" {message}";
            }

            Trace.WriteLine(output);
        }
    }
}
