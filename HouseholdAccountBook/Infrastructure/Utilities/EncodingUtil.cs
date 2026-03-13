using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HouseholdAccountBook.Infrastructure.Utilities
{
    /// <summary>
    /// エンコーディングユーティリティ
    /// </summary>
    public class EncodingUtil
    {
        /// <summary>
        /// 文字エンコーディング一覧を取得する
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<int, string>> GetTextEncodingList()
        {
            List<EncodingInfo> encodingInfos = [.. Encoding.GetEncodings()];
            encodingInfos.Sort(static (info1, info2) => string.Compare(info1.Name, info2.Name, StringComparison.Ordinal));

            IEnumerable<KeyValuePair<int, string>> encodingList = [.. encodingInfos.Select(static info =>
                new KeyValuePair<int, string>(info.CodePage, $"{info.Name.ToUpper(System.Globalization.CultureInfo.CurrentCulture)} ({info.DisplayName})")
            )];
            return encodingList;
        }
    }
}
