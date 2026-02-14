using HouseholdAccountBook.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace HouseholdAccountBook.Adapters.Logger
{
    /// <summary>
    /// ログ出力実装クラス
    /// </summary>
    public class LogImpl : SingletonBase<LogImpl>
    {
        #region ログファイル
        #endregion

        #region フィールド
        private static readonly string mLogFileListenerName = "logFileOutput";
        private TextWriterTraceListener mLogFileListener;
        #endregion

        #region プロパティ
        /// <summary>
        /// ログファイルのフォルダパス
        /// </summary>
        public static string LogFolderPath { get; set; } = @".\Logs";
        /// <summary>
        /// ログファイル ポストフィックス
        /// </summary>
        public static string LogFileExt { get; set; } = "txt";
        /// <summary>
        /// ログファイルパス
        /// </summary>
        public static string LogFilePath {
            get {
                App app = Application.Current as App;
                DateTime dt = app.StartupTime;
                return string.Format($@"{LogFolderPath}\{dt:yyyyMMdd_HHmmss}.{LogFileExt}");
            }
        }
        /// <summary>
        /// ログファイル名パターン
        /// </summary>
        public static string LogFileNamePattern => $"*_*.{LogFileExt}";

        /// <summary>
        /// 出力ログレベル
        /// </summary>
        public Log.LogLevel OutputLogLevel {
            get;
            set {
                field = Log.LogLevel.Info;
                Log.Info($"Set OutputLogLevel to {value}");
                field = value;
            }
        } = Log.LogLevel.Debug;

        /// <summary>
        /// ログファイル出力有無
        /// </summary>
        public bool OutputLogToFile { get; set; } = true;
        /// <summary>
        /// ログファイル出力数
        /// </summary>
        public int LogFileAmount { get; set; } = 1;
        #endregion

        /// <summary>
        /// ログファイル出力リスナーの削除中か
        /// </summary>
        private bool mOnDeleteLogFileListener = false;

        static LogImpl() => Register(static () => new LogImpl());

        /// <summary>
        /// <see cref="Log"/> クラスの新しいインスタンスを初期化します
        /// </summary>
        private LogImpl()
        {
            Log.OutputImpl = this.WriteLine;
            Log.IsOutputImpl = level => level >= this.OutputLogLevel;

            Trace.AutoFlush = true;
            _ = Trace.Listeners.Add(new ConsoleTraceListener());
        }

        ~LogImpl()
        {
            this.DeleteLogFileListener();
        }

        /// <summary>
        /// ログファイル出力リスナーを生成する
        /// </summary>
        private void CreateLogFileListener()
        {
            if (this.mLogFileListener == null) {
                if (!Directory.Exists(LogFolderPath)) { _ = Directory.CreateDirectory(LogFolderPath); }

                TextWriter sw = new StreamWriter(LogFilePath, true, Encoding.UTF8);

                this.mLogFileListener = new TextWriterTraceListener {
                    Name = mLogFileListenerName,
                    Writer = sw,
                    TraceOutputOptions = TraceOptions.DateTime | TraceOptions.Timestamp | TraceOptions.ProcessId | TraceOptions.ThreadId
                };

                _ = Trace.Listeners.Add(this.mLogFileListener);

                // リスナーを登録してから開始ログを出力する
                Log.Info("== Start to output log ==");

                // リスナーを生成するときのみ古いログファイルを削除する
                DeleteOldLogFiles();
            }
        }

        /// <summary>
        /// ログファイル出力リスナーを削除する
        /// </summary>
        private void DeleteLogFileListener()
        {
            // 多重呼び出し防止
            if (this.mOnDeleteLogFileListener) { return; }

            this.mOnDeleteLogFileListener = true;
            if (this.mLogFileListener != null) {
                // 終了ログを出力してからリスナーを削除する
                Log.Info("== Finish to outout log ==");

                this.mLogFileListener.Close();
                this.mLogFileListener.Dispose();
                this.mLogFileListener = null;
            }
            Trace.Listeners.Remove(mLogFileListenerName);
            this.mOnDeleteLogFileListener = false;
        }

        /// <summary>
        /// ログファイル一覧を取得する
        /// </summary>
        /// <returns>ログファイル一覧</returns>
        public static List<string> GetLogFiles() => FileUtil.GetFiles(LogFolderPath, LogFileNamePattern, SearchOption.TopDirectoryOnly);

        /// <summary>
        /// 古いログファイルを削除する
        /// </summary>
        public static void DeleteOldLogFiles() => FileUtil.DeleteOldFiles(LogFolderPath, LogFileNamePattern, Instance.LogFileAmount);

        /// <summary>
        /// ログファイルにログを1行出力する
        /// </summary>
        /// <param name="level">ログレベル</param>
        /// <param name="message">出力メッセージ</param>
        /// <param name="filePath">出力元ファイルパス</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数(負の場合は絶対値の数だけ「-」を繰り返す)</param>
        private void WriteLine(Log.LogLevel level, string message, string filePath, string methodName, int lineNumber)
        {
            if (this.OutputLogToFile && 0 < this.LogFileAmount) {
                this.CreateLogFileListener();
            }
            else {
                this.DeleteLogFileListener();
            }

            if (level < this.OutputLogLevel) { return; }

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
