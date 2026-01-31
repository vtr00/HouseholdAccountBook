using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace HouseholdAccountBook.Adapters.Logger
{
    /// <summary>
    /// ウィンドウ情報ログ
    /// </summary>
    public class WindowLog
    {
        #region フィールド
        /// <summary>
        /// 保存対象のウィンドウ
        /// </summary>
        private readonly Window mWindow;

        /// <summary>
        /// ウィンドウの状態(最終保存値)
        /// </summary>
        private WindowState mLastSavedWindowState;
        /// <summary>
        /// ウィンドウの境界(最終保存値)
        /// </summary>
        private Rect mLastSavedRect;
        /// <summary>
        /// 古いログファイルを削除済か
        /// </summary>
        private bool mDeletedOldLogs = false;
        #endregion

        #region プロパティ
        /// <summary>
        /// ファイルのフォルダパス
        /// </summary>
        public static string WindowLocationFolderPath { get; set; } = @".\WindowLocations";
        /// <summary>
        /// ファイル ポストフィックス
        /// </summary>
        public static string WindowLocationFileExt { get; set; } = "txt";
        /// <summary>
        /// ファイルパス
        /// </summary>
        public static string WindowLocationFilePath(string windowName) => string.Format($@"{WindowLocationFolderPath}\{windowName}_{AppStartupTime:yyyyMMdd_HHmmss}.{WindowLocationFileExt}");
        /// <summary>
        /// ファイル名パターン
        /// </summary>
        public static string WindowLocationFileNamePattern(string windowName) => $"{windowName}_*_*.{WindowLocationFileExt}";

        /// <summary>
        /// ログ出力有無
        /// </summary>
        public static bool OutputLog { get; set; } = false;
        /// <summary>
        /// ログ出力数
        /// </summary>
        public static int LogFileAmount { get; set; } = 0;
        /// <summary>
        /// アプリ起動時間
        /// </summary>
        public static DateTime AppStartupTime { get; set; } = DateTime.Now;
        #endregion

        /// <summary>
        /// <see cref="WindowLog"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="window"></param>
        public WindowLog(Window window)
        {
            this.mWindow = window;

            SystemEvents.DisplaySettingsChanging += this.SystemEvents_DisplaySettingsChanging;
            SystemEvents.DisplaySettingsChanged += this.SystemEvents_DisplaySettingsChanged;
        }

        /// <summary>
        /// <see cref="WindowLog"/> クラスのインスタンスを破棄します。
        /// </summary>
        ~WindowLog()
        {
            SystemEvents.DisplaySettingsChanging -= this.SystemEvents_DisplaySettingsChanging;
            SystemEvents.DisplaySettingsChanged -= this.SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanging(object sender, EventArgs e) => this.Log("DisplaySettingsChanging", true);

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e) => this.Log("DisplaySettingsChanged", true);

        /// <summary>
        /// ウィンドウの状態、位置をログファイルに保存する
        /// </summary>
        /// <param name="comment">コメント</param>
        /// <param name="forceLog">状態、位置が変わっていなくても保存するか</param>
        public void Log(string comment = "", bool forceLog = false)
        {
            if (OutputLog && 0 < LogFileAmount) {
                if (!forceLog &&
                    this.mLastSavedWindowState == this.mWindow.WindowState &&
                    this.mLastSavedRect == this.mWindow.RestoreBounds) { return; }

                this.mLastSavedWindowState = this.mWindow.WindowState;
                this.mLastSavedRect = this.mWindow.RestoreBounds;
                string windowState = this.mWindow.WindowState switch {
                    WindowState.Maximized => "Max",
                    WindowState.Minimized => "Min",
                    WindowState.Normal => "Normal",
                    _ => "---",
                };

                // ディレクトリ生成
                if (!Directory.Exists(WindowLocationFolderPath)) { _ = Directory.CreateDirectory(WindowLocationFolderPath); }

                using (FileStream fs = new(WindowLocationFilePath(this.mWindow.Name), FileMode.Append)) {
                    using (StreamWriter sw = new(fs)) {
                        if (fs.Length == 0) {
                            sw.WriteLine("yyyy-MM-dd HH:mm:ss.ffff\tState\tLeft\tTop\tHeight\tWidth");
                        }

                        sw.Write(string.Format($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff}\t{windowState}\t{this.mWindow.Left}\t{this.mWindow.Top}\t{this.mWindow.Height}\t{this.mWindow.Width}"));
                        if (!string.IsNullOrEmpty(comment)) {
                            sw.Write(string.Format($"\t{comment}"));
                        }
                        sw.WriteLine();
                    }
                }
            }

            if (!this.mDeletedOldLogs) {
                this.DeleteOldWindowLogs();
            }
        }

        /// <summary>
        /// 古いウィンドウ情報ファイルを削除する
        /// </summary>
        public void DeleteOldWindowLogs()
        {
            this.mDeletedOldLogs = true;

            DeleteOldWindowLogs(this.mWindow.Name);
        }
        /// <summary>
        /// 全てのウィンドウの古いウィンドウ情報ファイルを削除する
        /// </summary>
        /// <param name="windowNameList">ウィンドウ名リスト</param>
        public static void DeleteAllOldWindowLogs(List<string> windowNameList)
        {
            foreach (string windowName in windowNameList) {
                DeleteOldWindowLogs(windowName);
            }
        }
        /// <summary>
        /// 古いウィンドウ情報ファイルを削除する
        /// </summary>
        /// <param name="windowName">ウィンドウ名</param>
        public static void DeleteOldWindowLogs(string windowName) =>
            FileUtil.DeleteOldFiles(WindowLocationFolderPath, WindowLocationFileNamePattern(windowName), LogFileAmount);
    }
}
