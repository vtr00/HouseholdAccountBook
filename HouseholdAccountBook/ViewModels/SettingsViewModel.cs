using Prism.Mvvm;
using System.Collections.ObjectModel;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 設定VM
    /// </summary>
    public class SettingsViewModel : BindableBase
    {
        /// <summary>
        /// 選択された設定タブインデックス
        /// </summary>
        #region SelectedTabIndex
        public int SelectedTabIndex
        {
            get { return _SelectedTabIndex; }
            set {
                if (SetProperty(ref _SelectedTabIndex, value)) {
                    SelectedTab = (SettingsTab)value;
                }
            }
        }
        private int _SelectedTabIndex = default(int);
        #endregion
        /// <summary>
        /// 選択された設定タブ種別
        /// </summary>
        #region SelectedTab
        public SettingsTab SelectedTab
        {
            get { return _SelectedTab; }
            set {
                if (SetProperty(ref _SelectedTab, value)) {
                    SelectedTabIndex = (int)value;
                }
            }
        }
        private SettingsTab _SelectedTab = default(SettingsTab);
        #endregion
        
        #region 項目設定
        /// <summary>
        /// 階層構造項目VMリスト
        /// </summary>
        #region HierachicalItemVMList
        public ObservableCollection<HierarchicalItemViewModel> HierachicalItemVMList
        {
            get { return _HierachicalItemVMList; }
            set { SetProperty(ref _HierachicalItemVMList, value); }
        }
        private ObservableCollection<HierarchicalItemViewModel> _HierachicalItemVMList = default(ObservableCollection<HierarchicalItemViewModel>);
        #endregion

        /// <summary>
        /// 選択された項目VM
        /// </summary>
        /// <remarks>Not Binded</remarks>
        #region SelectedItemVM
        public HierarchicalItemViewModel SelectedItemVM
        {
            get { return _SelectedItemVM; }
            set {
                if (_SelectedItemVM != value) {
                    if(value == null && _SelectedItemVM != null) {
                        _SelectedItemVM.IsSelected = false;
                    }

                    SetProperty(ref _SelectedItemVM, value);

                    // 選択状態を更新する(_SelectedItemVMより後で更新)
                    if (value != null && !value.IsSelected) {
                        value.IsSelected = true;
                    }
                }
            }
        }
        private HierarchicalItemViewModel _SelectedItemVM = default(HierarchicalItemViewModel);
        #endregion
        #endregion

        #region 帳簿設定
        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookSettingViewModel> BookVMList
        {
            get { return _BookVMList; }
            set { SetProperty(ref _BookVMList, value); }
        }
        private ObservableCollection<BookSettingViewModel> _BookVMList = default(ObservableCollection<BookSettingViewModel>);
        #endregion

        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookSettingViewModel SelectedBookVM
        {
            get { return _SelectedBookVM; }
            set { SetProperty(ref _SelectedBookVM, value); }
        }
        private BookSettingViewModel _SelectedBookVM = default(BookSettingViewModel);
        #endregion
        #endregion
        
        #region その他

        #endregion

        #region 内部クラス定義
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
            /// カテゴリ
            /// </summary>
            Category,
            /// <summary>
            /// アイテム
            /// </summary>
            Item
        }

        /// <summary>
        /// 階層構造項目VM
        /// </summary>
        public class HierarchicalItemViewModel : BindableBase
        {
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
        
        /// <summary>
        /// 帳簿VM(設定用)
        /// </summary>
        public class BookSettingViewModel : BindableBase
        {
            /// <summary>
            /// 帳簿ID
            /// </summary>
            public int? Id { get; set; }

            /// <summary>
            /// 帳簿名
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
            /// 初期値
            /// </summary>
            #region InitialValue
            public int InitialValue
            {
                get { return _InitialValue; }
                set { SetProperty(ref _InitialValue, value); }
            }
            private int _InitialValue = 0;
            #endregion

            /// <summary>
            /// 支払日
            /// </summary>
            #region PayDay
            public int? PayDay
            {
                get { return _PayDay; }
                set { SetProperty(ref _PayDay, value); }
            }
            private int? _PayDay = default(int?);
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
        }

        /// <summary>
        /// 関係性VM
        /// </summary>
        public class RelationViewModel : BindableBase
        {
            /// <summary>
            /// 関係があるか
            /// </summary>
            #region IsRelated
            public bool IsRelated
            {
                get { return _IsRelated; }
                set { SetProperty(ref _IsRelated, value); }
            }
            private bool _IsRelated = default(bool);
            #endregion

            /// <summary>
            /// ID
            /// </summary>
            public int Id { get; set; }
            
            /// <summary>
            /// 表示名
            /// </summary>
            public string Name { get; set; }
        }
        #endregion
    }
}
