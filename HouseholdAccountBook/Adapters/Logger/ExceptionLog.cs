using Newtonsoft.Json;
using System;
using System.IO;

namespace HouseholdAccountBook.Adapters.Logger
{
    public class ExceptionLog
    {
        #region プロパティ
        /// <summary>
        /// フォルダパス
        /// </summary>
        public static string FolderPath { get; set; } = @".\UnhandledExceptions";
        /// <summary>
        /// ファイル ポストフィックス
        /// </summary>
        public static string FileExt { get; set; } = "txt";
        /// <summary>
        /// 新規ファイルパス
        /// </summary>
        private static string NewFilePath {
            get {
                DateTime dt = DateTime.Now;
                return string.Format($@"{FolderPath}\{dt:yyyyMMdd_HHmmss}.{FileExt}");
            }
        }
        /// <summary>
        /// ファイル名パターン
        /// </summary>
        public static string FileNamePattern => $"*_*.{FileExt}";

        /// <summary>
        /// 例外ログファイル数
        /// </summary>
        public static int LogFileAmount { get; set; } = 1;

        /// <summary>
        /// 保存先ファイルパス
        /// </summary>
        public string RelatedFilePath { get; set; }
        #endregion

        public ExceptionLog() { }

        /// <summary>
        /// 例外ログを出力する
        /// </summary>
        /// <param name="e">出力対象の例外</param>
        public void Log(Exception e)
        {
            if (0 < LogFileAmount) {
                if (!Directory.Exists(FolderPath)) {
                    _ = Directory.CreateDirectory(FolderPath);
                }

                // 例外情報をファイルに保存する
                this.RelatedFilePath = NewFilePath;
                string jsonCode = JsonConvert.SerializeObject(e, Formatting.Indented);
                File.WriteAllText(this.RelatedFilePath, jsonCode);
            }

            // 古い例外情報ファイルを削除する
            DeleteOldExceptionLogs();
        }

        /// <summary>
        /// 古い例外情報ファイルを削除する
        /// </summary>
        public static void DeleteOldExceptionLogs() => FileUtil.DeleteOldFiles(FolderPath, FileNamePattern, LogFileAmount);
    }
}
