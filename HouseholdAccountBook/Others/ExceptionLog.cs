using Newtonsoft.Json;
using System;
using System.IO;
using static HouseholdAccountBook.Others.FileConstants;

namespace HouseholdAccountBook
{
    internal class ExceptionLog
    {
        public string RelatedFilePath { get; set; }

        public ExceptionLog() { }

        public void Log(Exception e)
        {
            if (!Directory.Exists(UnhandledExceptionInfoFolderPath)) {
                Directory.CreateDirectory(UnhandledExceptionInfoFolderPath);
            }

            // 例外情報をファイルに保存する
            this.RelatedFilePath = UnhandledExceptionInfoFilePath;
            string jsonCode = JsonConvert.SerializeObject(e, Formatting.Indented);
            using (FileStream fs = new FileStream(this.RelatedFilePath, FileMode.Create)) {
                using (StreamWriter sw = new StreamWriter(fs)) {
                    sw.WriteLine(jsonCode);
                }
            }
        }
    }
}
