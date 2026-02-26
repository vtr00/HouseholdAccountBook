using HouseholdAccountBook.Models;
using System;

namespace HouseholdAccountBook.Models.DomainModels
{
    /// <summary>
    /// 帳簿Model
    /// </summary>
    public class BookModel
    {
        #region プロパティ
        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int? Id { get; init; }

        /// <summary>
        /// 帳簿名
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; init; }

        /// <summary>
        /// 帳簿種別
        /// </summary>
        public BookKind BookKind { get; init; }

        #region 期間情報
        /// <summary>
        /// 開始日
        /// </summary>
        public DateTime? StartTime { get; init; }
        /// <summary>
        /// 終了日
        /// </summary>
        public DateTime? EndTime { get; init; }
        #endregion

        #region 支払い情報
        /// <summary>
        /// 支払日
        /// </summary>
        public int? PayDay { get; init; }
        /// <summary>
        /// 支払い元帳簿ID
        /// </summary>
        public int? DebitBookId { get; init; }
        #endregion

        #region CSV情報
        /// <summary>
        /// CSVフォルダパス
        /// </summary>
        public string CsvFolderPath { get; init; }
        /// <summary>
        /// 文字エンコーディング
        /// </summary>
        public int TextEncoding { get; init; }

        /// <summary>
        /// 日付 位置(1開始)
        /// </summary>
        public int? ActDateIndex { get; init; }
        /// <summary>
        /// 支出 位置(1開始)
        /// </summary>
        public int? ExpensesIndex { get; init; }
        /// <summary>
        /// 項目名 位置(1開始)
        /// </summary>
        public int? ItemNameIndex { get; init; }
        #endregion
        #endregion
    }
}
