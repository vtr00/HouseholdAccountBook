using CsvHelper;
using CsvHelper.Configuration;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        /// CSV比較で用いるデータを読み込む
        /// </summary>
        /// <param name="csvFilePathList">読み込むCSVファイルのパスリスト</param>
        /// <param name="actDateIndex">日付インデックス</param>
        /// <param name="itemNameIndex">項目名インデックス</param>
        /// <param name="expensesIndex">支出インデックス</param>
        /// <param name="encoding">エンコーディング</param>
        /// <returns>読込結果</returns>
        public static async Task<IEnumerable<CsvComparisonViewModel>> LoadCsvCompListAsync(IEnumerable<string> csvFilePathList, int actDateIndex, int itemNameIndex, int expensesIndex, Encoding encoding)
        {
            CsvConfiguration csvConfig = new(CultureInfo.CurrentCulture) {
                HasHeaderRecord = true,
                MissingFieldFound = mffa => { }
            };

            List<CsvComparisonViewModel> tmpVMList = [];
            foreach (string tmpFileName in csvFilePathList) {
                using (CsvReader reader = new(new StreamReader(tmpFileName, encoding), csvConfig)) {
                    List<CsvComparisonViewModel> tmpVMList2 = [];
                    while (await reader.ReadAsync()) {
                        try {
                            if (!reader.TryGetField(actDateIndex - 1, out DateTime date)) {
                                continue;
                            }
                            if (!reader.TryGetField(itemNameIndex - 1, out string name)) {
                                // 項目名は読込みに失敗してもOK
                                name = null;
                            }
                            if (!reader.TryGetField(expensesIndex - 1, out string valueStr) ||
                                !int.TryParse(valueStr, NumberStyles.Any, NumberFormatInfo.CurrentInfo, out int value)) {
                                continue;
                            }

                            tmpVMList2.Add(new() { Record = new() { Date = date, Name = name, Value = value } });
                        }
                        catch (Exception) { }
                    }

                    // 有効な行があれば追加する
                    if (tmpVMList2.Count != 0) {
                        tmpVMList.AddRange(tmpVMList2);
                    }
                }
            }

            return tmpVMList;
        }

        /// <summary>
        /// CSVファイルに保存する
        /// </summary>
        /// <param name="filePath">保存先ファイルパス</param>
        /// <param name="saveData">保存するデータ</param>
        /// <returns></returns>
        public static async Task SaveDataAsync(string filePath, IEnumerable<IEnumerable<string>> saveData)
        {
            CsvConfiguration config = new(CultureInfo.CurrentCulture) {
                HasHeaderRecord = true,              // ヘッダー出力
                Delimiter = ",",                     // 区切り文字
                NewLine = Environment.NewLine,       // 改行コード
                Encoding = new UTF8Encoding(true),   // BOM付きUTF-8（重要）
                ShouldQuote = args => true           // Excel誤認識対策（安全重視）
            };
            using (CsvWriter writer = new(new StreamWriter(filePath, false, Encoding.UTF8), config)) {
                foreach (IEnumerable<string> row in saveData) {
                    foreach (string str in row) {
                        writer.WriteField(str);
                    }
                    await writer.NextRecordAsync();
                }
            }
        }
    }
}
