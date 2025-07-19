namespace HouseholdAccountBook.Dto.Others
{
    public class SummaryInfoDto
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SummaryInfoDto() { }

        /// <summary>
        /// 収支種別
        /// </summary>
        public int BalanceKind { get; set; } = (int)ConstValue.ConstValue.BalanceKind.Others;
        /// <summary>
        /// 分類ID
        /// </summary>
        public int CategoryId { get; set; } = -1;
        /// <summary>
        /// 分類名
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = -1;
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; } = string.Empty;
        /// <summary>
        /// 合計
        /// </summary>
        public int Total { get; set; } = 0;
    }
}
