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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        /// 比較結果が変更された時
        /// </summary>
        public event EventHandler<EventArgs<int>> ChangedIsMatch;
        /// <summary>
        /// 比較結果が同時に複数変更された時
        /// </summary>
        public event EventHandler<EventArgs> ChangedIsMatches;
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
            this.LoadSetting();
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
        /// CSVファイルオープン処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenCsvFilesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
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
                MissingFieldFound = (handlerNames, index, contexts) => { }
            };
            List<CsvComparisonViewModel> tmpList = new List<CsvComparisonViewModel>();
            foreach (string tmpFileName in ofd.FileNames) {
                using (CsvReader reader = new CsvReader(new StreamReader(tmpFileName, Encoding.GetEncoding("Shift_JIS")), csvConfig)) {
                    while (reader.Read()) {
                        try {
                            if (reader.TryGetField(actDateIndex.Value, out DateTime date) && reader.TryGetField(itemNameIndex.Value, out string name) && reader.TryGetField(outgoIndex.Value, out int value)) {
                                tmpList.Add(new CsvComparisonViewModel() { Record = new CsvComparisonViewModel.CsvRecord() { Date = date, Name = name, Value = value } });
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            foreach (CsvComparisonViewModel vm in tmpList) {
                this.WVM.CsvComparisonVMList.Add(vm);
            }
            this.csvCompDataGrid.ScrollToTop();

            this.UpdateComparisonInfoAsync();

            // 合計値を計算する
            this.WVM.SumValue = this.WVM.CsvComparisonVMList.Sum(vm => vm.Record.Value);

            this.Cursor = null;
        }

        /// <summary>
        /// 項目編集可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditActionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 選択されている帳簿項目が1つだけ存在していて、選択している帳簿項目のIDが値を持つ
            e.CanExecute = this.WVM.SelectedCsvComparisonVM != null && this.WVM.SelectedCsvComparisonVM.ActionId.HasValue;
        }

        /// <summary>
        /// 項目編集処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ActionRegistrationWindow arw = new ActionRegistrationWindow(this.builder, this.WVM.SelectedCsvComparisonVM.ActionId.Value);
            arw.Registrated += (sender2, e2) => {
                this.UpdateComparisonInfoAsync();
            };
            arw.ShowDialog();
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
                    foreach(var vm in this.WVM.CsvComparisonVMList) {
                        if (vm.ActionId.HasValue) {
                            await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET is_match = 1, update_time = 'now', updater = @{1}
WHERE action_id = @{0} AND is_match <> 1;", vm.ActionId, Updater);
                        }
                    }
                });
            }

            this.ChangedIsMatches?.Invoke(this, new EventArgs());
            this.UpdateComparisonInfoAsync();

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
        private void UpdateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            this.UpdateComparisonInfoAsync();

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
            this.WVM.CsvComparisonVMList.Clear();
            this.WVM.CsvFileName = default;
        }

        /// <summary>
        /// 一致チェックボックスが変更されたら保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeIsMatchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CsvComparisonViewModel vm = (this.WVM.SelectedCsvComparisonVM = (e.OriginalSource as CheckBox)?.DataContext as CsvComparisonViewModel);
            if (vm.ActionId.HasValue) {
                this.ChangeIsMatchAsync(vm.ActionId.Value, vm.IsMatch);
            }
        }
        #endregion

        /// <summary>
        /// フォーム読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CsvComparisonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await this.UpdateBookListAsync(this.selectedBookId);
        }

        /// <summary>
        /// フォーム終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsvComparisonWindow_Closed(object sender, EventArgs e)
        {
            this.SaveSetting();
        }

        /// <summary>
        /// マウスのホーバーでチェックを入れる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) { 
                CheckBox checkBox = sender as CheckBox;
                checkBox.IsChecked = !checkBox.IsChecked;

                CsvComparisonViewModel vm = (this.WVM.SelectedCsvComparisonVM = checkBox?.DataContext as CsvComparisonViewModel);
                if (vm.ActionId.HasValue) {
                    this.ChangeIsMatchAsync(vm.ActionId.Value, vm.IsMatch);
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
        private async void UpdateComparisonInfoAsync()
        {
            // 指定された帳簿内で、日付、金額が一致する帳簿項目を探す
            using (DaoBase dao = this.builder.Build()) {
                foreach(var vm in this.WVM.CsvComparisonVMList) {
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
                        bool ans = this.WVM.CsvComparisonVMList.Where((tmpVM) => { return tmpVM.ActionId == actionId; }).Count() != 0;
                        if (!ans) {
                            vm.ActionId = actionId;
                            vm.ItemName = itemName;
                            vm.ShopName = shopName;
                            vm.Remark = remark;
                            vm.IsMatch = isMatch;
                        }
                        return ans;
                    });
                }
            }
            
            List<CsvComparisonViewModel> list = this.WVM.CsvComparisonVMList.ToList();
            // 日付と帳簿項目IDでソートする
            list.Sort((vm1, vm2) => {
                int rslt = (int)((vm1.Record.Date - vm2.Record.Date).TotalDays);
                if (rslt == 0) { rslt = (vm1.ActionId ?? 0) - (vm2.ActionId ?? 0); }
                return rslt;
            });
            this.WVM.CsvComparisonVMList = new ObservableCollection<CsvComparisonViewModel>(list);
        }
        #endregion

        #region 設定反映用の関数
        /// <summary>
        /// 設定を読み込む
        /// </summary>
        private void LoadSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (0 <= settings.CsvComparisonWindow_Left) {
                this.Left = settings.CsvComparisonWindow_Left;
            }
            if (0 <= settings.CsvComparisonWindow_Top) {
                this.Top = settings.CsvComparisonWindow_Top;
            }
            if (settings.CsvComparisonWindow_Width != -1) {
                this.Width = settings.CsvComparisonWindow_Width;
            }
            if (settings.CsvComparisonWindow_Height != -1) {
                this.Height = settings.CsvComparisonWindow_Height;
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        private void SaveSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;
            settings.CsvComparisonWindow_Left = this.Left;
            settings.CsvComparisonWindow_Top = this.Top;
            settings.CsvComparisonWindow_Width = this.Width;
            settings.CsvComparisonWindow_Height = this.Height;
            settings.Save();
        }
        #endregion

        /// <summary>
        /// 一致フラグを更新する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <param name="isMatch">一致フラグ</param>
        private async void ChangeIsMatchAsync(int actionId, bool isMatch)
        {
            using (DaoBase dao = this.builder.Build()) {
                await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET is_match = @{0}, update_time = 'now', updater = @{1}
WHERE action_id = @{2};", isMatch ? 1 : 0, Updater, actionId);
            }

            this.ChangedIsMatch?.Invoke(this, new EventArgs<int>(actionId));
        }
    }
}
