using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using static HouseholdAccountBook.Others.FileConstants;

namespace HouseholdAccountBook
{
    public class WindowLog
    {
        /// <summary>
        /// 保存対象のウィンドウ
        /// </summary>
        private readonly Window window = null;

        /// <summary>
        /// ウィンドウの状態(最終保存値)
        /// </summary>
        private WindowState lastSavedWindowState = default;
        /// <summary>
        /// ウィンドウの境界(最終保存値)
        /// </summary>
        private Rect lastSavedRect = new();

        /// <summary>
        /// <see cref="WindowLog"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="window"></param>
        public WindowLog(Window window)
        {
            this.window = window;

            SystemEvents.DisplaySettingsChanging += this.SystemEvents_DisplaySettingsChanging;
            SystemEvents.DisplaySettingsChanged += this.SystemEvents_DisplaySettingsChanged;
        }

        /// <summary>
        /// <see cref="WindowLog"/> クラスのインスタンスを破棄します。
        /// </summary>
        /// <param name="window"></param>
        ~WindowLog()
        {
            SystemEvents.DisplaySettingsChanging -= this.SystemEvents_DisplaySettingsChanging;
            SystemEvents.DisplaySettingsChanged -= this.SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanging(object sender, EventArgs e)
        {
            this.Log("DisplaySettingsChanging", true);
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            this.Log("DisplaySettingsChanged", true);
        }

        /// <summary>
        /// ウィンドウの状態、位置をログファイルに保存する
        /// </summary>
        /// <param name="comment">コメント</param>
        /// <param name="forceLog">状態、位置が変わっていなくても保存するか</param>
        public void Log(string comment = "", bool forceLog = false)
        {
            if (!forceLog &&
                this.lastSavedWindowState == this.window.WindowState &&
                this.lastSavedRect == this.window.RestoreBounds) return;

            this.lastSavedWindowState = this.window.WindowState;
            this.lastSavedRect = this.window.RestoreBounds;
            string windowState = this.window.WindowState switch {
                WindowState.Maximized => "Max",
                WindowState.Minimized => "Min",
                WindowState.Normal => "Normal",
                _ => "---",
            };
            /// ディレクトリ生成
            if (!Directory.Exists(WindowLocationFolderPath)) _ = Directory.CreateDirectory(WindowLocationFolderPath);

            using (FileStream fs = new(WindowLocationFilePath(this.window.Name), FileMode.Append)) {
                using (StreamWriter sw = new(fs)) {
                    if (fs.Length == 0) {
                        sw.WriteLine("yyyy-MM-dd HH:mm:ss.ffff\tState\tLeft\tTop\tHeight\tWidth");
                    }

                    sw.Write(string.Format($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff}\t{windowState}\t{this.window.Left}\t{this.window.Top}\t{this.window.Height}\t{this.window.Width}"));
                    if (!string.IsNullOrEmpty(comment)) {
                        sw.Write(string.Format($"\t{comment}"));
                    }
                    sw.WriteLine();
                }
            }
        }
    }
}
