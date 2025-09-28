using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace HouseholdAccountBook.Extensions
{
    public class EncodingExtensions
    {
        /// <summary>
        /// 文字エンコーディング一覧を取得する
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<KeyValuePair<int, string>> GetTextEncodingList()
        {
            List<EncodingInfo> encodingInfos = [.. Encoding.GetEncodings()];
            encodingInfos.Sort(static (info1, info2) => { return string.Compare(info1.Name, info2.Name, StringComparison.Ordinal); });

            var encodingList = new ObservableCollection<KeyValuePair<int, string>>(encodingInfos.Select(static (info) => {
                return new KeyValuePair<int, string>(info.CodePage, $"{info.Name.ToUpper(System.Globalization.CultureInfo.CurrentCulture)} ({info.DisplayName})");
            }));
            return encodingList;
        }
    }
}
