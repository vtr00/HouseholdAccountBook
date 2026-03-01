using HouseholdAccountBook.Models.ValueObjects;
using System;

namespace HouseholdAccountBook.Models.UiDto
{
    /// <summary>
    /// 帳簿Model
    /// </summary>
    /// <param name="id">帳簿ID</param>
    /// <param name="name">帳簿名</param>
    public class BookModel(BookIdObj id, string name)
    {
        #region プロパティ
        /// <summary>
        /// 帳簿ID
        /// </summary>
        public BookIdObj Id { get; init; } = id;

        /// <summary>
        /// 帳簿名
        /// </summary>
        public string Name { get; init; } = name;

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
        /// 期間
        /// </summary>
        public PeriodObj<DateOnly> Period { get; init; }
        #endregion

        #region 支払い情報
        /// <summary>
        /// 支払日
        /// </summary>
        public int? PayDay { get; init; }
        /// <summary>
        /// 支払い元帳簿ID
        /// </summary>
        public BookIdObj DebitBookId { get; init; }
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
