﻿using HouseholdAccountBook.Others;
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
        public static LogLevel OutputLogLevel = LogLevel.Debug;

        private static readonly string listenerName = "logFileOutput";
        private TextWriterTraceListener listener;

        static Log() => Register(static () => new Log());

        /// <summary>
        /// <see cref="Log"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        private Log()
        {
            Trace.AutoFlush = true;
        }

        ~Log()
        {
            if (this.listener != null) {
                this.listener.Close();
                this.listener.Dispose();
                this.listener = null;
            }
        }

        /// <summary>
        /// リスナーを生成する
        /// </summary>
        private void CreateListener()
        {
            if (this.listener == null) {
                if (!Directory.Exists(LogFolderPath)) _ = Directory.CreateDirectory(LogFolderPath);
                TextWriter sw = new StreamWriter(LogFilePath, true, Encoding.UTF8);

                this.listener = new TextWriterTraceListener {
                    Name = listenerName,
                    Writer = sw,
                    TraceOutputOptions = TraceOptions.DateTime | TraceOptions.Timestamp | TraceOptions.ProcessId | TraceOptions.ThreadId
                };

                _ = Trace.Listeners.Add(this.listener);
            }
        }

        /// <summary>
        /// ログファイルに関数開始ログを1行出力する
        /// </summary>
        /// <param name="args">引数を含む匿名クラス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        static public void FuncStart(object args = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            Vars("func start", args, fileName, methodName, lineNumber);
        }
        /// <summary>
        /// ログファイルに関数終了ログを1行出力する
        /// </summary>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        static public void FuncEnd([CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null)
        {
            Info("func end", fileName, methodName, -1);
        }
        /// <summary>
        /// ログファイルに変数ログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="args">引数を含む匿名クラス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        static public void Vars(string message = "", object vars = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            string details = string.Empty;
            if (vars != null) {
                // オブジェクトを文字列化する
                static string toString(object tmp, int depth = 0)
                {
                    // 再帰呼び出しの深さ制限
                    depth++;
                    if (10 < depth) {
                        return "[overflow]";
                    }

                    if (tmp is null) return "null";

                    if (tmp.GetType().IsPrimitive) return $"{tmp}";
                    if (tmp.GetType().IsEnum) return $"{tmp}";

                    switch (tmp) {
                        case string s:
                            return $"\"{s}\"";
                        case DateTime dt:
                            if (dt.TimeOfDay == TimeSpan.Zero) {
                                return $"{dt:yyyy-MM-dd}";
                            }
                            else {
                                return $"{dt:yyyy-MM-dd HH:mm:ss}";
                            }
                        case IEnumerable enumerable:
                            return $"[{string.Join(", ", enumerable.Cast<object>().Select(item => toString(item, depth)))}]";
                        default:
                            PropertyInfo[] propInfos = tmp.GetType().GetProperties();
                            return string.Join(", ", propInfos.Select(propInfo => $"{propInfo.Name}:{toString(propInfo.GetValue(tmp), depth)}"));
                    }
                }
                details += toString(vars);
            }

            string tmpMessage = string.Join(message != string.Empty && details != string.Empty ? " " : "", message, details);
            Info(tmpMessage, fileName, methodName, lineNumber);
        }

        /// <summary>
        /// ログファイルにエラーログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        static public void Error(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            Instance.WriteLine(LogLevel.Error, message, fileName, methodName, lineNumber);
        }
        /// <summary>
        /// ログファイルに警告ログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        static public void Warning(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            Instance.WriteLine(LogLevel.Warning, message, fileName, methodName, lineNumber);
        }
        /// <summary>
        /// ログファイルに情報ログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        static public void Info(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            Instance.WriteLine(LogLevel.Info, message, fileName, methodName, lineNumber);
        }
        /// <summary>
        /// ログファイルにデバッグログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        static public void Debug(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            Instance.WriteLine(LogLevel.Debug, message, fileName, methodName, lineNumber);
        }

        /// <summary>
        /// ログファイルにログを1行出力する
        /// </summary>
        /// <param name="level">ログレベル</param>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        private void WriteLine(LogLevel level, string message, string fileName, string methodName, int lineNumber)
        {
            Settings settings = Settings.Default;
            if (!settings.App_OutputFlag_OperationLog) return;
            if (level < OutputLogLevel) return;

            this.CreateListener();

            int index = fileName.LastIndexOf('\\');
            string className = fileName.Substring(index + 1, fileName.IndexOf('.', index + 1) - index - 1);

            string output = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{Environment.CurrentManagedThreadId:000}] [{level,-5}] {className}";
            if (methodName != ".ctor") {
                output += $"::{methodName}";
            }

            if (0 <= lineNumber) {
                output += $"({lineNumber})";
            }
            else {
                output += "(-)";
            }

            if (!string.IsNullOrEmpty(message)) {
                output += $" {message}";
            }

            Trace.WriteLine(output);
        }
    }
}
