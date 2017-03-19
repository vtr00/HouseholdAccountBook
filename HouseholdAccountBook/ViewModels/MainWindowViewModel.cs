using HouseholdAccountBook.Extentions;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// メインウィンドウVM
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// 表示日付の更新中か
        /// </summary>
        private bool OnUpdateDisplayedDate = false;

        /// <summary>
        /// 選択されたタブインデックス
        /// </summary>
        #region SelectedTabIndex
        public int SelectedTabIndex
        {
            get { return _SelectedTabIndex; }
            set {
                if (SetProperty(ref _SelectedTabIndex, value)) {
                    SelectedTab = (Tab)value;
                }
            }
        }
        private int _SelectedTabIndex = default(int);
        #endregion
        /// <summary>
        /// 選択されたタブ種別
        /// </summary>
        #region SelectedTab
        public Tab SelectedTab
        {
            get { return _SelectedTab; }
            set {
                if (SetProperty(ref _SelectedTab, value)) {
                    SelectedTabIndex = (int)value;
                }
            }
        }
        private Tab _SelectedTab = default(Tab);
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get { return _BookVMList; }
            set { SetProperty(ref _BookVMList, value); }
        }
        private ObservableCollection<BookViewModel> _BookVMList;
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get { return _SelectedBookVM; }
            set { SetProperty(ref _SelectedBookVM, value); }
        }
        private BookViewModel _SelectedBookVM;
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
                if (SetProperty(ref _DisplayedMonth, value)) {
                    if (!OnUpdateDisplayedDate) {
                        OnUpdateDisplayedDate = true;
                        // 表示月の年度の最初の月を表示年とする
                        DisplayedYear = value.GetFirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);
                        OnUpdateDisplayedDate = false;
                    }
                }
            }
        }
        private DateTime _DisplayedMonth;
        #endregion

        /// <summary>
        /// 帳簿項目VMリスト
        /// </summary>
        #region ActionVMList
        public ObservableCollection<ActionViewModel> ActionVMList
        {
            get { return _ActionVMList; }
            set { SetProperty(ref _ActionVMList, value); }
        }
        private ObservableCollection<ActionViewModel> _ActionVMList;
        #endregion
        /// <summary>
        /// 選択された帳簿項目VM
        /// </summary>
        #region SelectedActionVM
        public ActionViewModel SelectedActionVM
        {
            get { return _SelectedActionVM; }
            set { SetProperty(ref _SelectedActionVM, value); }
        }
        private ActionViewModel _SelectedActionVM = default(ActionViewModel);
        #endregion

        /// <summary>
        /// 合計項目VMリスト
        /// </summary>
        #region SummaryVMList
        public ObservableCollection<SummaryViewModel> SummaryVMList
        {
            get { return _SummaryVMList; }
            set { SetProperty(ref _SummaryVMList, value); }
        }
        private ObservableCollection<SummaryViewModel> _SummaryVMList;
        #endregion
        /// <summary>
        /// 選択された合計項目VM
        /// </summary>
        #region SelectedSummaryVM
        public SummaryViewModel SelectedSummaryVM
        {
            get { return _SelectedSummaryVM; }
            set { SetProperty(ref _SelectedSummaryVM, value); }
        }
        private SummaryViewModel _SelectedSummaryVM = default(SummaryViewModel);
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
                if (SetProperty(ref _DisplayedYear, value)) {
                    int yearDiff = value.Year - _DisplayedYear.Year;
                    if (!OnUpdateDisplayedDate) {
                        OnUpdateDisplayedDate = true;
                        // 表示年の差分を表示月に反映する
                        DisplayedMonth = DisplayedMonth.AddYears(yearDiff);
                        OnUpdateDisplayedDate = false;
                    }
                }
            }
        }
        private DateTime _DisplayedYear = default(DateTime);
        #endregion

        /// <summary>
        /// 表示月リスト
        /// </summary>
        #region DisplayedMonths
        public ObservableCollection<string> DisplayedMonths
        {
            get { return _DisplayedMonths; }
            set { SetProperty(ref _DisplayedMonths, value); }
        }
        private ObservableCollection<string> _DisplayedMonths = default(ObservableCollection<string>);
        #endregion

        /// <summary>
        /// 年内合計項目VMリスト
        /// </summary>
        #region SummaryWithinYearVMList
        public ObservableCollection<SummaryWithinYearViewModel> SummaryWithinYearVMList
        {
            get { return _SummaryWithinYearVMList; }
            set { SetProperty(ref _SummaryWithinYearVMList, value); }
        }
        private ObservableCollection<SummaryWithinYearViewModel> _SummaryWithinYearVMList = default(ObservableCollection<SummaryWithinYearViewModel>);
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
    }
}
