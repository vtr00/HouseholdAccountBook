using HouseholdAccountBook.Interfaces;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 階層構造設定VM
    /// </summary>
    public class HierarchicalSettingViewModel : BindableBase, IMultiSelectable
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

        #region プロパティ
        /// <summary>
        /// 種類
        /// </summary>
        public HierarchicalKind Kind { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 親要素VM
        /// </summary>
        public HierarchicalSettingViewModel ParentVM { get; set; }

        /// <summary>
        /// ソート順
        /// </summary>
        #region SortOrder
        public int SortOrder
        {
            get => this._SortOrder;
            set => this.SetProperty(ref this._SortOrder, value);
        }
        private int _SortOrder = default;
        #endregion

        /// <summary>
        /// 名称
        /// </summary>
        #region Name
        public string Name
        {
            get => this._Name;
            set => this.SetProperty(ref this._Name, value);
        }
        private string _Name = default;
        #endregion

        /// <summary>
        /// 選択されているか
        /// </summary>
        #region IsSelected
        public bool IsSelected
        {
            get => this._IsSelected;
            set => this.SetProperty(ref this._IsSelected, value);
        }
        private bool _IsSelected = default;
        #endregion

        /// <summary>
        /// 子要素VMリスト
        /// </summary>
        #region ChildrenVMList
        public ObservableCollection<HierarchicalSettingViewModel> ChildrenVMList
        {
            get => this._ChildrenVMList;
            set => this.SetProperty(ref this._ChildrenVMList, value);
        }
        private ObservableCollection<HierarchicalSettingViewModel> _ChildrenVMList = default;
        #endregion

        /// <summary>
        /// 関係性VMリスト
        /// </summary>
        #region RelationVMList
        public ObservableCollection<RelationViewModel> RelationVMList
        {
            get => this._RelationVMList;
            set => this.SetProperty(ref this._RelationVMList, value);
        }
        private ObservableCollection<RelationViewModel> _RelationVMList = default;
        #endregion
        /// <summary>
        /// 選択された関係性VM
        /// </summary>
        #region SelectedRelationVM
        public RelationViewModel SelectedRelationVM
        {
            get => this._SelectedRelationVM;
            set => this.SetProperty(ref this._SelectedRelationVM, value);
        }
        private RelationViewModel _SelectedRelationVM = default;
        #endregion

        /// <summary>
        /// 店舗VMリスト
        /// </summary>
        #region ShopVMList
        public ObservableCollection<ShopViewModel> ShopVMList
        {
            get => this._ShopVMList;
            set => this.SetProperty(ref this._ShopVMList, value);
        }
        private ObservableCollection<ShopViewModel> _ShopVMList = default;
        #endregion
        /// <summary>
        /// 選択された店舗VM
        /// </summary>
        #region SelectedShopVM
        public ShopViewModel SelectedShopVM
        {
            get => this._SelectedShopVM;
            set => this.SetProperty(ref this._SelectedShopVM, value);
        }
        private ShopViewModel _SelectedShopVM = default;
        #endregion

        /// <summary>
        /// 備考VMリスト
        /// </summary>
        #region RemarkVMList
        public ObservableCollection<RemarkViewModel> RemarkVMList
        {
            get => this._RemarkVMList;
            set => this.SetProperty(ref this._RemarkVMList, value);
        }
        private ObservableCollection<RemarkViewModel> _RemarkVMList = default;
        #endregion
        /// <summary>
        /// 選択された備考VM
        /// </summary>
        #region SelectedRemarkVM
        public RemarkViewModel SelectedRemarkVM
        {
            get => this._SelectedRemarkVM;
            set => this.SetProperty(ref this._SelectedRemarkVM, value);
        }
        private RemarkViewModel _SelectedRemarkVM = default;
        #endregion

        /// <summary>
        /// リネーム不可能か
        /// </summary>
        #region CantRename
        public bool CantRename => this.Kind == HierarchicalKind.Balance;
        #endregion

        /// <summary>
        /// 設定可能か
        /// </summary>
        #region IsEnabled
        public bool IsEnabled => this.Kind == HierarchicalKind.Item;
        #endregion
        #endregion
    }
}
