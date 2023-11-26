using HouseholdAccountBook.UserEventArgs;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// CSV比較ウィンドウVM
    /// </summary>
    public class CsvComparisonWindowViewModel : BindableBase
    {
        #region イベント
        /// <summary>
        /// 帳簿変更時のイベント
        /// </summary>
        public event Action<int?> BookChanged;
        #endregion

        #region プロパティ
        /// <summary>
        /// CSVファイルパス(1番目)
        /// </summary>
        #region CsvFilePathes
        public string CsvFilePathes
        {
            get {
                if (0 < this._CsvFilePathList.Count) {
                    return string.Join(",", this._CsvFilePathList);
                }
                else {
                    return null;
                }
            }
        }
        #endregion
        /// <summary>
        /// CSVファイルパスリスト
        /// </summary>
        #region CsvFilePathList
        public ObservableCollection<string> CsvFilePathList
        {
            get => this._CsvFilePathList;
            set => this.SetProperty(ref this._CsvFilePathList, value);
        }
        private ObservableCollection<string> _CsvFilePathList = new ObservableCollection<string>();
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookComparisonViewModel> BookVMList
        {
            get => this._BookVMList;
            set => this.SetProperty(ref this._BookVMList, value);
        }
        private ObservableCollection<BookComparisonViewModel> _BookVMList = default;
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookComparisonViewModel SelectedBookVM
        {
            get => this._SelectedBookVM;
            set {
                if (this.SetProperty(ref this._SelectedBookVM, value)) {
                    this.BookChanged?.Invoke(value.Id);
                }
            }
        }
        private BookComparisonViewModel _SelectedBookVM = new BookComparisonViewModel() { };
        #endregion

        /// <summary>
        /// CSV比較VMリスト
        /// </summary>
        #region CsvComparisonVMList
        public ObservableCollection<CsvComparisonViewModel> CsvComparisonVMList
        {
            get => this._CsvComparisonVMList;
            set => this.SetProperty(ref this._CsvComparisonVMList, value);
        }
        private ObservableCollection<CsvComparisonViewModel> _CsvComparisonVMList = new ObservableCollection<CsvComparisonViewModel>();
        #endregion
        /// <summary>
        /// CSV比較VMの未チェック数
        /// </summary>
        #region UncheckedNum
        public int AllUncheckedCount => this.CsvComparisonVMList.Count((vm) => !vm.IsMatch);
        #endregion
        /// <summary>
        /// CSV比較VMの個数
        /// </summary>
        #region AllCount
        public int AllCount => this.CsvComparisonVMList.Count;
        #endregion
        /// <summary>
        /// CSV比較VMの合計値
        /// </summary>
        #region AllSumValue
        public int AllSumValue => this.CsvComparisonVMList.Sum((vm) => vm.Record.Value);
        #endregion
        
        /// <summary>
        /// 選択されたCSV比較VM(先頭)
        /// </summary>
        #region SelectedCsvComparisonVM
        public CsvComparisonViewModel SelectedCsvComparisonVM
        {
            get => this._SelectedCsvComparisonVM;
            set => this.SetProperty(ref this._SelectedCsvComparisonVM, value);
        }
        private CsvComparisonViewModel _SelectedCsvComparisonVM = default;
        #endregion
        /// <summary>
        /// 選択されたCSV比較VMリスト
        /// </summary>
        #region SelectedCsvComparisonVMList
        public ObservableCollection<CsvComparisonViewModel> SelectedCsvComparisonVMList { get; } = new ObservableCollection<CsvComparisonViewModel>();
        #endregion
        /// <summary>
        /// 選択されたCSV比較VMの未チェック数
        /// </summary>
        #region UncheckedNum
        public int SelectedUncheckedCount => this.SelectedCsvComparisonVMList.Count((vm) => !vm.IsMatch);
        #endregion
        /// <summary>
        /// 選択されたCSV比較VMの個数
        /// </summary>
        #region SelectedCount
        public int SelectedCount => this.SelectedCsvComparisonVMList.Count;
        #endregion
        /// <summary>
        /// 選択されたCSV比較VMの合計値
        /// </summary>
        #region SelectedSumValue
        public int SelectedSumValue => this.SelectedCsvComparisonVMList.Sum((vm) => vm.Record.Value);
        #endregion
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CsvComparisonWindowViewModel()
        {
            this.CsvFilePathList.CollectionChanged += (sender, args) => {
                this.RaisePropertyChanged(nameof(this.CsvFilePathes));
            };

            this.CsvComparisonVMList.CollectionChanged += (sender, args) => {
                this.RaisePropertyChanged(nameof(this.AllUncheckedCount));
                this.RaisePropertyChanged(nameof(this.AllCount));
                this.RaisePropertyChanged(nameof(this.AllSumValue));

                if (args.OldItems != null) {
                    foreach (object tmp in args.OldItems) {
                        if (tmp is CsvComparisonViewModel vm) {
                            vm.IsMatchChanged -= this.RaiseUncheckedCountChanged;
                        }
                    }
                }
                if (args.NewItems != null) {
                    foreach (object tmp in args.NewItems) {
                        if (tmp is CsvComparisonViewModel vm) {
                            vm.IsMatchChanged += this.RaiseUncheckedCountChanged;
                        }
                    }
                }
            };
            this.SelectedCsvComparisonVMList.CollectionChanged += (sender, args) => {
                this.RaisePropertyChanged(nameof(this.SelectedUncheckedCount));
                this.RaisePropertyChanged(nameof(this.SelectedCount));
                this.RaisePropertyChanged(nameof(this.SelectedSumValue));
            };
        }

        /// <summary>
        /// 未チェック数変更を通知する
        /// </summary>
        public void RaiseUncheckedCountChanged(EventArgs<int?, bool> e)
        {
            this.RaisePropertyChanged(nameof(this.AllUncheckedCount));
            this.RaisePropertyChanged(nameof(this.SelectedUncheckedCount));
        }
    }
}
