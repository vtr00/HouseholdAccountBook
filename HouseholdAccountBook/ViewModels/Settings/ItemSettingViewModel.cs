using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using System.Collections.ObjectModel;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// 分類/項目設定VM
    /// </summary>
    public class ItemSettingViewModel : BindableBase
    {
        #region プロパティ
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
        /// 入力された名称
        /// </summary>
        #region InputedName
        public string InputedName {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 関係性VMリスト
        /// </summary>
        #region RelationVMList
        public ObservableCollection<RelationModel> RelationVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された関係性VM
        /// </summary>
        #region SelectedRelationVM
        public RelationViewModel SelectedRelationVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 店舗VMリスト
        /// </summary>
        #region ShopVMList
        public ObservableCollection<ShopModel> ShopVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された店舗VM
        /// </summary>
        #region SelectedShopVM
        public ShopModel SelectedShopVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 備考VMリスト
        /// </summary>
        #region RemarkVMList
        public ObservableCollection<RemarkModel> RemarkVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された備考VM
        /// </summary>
        #region SelectedRemarkVM
        public RemarkModel SelectedRemarkVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

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
            this.Id = -1;
            this.SortOrder = -1;
            this.InputedName = string.Empty;
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
            this.InputedName = category.Name;
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
            this.InputedName = item.Name;
        }
    }
}
