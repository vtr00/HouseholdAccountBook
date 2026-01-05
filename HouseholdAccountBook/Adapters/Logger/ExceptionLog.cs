using Newtonsoft.Json;
using System;
using System.IO;
using static HouseholdAccountBook.Adapters.FileConstants;

namespace HouseholdAccountBook.Adapters.Logger
{
    public class ExceptionLog
    {
        public string RelatedFilePath { get; set; }

        public ExceptionLog() { }

        /// <summary>
        /// 例外ログを出力する
        /// </summary>
        /// <param name="e">出力対象の例外</param>
        public void Log(Exception e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (0 < settings.App_UnhandledExceptionLogNum) {
                if (!Directory.Exists(UnhandledExceptionInfoFolderPath)) {
                    _ = Directory.CreateDirectory(UnhandledExceptionInfoFolderPath);
                }

                // 例外情報をファイルに保存する
                this.RelatedFilePath = UnhandledExceptionInfoFilePath;
                string jsonCode = JsonConvert.SerializeObject(e, Formatting.Indented);
                File.WriteAllText(this.RelatedFilePath, jsonCode);
            }

            // 古い例外情報ファイルを削除する
            DeleteOldExceptionLogs();
        }

        /// <summary>
        /// 古い例外情報ファイルを削除する
        /// </summary>
        public static void DeleteOldExceptionLogs()
        {
            Properties.Settings settings = Properties.Settings.Default;
            FileUtil.DeleteOldFiles(UnhandledExceptionInfoFolderPath, UnhandledExceptionInfoFileNamePattern, settings.App_UnhandledExceptionLogNum);
        }
    }
}
