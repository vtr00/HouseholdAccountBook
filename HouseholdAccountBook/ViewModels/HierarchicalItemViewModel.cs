using HouseholdAccountBook.Interfaces;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 階層構造項目VM
    /// </summary>
    public partial class HierarchicalItemViewModel : BindableBase, IMultiSelectable
    {
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
        public HierarchicalItemViewModel ParentVM { get; set; }

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
        public ObservableCollection<HierarchicalItemViewModel> ChildrenVMList
        {
            get => this._ChildrenVMList;
            set => this.SetProperty(ref this._ChildrenVMList, value);
        }
        private ObservableCollection<HierarchicalItemViewModel> _ChildrenVMList = default;
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
        /// 店舗名リスト
        /// </summary>
        #region ShopNameList
        public ObservableCollection<string> ShopNameList
        {
            get => this._ShopNameList;
            set => this.SetProperty(ref this._ShopNameList, value);
        }
        private ObservableCollection<string> _ShopNameList = default;
        #endregion
        /// <summary>
        /// 選択された店舗名
        /// </summary>
        #region SelectedShopName
        public string SelectedShopName
        {
            get => this._SelectedShopName;
            set => this.SetProperty(ref this._SelectedShopName, value);
        }
        private string _SelectedShopName = default;
        #endregion

        /// <summary>
        /// 備考リスト
        /// </summary>
        #region RemarkList
        public ObservableCollection<string> RemarkList
        {
            get => this._RemarkList;
            set => this.SetProperty(ref this._RemarkList, value);
        }
        private ObservableCollection<string> _RemarkList = default;
        #endregion
        /// <summary>
        /// 選択された備考
        /// </summary>
        #region SelectedRemark
        public string SelectedRemark
        {
            get => this._SelectedRemark;
            set => this.SetProperty(ref this._SelectedRemark, value);
        }
        private string _SelectedRemark = default;
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
