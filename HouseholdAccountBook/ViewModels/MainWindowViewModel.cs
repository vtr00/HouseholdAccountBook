using HouseholdAccountBook.Extentions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// メインウィンドウVM
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 更新中か
        /// </summary>
        private bool onUpdate = false;

        /// <summary>
        /// 選択されたタブインデックス
        /// </summary>
        #region SelectedTabIndex
        public int SelectedTabIndex
        {
            get { return _SelectedTabIndex; }
            set {
                if (_SelectedTabIndex != value) {
                    _SelectedTabIndex = value;
                    PropertyChanged?.Raise(this, _nameSelectedTabIndex);

                    SelectedTab = (Tab)value;
                }
            }
        }
        private int _SelectedTabIndex;
        internal static readonly string _nameSelectedTabIndex = PropertyName<MainWindowViewModel>.Get(x => x.SelectedTabIndex);
        #endregion
        /// <summary>
        /// 選択されたタブ種別
        /// </summary>
        #region SelectedTab
        public Tab SelectedTab
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
        private Tab _SelectedTab = default(Tab);
        internal static readonly string _nameSelectedTab = PropertyName<MainWindowViewModel>.Get(x => x.SelectedTab);
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get { return _BookVMList; }
            set {
                if (_BookVMList != value) {
                    _BookVMList = value;
                    PropertyChanged?.Raise(this, _nameBookVMList);
                }
            }
        }
        private ObservableCollection<BookViewModel> _BookVMList;
        internal static readonly string _nameBookVMList = PropertyName<MainWindowViewModel>.Get(x => x.BookVMList);
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get { return _SelectedBookVM; }
            set {
                if (_SelectedBookVM != value) {
                    _SelectedBookVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedBookVM);
                }
            }
        }
        private BookViewModel _SelectedBookVM;
        internal static readonly string _nameSelectedBookVM = PropertyName<MainWindowViewModel>.Get(x => x.SelectedBookVM);
        #endregion

        #region 帳簿タブ
        /// <summary>
        /// 表示月
        /// </summary>
        #region DisplayedMonth
        public DateTime DisplayedMonth
        {
            get { return _DisplayedMonth; }
            set {
                if (_DisplayedMonth != value) {
                    _DisplayedMonth = value;
                    PropertyChanged?.Raise(this, _nameDisplayedMonth);

                    if (!onUpdate) {
                        onUpdate = true;
                        // 表示月の年度の最初の月を表示年とする
                        DisplayedYear = value.FirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);
                        onUpdate = false;
                    }
                }
            }
        }
        private DateTime _DisplayedMonth;
        internal static readonly string _nameDisplayedMonth = PropertyName<MainWindowViewModel>.Get(x => x.DisplayedMonth);
        #endregion

        /// <summary>
        /// 帳簿項目VMリスト
        /// </summary>
        #region ActionVMList
        public ObservableCollection<ActionViewModel> ActionVMList
        {
            get { return _ActionVMList; }
            set {
                if (_ActionVMList != value) {
                    _ActionVMList = value;
                    PropertyChanged?.Raise(this, _nameActionVMList);
                }
            }
        }
        private ObservableCollection<ActionViewModel> _ActionVMList;
        internal static readonly string _nameActionVMList = PropertyName<MainWindowViewModel>.Get(x => x.ActionVMList);
        #endregion
        /// <summary>
        /// 選択された帳簿項目VM
        /// </summary>
        #region SelectedActionVM
        public ActionViewModel SelectedActionVM
        {
            get { return _SelectedActionVM; }
            set {
                if (_SelectedActionVM != value) {
                    _SelectedActionVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedActionVM);
                }
            }
        }
        private ActionViewModel _SelectedActionVM = default(ActionViewModel);
        internal static readonly string _nameSelectedActionVM = PropertyName<MainWindowViewModel>.Get(x => x.SelectedActionVM);
        #endregion

        /// <summary>
        /// 合計項目VMリスト
        /// </summary>
        #region SummaryVMList
        public ObservableCollection<SummaryViewModel> SummaryVMList
        {
            get { return _SummaryVMList; }
            set {
                if (_SummaryVMList != value) {
                    _SummaryVMList = value;
                    PropertyChanged?.Raise(this, _nameSummaryVMList);
                }
            }
        }
        private ObservableCollection<SummaryViewModel> _SummaryVMList;
        internal static readonly string _nameSummaryVMList = PropertyName<MainWindowViewModel>.Get(x => x.SummaryVMList);
        #endregion
        /// <summary>
        /// 選択された合計項目VM
        /// </summary>
        #region SelectedSummaryVM
        public SummaryViewModel SelectedSummaryVM
        {
            get { return _SelectedSummaryVM; }
            set {
                if (_SelectedSummaryVM != value) {
                    _SelectedSummaryVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedSummaryVM);
                }
            }
        }
        private SummaryViewModel _SelectedSummaryVM = default(SummaryViewModel);
        internal static readonly string _nameSelectedSummaryVM = PropertyName<MainWindowViewModel>.Get(x => x.SelectedSummaryVM);
        #endregion
        #endregion

        #region 年間一覧タブ
        /// <summary>
        /// 表示年
        /// </summary>
        #region DisplayedYear
        public DateTime DisplayedYear
        {
            get { return _DisplayedYear; }
            set {
                if (_DisplayedYear != value) {
                    int yearDiff = value.Year - _DisplayedYear.Year;

                    _DisplayedYear = value;
                    PropertyChanged?.Raise(this, _nameDisplayedYear);

                    if (!onUpdate) {
                        onUpdate = true;
                        // 表示年の差分を表示月に反映する
                        DisplayedMonth = DisplayedMonth.AddYears(yearDiff);
                        onUpdate = false;
                    }
                }
            }
        }
        private DateTime _DisplayedYear;
        internal static readonly string _nameDisplayedYear = PropertyName<MainWindowViewModel>.Get(x => x.DisplayedYear);
        #endregion

        /// <summary>
        /// 表示月リスト
        /// </summary>
        #region DisplayedMonths
        public ObservableCollection<string> DisplayedMonths
        {
            get { return _DisplayedMonths; }
            set {
                if (_DisplayedMonths != value) {
                    _DisplayedMonths = value;
                    PropertyChanged?.Raise(this, _nameDisplayedMonths);
                }
            }
        }
        private ObservableCollection<string> _DisplayedMonths = default(ObservableCollection<string>);
        internal static readonly string _nameDisplayedMonths = PropertyName<MainWindowViewModel>.Get(x => x.DisplayedMonths);
        #endregion

        /// <summary>
        /// 年内合計項目VMリスト
        /// </summary>
        #region SummaryWithinYearVMList
        public ObservableCollection<SummaryWithinYearViewModel> SummaryWithinYearVMList
        {
            get { return _SummaryWithinYearVMList; }
            set {
                if (_SummaryWithinYearVMList != value) {
                    _SummaryWithinYearVMList = value;
                    PropertyChanged?.Raise(this, _nameSummaryWithinYearVMList);
                }
            }
        }
        private ObservableCollection<SummaryWithinYearViewModel> _SummaryWithinYearVMList = default(ObservableCollection<SummaryWithinYearViewModel>);
        internal static readonly string _nameSummaryWithinYearVMList = PropertyName<MainWindowViewModel>.Get(x => x.SummaryWithinYearVMList);
        #endregion
        #endregion
        
        /// <summary>
        /// デバッグビルドか
        /// </summary>
        public bool IsDebug
        {
            get {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// プロパティ変更イベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
