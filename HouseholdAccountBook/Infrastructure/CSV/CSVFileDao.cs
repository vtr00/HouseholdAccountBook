using CsvHelper;
using HouseholdAccountBook.Infrastructure.Logger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.CSV
{
    /// <summary>
    /// CSVファイルのDao
    /// </summary>
    public static class CSVFileDao
    {
        /// <summary>
        /// 祝日CSV設定
        /// </summary>
        public class HolidayCSVConfigulation
        {
            /// <summary>
            /// URL
            /// </summary>
            public string Url { get; set; }
            /// <summary>
            /// テキストエンコーディング
            /// </summary>
            public Encoding TextEncoding { get; set; }
            /// <summary>
            /// 日付のインデックス(0開始)
            /// </summary>
            public int DateIndex { get; set; }
        }

        /// <summary>
        /// CSVファイルからCSV比較で用いるデータを読み込む
        /// </summary>
        /// <param name="csvFilePathList">読み込むCSVファイルのパスリスト</param>
        /// <param name="actDateIndex">日付インデックス(1開始)</param>
        /// <param name="itemNameIndex">項目名インデックス(1開始)</param>
        /// <param name="expensesIndex">支出インデックス(1開始)</param>
        /// <param name="encoding">エンコーディング</param>
        /// <returns>読込結果</returns>
        public static async Task<IEnumerable<ActionCsvDto>> LoadCsvCompListAsync(IEnumerable<string> csvFilePathList, int actDateIndex, int itemNameIndex, int expensesIndex, Encoding encoding)
        {
            using FuncLog funcLog = new(new { csvFilePathList, actDateIndex, itemNameIndex, expensesIndex, encoding });

            CsvReaderOptions options = new() {
                HasHeader = false,
                Separator = ',',
                FormatProvider = CultureInfo.CurrentCulture,
                MissingField = CsvMissingFieldHandling.ReturnEmpty,
                MalformedData = CsvMalformedDataHandling.Skip
            };

            List<ActionCsvDto> tmpDtoList = [];
            foreach (string tmpFileName in csvFilePathList) {
                using CsvReader reader = CsvReader.Create(new StreamReader(tmpFileName, encoding), options);
                List<ActionCsvDto> tmpDtoList2 = [];
                while (await reader.ReadAsync()) {
                    try {
                        if (actDateIndex <= 0 ||
                            !DateTime.TryParse(reader.GetString(actDateIndex - 1), CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime date)) {
                            continue;
                        }

                        // 項目名が欠落している場合は null、空欄の場合は空文字とする
                        string name = null;
                        if (0 < itemNameIndex && itemNameIndex <= reader.FieldCount) {
                            name = reader.GetString(itemNameIndex - 1);
                        }

                        if (expensesIndex <= 0 ||
                            !decimal.TryParse(reader.GetString(expensesIndex - 1), NumberStyles.Any, NumberFormatInfo.CurrentInfo, out decimal value)) {
                            continue;
                        }

                        tmpDtoList2.Add(new() { Date = date, Name = name, Value = value });
                    }
                    catch (Exception) { }
                }

                // 有効な行があれば追加する
                if (tmpDtoList2.Count != 0) {
                    tmpDtoList.AddRange(tmpDtoList2);
                }
            }

            return tmpDtoList;
        }

        /// <summary>
        /// 国民の祝日リストを取得する
        /// </summary>
        /// <param name="config">CSV設定</param>
        /// <returns>祝日リスト</returns>
        public static async Task<IEnumerable<DateOnly>> DownloadHolidayListAsync(HolidayCSVConfigulation config)
        {
            using FuncLog funcLog = new(config);

            List<DateOnly> holidayList = [];

            if (config.Url != string.Empty) {
                Uri uri = new(config.Url);

                CsvReaderOptions options = new() {
                    HasHeader = false,
                    Separator = ',',
                    FormatProvider = CultureInfo.CurrentCulture,
                    MissingField = CsvMissingFieldHandling.ReturnEmpty,
                    MalformedData = CsvMalformedDataHandling.Skip
                };

                HttpClientHandler handler = new() { SslProtocols = System.Security.Authentication.SslProtocols.Tls12 };
                try {
                    using HttpClient client = new(handler);
                    Stream stream = await client.GetStreamAsync(uri);
                    if (stream.CanRead) {
                        holidayList.Clear();

                        // CSVファイルを読み込む
                        using CsvReader reader = CsvReader.Create(new StreamReader(stream, config.TextEncoding), options);
                        while (reader.Read()) {
                            if (0 <= config.DateIndex && reader.TryParse(config.DateIndex, out DateOnly dateTime)) {
                                holidayList.Add(dateTime);
                            }
                        }
                    }
                }
                catch (Exception) { }
            }

            return holidayList;
        }

        /// <summary>
        /// CSVファイルに保存する
        /// </summary>
        /// <param name="filePath">保存先ファイルパス</param>
        /// <param name="saveData">保存するデータ</param>
        /// <returns></returns>
        public static async Task SaveDataAsync(string filePath, IEnumerable<IEnumerable<string>> saveData)
        {
            using FuncLog funcLog = new(new { filePath });

            CsvWriterOptions options = new() {
                Separator = ',',
                NewLine = Environment.NewLine,
                Quoting = CsvQuoting.Always,
                ValidateFieldCount = false
            };

            using (StreamWriter streamWriter = new(filePath, false, new UTF8Encoding(true)))
            using (CsvWriter writer = CsvWriter.Create(streamWriter, options, leaveOpen: true)) {
                foreach (IEnumerable<string> row in saveData) {
                    foreach (string str in row) {
                        writer.WriteField(str);
                    }
                    if (writer.FieldCount == 0) {
                        // 列のない行も空行として出力する
                        await writer.FlushAsync();
                        await streamWriter.WriteAsync(Environment.NewLine);
                    }
                    else {
                        await writer.EndRecordAsync();
                    }
                }
            }
        }
    }
}
