using CsvHelper;
using CsvHelper.Configuration;
using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Dto;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.UserEventArgs;
using HouseholdAccountBook.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// CsvComparisonWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CsvComparisonWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DAOビルダ
        /// </summary>
        private readonly DaoBuilder builder;
        /// <summary>
        /// 選択された帳簿ID
        /// </summary>
        private readonly int? selectedBookId;
        #endregion

        #region イベント
        /// <summary>
        /// 比較結果変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<int?>> IsMatchChanged;
        /// <summary>
        /// 複数の比較結果変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs> ActionsStatusChanged;
        /// <summary>
        /// 帳簿変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<int?>> BookChanged;
        #endregion

        /// <summary>
        /// <see cref="CsvComparisonWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        public CsvComparisonWindow(DaoBuilder builder, int? selectedBookId)
        {
            this.builder = builder;
            this.selectedBookId = selectedBookId;

            this.InitializeComponent();
        }

        #region イベントハンドラ
        #region コマンド
        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// CSVファイルオープン可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenCsvFilesCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedBookVM.ActDateIndex.HasValue && this.WVM.SelectedBookVM.ItemNameIndex.HasValue && this.WVM.SelectedBookVM.OutgoIndex.HasValue;
        }

        /// <summary>
        /// CSVファイルを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OpenCsvFilesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            string directory = string.Empty;
            string fileName = string.Empty;
            if (settings.App_CsvFilePath != string.Empty) {
                directory = Path.GetDirectoryName(settings.App_CsvFilePath);
                fileName = Path.GetFileName(settings.App_CsvFilePath);
            }

            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = "ファイル選択",
                Filter = "CSVファイル|*.csv",
                Multiselect = true
            };

            if (ofd.ShowDialog() == false) return;

            this.Cursor = Cursors.Wait;

            this.WVM.CsvFileName = Path.GetFileName(ofd.FileName);

            // 開いたCSVファイルのパスを設定として保存する
            settings.App_CsvFilePath = ofd.FileName;
            settings.Save();

            int? actDateIndex = this.WVM.SelectedBookVM.ActDateIndex;
            int? itemNameIndex = this.WVM.SelectedBookVM.ItemNameIndex;
            int? outgoIndex = this.WVM.SelectedBookVM.OutgoIndex;

            // CSVファイルを読み込む
            CsvConfiguration csvConfig = new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture) {
                HasHeaderRecord = true,
                MissingFieldFound = (mffa) => { }
            };
            List<CsvComparisonViewModel> tmpList = new List<CsvComparisonViewModel>();
            foreach (string tmpFileName in ofd.FileNames) {
                using (CsvReader reader = new CsvReader(new StreamReader(tmpFileName, Encoding.GetEncoding("Shift_JIS")), csvConfig)) {
                    while (reader.Read()) {
                        try {
                            if (reader.TryGetField(actDateIndex.Value, out DateTime date) && reader.TryGetField(itemNameIndex.Value, out string name) && reader.TryGetField(outgoIndex.Value, out string valueStr) &&
                                int.TryParse(valueStr, NumberStyles.Any, NumberFormatInfo.CurrentInfo, out int value)) {
                                tmpList.Add(new CsvComparisonViewModel() { Record = new CsvComparisonViewModel.CsvRecord() { Date = date, Name = name, Value = value } });
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            tmpList.Sort((tmp1, tmp2) => { return (int)(tmp1.Record.Date - tmp2.Record.Date).TotalDays; });
            foreach (CsvComparisonViewModel vm in tmpList) {
                this.WVM.CsvComparisonVMList.Add(vm);
                this.csvCompDataGrid.ScrollToButtom();
            }

            await this.UpdateComparisonInfoAsync();

            this.Cursor = null;
        }

        /// <summary>
        /// 項目追加可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 選択されている帳簿項目が1つ以上存在していて、値を持たない帳簿項目IDが1つ以上ある
            e.CanExecute = this.WVM.SelectedCsvComparisonVMList.Count != 0 && this.WVM.SelectedCsvComparisonVMList.Count((vm) => !vm.ActionId.HasValue) > 0;
        }

        /// <summary>
        /// 項目追加ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            List<CsvComparisonViewModel.CsvRecord> vmList = new List<CsvComparisonViewModel.CsvRecord>(this.WVM.SelectedCsvComparisonVMList.Where((vm) => !vm.ActionId.HasValue).Select((vm) => vm.Record));
            if (vmList.Count() == 1) {
                CsvComparisonViewModel.CsvRecord record = vmList[0];
                ActionRegistrationWindow arw = new ActionRegistrationWindow(this.builder, this.WVM.SelectedBookVM.Id.Value, record) { Owner = this };
                arw.LoadWindowSetting();

                arw.Registrated += async (sender2, e2) => {
                    foreach (int value in e2.Value) {
                        await this.ChangeIsMatchAsync(value, true);
                    }
                    await this.UpdateComparisonInfoAsync();
                    this.ActionsStatusChanged?.Invoke(this, new EventArgs());
                };
                arw.ShowDialog();
            }
            else {
                ActionListRegistrationWindow alrw = new ActionListRegistrationWindow(this.builder, this.WVM.SelectedBookVM.Id.Value, vmList) { Owner = this };
                alrw.LoadWindowSetting();

                alrw.Registrated += async (sender2, e2) => {
                    foreach (int value in e2.Value) {
                        await this.ChangeIsMatchAsync(value, true);
                    }
                    await this.UpdateComparisonInfoAsync();
                    this.ActionsStatusChanged?.Invoke(this, new EventArgs());
                };
                alrw.ShowDialog();
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
        private void EditActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ActionRegistrationWindow arw = new ActionRegistrationWindow(this.builder, this.WVM.SelectedCsvComparisonVM.ActionId.Value) { Owner = this };
            arw.LoadWindowSetting();

            arw.Registrated += async (sender2, e2) => {
                await this.UpdateComparisonInfoAsync();
                this.ActionsStatusChanged?.Invoke(this, new EventArgs());
            };
            arw.ShowDialog();
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
            if (this.WVM.SelectedCsvComparisonVMList.Count != 0 && this.WVM.SelectedCsvComparisonVMList.Count((vm) => !vm.ActionId.HasValue) > 0) {
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
            if (this.WVM.CsvComparisonVMList != null) {
                foreach (CsvComparisonViewModel vm in this.WVM.CsvComparisonVMList) {
                    e.CanExecute |= vm.ActionId.HasValue;
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
            Cursor cursor = this.Cursor;
            this.Cursor = Cursors.Wait;

            using (DaoBase dao = this.builder.Build()) {
                await dao.ExecTransactionAsync(async () => {
                    foreach (var vm in this.WVM.CsvComparisonVMList) {
                        if (vm.ActionId.HasValue) {
                            await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET is_match = 1, update_time = 'now', updater = @{1}
WHERE action_id = @{0} AND is_match <> 1;", vm.ActionId, Updater);
                        }
                    }
                });
            }

            await this.UpdateComparisonInfoAsync();
            this.ActionsStatusChanged?.Invoke(this, new EventArgs());

            this.Cursor = cursor;
        }

        /// <summary>
        /// 比較情報を更新可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.CsvFileName != default && this.WVM.SelectedBookVM != null && this.WVM.CsvComparisonVMList.Count != 0;
        }

        /// <summary>
        /// 比較情報を更新する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            await this.UpdateComparisonInfoAsync();

            this.Cursor = null;
        }

        /// <summary>
        /// リストをクリア可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearListCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.CsvComparisonVMList.Count > 0;
        }

        /// <summary>
        /// リストをクリアする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearListCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.ClearComparisonInfo();
        }

        /// <summary>
        /// 一致チェックボックスを変更可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeIsMatchCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // これを使って判定するとチェックされなくなるので使用しない
            CsvComparisonViewModel vm = (e.OriginalSource as CheckBox)?.DataContext as CsvComparisonViewModel;
            e.CanExecute = vm.ActionId.HasValue;
        }

        /// <summary>
        /// 一致チェックボックスが変更されたら保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeIsMatchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CsvComparisonViewModel vm = (e.OriginalSource as CheckBox)?.DataContext as CsvComparisonViewModel;
            this.WVM.SelectedCsvComparisonVM = vm;
            if (vm.ActionId.HasValue) {
                await this.ChangeIsMatchAsync(vm.ActionId.Value, vm.IsMatch);
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
        /// ウィンドウ終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsvComparisonWindow_Closed(object sender, EventArgs e)
        {
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
                CheckBox checkBox = sender as CheckBox;
                checkBox.IsChecked = !checkBox.IsChecked;

                CsvComparisonViewModel vm = (this.WVM.SelectedCsvComparisonVM = checkBox?.DataContext as CsvComparisonViewModel);
                if (vm.ActionId.HasValue) {
                    await this.ChangeIsMatchAsync(vm.ActionId.Value, vm.IsMatch);
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

            ObservableCollection<BookComparisonViewModel> bookCompVMList = new ObservableCollection<BookComparisonViewModel>();
            BookComparisonViewModel selectedBookCompVM = null;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT * 
FROM mst_book 
WHERE del_flg = 0 AND book_kind <> @{0}
ORDER BY sort_order;", (int)BookKind.Wallet);
                reader.ExecWholeRow((count, record) => {
                    string jsonCode = record["json_code"];
                    MstBookJsonObject jsonObj = JsonConvert.DeserializeObject<MstBookJsonObject>(jsonCode);

                    BookComparisonViewModel vm = new BookComparisonViewModel() {
                        Id = record.ToInt("book_id"),
                        Name = record["book_name"],
                        ActDateIndex = jsonObj?.CsvActDateIndex,
                        OutgoIndex = jsonObj?.CsvOutgoIndex,
                        ItemNameIndex = jsonObj?.CsvItemNameIndex
                    };
                    bookCompVMList.Add(vm);

                    if (vm.Id == tmpBookId) {
                        selectedBookCompVM = vm;
                    }
                    return true;
                });
            }
            this.WVM.BookVMList = bookCompVMList;
            this.WVM.SelectedBookVM = selectedBookCompVM ?? bookCompVMList[0];
        }

        /// <summary>
        /// 比較情報を更新する
        /// </summary>
        private async Task UpdateComparisonInfoAsync()
        {
            // 指定された帳簿内で、日付、金額が一致する帳簿項目を探す
            using (DaoBase dao = this.builder.Build()) {
                foreach (var vm in this.WVM.CsvComparisonVMList) {
                    // 前回の結果をクリアする
                    vm.ClearActionInfo();

                    DaoReader reader = await dao.ExecQueryAsync(@"
SELECT A.action_id, I.item_name, A.act_value, A.shop_name, A.remark, A.is_match
FROM hst_action A
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = A.item_id
WHERE to_date(to_char(act_time, 'YYYY-MM-DD'), 'YYYY-MM-DD') = @{0} AND A.act_value = -@{1} AND book_id = @{2} AND A.del_flg = 0;", vm.Record.Date, vm.Record.Value, this.WVM.SelectedBookVM.Id);

                    reader.ExecWholeRow((count, record) => {
                        int actionId = record.ToInt("action_id");
                        string itemName = record["item_name"];
                        int outgo = Math.Abs(record.ToInt("act_value"));
                        string shopName = record["shop_name"];
                        string remark = record["remark"];
                        bool isMatch = record.ToInt("is_match") == 1;

                        // 帳簿項目IDが使用済なら次のレコードを調べるようにする
                        bool checkNext = this.WVM.CsvComparisonVMList.Where((tmpVM) => { return tmpVM.ActionId == actionId; }).Count() != 0;
                        if (!checkNext) {
                            vm.ActionId = actionId;
                            vm.ItemName = itemName;
                            vm.ShopName = shopName;
                            vm.Remark = remark;
                            vm.IsMatch = isMatch;
                        }
                        return checkNext;
                    });
                }
            }
        }

        /// <summary>
        /// 比較情報をクリアする
        /// </summary>
        private void ClearComparisonInfo()
        {
            // リストをクリアする
            this.WVM.CsvComparisonVMList.Clear();
            this.WVM.CsvFileName = default;
        }
        #endregion

        #region 設定反映用の関数
        /// <summary>
        /// ウィンドウ設定を読み込む
        /// </summary>
        public void LoadWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.App_IsPositionSaved && (-10 <= settings.CsvComparisonWindow_Left && 0 <= settings.CsvComparisonWindow_Top)) {
                this.Left = settings.CsvComparisonWindow_Left;
                this.Top = settings.CsvComparisonWindow_Top;
            }
            else {
                this.MoveOwnersCenter();
            }

            if (settings.CsvComparisonWindow_Width != -1 && settings.CsvComparisonWindow_Height != -1) {
                this.Width = settings.CsvComparisonWindow_Width;
                this.Height = settings.CsvComparisonWindow_Height;
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
            this.WVM.BookChanged += (bookId) => {
                this.ClearComparisonInfo();

                this.BookChanged?.Invoke(this, new EventArgs<int?>(bookId));
            };
        }

        /// <summary>
        /// 一致フラグを更新する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <param name="isMatch">一致フラグ</param>
        private async Task ChangeIsMatchAsync(int actionId, bool isMatch)
        {
            using (DaoBase dao = this.builder.Build()) {
                await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET is_match = @{0}, update_time = 'now', updater = @{1}
WHERE action_id = @{2};", isMatch ? 1 : 0, Updater, actionId);
            }

            this.IsMatchChanged?.Invoke(this, new EventArgs<int?>(actionId));
        }
    }
}
