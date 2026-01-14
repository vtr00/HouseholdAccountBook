using CsvHelper;
using CsvHelper.Configuration;
using HouseholdAccountBook.Adapters.Dao.Compositions;
using HouseholdAccountBook.Adapters.Dao.DbTable;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Dto.Others;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Args;
using HouseholdAccountBook.Args.RequestEventArgs;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Utilites;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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
        #region イベント
        /// <summary>
        /// 帳簿変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int?>> BookChanged;
        /// <summary>
        /// 帳簿項目変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> ActionChanged;
        /// <summary>
        /// 一致フラグ変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<int?, bool>> IsMatchChanged;

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

        #region プロパティ
        /// <summary>
        /// CSVファイルパス
        /// </summary>
        #region CsvFilePathes
        public string CsvFilePathes => 0 < this.CsvFilePathList.Count ? string.Join(",", this.CsvFilePathList) : null;
        #endregion
        /// <summary>
        /// CSVファイルパスリスト
        /// </summary>
        #region CsvFilePathList
        public ObservableCollection<string> CsvFilePathList {
            get;
            set => this.SetProperty(ref field, value);
        } = [];
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookComparisonViewModel> BookVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookComparisonViewModel SelectedBookVM {
            get;
            set {
                int? oldValue = field?.Id;
                if (this.SetProperty(ref field, value)) {
                    this.BookChanged?.Invoke(this, new ChangedEventArgs<int?>() { OldValue = oldValue, NewValue = value?.Id });
                }
            }
        } = new() { };
        #endregion
        /// <summary>
        /// 選択された帳簿ID
        /// </summary>
        #region SelectedBookId
        public int? SelectedBookId {
            get => this.SelectedBookVM?.Id;
            set {
                int? oldValue = this.SelectedBookId;
                // 現在の選択と異なる場合
                if (this.SelectedBookVM?.Id != value) {
                    // 帳簿VMリストから該当する帳簿VMを選択する
                    this.SelectedBookVM = this.BookVMList.FirstOrElementAtOrDefault(vm => vm.Id == value, 0);
                    // 当てはまる帳簿がなく現在の選択と同じになった場合に変更イベントを発行する
                    if (oldValue == this.SelectedBookId) {
                        this.BookChanged?.Invoke(this, new ChangedEventArgs<int?>() { OldValue = oldValue, NewValue = this.SelectedBookId });
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// CSV比較VMリスト
        /// </summary>
        #region CsvComparisonVMList
        public ObservableCollection<CsvComparisonViewModel> CsvComparisonVMList {
            get;
            set => this.SetProperty(ref field, value);
        } = [];
        #endregion
        /// <summary>
        /// CSV比較VMのチェック数
        /// </summary>
        #region AllCheckedNum
        public int AllCheckedCount => this.CsvComparisonVMList.Count(vm => vm.IsMatch);
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
        public int AllSumValue => this.CsvComparisonVMList.Sum(vm => vm.Record.Value);
        #endregion

        /// <summary>
        /// 選択されたCSV比較VM(先頭)
        /// </summary>
        #region SelectedCsvComparisonVM
        public CsvComparisonViewModel SelectedCsvComparisonVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択されたCSV比較VMリスト
        /// </summary>
        #region SelectedCsvComparisonVMList
        public ObservableCollection<CsvComparisonViewModel> SelectedCsvComparisonVMList { get; } = [];
        #endregion
        /// <summary>
        /// 選択されたCSV比較VMのチェック数
        /// </summary>
        #region SelectedCheckedNum
        public int SelectedCheckedCount => this.SelectedCsvComparisonVMList.Count(vm => vm.IsMatch);
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
        public int SelectedSumValue => this.SelectedCsvComparisonVMList.Sum(vm => vm.Record.Value);
        #endregion

        /// <summary>
        /// チェック数変更を通知する
        /// </summary>
        private void RaiseCheckedCountChanged(EventArgs<int?, bool> e)
        {
            this.RaisePropertyChanged(nameof(this.AllCheckedCount));
            this.RaisePropertyChanged(nameof(this.SelectedCheckedCount));
        }

        #region コマンド
        /// <summary>
        /// CSVファイルオープンコマンド
        /// </summary>
        public ICommand OpenCsvFilesCommand => new RelayCommand(this.OpenCsvFilesCommand_Executed);
        /// <summary>
        /// CSVファイル移動コマンド
        /// </summary>
        public ICommand MoveCsvFilesCommand => new RelayCommand(this.MoveCsvFilesCommand_Executed, this.MoveCsvFilesCommand_CanExecute);
        /// <summary>
        /// CSVファイルクローズコマンド
        /// </summary>
        public ICommand CloseCsvFilesCommand => new RelayCommand(this.CloseCsvFilesCommand_Executed, this.CloseCsvFilesCommand_CanExecute);
        /// <summary>
        /// 帳簿項目追加コマンド
        /// </summary>
        public ICommand AddActionCommand => new RelayCommand(this.AddActionCommand_Executed, this.AddActionCommand_CanExecute);
        /// <summary>
        /// 帳簿項目編集コマンド
        /// </summary>
        public ICommand EditActionCommand => new RelayCommand(this.EditActionCommand_Executed, this.EditActionCommand_CanExecute);
        /// <summary>
        /// 帳簿項目追加/編集コマンド
        /// </summary>
        public ICommand AddOrEditActionCommand => new RelayCommand(this.AddOrEditActionCommand_Executed, this.AddOrEditActionCommand_CanExecute);
        /// <summary>
        /// 一括チェックコマンド
        /// </summary>
        public ICommand BulkCheckCommand => new RelayCommand(this.BulkCheckCommand_Executed, this.BulkCheckCommand_CanExecute);
        /// <summary>
        /// リスト更新コマンド
        /// </summary>
        public ICommand UpdateListCommand => new RelayCommand(this.UpdateListCommand_Executed, this.UpdateListCommand_CanExecute);
        /// <summary>
        /// 一致チェック変更コマンド
        /// </summary>
        public ICommand ChangeIsMatchCommand => new RelayCommand(this.ChangeIsMatchCommand_Executed);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// CSVファイルオープンコマンド処理
        /// </summary>
        private async void OpenCsvFilesCommand_Executed()
        {
            Properties.Settings settings = Properties.Settings.Default;
            (string folderPath, string fileName) = PathExtensions.GetSeparatedPath(settings.App_CsvFilePath, App.GetCurrentDir());

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
                settings.App_CsvFilePath = e.FileNames[0];
                settings.Save();

                foreach (string tmpFileName in e.FileNames) {
                    if (!this.CsvFilePathList.Contains(tmpFileName)) {
                        this.CsvFilePathList.Add(tmpFileName);
                    }
                }
                this.LoadCsvFiles(e.FileNames);
                await this.UpdateComparisonVMListAsync(true);
            }

        }

        /// <summary>
        /// CSVファイル移動コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool MoveCsvFilesCommand_CanExecute()
        {
            // CSVフォルダパスの指定がある
            bool canExecute = this.SelectedBookVM?.CsvFolderPath != null;

            if (canExecute) {
                // いずれかのファイルが存在してかつ移動済ではない
                bool canExecuteLocal = false;
                string dstFolderPath = this.SelectedBookVM.CsvFolderPath;
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
        private async void MoveCsvFilesCommand_Executed()
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                // ファイルの移動を試みる
                List<string> tmpCsvFilePathList = [];
                string dstFolderPath = this.SelectedBookVM.CsvFolderPath;
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
                this.ReloadCsvFiles();
                await this.UpdateComparisonVMListAsync();
            }
        }

        /// <summary>
        /// CSVファイルクローズコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool CloseCsvFilesCommand_CanExecute() => 0 < this.CsvFilePathList.Count;
        /// <summary>
        /// CSVファイルクローズコマンド処理
        /// </summary>
        private void CloseCsvFilesCommand_Executed()
        {
            // リストをクリアする
            this.CsvComparisonVMList.Clear();
            // CSVファイルリストをクリアする
            this.CsvFilePathList.Clear();
        }

        /// <summary>
        /// 帳簿項目追加コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>選択されている帳簿項目が1つ以上存在していて、値を持たない帳簿項目IDが1つ以上ある</remarks>
        private bool AddActionCommand_CanExecute() => this.SelectedCsvComparisonVMList.Count != 0 && this.SelectedCsvComparisonVMList.Any(vm => !vm.ActionId.HasValue);
        /// <summary>
        /// 帳簿項目追加コマンド処理
        /// </summary>
        private void AddActionCommand_Executed()
        {
            List<CsvComparisonViewModel> vmList = [.. this.SelectedCsvComparisonVMList.Where(vm => !vm.ActionId.HasValue)];
            List<CsvViewModel> recordList = [.. vmList.Select(vm => vm.Record)];

            async void Registered(object sender, EventArgs<List<int>> e)
            {
                // CSVの項目をベースに追加したので既定で一致フラグを立てる
                foreach (int actionId in e.Value) {
                    await this.SaveIsMatchAsync(actionId, true);
                }

                // 表示を更新する
                this.ActionChanged?.Invoke(this, e);
                await this.UpdateComparisonVMListAsync();
            }

            if (recordList.Count == 1) {
                CsvViewModel record = recordList[0];
                this.AddActionRequested?.Invoke(this, new AddActionRequestEventArgs() {
                    DbHandlerFactory = this.mDbHandlerFactory,
                    InitialBookId = this.SelectedBookVM.Id.Value,
                    InitialRecord = record,
                    Registered = Registered
                });
            }
            else {
                this.AddActionListRequested?.Invoke(this, new AddActionListRequestEventArgs() {
                    DbHandlerFactory = this.mDbHandlerFactory,
                    InitialBookId = this.SelectedBookVM.Id.Value,
                    InitialRecordList = recordList,
                    Registered = Registered
                });
            }
        }

        /// <summary>
        /// 帳簿項目編集コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>選択されている帳簿項目が1つだけ存在していて、選択している帳簿項目のIDが値を持つ</remarks>
        private bool EditActionCommand_CanExecute() => this.SelectedCsvComparisonVMList.Count == 1 && this.SelectedCsvComparisonVM.ActionId.HasValue;
        /// <summary>
        /// 帳簿項目編集コマンド処理
        /// </summary>
        private async void EditActionCommand_Executed()
        {
            // グループ種別を特定する
            int? groupKind = null;
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                GroupInfoDao groupInfoDao = new(dbHandler);
                var dto = await groupInfoDao.FindByActionId(this.SelectedCsvComparisonVM.ActionId.Value);
                groupKind = dto.GroupKind;
            }

            async void Registered(object sender, EventArgs<List<int>> e)
            {
                // 表示を更新する
                this.ActionChanged?.Invoke(this, e);
                await this.UpdateComparisonVMListAsync();
            }

            switch (groupKind) {
                case (int)GroupKind.Move:
                    Debug.Assert(true);
                    break;
                case (int)GroupKind.ListReg:
                    this.EditActionListRequested?.Invoke(this, new EditActionListRequestEventArgs() {
                        DbHandlerFactory = this.mDbHandlerFactory,
                        TargetGroupId = this.SelectedCsvComparisonVM.GroupId.Value,
                        Registered = Registered
                    });
                    break;
                case (int)GroupKind.Repeat:
                default:
                    this.EditActionRequested?.Invoke(this, new EditActionRequestEventArgs() {
                        DbHandlerFactory = this.mDbHandlerFactory,
                        TargetActionId = this.SelectedCsvComparisonVM.ActionId.Value,
                        Registered = Registered
                    });
                    break;
            }

        }

        /// <summary>
        /// 帳簿項目追加/編集コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool AddOrEditActionCommand_CanExecute() => this.AddActionCommand_CanExecute() | this.EditActionCommand_CanExecute();
        /// <summary>
        /// 帳簿項目追加/編集コマンド処理
        /// </summary>
        private void AddOrEditActionCommand_Executed()
        {
            // 選択されている帳簿項目が1つ以上存在していて、値を持たない帳簿項目IDが1つ以上ある
            if (0 < this.SelectedCsvComparisonVMList.Count && this.SelectedCsvComparisonVMList.Any(vm => !vm.ActionId.HasValue)) {
                this.AddActionCommand_Executed();
            }
            else {
                this.EditActionCommand_Executed();
            }
        }

        /// <summary>
        /// 一括チェックコマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool BulkCheckCommand_CanExecute()
        {
            // 対応する帳簿項目が存在し、未チェックである
            bool canExecute = false;
            if (this.CsvComparisonVMList != null) {
                foreach (CsvComparisonViewModel vm in this.CsvComparisonVMList) {
                    canExecute |= vm.ActionId.HasValue && !vm.IsMatch;
                    if (canExecute) { break; }
                }
            }
            return canExecute;
        }
        /// <summary>
        /// 一括チェックコマンド処理
        /// </summary>
        private async void BulkCheckCommand_Executed()
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                foreach (CsvComparisonViewModel vm in this.CsvComparisonVMList) {
                    if (vm.ActionId.HasValue && !vm.IsMatch) {
                        vm.IsMatch = true;
                        this.IsMatchChanged?.Invoke(this, new EventArgs<int?, bool>(vm.ActionId, vm.IsMatch));
                        await this.SaveIsMatchAsync(vm.ActionId.Value, vm.IsMatch);
                    }
                }
            }
        }

        /// <summary>
        /// 更新コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool UpdateListCommand_CanExecute() => 0 < this.CsvFilePathList.Count && this.SelectedBookVM != null && 0 < this.CsvComparisonVMList.Count;
        /// <summary>
        /// 更新コマンド処理
        /// </summary>
        private async void UpdateListCommand_Executed()
        {
            using (WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create()) {
                await this.UpdateComparisonVMListAsync();
            }
        }

        /// <summary>
        /// 一致チェック変更コマンド処理
        /// </summary>
        private async void ChangeIsMatchCommand_Executed()
        {
            if (this.SelectedCsvComparisonVM.ActionId.HasValue) {
                this.IsMatchChanged?.Invoke(this, new EventArgs<int?, bool>(this.SelectedCsvComparisonVM.ActionId, this.SelectedCsvComparisonVM.IsMatch));
                await this.SaveIsMatchAsync(this.SelectedCsvComparisonVM.ActionId.Value, this.SelectedCsvComparisonVM.IsMatch);
            }
        }
        #endregion

        #region ウィンドウ設定プロパティ
        protected override (double, double) WindowSizeSettingRaw {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return (settings.CsvComparisonWindow_Width, settings.CsvComparisonWindow_Height);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.CsvComparisonWindow_Width = value.Item1;
                settings.CsvComparisonWindow_Height = value.Item2;
                settings.Save();
            }
        }

        public override Point WindowPointSetting {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return new Point(settings.CsvComparisonWindow_Left, settings.CsvComparisonWindow_Top);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.CsvComparisonWindow_Left = value.X;
                settings.CsvComparisonWindow_Top = value.Y;
                settings.Save();
            }
        }
        #endregion

        public override async Task LoadAsync() => await this.LoadAsync(null);

        /// <summary>
        /// DBから読み込む
        /// </summary>
        /// <param name="initialBookId">初期選択する帳簿のID</param>
        /// <returns></returns>
        public async Task LoadAsync(int? initialBookId)
        {
            using FuncLog funcLog = new(new { initialBookId });

            await this.UpdateBookCompListAsync(initialBookId);
        }

        /// <summary>
        /// 帳簿VM(比較用)を更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        public async Task UpdateBookCompListAsync(int? bookId = null)
        {
            using FuncLog funcLog = new(new { bookId });

            ViewModelLoader loader = new(this.mDbHandlerFactory);
            int? tmpBookId = bookId ?? this.SelectedBookVM?.Id;
            this.BookVMList = await loader.UpdateBookCompListAsync();
            this.SelectedBookVM = this.BookVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpBookId, 0);
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            this.CsvFilePathList.CollectionChanged += (sender, e) => {
                using FuncLog funcLog = new(new { e.OldItems, e.NewItems }, methodName: nameof(this.CsvFilePathList.CollectionChanged));

                this.RaisePropertyChanged(nameof(this.CsvFilePathes));
            };
            this.CsvComparisonVMList.CollectionChanged += (sender, e) => {
                using FuncLog funcLog = new(new { e.OldItems, e.NewItems }, methodName: nameof(this.CsvComparisonVMList.CollectionChanged));

                this.RaisePropertyChanged(nameof(this.AllCheckedCount));
                this.RaisePropertyChanged(nameof(this.AllCount));
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
            this.SelectedCsvComparisonVMList.CollectionChanged += (sender, e) => {
                using FuncLog funcLog = new(new { e.OldItems, e.NewItems }, methodName: nameof(this.SelectedCsvComparisonVMList.CollectionChanged));

                this.RaisePropertyChanged(nameof(this.SelectedCheckedCount));
                this.RaisePropertyChanged(nameof(this.SelectedCount));
                this.RaisePropertyChanged(nameof(this.SelectedSumValue));
            };

            this.BookChanged += async (sender, e) => {
                using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, methodName: nameof(this.BookChanged));

                this.ReloadCsvFiles();
                await this.UpdateComparisonVMListAsync(true);
            };
        }

        /// <summary>
        /// 指定されたCSVファイルを追加で読み込む
        /// </summary>
        public void LoadCsvFiles(IList<string> csvFilePathList)
        {
            using FuncLog funcLog = new(new { csvFilePathList });

            if (this.SelectedBookVM == null) {
                return;
            }

            // CSVファイル上の対象インデックスを取得する
            int actDateIndex = this.SelectedBookVM.ActDateIndex.Value;
            int itemNameIndex = this.SelectedBookVM.ItemNameIndex.Value;
            int expensesIndex = this.SelectedBookVM.ExpensesIndex.Value;

            // CSVファイルを読み込む
            CsvConfiguration csvConfig = new(CultureInfo.CurrentCulture) {
                HasHeaderRecord = true,
                MissingFieldFound = mffa => { }
            };
            List<CsvComparisonViewModel> tmpVMList = [];
            foreach (string tmpFileName in csvFilePathList) {
                using (CsvReader reader = new(new StreamReader(tmpFileName, Encoding.GetEncoding(this.SelectedBookVM.TextEncoding)), csvConfig)) {
                    List<CsvComparisonViewModel> tmpVMList2 = [];
                    while (reader.Read()) {
                        try {
                            if (!reader.TryGetField(actDateIndex - 1, out DateTime date)) {
                                continue;
                            }
                            if (!reader.TryGetField(itemNameIndex - 1, out string name)) {
                                // 項目名は読込みに失敗してもOK
                                name = null;
                            }
                            if (!reader.TryGetField(expensesIndex - 1, out string valueStr) ||
                                !int.TryParse(valueStr, NumberStyles.Any, NumberFormatInfo.CurrentInfo, out int value)) {
                                continue;
                            }

                            tmpVMList2.Add(new() { Record = new() { Date = date, Name = name, Value = value } });
                        }
                        catch (Exception) { }
                    }

                    // 有効な行があれば追加する
                    if (0 < tmpVMList2.Count) {
                        tmpVMList.AddRange(tmpVMList2);
                    }
                }
            }

            // 有効な行があればリストに追加する(日付昇順)
            if (0 < tmpVMList.Count) {
                tmpVMList.Sort((tmp1, tmp2) => (int)(tmp1.Record.Date - tmp2.Record.Date).TotalDays);
                foreach (CsvComparisonViewModel vm in tmpVMList) {
                    this.CsvComparisonVMList.Add(vm);
                }
            }
        }

        /// <summary>
        /// CSVファイルを再読み込みする
        /// </summary>
        /// <returns></returns>
        public void ReloadCsvFiles()
        {
            using FuncLog funcLog = new();

            // リストをクリアする
            this.CsvComparisonVMList.Clear();

            this.LoadCsvFiles(this.CsvFilePathList);
        }

        /// <summary>
        /// 帳簿項目と比較してCSV比較VMリストを更新する
        /// </summary>
        /// <param name="isScroll">スクロールするか</param>
        public async Task UpdateComparisonVMListAsync(bool isScroll = false)
        {
            using FuncLog funcLog = new(new { isScroll });

            // 指定された帳簿内で、日付、金額が一致する帳簿項目を探す
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                foreach (var vm in this.CsvComparisonVMList) {
                    // 前回の帳簿項目情報をクリアする
                    vm.ClearActionInfo();

                    ActionCompInfoDao actionCompInfoDao = new(dbHandler);
                    var dtoList = await actionCompInfoDao.FindMatchesWithCsvAsync(this.SelectedBookVM.Id.Value, vm.Record.Date, vm.Record.Value);
                    foreach (ActionCompInfoDto dto in dtoList) {
                        // 帳簿項目IDが使用済なら次のレコードを調べるようにする
                        bool checkNext = this.CsvComparisonVMList.Where(tmpVM => tmpVM.ActionId == dto.ActionId).Any();
                        // 帳簿項目情報を紐付ける
                        if (!checkNext) {
                            vm.ActionId = dto.ActionId;
                            vm.ItemName = dto.ItemName;
                            vm.ShopName = dto.ShopName;
                            vm.Remark = dto.Remark;
                            vm.IsMatch = dto.IsMatch == 1;
                            vm.GroupId = dto.GroupId;

                            break;
                        }
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
        public async Task SaveIsMatchAsync(int actionId, bool isMatch)
        {
            using FuncLog funcLog = new(new { actionId, isMatch });

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                HstActionDao hstActionDao = new(dbHandler);
                _ = await hstActionDao.UpdateIsMatchByIdAsync(actionId, isMatch ? 1 : 0);
            }
        }
    }
}
