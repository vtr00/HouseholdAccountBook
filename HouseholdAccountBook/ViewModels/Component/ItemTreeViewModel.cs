using HouseholdAccountBook.Models;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 項目ツリーVM
    /// </summary>
    public class ItemTreeViewModel : HierarchicalViewModel<ItemTreeViewModel>
    {
        /// <summary>
        /// 階層種別
        /// </summary>
        public HierarchicalKind Kind => (HierarchicalKind)this.Depth;
    }
}
