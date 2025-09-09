using CsvHelper;
using CsvHelper.Configuration;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Models.Dao.Compositions;
using HouseholdAccountBook.Models.Dao.DbTable;
using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.Models.DbHandler.Abstract;
using HouseholdAccountBook.Models.Dto.DbTable;
using HouseholdAccountBook.Models.Dto.Others;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.ViewModels.Component;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static HouseholdAccountBook.Extensions.FrameworkElementExtensions;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// CsvComparisonWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CsvComparisonWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory dbHandlerFactory;
        /// <summary>
        /// 選択された帳簿ID
        /// </summary>
        private readonly int? selectedBookId;
        #endregion

        #region イベント
        /// <summary>
        /// 一致フラグ変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<int?, bool>> IsMatchChanged;
        /// <summary>
        /// 帳簿項目変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> ActionChanged;
        /// <summary>
        /// 帳簿変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<int?>> BookChanged;
        /// <summary>
        /// ウィンドウ非表示時のイベント
        /// </summary>
        public event EventHandler Hided;
        #endregion

        /// <summary>
        /// <see cref="CsvComparisonWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        public CsvComparisonWindow(DbHandlerFactory dbHandlerFactory, int? selectedBookId)
        {
            this.dbHandlerFactory = dbHandlerFactory;
            this.selectedBookId = selectedBookId;

            this.InitializeComponent();
        }

        #region イベントハンドラ
        #region コマンド
        #region ファイル
        /// <summary>
        /// CSVファイルオープン可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenCsvFilesCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// CSVファイルを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OpenCsvFilesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
            (string folderPath, string fileName) = PathExtensions.GetSeparatedPath(settings.App_CsvFilePath, App.GetCurrentDir());

            OpenFileDialog ofd = new() {
                CheckFileExists = true,
                InitialDirectory = folderPath,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_CsvFile + "|*.csv",
                Multiselect = true
            };

            if (ofd.ShowDialog(this) == true) {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    // 開いたCSVファイルのパスを設定として保存する(複数存在する場合は先頭のみ)
                    settings.App_CsvFilePath = ofd.FileName;
                    settings.Save();

                    foreach (string tmpFileName in ofd.FileNames) {
                        if (!this.WVM.CsvFilePathList.Contains(tmpFileName)) {
                            this.WVM.CsvFilePathList.Add(tmpFileName);
                        }
                    }

                    // CSVファイルを再読み込みする
                    this.ReloadCsvFiles();
                    await this.UpdateComparisonVMListAsync(true);
                }
            }
        }

        /// <summary>
        /// CSVファイル移動可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveCsvFilesCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // CSVフォルダパスの指定がある
            e.CanExecute = this.WVM.SelectedBookVM.CsvFolderPath != null;

            if (e.CanExecute) {
                // いずれかのファイルが存在してかつ移動済ではない
                bool canExecute = false;
                string dstFolderPath = this.WVM.SelectedBookVM.CsvFolderPath;
                foreach (string srcFilePath in this.WVM.CsvFilePathList) {
                    string dstFilePath = Path.Combine(dstFolderPath, Path.GetFileName(srcFilePath));
                    canExecute |= File.Exists(srcFilePath) & srcFilePath.CompareTo(dstFilePath) != 0;
                }
                e.CanExecute &= canExecute;
            }
        }

        /// <summary>
        /// CSVファイルを移動する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MoveCsvFilesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                // ファイルの移動を試みる
                List<string> tmpCsvFilePathList = [];
                string dstFolderPath = this.WVM.SelectedBookVM.CsvFolderPath;
                foreach (string srcFilePath in this.WVM.CsvFilePathList) {
                    if (!File.Exists(srcFilePath)) continue;

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
                this.WVM.CsvFilePathList.Clear();
                foreach (string tmpCsvFilePath in tmpCsvFilePathList) {
                    this.WVM.CsvFilePathList.Add(tmpCsvFilePath);
                }

                // CSVファイルを再読み込みする
                this.ReloadCsvFiles();
                await this.UpdateComparisonVMListAsync();
            }
        }

        /// <summary>
        /// CSVファイルをクローズ可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseCsvFilesCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = 0 < this.WVM.CsvFilePathList.Count;
        }

        /// <summary>
        /// CSVファイルを閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseCsvFilesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.CloseCsvFiles();
        }

        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Hide();
            this.Hided?.Invoke(sender, e);

            this.SaveWindowSetting();
        }
        #endregion

        #region 編集
        /// <summary>
        /// 項目追加可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 選択されている帳簿項目が1つ以上存在していて、値を持たない帳簿項目IDが1つ以上ある
            e.CanExecute = this.WVM.SelectedCsvComparisonVMList.Count != 0 && this.WVM.SelectedCsvComparisonVMList.Any((vm) => !vm.ActionId.HasValue);
        }

        /// <summary>
        /// 項目追加ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            List<CsvComparisonViewModel> vmList = new(this.WVM.SelectedCsvComparisonVMList.Where((vm) => !vm.ActionId.HasValue));
            List<CsvViewModel> recordList = new(vmList.Select((vm) => vm.Record));
            async void func(object sender2, EventArgs<List<int>> e2)
            {
                // CSVの項目をベースに追加したので既定で一致フラグを立てる
                foreach (int actionId in e2.Value) {
                    await this.SaveIsMatchAsync(actionId, true);
                }

                // 表示を更新する
                this.ActionChanged?.Invoke(this, e2);
                await this.UpdateComparisonVMListAsync();
            }

            if (recordList.Count == 1) {
                CsvViewModel record = recordList[0];
                ActionRegistrationWindow arw = new(this.dbHandlerFactory, this.WVM.SelectedBookVM.Id.Value, record) { Owner = this };
                arw.LoadWindowSetting();

                arw.Registrated += func;
                _ = arw.ShowDialog();
            }
            else {
                ActionListRegistrationWindow alrw = new(this.dbHandlerFactory, this.WVM.SelectedBookVM.Id.Value, recordList) { Owner = this };
                alrw.LoadWindowSetting();

                alrw.Registrated += func;
                _ = alrw.ShowDialog();
            }
        }

        /// <summary>
        /// 項目編集可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditActionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 選択されている帳簿項目が1つだけ存在していて、選択している帳簿項目のIDが値を持つ
            e.CanExecute = this.WVM.SelectedCsvComparisonVMList.Count == 1 && this.WVM.SelectedCsvComparisonVM.ActionId.HasValue;
        }

        /// <summary>
        /// 項目編集ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EditActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // グループ種別を特定する
            int? groupKind = null;
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                GroupInfoDao groupInfoDao = new(dbHandler);
                var dto = await groupInfoDao.FindByActionId(this.WVM.SelectedCsvComparisonVM.ActionId.Value);
                groupKind = dto.GroupKind;
            }

            switch (groupKind) {
                case (int)GroupKind.Move:
                    Debug.Assert(true);
                    break;
                case (int)GroupKind.ListReg:
                    // リスト登録された帳簿項目の編集時の処理
                    ActionListRegistrationWindow alrw = new(this.dbHandlerFactory, this.WVM.SelectedCsvComparisonVM.GroupId.Value) { Owner = this };
                    alrw.LoadWindowSetting();

                    // 登録時イベントを登録する
                    alrw.Registrated += async (sender2, e2) => {
                        // 表示を更新する
                        this.ActionChanged?.Invoke(this, e2);
                        await this.UpdateComparisonVMListAsync();
                    };
                    _ = alrw.ShowDialog();
                    break;
                case (int)GroupKind.Repeat:
                default:
                    // 移動・リスト登録以外の帳簿項目の編集時の処理
                    ActionRegistrationWindow arw = new(this.dbHandlerFactory, this.WVM.SelectedCsvComparisonVM.ActionId.Value) { Owner = this };
                    arw.LoadWindowSetting();

                    // 登録時イベントを登録する
                    arw.Registrated += async (sender2, e2) => {
                        // 表示を更新する
                        this.ActionChanged?.Invoke(this, e2);
                        await this.UpdateComparisonVMListAsync();
                    };
                    _ = arw.ShowDialog();
                    break;
            }

        }

        /// <summary>
        /// 項目追加/編集可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddOrEditActionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            this.AddActionCommand_CanExecute(sender, e);
            if (!e.CanExecute) {
                this.EditActionCommand_CanExecute(sender, e);
            }
        }

        /// <summary>
        /// 項目追加/編集ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddOrEditActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // 選択されている帳簿項目が1つ以上存在していて、値を持たない帳簿項目IDが1つ以上ある
            if (0 < this.WVM.SelectedCsvComparisonVMList.Count && this.WVM.SelectedCsvComparisonVMList.Any((vm) => !vm.ActionId.HasValue)) {
                this.AddActionCommand_Executed(sender, e);
            }
            else {
                this.EditActionCommand_Executed(sender, e);
            }
        }

        /// <summary>
        /// 一括チェック可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BulkCheckCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 対応する帳簿項目が存在し、未チェックである
            e.CanExecute = false;
            if (this.WVM.CsvComparisonVMList != null) {
                foreach (CsvComparisonViewModel vm in this.WVM.CsvComparisonVMList) {
                    e.CanExecute |= vm.ActionId.HasValue && !vm.IsMatch;
                    if (e.CanExecute) { break; }
                }
            }
        }

        /// <summary>
        /// 一括チェック処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BulkCheckCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                foreach (CsvComparisonViewModel vm in this.WVM.CsvComparisonVMList) {
                    if (vm.ActionId.HasValue && !vm.IsMatch) {
                        vm.IsMatch = true;
                        this.IsMatchChanged?.Invoke(this, new EventArgs<int?, bool>(vm.ActionId, vm.IsMatch));
                        await this.SaveIsMatchAsync(vm.ActionId.Value, vm.IsMatch);
                    }
                }
            }
        }
        #endregion

        #region 表示
        /// <summary>
        /// 比較情報を更新可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateListCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = 0 < this.WVM.CsvFilePathList.Count && this.WVM.SelectedBookVM != null && 0 < this.WVM.CsvComparisonVMList.Count;
        }

        /// <summary>
        /// 比較情報を更新する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateListCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                await this.UpdateComparisonVMListAsync();
            }
        }
        #endregion

        /// <summary>
        /// 一致チェックボックスが変更されたら保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeIsMatchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.OriginalSource is CheckBox checkBox) {
                if (checkBox.DataContext is CsvComparisonViewModel vm) {
                    this.WVM.SelectedCsvComparisonVM = vm;
                    if (vm.ActionId.HasValue) {
                        this.IsMatchChanged?.Invoke(this, new EventArgs<int?, bool>(vm.ActionId, vm.IsMatch));
                        await this.SaveIsMatchAsync(vm.ActionId.Value, vm.IsMatch);
                    }
                }
            }
        }
        #endregion

        #region ウィンドウ
        /// <summary>
        /// ウィンドウ読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CsvComparisonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await this.UpdateBookListAsync(this.selectedBookId);

            this.RegisterEventHandlerToWVM();

            var dcr = VisualTreeHelper.GetChild(this.csvCompDataGrid, 0) as Decorator;
            var sv = dcr.Child as ScrollViewer;
            sv.ScrollChanged += this.CsvCompDataGrid_ScrollChanged;
        }

        /// <summary>
        /// ウィンドウ表示状態変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CsvComparisonWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            if (!oldValue && newValue) {
                await this.UpdateComparisonVMListAsync();
            }
        }

        /// <summary>
        /// ウィンドウ終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsvComparisonWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            this.Hide();
            this.Hided?.Invoke(sender, e);

            this.SaveWindowSetting();
        }
        #endregion

        /// <summary>
        /// DataGridスクロール時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsvCompDataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // TODO: 一致列の有効無効の表示状態を更新したい
            // 表示を更新する
            DataGrid dataGrid = sender as DataGrid;
            dataGrid?.UpdateLayout();
        }

        /// <summary>
        /// CheckBoxマウスホーバー時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CheckBox_MouseEnter(object sender, MouseEventArgs e)
        {
            // Ctrlキーが押されていたらチェックを入れる
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                if (sender is CheckBox checkBox) {
                    checkBox.IsChecked = !checkBox.IsChecked;

                    if (checkBox?.DataContext is CsvComparisonViewModel vm) {
                        this.WVM.SelectedCsvComparisonVM = vm;
                        if (vm.ActionId.HasValue) {
                            await this.SaveIsMatchAsync(vm.ActionId.Value, vm.IsMatch);
                        }
                    }
                }
            }
        }
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        private async Task UpdateBookListAsync(int? bookId = null)
        {
            int? tmpBookId = bookId ?? this.WVM.SelectedBookVM?.Id;

            ObservableCollection<BookComparisonViewModel> bookCompVMList = [];
            BookComparisonViewModel selectedBookCompVM = null;
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                MstBookDao mstBookDao = new(dbHandler);

                var dtoList = await mstBookDao.FindIfJsonCodeExistsAsync();
                foreach (MstBookDto dto in dtoList) {
                    MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : new(dto.JsonCode);
                    if (jsonObj is null) continue;

                    BookComparisonViewModel vm = new() {
                        Id = dto.BookId,
                        Name = dto.BookName,
                        CsvFolderPath = jsonObj.CsvFolderPath == string.Empty ? null : jsonObj.CsvFolderPath,
                        TextEncoding = jsonObj.TextEncoding,
                        ActDateIndex = jsonObj.CsvActDateIndex + 1,
                        ExpensesIndex = jsonObj.CsvOutgoIndex + 1,
                        ItemNameIndex = jsonObj.CsvItemNameIndex + 1
                    };
                    if (vm.CsvFolderPath == null || vm.ActDateIndex == null || vm.ExpensesIndex == null || vm.ItemNameIndex == null) continue;

                    bookCompVMList.Add(vm);

                    if (vm.Id == tmpBookId) {
                        selectedBookCompVM = vm;
                    }
                }
            }
            this.WVM.BookVMList = bookCompVMList;
            this.WVM.SelectedBookVM = selectedBookCompVM ?? bookCompVMList[0];
        }

        /// <summary>
        /// CSVファイルを再読み込みする
        /// </summary>
        /// <returns></returns>
        private void ReloadCsvFiles()
        {
            // リストをクリアする
            this.WVM.CsvComparisonVMList.Clear();

            this.LoadCsvFiles(this.WVM.CsvFilePathList);
        }

        /// <summary>
        /// 指定されたCSVファイルを追加で読み込む
        /// </summary>
        private void LoadCsvFiles(IList<string> csvFilePathList)
        {
            // CSVファイル上の対象インデックスを取得する
            int actDateIndex = this.WVM.SelectedBookVM.ActDateIndex.Value;
            int itemNameIndex = this.WVM.SelectedBookVM.ItemNameIndex.Value;
            int expensesIndex = this.WVM.SelectedBookVM.ExpensesIndex.Value;

            // CSVファイルを読み込む
            CsvConfiguration csvConfig = new(CultureInfo.CurrentCulture) {
                HasHeaderRecord = true,
                MissingFieldFound = (mffa) => { }
            };
            List<CsvComparisonViewModel> tmpVMList = [];
            foreach (string tmpFileName in csvFilePathList) {
                using (CsvReader reader = new(new StreamReader(tmpFileName, Encoding.GetEncoding(this.WVM.SelectedBookVM.TextEncoding)), csvConfig)) {
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

                            tmpVMList2.Add(new CsvComparisonViewModel() { Record = new CsvViewModel() { Date = date, Name = name, Value = value } });
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
                tmpVMList.Sort((tmp1, tmp2) => { return (int)(tmp1.Record.Date - tmp2.Record.Date).TotalDays; });
                foreach (CsvComparisonViewModel vm in tmpVMList) {
                    this.WVM.CsvComparisonVMList.Add(vm);
                }
            }
        }

        /// <summary>
        /// 帳簿項目と比較してCSV比較VMリストを更新する
        /// </summary>
        /// <param name="isScroll">スクロールするか</param>
        private async Task UpdateComparisonVMListAsync(bool isScroll = false)
        {
            // 指定された帳簿内で、日付、金額が一致する帳簿項目を探す
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                foreach (var vm in this.WVM.CsvComparisonVMList) {
                    // 前回の帳簿項目情報をクリアする
                    vm.ClearActionInfo();

                    ActionCompInfoDao actionCompInfoDao = new(dbHandler);
                    var dtoList = await actionCompInfoDao.FindMatchesWithCsvAsync(this.WVM.SelectedBookVM.Id.Value, vm.Record.Date, vm.Record.Value);
                    foreach (ActionCompInfoDto dto in dtoList) {
                        // 帳簿項目IDが使用済なら次のレコードを調べるようにする
                        bool checkNext = this.WVM.CsvComparisonVMList.Where((tmpVM) => { return tmpVM.ActionId == dto.ActionId; }).Any();
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
                this.csvCompDataGrid.ScrollToButtom();
            }
        }

        /// <summary>
        /// CSVファイルを閉じる
        /// </summary>
        private void CloseCsvFiles()
        {
            // リストをクリアする
            this.WVM.CsvComparisonVMList.Clear();
            // CSVファイルリストをクリアする
            this.WVM.CsvFilePathList.Clear();
        }
        #endregion

        #region 設定反映用の関数
        /// <summary>
        /// ウィンドウ設定を読み込む
        /// </summary>
        public void LoadWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.CsvComparisonWindow_Width != -1 && settings.CsvComparisonWindow_Height != -1) {
                this.Width = settings.CsvComparisonWindow_Width;
                this.Height = settings.CsvComparisonWindow_Height;
            }

            if (settings.App_IsPositionSaved && -10 <= settings.CsvComparisonWindow_Left && 0 <= settings.CsvComparisonWindow_Top) {
                this.Left = settings.CsvComparisonWindow_Left;
                this.Top = settings.CsvComparisonWindow_Top;
            }
            else {
                this.MoveOwnersCenter();
            }
        }

        /// <summary>
        /// ウィンドウ設定を保存する
        /// </summary>
        public void SaveWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (this.WindowState == WindowState.Normal) {
                if (settings.App_IsPositionSaved) {
                    settings.CsvComparisonWindow_Left = this.Left;
                    settings.CsvComparisonWindow_Top = this.Top;
                }
                settings.CsvComparisonWindow_Width = this.Width;
                settings.CsvComparisonWindow_Height = this.Height;
                settings.Save();
            }
        }
        #endregion

        /// <summary>
        /// イベントハンドラをWVMに登録する
        /// </summary>
        private void RegisterEventHandlerToWVM()
        {
            this.WVM.BookChanged += async (bookId) => {
                this.BookChanged?.Invoke(this, new EventArgs<int?>(bookId));

                this.ReloadCsvFiles();
                await this.UpdateComparisonVMListAsync(true);
            };
        }

        /// <summary>
        /// 一致フラグを保存する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <param name="isMatch">一致フラグ</param>
        /// <returns></returns>
        private async Task SaveIsMatchAsync(int actionId, bool isMatch)
        {
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                HstActionDao hstActionDao = new(dbHandler);
                _ = await hstActionDao.UpdateIsMatchByIdAsync(actionId, isMatch ? 1 : 0);
            }
        }
    }
}
