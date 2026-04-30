using HouseholdAccountBook.Infrastructure.CSV;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Infrastructure.Utilities.Args;
using HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// CSV比較ウィンドウVM
    /// </summary>
    public class CsvComparisonWindowViewModel : WindowViewModelBase
    {
        /// <summary>
        /// CSV比較サービス
        /// </summary>
        private CsvCompService mService;

        #region イベント
        /// <summary>
        /// 帳簿変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<BookIdObj>> BookChanged;
        /// <summary>
        /// 帳簿項目変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<IEnumerable<ActionIdObj>>> ActionChanged;
        /// <summary>
        /// 一致フラグ変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<ActionIdObj, bool>> IsMatchChanged;

        /// <summary>
        /// 最下部までスクロール要求時イベント
        /// </summary>
        public event EventHandler ScrollToButtomRequested;
        /// <summary>
        /// 帳簿項目追加要求時イベント
        /// </summary>
        public event EventHandler<AddActionRequestEventArgs> AddActionRequested;
        /// <summary>
        /// 帳簿項目リスト追加要求時イベント
        /// </summary>
        public event EventHandler<AddActionListRequestEventArgs> AddActionListRequested;
        /// <summary>
        /// 帳簿項目編集要求時イベント
        /// </summary>
        public event EventHandler<EditActionRequestEventArgs> EditActionRequested;
        /// <summary>
        /// 帳簿項目リスト編集要求時イベント
        /// </summary>
        public event EventHandler<EditActionListRequestEventArgs> EditActionListRequested;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// CSVファイルパス
        /// </summary>
        public string CsvFilePathes => 0 < this.CsvFilePathList.Count ? string.Join(",", this.CsvFilePathList) : null;
        /// <summary>
        /// CSVファイルパスリスト
        /// </summary>
        public ObservableCollection<string> CsvFilePathList {
            get;
            set => this.SetProperty(ref field, value);
        } = [];

        /// <summary>
        /// 帳簿セレクタVM
        /// </summary>
        public SelectorViewModel<BookModel, BookIdObj> BookSelectorVM => field ??= new(static vm => vm?.Id, this.mBusyService);
        /// <summary>
        /// 選択された帳簿ID
        /// </summary>
        public BookIdObj SelectedBookId {
            get => this.BookSelectorVM.SelectedKey;
            set {
                BookIdObj oldValue = this.BookSelectorVM.SelectedKey;
                // 現在の選択と異なる場合
                if (oldValue != value) {
                    this.BookSelectorVM.SelectedKey = value;
                    // 当てはまる帳簿がなく現在の選択と同じになった場合に変更イベントを発行する
                    if (oldValue == this.BookSelectorVM.SelectedKey) {
                        this.BookChanged?.Invoke(this, new ChangedEventArgs<BookIdObj>() { OldValue = oldValue, NewValue = this.BookSelectorVM.SelectedKey });
                    }
                }
            }
        }

        /// <summary>
        /// CSV比較セレクタVM
        /// </summary>
        public SelectorViewModel<CsvComparisonViewModel> CsvCompSelectorVM => field ??= new(this.mBusyService);
        /// <summary>
        /// CSV比較VMのチェック数
        /// </summary>
        public int AllCheckedCount => this.CsvCompSelectorVM.ItemList.Count(static vm => vm.IsMatch);
        /// <summary>
        /// CSV比較VMの合計値
        /// </summary>
        public int AllSumValue => this.CsvCompSelectorVM.ItemList.Sum(static vm => vm.Record.Value);

        /// <summary>
        /// 選択されたCSV比較VMのチェック数
        /// </summary>
        public int SelectedCheckedCount => this.CsvCompSelectorVM.SelectedItemList.Count(static vm => vm.IsMatch);
        /// <summary>
        /// 選択されたCSV比較VMの合計値
        /// </summary>
        public int SelectedSumValue => this.CsvCompSelectorVM.SelectedItemList.Sum(static vm => vm.Record.Value);

        /// <summary>
        /// チェック数変更を通知する
        /// </summary>
        private void RaiseCheckedCountChanged(EventArgs<ActionIdObj, bool> e)
        {
            this.RaisePropertyChanged(nameof(this.AllCheckedCount));
            this.RaisePropertyChanged(nameof(this.SelectedCheckedCount));
        }
        #endregion

        #region コマンド
        /// <summary>
        /// CSVファイルオープンコマンド
        /// </summary>
        public ICommand OpenCsvFilesCommand => field ??= new AsyncRelayCommand(this.OpenCsvFilesCommand_ExecuteAsync, null, this.mBusyService);
        /// <summary>
        /// CSVファイルオープンコマンド処理
        /// </summary>
        private async Task OpenCsvFilesCommand_ExecuteAsync()
        {
            (string folderPath, string fileName) = PathUtil.GetSeparatedPath(UserSettingService.Instance.CsvCompFile, App.GetCurrentDir());

            OpenFileDialogRequestEventArgs e = new() {
                CheckFileExists = true,
                InitialDirectory = folderPath,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_CsvFile + "|*.csv",
                Multiselect = true
            };
            if (this.OpenFilesDialogRequest(e)) {
                // 開いたCSVファイルのパスを設定として保存する(複数存在する場合は先頭のみ)
                UserSettingService.Instance.CsvCompFile = e.FileNames.First();

                foreach (string tmpFileName in e.FileNames) {
                    if (!this.CsvFilePathList.Contains(tmpFileName)) {
                        this.CsvFilePathList.Add(tmpFileName);
                    }
                }
                await this.LoadCsvFilesAsync(e.FileNames);
                await this.UpdateComparisonVMListAsync(true);
            }
        }

        /// <summary>
        /// CSVファイル移動コマンド
        /// </summary>
        public ICommand MoveCsvFilesCommand => field ??= new AsyncRelayCommand(this.MoveCsvFilesCommand_ExecuteAsync, this.MoveCsvFilesCommand_CanExecute, this.mBusyService);
        /// <summary>
        /// CSVファイル移動コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool MoveCsvFilesCommand_CanExecute()
        {
            // CSVフォルダパスの指定がある
            bool canExecute = this.BookSelectorVM.SelectedItem?.CsvFolderPath != null;

            if (canExecute) {
                // いずれかのファイルが存在してかつ移動済ではない
                bool canExecuteLocal = false;
                string dstFolderPath = this.BookSelectorVM.SelectedItem.CsvFolderPath;
                foreach (string srcFilePath in this.CsvFilePathList) {
                    string dstFilePath = Path.Combine(dstFolderPath, Path.GetFileName(srcFilePath));
                    canExecuteLocal |= File.Exists(srcFilePath) & srcFilePath.CompareTo(dstFilePath) != 0;
                }
                canExecute &= canExecuteLocal;
            }
            return canExecute;
        }
        /// <summary>
        /// CSVファイル移動コマンド処理
        /// </summary>
        private async Task MoveCsvFilesCommand_ExecuteAsync()
        {
            // ファイルの移動を試みる
            List<string> tmpCsvFilePathList = [];
            string dstFolderPath = this.BookSelectorVM.SelectedItem.CsvFolderPath;
            foreach (string srcFilePath in this.CsvFilePathList) {
                if (!File.Exists(srcFilePath)) { continue; }

                // 移動元と移動先が一致するなら移動しない
                string dstFilePath = Path.Combine(dstFolderPath, Path.GetFileName(srcFilePath));
                if (srcFilePath.CompareTo(dstFilePath) == 0) {
                    tmpCsvFilePathList.Add(srcFilePath);
                    continue;
                }

                // ファイルを移動する(既に存在すれば上書き)
                try {
                    if (File.Exists(dstFilePath)) {
                        File.Delete(dstFilePath);
                    }

                    File.Move(srcFilePath, dstFilePath);
                    tmpCsvFilePathList.Add(dstFilePath);
                }
                catch (Exception exp) {
                    _ = MessageBox.Show($"{Properties.Resources.Message_FoultToMoveCsv}({exp.Message})", Properties.Resources.Title_Conformation);
                }
            }

            // 移動に成功したファイルだけ記録する
            this.CsvFilePathList.Clear();
            foreach (string tmpCsvFilePath in tmpCsvFilePathList) {
                this.CsvFilePathList.Add(tmpCsvFilePath);
            }

            // CSVファイルを再読み込みする
            await this.ReloadCsvFilesAsync();
            await this.UpdateComparisonVMListAsync();
        }

        /// <summary>
        /// CSVファイルクローズコマンド
        /// </summary>
        public ICommand CloseCsvFilesCommand => field ??= new RelayCommand(this.CloseCsvFilesCommand_Execute, this.CloseCsvFilesCommand_CanExecute);
        /// <summary>
        /// CSVファイルクローズコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool CloseCsvFilesCommand_CanExecute() => 0 < this.CsvFilePathList.Count;
        /// <summary>
        /// CSVファイルクローズコマンド処理
        /// </summary>
        private void CloseCsvFilesCommand_Execute()
        {
            // リストをクリアする
            this.CsvCompSelectorVM.ItemList.Clear();
            // CSVファイルリストをクリアする
            this.CsvFilePathList.Clear();
        }

        /// <summary>
        /// 帳簿項目追加コマンド
        /// </summary>
        public ICommand AddActionCommand => field ??= new RelayCommand(this.AddActionCommand_Execute, this.AddActionCommand_CanExecute);
        /// <summary>
        /// 帳簿項目追加コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>選択されているCSVデータが1つ以上存在していて、帳簿に紐づかない項目が1つ以上ある</remarks>
        private bool AddActionCommand_CanExecute() => this.CsvCompSelectorVM.SelectedCount != 0 && this.CsvCompSelectorVM.SelectedItemList.Any(vm => vm.Action is null);
        /// <summary>
        /// 帳簿項目追加コマンド処理
        /// </summary>
        private void AddActionCommand_Execute()
        {
            List<CsvComparisonViewModel> vmList = [.. this.CsvCompSelectorVM.SelectedItemList.Where(vm => vm.Action is null)];
            List<ActionCsvDto> recordList = [.. vmList.Select(vm => vm.Record)];

            async void Registered(object sender, EventArgs<IEnumerable<ActionIdObj>> e)
            {
                // CSVの項目をベースに追加したので既定で一致フラグを立てる
                foreach (ActionIdObj actionId in e.Value) {
                    await this.SaveIsMatchAsync(actionId, true);
                }

                // 表示を更新する
                this.ActionChanged?.Invoke(this, e);
                await this.UpdateComparisonVMListAsync();
            }

            if (recordList.Count == 1) {
                ActionCsvDto record = recordList[0];
                this.AddActionRequested?.Invoke(this, new AddActionRequestEventArgs() {
                    DbHandlerFactory = this.mDbHandlerFactory,
                    InitialBookId = this.BookSelectorVM.SelectedKey,
                    InitialRecord = record,
                    Registered = Registered
                });
            }
            else {
                this.AddActionListRequested?.Invoke(this, new AddActionListRequestEventArgs() {
                    DbHandlerFactory = this.mDbHandlerFactory,
                    InitialBookId = this.BookSelectorVM.SelectedKey,
                    InitialRecordList = recordList,
                    Registered = Registered
                });
            }
        }

        /// <summary>
        /// 帳簿項目編集コマンド
        /// </summary>
        public ICommand EditActionCommand => field ??= new AsyncRelayCommand(this.EditActionCommand_ExecuteAsync, this.EditActionCommand_CanExecute, this.mBusyService);
        /// <summary>
        /// 帳簿項目編集コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>選択されているCSVデータが1つだけ存在していて、帳簿項目に紐づいている</remarks>
        private bool EditActionCommand_CanExecute() => this.CsvCompSelectorVM.SelectedCount == 1 && this.CsvCompSelectorVM.SelectedItem.Action is not null;
        /// <summary>
        /// 帳簿項目編集コマンド処理
        /// </summary>
        private async Task EditActionCommand_ExecuteAsync()
        {
            // グループ種別を特定する
            AppCommonService service = new(this.mDbHandlerFactory);
            GroupKind kind = await service.LoadGroupKind(this.CsvCompSelectorVM.SelectedItem.Action.ActionId);

            async void Registered(object sender, EventArgs<IEnumerable<ActionIdObj>> e)
            {
                // 表示を更新する
                this.ActionChanged?.Invoke(this, e);
                await this.UpdateComparisonVMListAsync();
            }

            switch (kind) {
                case GroupKind.Move:
                    Debug.Assert(true);
                    break;
                case GroupKind.ListReg:
                    this.EditActionListRequested?.Invoke(this, new EditActionListRequestEventArgs() {
                        DbHandlerFactory = this.mDbHandlerFactory,
                        TargetGroupId = this.CsvCompSelectorVM.SelectedItem.Action.GroupId,
                        Registered = Registered
                    });
                    break;
                case GroupKind.Repeat:
                case GroupKind.NotInOne:
                default:
                    this.EditActionRequested?.Invoke(this, new EditActionRequestEventArgs() {
                        DbHandlerFactory = this.mDbHandlerFactory,
                        TargetActionId = this.CsvCompSelectorVM.SelectedItem.Action.ActionId,
                        Registered = Registered
                    });
                    break;
            }
        }

        /// <summary>
        /// 帳簿項目追加/編集コマンド
        /// </summary>
        public ICommand AddOrEditActionCommand => field ??= new AsyncRelayCommand(this.AddOrEditActionCommand_ExecuteAsync, this.AddOrEditActionCommand_CanExecute, this.mBusyService);
        /// <summary>
        /// 帳簿項目追加/編集コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool AddOrEditActionCommand_CanExecute() => this.AddActionCommand_CanExecute() | this.EditActionCommand_CanExecute();
        /// <summary>
        /// 帳簿項目追加/編集コマンド処理
        /// </summary>
        private async Task AddOrEditActionCommand_ExecuteAsync()
        {
            // 選択されているCSVデータが1つ以上存在していて、帳簿項目に紐づかない項目が1つ以上ある
            if (0 < this.CsvCompSelectorVM.SelectedCount && this.CsvCompSelectorVM.SelectedItemList.Any(vm => vm.Action is null)) {
                this.AddActionCommand_Execute();
            }
            else {
                await this.EditActionCommand_ExecuteAsync();
            }
        }

        /// <summary>
        /// 一括チェックコマンド
        /// </summary>
        public ICommand BulkCheckCommand => field ??= new AsyncRelayCommand(this.BulkCheckCommand_Execute, this.BulkCheckCommand_CanExecute, this.mBusyService);
        /// <summary>
        /// 一括チェックコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool BulkCheckCommand_CanExecute()
        {
            // 対応する帳簿項目が存在し、未チェックである
            bool canExecute = false;
            foreach (CsvComparisonViewModel vm in this.CsvCompSelectorVM.ItemList) {
                canExecute |= vm.Action is not null && !vm.IsMatch;
                if (canExecute) { break; }
            }
            return canExecute;
        }
        /// <summary>
        /// 一括チェックコマンド処理
        /// </summary>
        private async Task BulkCheckCommand_Execute()
        {
            foreach (CsvComparisonViewModel vm in this.CsvCompSelectorVM.ItemList) {
                if (vm.Action is not null && !vm.IsMatch) {
                    vm.IsMatch = true;
                    this.IsMatchChanged?.Invoke(this, new EventArgs<ActionIdObj, bool>(vm.Action.ActionId, vm.IsMatch));
                    await this.SaveIsMatchAsync(vm.Action.ActionId, vm.IsMatch);
                }
            }
        }

        /// <summary>
        /// リスト更新コマンド
        /// </summary>
        public ICommand UpdateListCommand => field ??= new AsyncRelayCommand(
            async () => await this.UpdateComparisonVMListAsync(),
            () => 0 < this.CsvFilePathList.Count && this.BookSelectorVM.SelectedItem != null && 0 < this.CsvCompSelectorVM.ItemList.Count, this.mBusyService);

        /// <summary>
        /// 一致チェック変更コマンド
        /// </summary>
        public ICommand ChangeIsMatchCommand => field ??= new AsyncRelayCommand(this.ChangeIsMatchCommand_Execute, null, this.mBusyService);
        /// <summary>
        /// 一致チェック変更コマンド処理
        /// </summary>
        private async Task ChangeIsMatchCommand_Execute()
        {
            if (this.CsvCompSelectorVM.SelectedItem.Action is not null) {
                this.IsMatchChanged?.Invoke(this, new EventArgs<ActionIdObj, bool>(this.CsvCompSelectorVM.SelectedItem.Action.ActionId, this.CsvCompSelectorVM.SelectedItem.IsMatch));
                await this.SaveIsMatchAsync(this.CsvCompSelectorVM.SelectedItem.Action.ActionId, this.CsvCompSelectorVM.SelectedItem.IsMatch);
            }
        }
        #endregion

        #region ウィンドウ設定プロパティ
        protected override (double, double) WindowSizeSettingRaw {
            get => UserSettingService.Instance.CsvComparisonWindowSize;
            set => UserSettingService.Instance.CsvComparisonWindowSize = value;
        }

        public override Point WindowPointSetting {
            get => UserSettingService.Instance.CsvComparisonWindowPoint;
            set => UserSettingService.Instance.CsvComparisonWindowPoint = value;
        }
        #endregion

        public override void Initialize(DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(dbHandlerFactory);

            this.mService = new(this.mDbHandlerFactory);

            this.BookSelectorVM.SetLoader(async () => await this.mService.UpdateBookCompListAsync());
        }

        public override async Task LoadAsync() => await this.LoadAsync(null);

        /// <summary>
        /// DBから読み込む
        /// </summary>
        /// <param name="initialBookId">初期選択する帳簿のID</param>
        /// <returns></returns>
        public async Task LoadAsync(BookIdObj initialBookId)
        {
            using FuncLog funcLog = new(new { initialBookId });
            using IDisposable disposable = this.mBusyService.Enter();

            await this.BookSelectorVM.LoadAsync(initialBookId);
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            this.CsvFilePathList.CollectionChanged += (sender, e) => {
                using FuncLog funcLog = new(new { e.OldItems, e.NewItems }, methodName: nameof(this.CsvFilePathList.CollectionChanged));

                this.RaisePropertyChanged(nameof(this.CsvFilePathes));
            };
            this.BookSelectorVM.SelectionChanged += async (sender, e) => {
                this.BookChanged?.Invoke(sender, e);

                await this.ReloadCsvFilesAsync();
                await this.UpdateComparisonVMListAsync(true);
            };
            this.CsvCompSelectorVM.CollectionChanged += (sender, e) => {
                this.RaisePropertyChanged(nameof(this.AllCheckedCount));
                this.RaisePropertyChanged(nameof(this.AllSumValue));

                if (e.OldItems != null) {
                    foreach (object tmp in e.OldItems) {
                        if (tmp is CsvComparisonViewModel vm) {
                            vm.IsMatchChanged -= this.RaiseCheckedCountChanged;
                        }
                    }
                }
                if (e.NewItems != null) {
                    foreach (object tmp in e.NewItems) {
                        if (tmp is CsvComparisonViewModel vm) {
                            vm.IsMatchChanged += this.RaiseCheckedCountChanged;
                        }
                    }
                }
            };
            this.CsvCompSelectorVM.SelectedCollectionChanged += (sender, e) => {
                this.RaisePropertyChanged(nameof(this.SelectedCheckedCount));
                this.RaisePropertyChanged(nameof(this.SelectedSumValue));
            };
        }

        /// <summary>
        /// 指定されたCSVファイルを追加で読み込む
        /// </summary>
        public async Task LoadCsvFilesAsync(IEnumerable<string> csvFilePathList)
        {
            using FuncLog funcLog = new(new { csvFilePathList });

            if (this.BookSelectorVM.SelectedKey == null) {
                return;
            }

            // CSVファイル上の対象インデックスを取得する
            int actDateIndex = this.BookSelectorVM.SelectedItem.ActDateIndex.Value;
            int itemNameIndex = this.BookSelectorVM.SelectedItem.ItemNameIndex.Value;
            int expensesIndex = this.BookSelectorVM.SelectedItem.ExpensesIndex.Value;

            // CSVファイルを読み込む
            List<CsvComparisonViewModel> tmpVMList = [..
                await CSVFileDao.LoadCsvCompListAsync(csvFilePathList, actDateIndex, itemNameIndex, expensesIndex, Encoding.GetEncoding(this.BookSelectorVM.SelectedItem.TextEncoding))];

            // 有効な行があればリストに追加する(日付昇順)
            if (0 < tmpVMList.Count) {
                tmpVMList.Sort((tmp1, tmp2) => (int)(tmp1.Record.Date - tmp2.Record.Date).TotalDays);
                foreach (CsvComparisonViewModel vm in tmpVMList) {
                    this.CsvCompSelectorVM.ItemList.Add(vm);
                }
            }
        }

        /// <summary>
        /// CSVファイルを再読み込みする
        /// </summary>
        /// <returns></returns>
        public async Task ReloadCsvFilesAsync()
        {
            using FuncLog funcLog = new();

            // リストをクリアする
            this.CsvCompSelectorVM.ItemList.Clear();

            await this.LoadCsvFilesAsync(this.CsvFilePathList);
        }

        /// <summary>
        /// 帳簿項目と比較してCSV比較VMリストを更新する
        /// </summary>
        /// <param name="isScroll">スクロールするか</param>
        public async Task UpdateComparisonVMListAsync(bool isScroll = false)
        {
            using FuncLog funcLog = new(new { isScroll });

            // 指定された帳簿内で、日付、金額が一致する帳簿項目を探す
            foreach (CsvComparisonViewModel vm in this.CsvCompSelectorVM.ItemList) {
                // 前回の帳簿項目情報をクリアする
                vm.ClearActionInfo();

                // 一致する帳簿項目を取得する
                IEnumerable<ActionModel> actionList = await this.mService.LoadMatchedActionAsync((int)this.BookSelectorVM.SelectedKey, vm.Record.Date, vm.Record.Value);

                foreach (ActionModel action in actionList) {
                    // 帳簿項目IDが使用済なら次のレコードを調べるようにする
                    bool checkNext = this.CsvCompSelectorVM.ItemList.Where(tmpVM => (int?)tmpVM.Action?.ActionId == action.ActionId).Any();
                    // 帳簿項目情報を紐付ける
                    if (!checkNext) {
                        vm.Action = action;
                        vm.IsMatch = action.IsMatch;

                        break;
                    }
                }
            }

            if (isScroll) {
                this.ScrollToButtomRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 一致フラグを保存する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <param name="isMatch">一致フラグ</param>
        /// <returns></returns>
        public async Task SaveIsMatchAsync(ActionIdObj actionId, bool isMatch)
        {
            using FuncLog funcLog = new(new { actionId, isMatch });

            AppCommonService service = new(this.mDbHandlerFactory);
            await service.SaveIsMatchAsync(actionId, isMatch);
        }
    }
}
