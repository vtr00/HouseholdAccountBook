using HouseholdAccountBook.Interfaces;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 階層構造項目VM
    /// </summary>
    public class HierarchicalItemViewModel : BindableBase, IMultiSelectable
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
            get { return _Name; }
            set { SetProperty(ref _Name, value); }
        }
        private string _Name = default(string);
        #endregion

        /// <summary>
        /// 選択されているか
        /// </summary>
        #region IsSelected
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetProperty(ref _IsSelected, value); }
        }
        private bool _IsSelected = default(bool);
        #endregion

        /// <summary>
        /// 子要素VMリスト
        /// </summary>
        #region ChildrenVMList
        public ObservableCollection<HierarchicalItemViewModel> ChildrenVMList
        {
            get { return _ChildrenVMList; }
            set { SetProperty(ref _ChildrenVMList, value); }
        }
        private ObservableCollection<HierarchicalItemViewModel> _ChildrenVMList = default(ObservableCollection<HierarchicalItemViewModel>);
        #endregion

        /// <summary>
        /// 関係性VMリスト
        /// </summary>
        #region RelationVMList
        public ObservableCollection<RelationViewModel> RelationVMList
        {
            get { return _RelationVMList; }
            set { SetProperty(ref _RelationVMList, value); }
        }
        private ObservableCollection<RelationViewModel> _RelationVMList = default(ObservableCollection<RelationViewModel>);
        #endregion
        /// <summary>
        /// 選択された関係性VM
        /// </summary>
        #region SelectedRelationVM
        public RelationViewModel SelectedRelationVM
        {
            get { return _SelectedRelationVM; }
            set { SetProperty(ref _SelectedRelationVM, value); }
        }
        private RelationViewModel _SelectedRelationVM = default(RelationViewModel);
        #endregion

        /// <summary>
        /// 店舗名リスト
        /// </summary>
        #region ShopNameList
        public ObservableCollection<string> ShopNameList
        {
            get { return _ShopNameList; }
            set { SetProperty(ref _ShopNameList, value); }
        }
        private ObservableCollection<string> _ShopNameList = default(ObservableCollection<string>);
        #endregion
        /// <summary>
        /// 選択された店舗名
        /// </summary>
        #region SelectedShopName
        public string SelectedShopName
        {
            get { return _SelectedShopName; }
            set { SetProperty(ref _SelectedShopName, value); }
        }
        private string _SelectedShopName = default(string);
        #endregion

        /// <summary>
        /// 備考リスト
        /// </summary>
        #region RemarkList
        public ObservableCollection<string> RemarkList
        {
            get { return _RemarkList; }
            set { SetProperty(ref _RemarkList, value); }
        }
        private ObservableCollection<string> _RemarkList = default(ObservableCollection<string>);
        #endregion
        /// <summary>
        /// 選択された備考
        /// </summary>
        #region SelectedRemark
        public string SelectedRemark
        {
            get { return _SelectedRemark; }
            set { SetProperty(ref _SelectedRemark, value); }
        }
        private string _SelectedRemark = default(string);
        #endregion

        /// <summary>
        /// リネーム不可能か
        /// </summary>
        #region CantRename
        public bool CantRename
        {
            get { return Kind == HierarchicalKind.Balance; }
        }
        #endregion

        /// <summary>
        /// 設定可能か
        /// </summary>
        #region IsEnabled
        public bool IsEnabled
        {
            get { return Kind == HierarchicalKind.Item; }
        }
        #endregion
    }

}
