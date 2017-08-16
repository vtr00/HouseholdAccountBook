using HouseholdAccountBook.Interfaces;
using Prism.Mvvm;

namespace HouseholdAccountBook.ViewModels
{
    public partial class HierarchicalItemViewModel : BindableBase, IMultiSelectable
    {
        /// <summary>
        /// 階層種別
        /// </summary>
        public enum HierarchicalKind
        {
            /// <summary>
            /// 帳簿
            /// </summary>
            Book,
            /// <summary>
            /// 収支
            /// </summary>
            Balance,
            /// <summary>
            /// 種別
            /// </summary>
            Category,
            /// <summary>
            /// 項目
            /// </summary>
            Item
        }
    }

}
