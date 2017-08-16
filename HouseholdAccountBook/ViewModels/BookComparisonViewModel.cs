namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 帳簿VM(CSV比較用)
    /// </summary>
    public class BookComparisonViewModel
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
        /// 日付インデックス
        /// </summary>
        public int ActDateIndex { get; set; }

        /// <summary>
        /// 支出インデックス
        /// </summary>
        public int OutgoIndex { get; set; }

        /// <summary>
        /// 項目名インデックス
        /// </summary>
        public int ItemNameIndex { get; set; }
        #endregion
    }
}
