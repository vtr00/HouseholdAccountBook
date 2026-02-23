using CsvHelper;
using CsvHelper.Configuration;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.Views.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Infrastructure
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
        public static async Task<List<CsvComparisonViewModel>> LoadCsvCompListAsync(IList<string> csvFilePathList, int actDateIndex, int itemNameIndex, int expensesIndex, Encoding encoding)
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
                    if (0 < tmpVMList2.Count) {
                        tmpVMList.AddRange(tmpVMList2);
                    }
                }
            }

            return tmpVMList;
        }

        /// <summary>
        /// DataGridのデータをCSVファイルに保存する
        /// </summary>
        /// <param name="filePath">保存先ファイルパス</param>
        /// <param name="columnInfos">列情報</param>
        /// <param name="items">エクスポートするデータ</param>
        /// <returns></returns>
        public static async Task SaveDataGridDataAsync(string filePath, IEnumerable<DataGridExtensions.ColumnInfo> columnInfos, IEnumerable<object> items)
        {
            List<List<string>> exportData = GetExportData(columnInfos, items);
            CsvConfiguration config = new(CultureInfo.CurrentCulture) {
                HasHeaderRecord = true,              // ヘッダー出力
                Delimiter = ",",                     // 区切り文字
                NewLine = Environment.NewLine,       // 改行コード
                Encoding = new UTF8Encoding(true),   // BOM付きUTF-8（重要）
                ShouldQuote = args => true           // Excel誤認識対策（安全重視）
            };
            using (CsvWriter writer = new(new StreamWriter(filePath, false, Encoding.UTF8), config)) {
                foreach (List<string> row in exportData) {
                    foreach (string str in row) {
                        writer.WriteField(str);
                    }
                    await writer.NextRecordAsync();
                }
            }
        }

        /// <summary>
        /// リフレクションの情報を元にエクスポートするデータを取得する
        /// </summary>
        /// <param name="columnInfos">列情報</param>
        /// <returns>エクスポートするデータ</returns>
        private static List<List<string>> GetExportData(IEnumerable<DataGridExtensions.ColumnInfo> columnInfos, IEnumerable<object> items)
        {
            List<List<string>> data = [];

            // ヘッダ名を取得する
            List<string> headers = [];
            foreach (DataGridExtensions.ColumnInfo columnInfo in columnInfos) {
                headers.Add(columnInfo.HeaderText);
            }
            data.Add(headers);

            // 列データを取得する
            foreach (object item in items) {
                List<string> values = [];

                foreach (DataGridExtensions.ColumnInfo columnInfo in columnInfos) {
                    // フィールドの生データを取得する
                    Regex regex = new(@"(.+)\[(\d+)\]");
                    Match match = regex.Match(columnInfo.PropertyPath);
                    object rawValue = null;
                    if (match.Success) {
                        // パスがプロパティの要素の場合
                        PropertyInfo prop = item.GetType().GetProperty(match.Groups[1].Value);
                        object tmp = prop?.GetValue(item);
                        if (tmp is List<int> list && int.TryParse(match.Groups[2].Value, out int idx)) {
                            rawValue = list[idx];
                        }
                    }
                    else {
                        // パスがプロパティの場合
                        PropertyInfo prop = item.GetType().GetProperty(columnInfo.PropertyPath);
                        rawValue = prop?.GetValue(item);
                    }

                    // Converterで変換する
                    object converted = rawValue;
                    if (columnInfo.Converter != null) {
                        converted = columnInfo.Converter.Convert(rawValue, typeof(string), columnInfo.ConverterParameter, columnInfo.ConverterCulture);
                    }
                    // 文字列フォーマットに従って文字列に変換する
                    string text = converted?.ToString();
                    if (!string.IsNullOrEmpty(columnInfo.StringFormat)) {
                        if (converted is IFormattable formattable) {
                            text = formattable.ToString(columnInfo.StringFormat, columnInfo.ConverterCulture);
                        }
                    }

                    values.Add(text);
                }
                data.Add(values);
            }

            return data;
        }
    }
}
