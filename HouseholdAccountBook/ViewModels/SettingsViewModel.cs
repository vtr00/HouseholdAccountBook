using HouseholdAccountBook.Extentions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 設定VM
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 選択された設定タブインデックス
        /// </summary>
        #region SelectedTabIndex
        public int SelectedTabIndex
        {
            get { return _SelectedTabIndex; }
            set {
                if (_SelectedTabIndex != value) {
                    _SelectedTabIndex = value;
                    PropertyChanged?.Raise(this, _nameSelectedTabIndex);

                    SelectedTab = (SettingsTab)value;
                }
            }
        }
        private int _SelectedTabIndex;
        internal static readonly string _nameSelectedTabIndex = PropertyName<MainWindowViewModel>.Get(x => x.SelectedTabIndex);
        #endregion
        /// <summary>
        /// 選択された設定タブ種別
        /// </summary>
        #region SelectedTab
        public SettingsTab SelectedTab
        {
            get { return _SelectedTab; }
            set {
                if (_SelectedTab != value) {
                    _SelectedTab = value;
                    PropertyChanged?.Raise(this, _nameSelectedTab);

                    SelectedTabIndex = (int)value;
                }
            }
        }
        private SettingsTab _SelectedTab = default(SettingsTab);
        internal static readonly string _nameSelectedTab = PropertyName<MainWindowViewModel>.Get(x => x.SelectedTab);
        #endregion
        
        #region 項目設定
        /// <summary>
        /// 階層構造項目VMリスト
        /// </summary>
        #region HierachicalItemVMList
        public ObservableCollection<HierarchicalItemViewModel> HierachicalItemVMList
        {
            get { return _HierachicalItemVMList; }
            set {
                if (_HierachicalItemVMList != value) {
                    _HierachicalItemVMList = value;
                    PropertyChanged.Raise(this, _nameHierachicalItemVMList);
                }
            }
        }
        private ObservableCollection<HierarchicalItemViewModel> _HierachicalItemVMList = default(ObservableCollection<HierarchicalItemViewModel>);
        internal static readonly string _nameHierachicalItemVMList = PropertyName<SettingsViewModel>.Get(x => x.HierachicalItemVMList);
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

                    _SelectedItemVM = value;
                    PropertyChanged.Raise(this, _nameSelectedItemVM);

                    // 選択状態を更新する(_SelectedItemVMより後で更新)
                    if(value != null && !value.IsSelected) {
                        value.IsSelected = true;
                    }
                }
            }
        }
        private HierarchicalItemViewModel _SelectedItemVM = default(HierarchicalItemViewModel);
        internal static readonly string _nameSelectedItemVM = PropertyName<SettingsViewModel>.Get(x => x.SelectedItemVM);
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
            set {
                if (_BookVMList != value) {
                    _BookVMList = value;
                    PropertyChanged.Raise(this, _nameBookVMList);
                }
            }
        }
        private ObservableCollection<BookSettingViewModel> _BookVMList = default(ObservableCollection<BookSettingViewModel>);
        internal static readonly string _nameBookVMList = PropertyName<SettingsViewModel>.Get(x => x.BookVMList);
        #endregion

        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookSettingViewModel SelectedBookVM
        {
            get { return _SelectedBookVM; }
            set {
                if (_SelectedBookVM != value) {
                    _SelectedBookVM = value;
                    PropertyChanged.Raise(this, _nameSelectedBookVM);
                }
            }
        }
        private BookSettingViewModel _SelectedBookVM = default(BookSettingViewModel);
        internal static readonly string _nameSelectedBookVM = PropertyName<SettingsViewModel>.Get(x => x.SelectedBookVM);
        #endregion
        #endregion
        
        #region その他

        #endregion

        /// <summary>
        /// プロパティ変更イベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

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
        public class HierarchicalItemViewModel : INotifyPropertyChanged
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
                set {
                    if (_Name != value) {
                        _Name = value;
                        PropertyChanged.Raise(this, _nameName);
                    }
                }
            }
            private string _Name = default(string);
            internal static readonly string _nameName = PropertyName<HierarchicalItemViewModel>.Get(x => x.Name);
            #endregion

            /// <summary>
            /// 選択されているか
            /// </summary>
            #region IsSelected
            public bool IsSelected
            {
                get { return _IsSelected; }
                set {
                    if (_IsSelected != value) {
                        _IsSelected = value;
                        PropertyChanged.Raise(this, _nameIsSelected);
                    }
                }
            }
            private bool _IsSelected = default(bool);
            internal static readonly string _nameIsSelected = PropertyName<HierarchicalItemViewModel>.Get(x => x.IsSelected);
            #endregion

            /// <summary>
            /// 子要素VMリスト
            /// </summary>
            #region ChildrenVMList
            public ObservableCollection<HierarchicalItemViewModel> ChildrenVMList
            {
                get { return _ChildrenVMList; }
                set {
                    if (_ChildrenVMList != value) {
                        _ChildrenVMList = value;
                        PropertyChanged.Raise(this, _nameChildrenVMList);
                    }
                }
            }
            private ObservableCollection<HierarchicalItemViewModel> _ChildrenVMList = default(ObservableCollection<HierarchicalItemViewModel>);
            internal static readonly string _nameChildrenVMList = PropertyName<HierarchicalItemViewModel>.Get(x => x.ChildrenVMList);
            #endregion

            /// <summary>
            /// 関係性VMリスト
            /// </summary>
            #region RelationVMList
            public ObservableCollection<RelationViewModel> RelationVMList
            {
                get { return _RelationVMList; }
                set {
                    if (_RelationVMList != value) {
                        _RelationVMList = value;
                        PropertyChanged.Raise(this, _nameRelationVMList);
                    }
                }
            }
            private ObservableCollection<RelationViewModel> _RelationVMList = default(ObservableCollection<RelationViewModel>);
            internal static readonly string _nameRelationVMList = PropertyName<HierarchicalItemViewModel>.Get(x => x.RelationVMList);
            #endregion

            /// <summary>
            /// 店舗名リスト
            /// </summary>
            #region ShopNameList
            public ObservableCollection<string> ShopNameList
            {
                get { return _ShopNameList; }
                set {
                    if (_ShopNameList != value) {
                        _ShopNameList = value;
                        PropertyChanged.Raise(this, _nameShopNameList);
                    }
                }
            }
            private ObservableCollection<string> _ShopNameList = default(ObservableCollection<string>);
            internal static readonly string _nameShopNameList = PropertyName<HierarchicalItemViewModel>.Get(x => x.ShopNameList);
            #endregion
            /// <summary>
            /// 選択された店舗名
            /// </summary>
            #region SelectedShopName
            public string SelectedShopName
            {
                get { return _SelectedShopName; }
                set {
                    if (_SelectedShopName != value) {
                        _SelectedShopName = value;
                        PropertyChanged?.Raise(this, _nameSelectedShopName);
                    }
                }
            }
            private string _SelectedShopName = default(string);
            internal static readonly string _nameSelectedShopName = PropertyName<HierarchicalItemViewModel>.Get(x => x.SelectedShopName);
            #endregion

            /// <summary>
            /// 備考リスト
            /// </summary>
            #region RemarkList
            public ObservableCollection<string> RemarkList
            {
                get { return _RemarkList; }
                set {
                    if (_RemarkList != value) {
                        _RemarkList = value;
                        PropertyChanged.Raise(this, _nameRemarkList);
                    }
                }
            }
            private ObservableCollection<string> _RemarkList = default(ObservableCollection<string>);
            internal static readonly string _nameRemarkList = PropertyName<HierarchicalItemViewModel>.Get(x => x.RemarkList);
            #endregion
            /// <summary>
            /// 選択された備考
            /// </summary>
            #region SelectedRemark
            public string SelectedRemark
            {
                get { return _SelectedRemark; }
                set {
                    if (_SelectedRemark != value) {
                        _SelectedRemark = value;
                        PropertyChanged?.Raise(this, _nameSelectedRemark);
                    }
                }
            }
            private string _SelectedRemark = default(string);
            internal static readonly string _nameSelectedRemark = PropertyName<HierarchicalItemViewModel>.Get(x => x.SelectedRemark);
            #endregion
            
            /// <summary>
            /// プロパティ変更イベントハンドラ
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;
        }
        
        /// <summary>
        /// 帳簿VM(設定用)
        /// </summary>
        public class BookSettingViewModel : INotifyPropertyChanged
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
                set {
                    if (_Name != value) {
                        _Name = value;
                        PropertyChanged.Raise(this, _nameName);
                    }
                }
            }
            private string _Name = default(string);
            internal static readonly string _nameName = PropertyName<BookSettingViewModel>.Get(x => x.Name);
            #endregion

            /// <summary>
            /// 初期値
            /// </summary>
            #region InitialValue
            public int InitialValue
            {
                get { return _InitialValue; }
                set {
                    if (_InitialValue != value) {
                        _InitialValue = value;
                        PropertyChanged.Raise(this, _nameInitialValue);
                    }
                }
            }
            private int _InitialValue = default(int);
            internal static readonly string _nameInitialValue = PropertyName<BookSettingViewModel>.Get(x => x.InitialValue);
            #endregion

            /// <summary>
            /// 支払日
            /// </summary>
            #region PayDay
            public int? PayDay
            {
                get { return _PayDay; }
                set {
                    if (_PayDay != value) {
                        _PayDay = value;
                        PropertyChanged.Raise(this, _namePayDay);
                    }
                }
            }
            private int? _PayDay = default(int?);
            internal static readonly string _namePayDay = PropertyName<BookSettingViewModel>.Get(x => x.PayDay);
            #endregion
            
            /// <summary>
            /// 関係性VMリスト
            /// </summary>
            #region RelationVMList
            public ObservableCollection<RelationViewModel> RelationVMList
            {
                get { return _RelationVMList; }
                set {
                    if (_RelationVMList != value) {
                        _RelationVMList = value;
                        PropertyChanged.Raise(this, _nameRelationVMList);
                    }
                }
            }
            private ObservableCollection<RelationViewModel> _RelationVMList = default(ObservableCollection<RelationViewModel>);
            internal static readonly string _nameRelationVMList = PropertyName<BookSettingViewModel>.Get(x => x.RelationVMList);
            #endregion
            
            /// <summary>
            /// プロパティ変更イベントハンドラ
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;
        }

        /// <summary>
        /// 関係性VM
        /// </summary>
        public class RelationViewModel : INotifyPropertyChanged
        {
            /// <summary>
            /// 関係があるか
            /// </summary>
            #region IsRelated
            public bool IsRelated
            {
                get { return _IsRelated; }
                set {
                    if (_IsRelated != value) {
                        _IsRelated = value;
                        PropertyChanged.Raise(this, _nameIsRelated);
                    }
                }
            }
            private bool _IsRelated = default(bool);
            internal static readonly string _nameIsRelated = PropertyName<RelationViewModel>.Get(x => x.IsRelated);
            #endregion

            /// <summary>
            /// ID
            /// </summary>
            public int Id { get; set; }
            
            /// <summary>
            /// 表示名
            /// </summary>
            public string Name { get; set; }
            
            /// <summary>
            /// プロパティ変更イベントハンドラ
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;
        }
        #endregion
    }
}
