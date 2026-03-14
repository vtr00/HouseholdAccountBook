using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.ViewModels.Abstract;
using System;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// 項目ツリーVM
    /// </summary>
    public class ItemTreeViewModel : HierarchicalViewModelBase<ItemTreeViewModel>
    {
        /// <summary>
        /// 階層種別
        /// </summary>
        public HierarchicalKind Kind => (HierarchicalKind)this.Depth;

        /// <summary>
        /// 項目ツリー 収支コンストラクタ
        /// </summary>
        /// <param name="kind">収支種別</param>
        /// <exception cref="System.NotImplementedException">収入/支出以外が指定された場合</exception>
        public ItemTreeViewModel(BalanceKind kind)
        {
            this.Depth = (int)HierarchicalKind.Balance;
            this.Id = (int)kind;
            this.SortOrder = -1;
            this.Name = kind switch {
                BalanceKind.Income => Properties.Resources.BalanceKind_Income,
                BalanceKind.Expenses => Properties.Resources.BalanceKind_Expenses,
                _ => throw new ArgumentException($"{kind}")
            };
            this.ParentVM = null;
            this.ChildrenVMList = [];
        }

        /// <summary>
        /// 項目ツリー 分類コンストラクタ
        /// </summary>
        /// <param name="category">分類Model</param>
        public ItemTreeViewModel(CategoryModel category)
        {
            this.Depth = (int)HierarchicalKind.Category;
            this.Id = category.Id;
            this.SortOrder = category.SortOrder;
            this.Name = category.Name;
        }

        /// <summary>
        /// 項目ツリー 項目コンストラクタ
        /// </summary>
        /// <param name="item">項目Model</param>
        public ItemTreeViewModel(ItemModel item)
        {
            this.Depth = (int)HierarchicalKind.Item;
            this.Id = item.Id;
            this.SortOrder = item.SortOrder;
            this.Name = item.Name;
        }
    }
}
