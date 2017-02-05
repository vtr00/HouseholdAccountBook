using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 収支種別VM
    /// </summary>
    public partial class BalanceKindViewModel
    {
        /// <summary>
        /// 収支種別
        /// </summary>
        public BalanceKind BalanceKind { get; set; }
        /// <summary>
        /// 収支種別名
        /// </summary>
        public string BalanceKindName { get; set; }
    }
}
