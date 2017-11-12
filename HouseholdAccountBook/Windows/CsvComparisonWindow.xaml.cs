using CsvHelper;
using CsvHelper.Configuration;
using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.UserEventArgs;
using HouseholdAccountBook.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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
        private DaoBuilder builder;
        #endregion

        #region イベント
        /// <summary>
        /// 比較結果が変更された時
        /// </summary>
        public event EventHandler<EventArgs<int>> ChangedIsMatch;
        #endregion

        /// <summary>
        /// <see cref="CsvComparisonWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="bookId">帳簿ID</param>
        public CsvComparisonWindow(DaoBuilder builder, int? bookId)
        {
            this.builder = builder;

            InitializeComponent();

            UpdateBookList(bookId);
            LoadSetting();
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
        /// CSVファイルを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenCsvFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
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
                Filter = "CSVファイル|*.csv"
            };

            if (ofd.ShowDialog() == false) return;

            this.WVM.CsvFileName = Path.GetFileName(ofd.FileName);

            // 開いたCSVファイルのパスを設定として保存する
            settings.App_CsvFilePath = ofd.FileName;
            settings.Save();

            // CSVファイルをマッピングする
            Configuration config = new Configuration() {
                HasHeaderRecord = true,  MissingFieldFound = (handlerName, index, contexts) => { }
            };
            List<CsvComparisonViewModel> tmpList = new List<CsvComparisonViewModel>();
            using (CsvReader reader = new CsvReader(new StreamReader(ofd.FileName, Encoding.GetEncoding(932)), config)) { 
                while (reader.Read()) {
                    try {
                        int actDateIndex = this.WVM.SelectedBookVM.ActDateIndex;
                        int itemNameIndex = this.WVM.SelectedBookVM.ItemNameIndex;
                        int outgoIndex = this.WVM.SelectedBookVM.OutgoIndex;
                        
                        if(reader.TryGetField<DateTime>(actDateIndex, out DateTime date) && reader.TryGetField<string>(itemNameIndex, out string name) && reader.TryGetField<int>(outgoIndex, out int value)) {
                            tmpList.Add(new CsvComparisonViewModel() { Record = new CsvComparisonViewModel.CsvRecord() { Date = date, Name = name, Value = value } });
                        }
                    }
                    catch (Exception) { }
                }
            }
            this.WVM.CsvComparisonVMList = new ObservableCollection<CsvComparisonViewModel>(tmpList);
            this.csvCompDataGrid.ScrollToTop();

            UpdateComparisonInfo();
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
                UpdateComparisonInfo();
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
        private void BulkCheckCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Cursor cursor = this.Cursor;
            this.Cursor = Cursors.Wait;

            using (DaoBase dao = this.builder.Build()) {
                dao.ExecTransaction(() => {
                    foreach (CsvComparisonViewModel vm in this.WVM.CsvComparisonVMList) {
                        if (vm.ActionId.HasValue) {
                            dao.ExecNonQuery(@"
UPDATE hst_action
SET is_match = 1, update_time = 'now', updater = @{1}
WHERE action_id = @{0} AND is_match <> 1;", vm.ActionId, Updater);
                        }
                    }
                });
            }
            UpdateComparisonInfo();

            this.Cursor = cursor;
        }

        /// <summary>
        /// 比較情報を更新可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.CsvFileName != default(string) && this.WVM.SelectedBookVM != null;
        }

        /// <summary>
        /// 比較情報を更新する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateComparisonInfo();
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
                ChangeIsMatch(vm.ActionId.Value, vm.IsMatch);
            }
        }
        #endregion

        /// <summary>
        /// フォーム終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsvComparisonWindow_Closed(object sender, EventArgs e)
        {
            SaveSetting();
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
                    ChangeIsMatch(vm.ActionId.Value, vm.IsMatch);
                }
            }
        }
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        private void UpdateBookList(int? bookId = null)
        {
            int? tmpBookId = bookId ?? this.WVM.SelectedBookVM?.Id;

            ObservableCollection<BookComparisonViewModel> bookCompVMList = new ObservableCollection<BookComparisonViewModel>();
            BookComparisonViewModel selectedBookCompVM = null;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT * 
FROM mst_book 
WHERE del_flg = 0 AND book_kind <> @{0}
ORDER BY sort_order;", (int)BookKind.Wallet);
                reader.ExecWholeRow((count, record) => {
                    BookComparisonViewModel vm = new BookComparisonViewModel() {
                        Id = record.ToInt("book_id"),
                        Name = record["book_name"],
                        ActDateIndex = record.ToNullableInt("csv_act_time_index") ?? 0,
                        OutgoIndex = record.ToNullableInt("csv_outgo_index") ?? 0,
                        ItemNameIndex = record.ToNullableInt("csv_item_name_index") ?? 0
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
        private void UpdateComparisonInfo()
        {
            // 指定された帳簿内で、日付、金額が一致する帳簿項目を探す
            using (DaoBase dao = this.builder.Build()) {
                foreach (CsvComparisonViewModel vm in this.WVM.CsvComparisonVMList) {
                    // 前回の結果をクリアする
                    vm.ClearActionInfo();

                    DaoReader reader = dao.ExecQuery(@"
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

            if (settings.CsvComparisonWindow_Left != -1) {
                this.Left = settings.CsvComparisonWindow_Left;
            }
            if (settings.CsvComparisonWindow_Top != -1) {
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
        private void ChangeIsMatch(int actionId, bool isMatch)
        {
            using (DaoBase dao = this.builder.Build()) {
                dao.ExecNonQuery(@"
UPDATE hst_action
SET is_match = @{0}, update_time = 'now', updater = @{1}
WHERE action_id = @{2};", isMatch ? 1 : 0, Updater, actionId);
            }

            ChangedIsMatch?.Invoke(this, new EventArgs<int>(actionId));
        }
    }
}
