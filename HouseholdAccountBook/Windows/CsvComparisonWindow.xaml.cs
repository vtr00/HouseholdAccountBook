using CsvHelper;
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

            // 開いたCSVファイルのパスを設定として保存する
            settings.App_CsvFilePath = ofd.FileName;
            settings.Save();

            // CSVファイルをマッピングする
            CsvConfiguration config = new CsvConfiguration() {
                HasHeaderRecord = true,
                WillThrowOnMissingField = false
            };
            using (CsvReader reader = new CsvReader(new StreamReader(ofd.FileName, Encoding.GetEncoding(932)), config)) { 
                ObservableCollection<CsvComparisonViewModel> list = new ObservableCollection<CsvComparisonViewModel>();
                while (reader.Read()) {
                    try {
                        int dateIndex = 0; // このインデックスは設定で指定するようにする
                        int nameIndex = 3;
                        int valueIndex = 5;
                        
                        if(reader.TryGetField<DateTime>(dateIndex, out DateTime date) && reader.TryGetField<string>(nameIndex, out string name) && reader.TryGetField<int>(valueIndex, out int value)) {
                            list.Add(new CsvComparisonViewModel() { Record = new CsvComparisonViewModel.CsvRecord() { Date = date, Name = name, Value = value } });
                        }
                    }
                    catch (Exception) { }
                }
                this.CsvComparisonWindowVM.CsvComparisonVMList = list;
            }

            // 指定された帳簿内で、日付、金額が一致する帳簿項目を探す
            using(DaoBase dao = builder.Build()) {
                foreach(CsvComparisonViewModel vm in this.CsvComparisonWindowVM.CsvComparisonVMList) {
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
                        if(!ans) {
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
        }

        /// <summary>
        /// 一致チェックボックスが変更されたら保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeIsMatchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CsvComparisonViewModel vm = (this.CsvComparisonWindowVM.SelectedCsvComparisonVM = (e.OriginalSource as CheckBox)?.DataContext as CsvComparisonViewModel);
            using (DaoBase dao = builder.Build()) {
                dao.ExecNonQuery(@"
UPDATE hst_action
SET is_match = @{0}, update_time = 'now', updater = @{1}
WHERE action_id = @{2};", vm.IsMatch ? 1 : 0, Updater, vm.ActionId);
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_MouseEnter(object sender, MouseEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            checkBox.IsChecked = !checkBox.IsChecked;
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
    }
}
