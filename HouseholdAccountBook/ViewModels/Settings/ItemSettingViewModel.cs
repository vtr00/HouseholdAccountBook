using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Common;
using HouseholdAccountBook.ViewModels.Component;
using System.Collections.Generic;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// 分類/項目設定VM
    /// </summary>
    public class ItemSettingViewModel : ViewModelBase
    {
        #region Bindingプロパティ
        /// <summary>
        /// 種類
        /// </summary>
        public HierarchicalKind Kind { get; init; }

        /// <summary>
        /// ID
        /// </summary>
        public IdObj Id { get; init; }

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; init; }

        /// <summary>
        /// 名称
        /// </summary>
        public InputableViewModel<string> NameVM { get; } = new();
        /// <summary>
        /// アセットリストVM
        /// </summary>
        public SelectableViewModel<AssetModel, AssetIdObj> AssetListVM => field ??= new(static vm => vm?.Id);
        /// <summary>
        /// 項目種別リストVM
        /// </summary>
        public SelectableViewModel<KeyValuePair<ItemKind, string>, ItemKind> ItemKindListVM => field ??= new(static p => p.Key);
        /// <summary>
        /// 未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager DirtyState => field ??= new(this.NameVM, this.AssetListVM, this.ItemKindListVM);

        /// <summary>
        /// 関連性リストVM
        /// </summary>
        public SelectableViewModel<RelationViewModel, AccountIdObj> RelationListVM => field ??= new(static vm => vm?.Id);

        /// <summary>
        /// 店舗リストVM
        /// </summary>
        public SelectableViewModel<ShopModel, string> ShopListVM => field ??= new(static vm => vm?.Name);

        /// <summary>
        /// 備考リストVM
        /// </summary>
        public SelectableViewModel<RemarkModel, string> RemarkListVM => field ??= new(static vm => vm?.Remark);

        /// <summary>
        /// リネーム可能か
        /// </summary>
        public bool IsRenamable => this.Kind != HierarchicalKind.Balance;

        /// <summary>
        /// 設定可能か
        /// </summary>
        public bool IsSettable => this.Kind == HierarchicalKind.Item;
        #endregion

        /// <summary>
        /// 収支設定コンストラクタ
        /// </summary>
        public ItemSettingViewModel()
        {
            this.Kind = HierarchicalKind.Balance;
            this.Id = IdObj.System;
            this.SortOrder = -1;
            this.NameVM.LoadedValue = string.Empty;
        }

        /// <summary>
        /// 分類設定コンストラクタ
        /// </summary>
        /// <param name="category">分類Model</param>
        public ItemSettingViewModel(CategoryModel category)
        {
            this.Kind = HierarchicalKind.Category;
            this.Id = category.Id;
            this.SortOrder = category.SortOrder;
            this.NameVM.LoadedValue = category.Name;
        }

        /// <summary>
        /// 項目設定コンストラクタ
        /// </summary>
        /// <param name="item">項目Model</param>
        public ItemSettingViewModel(ItemModel item)
        {
            this.Kind = HierarchicalKind.Item;
            this.Id = item.Id;
            this.SortOrder = item.SortOrder;
            this.NameVM.LoadedValue = item.Name;
        }
    }
}
