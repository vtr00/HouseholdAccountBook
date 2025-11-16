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

        public void Log(Exception e)
        {
            if (!Directory.Exists(UnhandledExceptionInfoFolderPath)) {
                _ = Directory.CreateDirectory(UnhandledExceptionInfoFolderPath);
            }

            // 例外情報をファイルに保存する
            this.RelatedFilePath = UnhandledExceptionInfoFilePath;
            string jsonCode = JsonConvert.SerializeObject(e, Formatting.Indented);
            File.WriteAllText(this.RelatedFilePath, jsonCode);
        }
    }
}
