﻿using CsvHelper;
using CsvHelper.Configuration;
using HouseholdAccountBook.Dao;
using HouseholdAccountBook.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Windows.Controls;
using static HouseholdAccountBook.ConstValue.ConstValue;
using HouseholdAccountBook.UserEventArgs;
using HouseholdAccountBook.Extensions;

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

        #region イベントハンドラ
        /// <summary>
        /// 比較結果が変更された時
        /// </summary>
        public event EventHandler<EventArgs<int>> ChangedIsMatch;
        #endregion

        /// <summary>
        /// CSV比較ウィンドウ
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

            this.CsvComparisonWindowVM.CsvFileName = Path.GetFileName(ofd.FileName);

            // 開いたCSVファイルのパスを設定として保存する
            settings.App_CsvFilePath = ofd.FileName;
            settings.Save();

            // CSVファイルをマッピングする
            CsvConfiguration config = new CsvConfiguration() {
                HasHeaderRecord = true,
                WillThrowOnMissingField = false
            };
            List<CsvComparisonViewModel> tmpList = new List<CsvComparisonViewModel>();
            using (CsvReader reader = new CsvReader(new StreamReader(ofd.FileName, Encoding.GetEncoding(932)), config)) { 
                while (reader.Read()) {
                    try {
                        int dateIndex = 0; // このインデックスは設定で指定するようにする
                        int nameIndex = 3;
                        int valueIndex = 5;
                        
                        if(reader.TryGetField<DateTime>(dateIndex, out DateTime date) && reader.TryGetField<string>(nameIndex, out string name) && reader.TryGetField<int>(valueIndex, out int value)) {
                            tmpList.Add(new CsvComparisonViewModel() { Record = new CsvComparisonViewModel.CsvRecord() { Date = date, Name = name, Value = value } });
                        }
                    }
                    catch (Exception) { }
                }
            }
            this.CsvComparisonWindowVM.CsvComparisonVMList = new ObservableCollection<CsvComparisonViewModel>(tmpList);
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
            e.CanExecute = this.CsvComparisonWindowVM.SelectedCsvComparisonVM != null && this.CsvComparisonWindowVM.SelectedCsvComparisonVM.ActionId.HasValue;
        }

        /// <summary>
        /// 項目編集処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ActionRegistrationWindow arw = new ActionRegistrationWindow(builder, this.CsvComparisonWindowVM.SelectedCsvComparisonVM.ActionId.Value);
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
            if (this.CsvComparisonWindowVM.CsvComparisonVMList != null) {
                foreach (CsvComparisonViewModel vm in this.CsvComparisonWindowVM.CsvComparisonVMList) {
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

            using (DaoBase dao = builder.Build()) {
                dao.ExecTransaction(() => {
                    foreach (CsvComparisonViewModel vm in this.CsvComparisonWindowVM.CsvComparisonVMList) {
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
            e.CanExecute = this.CsvComparisonWindowVM.CsvFileName != default(string) && this.CsvComparisonWindowVM.SelectedBookVM != null;
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
            CsvComparisonViewModel vm = (this.CsvComparisonWindowVM.SelectedCsvComparisonVM = (e.OriginalSource as CheckBox)?.DataContext as CsvComparisonViewModel);
            if (vm.ActionId.HasValue) {
                ChangeIsMatch(vm.ActionId.Value, vm.IsMatch);
            }
        }
        #endregion

        #region イベントハンドラ
        /// <summary>
        /// フォーム終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsvComparisonWindow_Closing(object sender, CancelEventArgs e)
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

                CsvComparisonViewModel vm = (this.CsvComparisonWindowVM.SelectedCsvComparisonVM = checkBox?.DataContext as CsvComparisonViewModel);
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
            int? tmpBookId = bookId ?? this.CsvComparisonWindowVM.SelectedBookVM?.Id;

            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            BookViewModel selectedBookVM = null;
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT * 
FROM mst_book 
WHERE del_flg = 0 AND book_kind <> @{0}
ORDER BY sort_order;", (int)BookKind.Wallet);
                reader.ExecWholeRow((count, record) => {
                    BookViewModel vm = new BookViewModel() { Id = record.ToInt("book_id"), Name = record["book_name"] };
                    bookVMList.Add(vm);

                    if (vm.Id == tmpBookId) {
                        selectedBookVM = vm;
                    }
                    return true;
                });
            }
            this.CsvComparisonWindowVM.BookVMList = bookVMList;
            this.CsvComparisonWindowVM.SelectedBookVM = selectedBookVM ?? bookVMList[0];
        }
        
        /// <summary>
        /// 比較情報を更新する
        /// </summary>
        private void UpdateComparisonInfo()
        {
            // 指定された帳簿内で、日付、金額が一致する帳簿項目を探す
            using (DaoBase dao = builder.Build()) {
                foreach (CsvComparisonViewModel vm in this.CsvComparisonWindowVM.CsvComparisonVMList) {
                    // 前回の結果をクリアする
                    vm.ClearActionInfo();

                    DaoReader reader = dao.ExecQuery(@"
SELECT A.action_id, I.item_name, A.act_value, A.shop_name, A.remark, A.is_match
FROM hst_action A
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = A.item_id
WHERE A.act_time = @{0} AND A.act_value = -@{1} AND book_id = @{2} AND A.del_flg = 0;", vm.Record.Date, vm.Record.Value, this.CsvComparisonWindowVM.SelectedBookVM.Id);

                    reader.ExecWholeRow((count, record) => {
                        int actionId = record.ToInt("action_id");
                        string itemName = record["item_name"];
                        int outgo = Math.Abs(record.ToInt("act_value"));
                        string shopName = record["shop_name"];
                        string remark = record["remark"];
                        bool isMatch = record.ToInt("is_match") == 1;

                        // 帳簿項目IDが使用済なら次のレコードを調べるようにする
                        bool ans = this.CsvComparisonWindowVM.CsvComparisonVMList.Where((tmpVM) => { return tmpVM.ActionId == actionId; }).Count() != 0;
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
            
            List<CsvComparisonViewModel> list = this.CsvComparisonWindowVM.CsvComparisonVMList.ToList();
            list.Sort((vm1, vm2) => {
                int rslt = (int)((vm1.Record.Date - vm2.Record.Date).TotalDays);
                if (rslt == 0) { rslt = (vm1.ActionId ?? 0) - (vm2.ActionId ?? 0); }
                return rslt;
            });
            this.CsvComparisonWindowVM.CsvComparisonVMList = new ObservableCollection<CsvComparisonViewModel>(list);
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
                Left = settings.CsvComparisonWindow_Left;
            }
            if (settings.CsvComparisonWindow_Top != -1) {
                Top = settings.CsvComparisonWindow_Top;
            }
            if (settings.CsvComparisonWindow_Width != -1) {
                Width = settings.CsvComparisonWindow_Width;
            }
            if (settings.CsvComparisonWindow_Height != -1) {
                Height = settings.CsvComparisonWindow_Height;
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        private void SaveSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;
            settings.CsvComparisonWindow_Left = Left;
            settings.CsvComparisonWindow_Top = Top;
            settings.CsvComparisonWindow_Width = Width;
            settings.CsvComparisonWindow_Height = Height;
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
            using (DaoBase dao = builder.Build()) {
                dao.ExecNonQuery(@"
UPDATE hst_action
SET is_match = @{0}, update_time = 'now', updater = @{1}
WHERE action_id = @{2};", isMatch ? 1 : 0, Updater, actionId);
            }

            ChangedIsMatch?.Invoke(this, new EventArgs<int>(actionId));
        }
    }
}