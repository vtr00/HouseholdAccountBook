namespace HouseholdAccountBook.Models.DomainModels
{
    /// <summary>
    /// 金額Model
    /// </summary>
    public class AmountModel
    {
        /// <summary>
        /// 収入
        /// </summary>
        public int? Income { get; set; }
        /// <summary>
        /// 支出
        /// </summary>
        public int? Expenses { get; set; }
        /// <summary>
        /// 収支種別
        /// </summary>
        public BalanceKind BalanceKind { get; set; }
    }
}
