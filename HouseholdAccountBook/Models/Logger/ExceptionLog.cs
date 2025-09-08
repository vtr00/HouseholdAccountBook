using Newtonsoft.Json;
using System;
using System.IO;
using static HouseholdAccountBook.Models.FileConstants;

namespace HouseholdAccountBook.Models.Logger
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
            using (FileStream fs = new(this.RelatedFilePath, FileMode.Create)) {
                using (StreamWriter sw = new(fs)) {
                    sw.WriteLine(jsonCode);
                }
            }
        }
    }
}
