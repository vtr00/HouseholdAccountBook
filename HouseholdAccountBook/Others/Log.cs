using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook
{
    /// <summary>
    /// トレースログ出力
    /// </summary>
    public static class Log
    {
        private static readonly string listenerName = "logFileOutput";
        private static TextWriterTraceListener listener = null;

        static Log()
        {
            Trace.AutoFlush = true;
            CreateNewFileListener();
        }

        /// <summary>
        /// リスナーを生成する
        /// </summary>
        static private void CreateNewFileListener()
        {
            if (listener == null) {
                listener = new TextWriterTraceListener();
                if (!Directory.Exists(LogFolderPath)) Directory.CreateDirectory(LogFolderPath);
                TextWriter sw = new StreamWriter(LogFilePath, true, Encoding.GetEncoding("Shift_JIS"));
                listener.Name = listenerName;
                listener.Writer = sw;
                listener.TraceOutputOptions = TraceOptions.DateTime | TraceOptions.Timestamp | TraceOptions.ProcessId | TraceOptions.ThreadId;

                Trace.Listeners.Add(listener);
            }
        }

        /// <summary>
        /// ログファイルを変更する
        /// </summary>
        static public void ChangeNewFile()
        {
            Trace.Listeners.Remove(listenerName);
            Close();
            CreateNewFileListener();
        }

        /// <summary>
        /// リスナーを閉じる
        /// </summary>
        static public void Close()
        {
            if (listener != null) {
                listener.Close();
                listener.Dispose();
                listener = null;
            }
        }

        /// <summary>
        /// ログファイルにエラーログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        static public void Error(string message = "", [CallerFilePath]string fileName = null, [CallerMemberName]string methodName = null, [CallerLineNumber]int lineNumber = 0)
        {
            WriteLine("[Error  ]", message, fileName, methodName, lineNumber);
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
            WriteLine("[Warning]", message, fileName, methodName, lineNumber);
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
            WriteLine("[Info   ]", message, fileName, methodName, lineNumber);
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
            WriteLine("[Debug  ]", message, fileName, methodName, lineNumber);
        }

        /// <summary>
        /// ログファイルにログを1行出力する
        /// </summary>
        /// <param name="type">出力タイプ</param>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        static private void WriteLine(string type, string message, string fileName, string methodName, int lineNumber)
        {
            if (!Directory.Exists(LogFolderPath)) Directory.CreateDirectory(LogFolderPath);

            int index = fileName.LastIndexOf("\\");
            string className = fileName.Substring(index + 1, fileName.IndexOf(".", index + 1) - index - 1);

            Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd hh:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId:000}] {type} [{className}::{methodName}({lineNumber})] {message}");
        }
    }
}
