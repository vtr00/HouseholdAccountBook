using HouseholdAccountBook.Extentions;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// 設定VM
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
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
        #region SelectedItemVM
        public HierarchicalItemViewModel SelectedItemVM
        {
            get { return _SelectedItemVM; }
            set {
                if (_SelectedItemVM != value) {
                    _SelectedItemVM = value;
                    PropertyChanged.Raise(this, _nameSelectedItemVM);
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
            /// 変更があったか
            /// </summary>
            #region IsChanged
            public bool IsChanged
            {
                get { return _IsChanged; }
                set {
                    if (_IsChanged != value) {
                        _IsChanged = value;
                        PropertyChanged.Raise(this, _nameIsChanged);
                    }
                }
            }
            private bool _IsChanged = default(bool);
            internal static readonly string _nameIsChanged = PropertyName<HierarchicalItemViewModel>.Get(x => x.IsChanged);
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
                get { return _BookName; }
                set {
                    if (_BookName != value) {
                        _BookName = value;
                        PropertyChanged.Raise(this, _nameBookName);
                    }
                }
            }
            private string _BookName = default(string);
            internal static readonly string _nameBookName = PropertyName<BookSettingViewModel>.Get(x => x.Name);
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
            /// 変更があったか
            /// </summary>
            #region IsChanged
            public bool IsChanged
            {
                get { return _IsChanged; }
                set {
                    if (_IsChanged != value) {
                        _IsChanged = value;
                        PropertyChanged.Raise(this, _nameIsChanged);
                    }
                }
            }
            private bool _IsChanged = default(bool);
            internal static readonly string _nameIsChanged = PropertyName<BookSettingViewModel>.Get(x => x.IsChanged);
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
            /// 変更があったか
            /// </summary>
            #region IsChanged
            public bool IsChanged
            {
                get { return _IsChanged; }
                set {
                    if (_IsChanged != value) {
                        _IsChanged = value;
                        PropertyChanged.Raise(this, _nameIsChanged);
                    }
                }
            }
            private bool _IsChanged = default(bool);
            internal static readonly string _nameIsChanged = PropertyName<RelationViewModel>.Get(x => x.IsChanged);
            #endregion
            
            /// <summary>
            /// プロパティ変更イベントハンドラ
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;
        }
        #endregion
    }
}
