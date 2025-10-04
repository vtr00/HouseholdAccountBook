using HouseholdAccountBook.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using static HouseholdAccountBook.Adapters.FileConstants;

namespace HouseholdAccountBook.Adapters.Logger
{
    /// <summary>
    /// トレースログ出力
    /// </summary>
    public class Log
    {
        /// <summary>
        /// シングルトンのインスタンス
        /// </summary>
        private static readonly Lazy<Log> singleton = new(() => new());
        /// <summary>
        /// インスタンス
        /// </summary>
        public static Log Instance => singleton.Value;

        private static readonly string listenerName = "logFileOutput";
        private TextWriterTraceListener listener = null;

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
        /// ログファイルにエラーログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        static public void Error(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            Instance.WriteLine("[Error  ]", message, fileName, methodName, lineNumber);
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
            Instance.WriteLine("[Warning]", message, fileName, methodName, lineNumber);
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
            Instance.WriteLine("[Info   ]", message, fileName, methodName, lineNumber);
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
            Instance.WriteLine("[Debug  ]", message, fileName, methodName, lineNumber);
        }

        /// <summary>
        /// ログファイルにログを1行出力する
        /// </summary>
        /// <param name="type">出力タイプ</param>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        private void WriteLine(string type, string message, string fileName, string methodName, int lineNumber)
        {
            Settings settings = Settings.Default;
            if (!settings.App_OutputFlag_OperationLog) return;

            this.CreateListener();

            int index = fileName.LastIndexOf('\\');
            string className = fileName.Substring(index + 1, fileName.IndexOf('.', index + 1) - index - 1);

            Trace.WriteLine($"{DateTime.Now:yyyy-MM-dd hh:mm:ss.fff} [{Environment.CurrentManagedThreadId:000}] {type} {className}::{methodName}:{lineNumber} {message}");
        }
    }
}
