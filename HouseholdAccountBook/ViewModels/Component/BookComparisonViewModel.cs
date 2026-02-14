using HouseholdAccountBook.ViewModels.Abstract;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 帳簿VM(CSV比較用)
    /// </summary>
    public class BookComparisonViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 帳簿名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// CSVフォルダパス
        /// </summary>
        public string CsvFolderPath { get; set; }

        /// <summary>
        /// 文字エンコーディング
        /// </summary>
        public int TextEncoding { get; set; }

        /// <summary>
        /// 日付 位置(1開始)
        /// </summary>
        public int? ActDateIndex { get; set; }

        /// <summary>
        /// 支出 位置(1開始)
        /// </summary>
        public int? ExpensesIndex { get; set; }

        /// <summary>
        /// 項目名 位置(1開始)
        /// </summary>
        public int? ItemNameIndex { get; set; }
        #endregion
    }
}
