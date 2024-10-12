using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.Others
{
    internal class WindowLog
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
        /// ウィンドウの位置の上端(最終保存値)
        /// </summary>
        private double lastSavedTop = default;
        /// <summary>
        /// ウィンドウの位置の左端(最終保存値)
        /// </summary>
        private double lastSavedLeft = default;
        /// <summary>
        /// ウィンドウの高さ(最終保存値)
        /// </summary>
        private double lastSavedHeight = default;
        /// <summary>
        /// ウィンドウの幅(最終保存値)
        /// </summary>
        private double lastSavedWidth = default;

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
        /// ウィンドウの状態、位置をファイルに保存する
        /// </summary>
        /// <param name="comment">コメント</param>
        /// <param name="forceLog">状態、位置が変わっていなくても保存するか</param>
        public void Log(string comment = "", bool forceLog = false)
        {
            if (!forceLog &&
                this.lastSavedWindowState == this.window.WindowState &&
                this.lastSavedLeft == this.window.Left && this.lastSavedTop == this.window.Top &&
                this.lastSavedHeight == this.window.Height && this.lastSavedWidth == this.window.Width) return;

            this.lastSavedWindowState = this.window.WindowState;
            this.lastSavedTop = this.window.Top;
            this.lastSavedLeft = this.window.Left;
            this.lastSavedWidth = this.window.Width;
            this.lastSavedHeight = this.window.Height;

            string windowState;
            switch (this.window.WindowState) {
                case WindowState.Maximized:
                    windowState = "Max";
                    break;
                case WindowState.Minimized:
                    windowState = "Min";
                    break;
                case WindowState.Normal:
                    windowState = "Normal";
                    break;
                default:
                    windowState = "---";
                    break;
            }

            /// ディレクトリ生成
            if (!Directory.Exists(WindowLocationFolderPath)) Directory.CreateDirectory(WindowLocationFolderPath);

            using (FileStream fs = new FileStream(ConstValue.ConstValue.WindowLocationFilePath(this.window.Name), FileMode.Append)) {
                using (StreamWriter sw = new StreamWriter(fs)) {
                    if (fs.Length == 0) {
                        sw.WriteLine("yyyy/MM/dd HH:mm:ss.ffff\tState\tLeft\tTop\tHeight\tWidth");
                    }

                    sw.Write(string.Format($"{DateTime.Now:yyyy/MM/dd HH:mm:ss.ffff}\t{windowState}\t{this.window.Left}\t{this.window.Top}\t{this.window.Height}\t{this.window.Width}"));
                    if (!string.IsNullOrEmpty(comment)) {
                        sw.Write(string.Format($"\t{comment}"));
                    }
                    sw.WriteLine();
                }
            }
        }
    }
}
