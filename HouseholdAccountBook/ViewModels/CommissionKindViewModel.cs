using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 手数料種別
    /// </summary>
    public class CommissionKindViewModel
    {
        #region プロパティ
        /// <summary>
        /// 手数料種別
        /// </summary>
        public CommissionKind CommissionKind { get; set; }
        /// <summary>
        /// 手数料種別名
        /// </summary>
        public string CommissionKindName { get; set; }
        #endregion
    }
}
