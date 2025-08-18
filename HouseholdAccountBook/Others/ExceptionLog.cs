using System;
using System.IO;
using System.Text.Json;
using static HouseholdAccountBook.Others.FileConstants;

namespace HouseholdAccountBook
{
    public class ExceptionLog
    {
        public string RelatedFilePath { get; set; }

        private static JsonSerializerOptions JsonSerializerOptions => new() {
            WriteIndented = true
        };

        public ExceptionLog() { }

        public void Log(Exception e)
        {
            if (!Directory.Exists(UnhandledExceptionInfoFolderPath)) {
                _ = Directory.CreateDirectory(UnhandledExceptionInfoFolderPath);
            }

            // 例外情報をファイルに保存する
            this.RelatedFilePath = UnhandledExceptionInfoFilePath;
            string jsonCode = JsonSerializer.Serialize(e, JsonSerializerOptions);
            using (FileStream fs = new(this.RelatedFilePath, FileMode.Create)) {
                using (StreamWriter sw = new(fs)) {
                    sw.WriteLine(jsonCode);
                }
            }
        }
    }
}
