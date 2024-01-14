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
    /// ログ出力
    /// </summary>
    public static class Log
    {
        static private StreamWriter sw = null;

        static Log()
        {
            TextWriterTraceListener listener = new TextWriterTraceListener();
            if (!Directory.Exists(LogFolderPath)) Directory.CreateDirectory(LogFolderPath);
            sw = new StreamWriter(LogFilePath, true, Encoding.GetEncoding("Shift_JIS"));
            listener.Writer = sw;
            listener.TraceOutputOptions = TraceOptions.DateTime | TraceOptions.Timestamp | TraceOptions.ProcessId | TraceOptions.ThreadId;

            System.Diagnostics.Debug.Listeners.Add(listener);
            System.Diagnostics.Debug.AutoFlush = true;
        }

        static public void Close()
        {
            if (sw != null) {
                sw.Close();
                sw.Dispose();
                sw = null;
            }
        }

        static public void Error(string message = "", [CallerFilePath]string fileName = null, [CallerMemberName]string methodName = null, [CallerLineNumber]int lineNumber = 0)
        {
            WriteLine("[Error  ]", message, fileName, methodName, lineNumber);
        }
        static public void Warning(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            WriteLine("[Warning]", message, fileName, methodName, lineNumber);
        }
        static public void Info(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            WriteLine("[Info   ]", message, fileName, methodName, lineNumber);
        }
        static public void Debug(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            WriteLine("[Debug  ]", message, fileName, methodName, lineNumber);
        }

        static private void WriteLine(string type, string message, string fileName, string methodName, int lineNumber)
        {
            if (!Directory.Exists(LogFolderPath)) Directory.CreateDirectory(LogFolderPath);

            int index = fileName.LastIndexOf("\\");
            string className = fileName.Substring(index + 1, fileName.IndexOf(".", index + 1) - index - 1);

            System.Diagnostics.Debug.WriteLine($"{DateTime.Now:yyyy/MM/dd hh:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId:000}] {type} [{className}::{methodName}({lineNumber})] {message}");
        }
    }
}
