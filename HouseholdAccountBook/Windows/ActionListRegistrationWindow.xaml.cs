﻿using HouseholdAccountBook.Dao;
using HouseholdAccountBook.UserControls;
using HouseholdAccountBook.UserEventArgs;
using HouseholdAccountBook.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// ActionListRegistrationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ActionListRegistrationWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DAOビルダ
        /// </summary>
        private DaoBuilder builder;
        /// <summary>
        /// MainWindowで選択された帳簿項目の日付
        /// </summary>
        private DateTime? selectedDateTime;
        /// <summary>
        /// 金額列の最後に選択したセル
        /// </summary>
        private DataGridCell lastDataGridCell;
        #endregion

        #region イベント
        /// <summary>
        /// 登録時のイベント
        /// </summary>
        public event EventHandler<EventArgs<int?>> Registrated = null;
        #endregion

        /// <summary>
        /// 複数の帳簿項目の新規登録のために <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="selectedDateTime">選択された日時</param>
        public ActionListRegistrationWindow(DaoBuilder builder, int? bookId, DateTime? selectedDateTime = null)
        {
            this.builder = builder;
            this.selectedDateTime = selectedDateTime;

            InitializeComponent();

            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            BookViewModel selectedBookVM = null;
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"SELECT book_id, book_name FROM mst_book WHERE del_flg = 0;");
                reader.ExecWholeRow((count, record) => {
                    BookViewModel vm = new BookViewModel() { Id = record.ToInt("book_id"), Name = record["book_name"] };
                    bookVMList.Add(vm);
                    if(selectedBookVM == null || bookId == vm.Id) {
                        selectedBookVM = vm;
                    }
                    return true;
                });
            }

            this.WVM.BookVMList = bookVMList;
            this.WVM.SelectedBookVM = selectedBookVM;
            this.WVM.SelectedBalanceKind = BalanceKind.Outgo;

            DateTime dateTime = selectedDateTime ?? DateTime.Today;
            this.WVM.DateValueVMList.Add(new DateValueViewModel() { ActDate = dateTime });

            UpdateCategoryList();
            UpdateItemList();
            UpdateShopList();
            UpdateRemarkList();

            LoadSetting();

            #region イベントハンドラの設定
            this.WVM.BookChanged += () => {
                UpdateCategoryList();
                UpdateItemList();
                UpdateShopList();
                UpdateRemarkList();
            };
            this.WVM.BalanceKindChanged += () => {
                UpdateCategoryList();
                UpdateItemList();
                UpdateShopList();
                UpdateRemarkList();
            };
            this.WVM.CategoryChanged += () => {
                UpdateItemList();
                UpdateShopList();
                UpdateRemarkList();
            };
            this.WVM.ItemChanged += () => {
                UpdateShopList();
                UpdateRemarkList();
            };
            #endregion
        }

        #region イベントハンドラ
        #region コマンド
        /// <summary>
        /// 登録コマンド判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.DateValueVMList.Count >= 1;
            foreach (DateValueViewModel vm in this.WVM.DateValueVMList) {
                if(!e.CanExecute) { break; }
                e.CanExecute &= vm.ActValue.HasValue;
            }
        }

        /// <summary>
        /// 登録コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // DB登録
            int? id = RegisterToDb();

            // MainWindow更新
            Registrated?.Invoke(this, new EventArgs<int?>(id));

            this.DialogResult = true;
            this.Close();
        }
        
        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// 数値入力ボタン押下コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonInputCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextBox textBox = _popup.PlacementTarget as TextBox;
            DateValueViewModel vm = textBox.DataContext as DateValueViewModel;

            switch (this.WVM.InputedKind) {
                case NumericInputButton.InputKind.Number:
                    int value = this.WVM.InputedValue.Value;
                    if (vm.ActValue == null) {
                        vm.ActValue = value;
                        textBox.SelectionStart = 1;
                    }
                    else {
                        int selectionStart = textBox.SelectionStart;
                        int selectionEnd = selectionStart + textBox.SelectionLength;
                        string forwardText = textBox.Text.Substring(0, selectionStart);
                        string selectedText = textBox.SelectedText;
                        string backwardText = textBox.Text.Substring(selectionEnd, textBox.Text.Length - selectionEnd);

                        if(int.TryParse(string.Format("{0}{1}{2}", forwardText, value, backwardText), out int outValue)) {
                            vm.ActValue = outValue;
                            textBox.SelectionStart = selectionStart + 1;
                        }
                    }
                    break;
                case NumericInputButton.InputKind.BackSpace:
                    if (vm.ActValue == 0) {
                        vm.ActValue = null;
                    }
                    else {
                        int selectionStart = textBox.SelectionStart;
                        int selectionLength = textBox.SelectionLength;
                        int selectionEnd = selectionStart + textBox.SelectionLength;
                        string forwardText = textBox.Text.Substring(0, selectionStart);
                        string backwardText = textBox.Text.Substring(selectionEnd, textBox.Text.Length - selectionEnd);

                        if (selectionLength != 0) {
                            if(int.TryParse(string.Format("{0}{1}", forwardText, backwardText), out int outValue)) {
                                vm.ActValue = outValue;
                                textBox.SelectionStart = selectionStart;
                            }
                        }
                        else if (selectionStart != 0) {
                            string newText = string.Format("{0}{1}", forwardText.Substring(0, selectionStart - 1), backwardText);
                            if(string.Empty == newText || int.TryParse(newText, out int outValue)) {
                                vm.ActValue = string.Empty == newText ? (int?)null : int.Parse(newText);
                                textBox.SelectionStart = selectionStart - 1;
                            }
                        }
                    }
                    break;
                case NumericInputButton.InputKind.Clear:
                    vm.ActValue = null;
                    break;
            }
            // 外れたフォーカスを元に戻す
            lastDataGridCell.IsEditing = true; // セルを編集モードにする - 画面がちらつくがやむを得ない？
            textBox.Focus();
            lastDataGridCell.Focus(); // Enterキーでの入力完了を有効にする
            //Keyboard.Focus(textBox); // キーでの数値入力を有効にする - 意図した動作にならない

            e.Handled = true;
        }
        #endregion

        /// <summary>
        /// フォーム終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionListRegistrationWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveSetting();
        }

        /// <summary>
        /// 日付金額リスト追加時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            if (this.WVM.DateValueVMList.Count == 0) {
                e.NewItem = new DateValueViewModel() { ActDate = selectedDateTime ?? DateTime.Now, ActValue = null };
            }
            else {
                // リストに入力済の末尾のデータの日付を追加時に採用する
                DateValueViewModel lastVM = this.WVM.DateValueVMList[this.WVM.DateValueVMList.Count - 1];
                e.NewItem = new DateValueViewModel() { ActDate = lastVM.ActDate, ActValue = null };
            }
        }
        
        /// <summary>
        /// 入力された値を表示前にチェックする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            bool yes_parse = false;
            if (sender != null) {
                // 既存のテキストボックス文字列に、新規に一文字追加された時、その文字列が数値として意味があるかどうかをチェック
                {
                    string tmp = textBox.Text + e.Text;
                    if (tmp == string.Empty) {
                        yes_parse = true;
                    }
                    else {
                        yes_parse = Int32.TryParse(tmp, out int xx);

                        // 範囲内かどうかチェック
                        if (yes_parse) {
                            if (xx < 0) {
                                yes_parse = false;
                            }
                        }
                    }
                }
            }
            // 更新したい場合は false, 更新したくない場合は true
            e.Handled = !yes_parse;
        }

        /// <summary>
        /// Popupを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!this.WVM.IsEditing) { 
                TextBox textBox = sender as TextBox;
                _popup.SetBinding(Popup.StaysOpenProperty, new Binding(TextBox.IsKeyboardFocusedProperty.Name) { Source = textBox });
                _popup.PlacementTarget = textBox;
                this.WVM.IsEditing = true;
            }
            e.Handled = true;
        }

        /// <summary>
        /// 選択された金額列のセルを記録する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            DataGridCell dataGridCell = sender as DataGridCell;
            
            // 新しいセルに移動していたら数値入力ボタンを非表示にする
            if (dataGridCell != lastDataGridCell) {
                this.WVM.IsEditing = false;
            }
            lastDataGridCell = dataGridCell;
        }
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// カテゴリリストを更新する
        /// </summary>
        /// <param name="categoryId">選択対象のカテゴリ</param>
        private void UpdateCategoryList(int? categoryId = null)
        {
            ObservableCollection<CategoryViewModel> categoryVMList = new ObservableCollection<CategoryViewModel>() {
                new CategoryViewModel() { Id = -1, Name = "(指定なし)" }
            };
            CategoryViewModel selectedCategoryVM = categoryVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT category_id, category_name FROM mst_category C 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM mst_item I WHERE I.category_id = C.category_id AND balance_kind = @{0} AND del_flg = 0 
  AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{1} AND RBI.item_id = I.item_id)) 
ORDER BY sort_order;", (int)this.WVM.SelectedBalanceKind, this.WVM.SelectedBookVM.Id);
                reader.ExecWholeRow((count, record) => {
                    CategoryViewModel vm = new CategoryViewModel() { Id = record.ToInt("category_id"), Name = record["category_name"] };
                    categoryVMList.Add(vm);
                    if(vm.Id == categoryId) {
                        selectedCategoryVM = vm;
                    }
                    return true;
                });
            }
            this.WVM.CategoryVMList = categoryVMList;
            this.WVM.SelectedCategoryVM = selectedCategoryVM;
        }

        /// <summary>
        /// 項目リストを更新する
        /// </summary>
        /// <param name="itemId">選択対象の項目</param>
        private void UpdateItemList(int? itemId = null)
        {
            ObservableCollection<ItemViewModel> itemVMList = new ObservableCollection<ItemViewModel>();
            ItemViewModel selectedItemVM = null;
            using (DaoBase dao = builder.Build()) {
                DaoReader reader;
                if (this.WVM.SelectedCategoryVM.Id == -1) {
                    reader = dao.ExecQuery(@"
SELECT item_id, item_name FROM mst_item I 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{0} AND RBI.item_id = I.item_id AND del_flg = 0)
  AND EXISTS (SELECT * FROM mst_category C WHERE C.category_id = I.category_id AND balance_kind = @{1} AND del_flg = 0)
ORDER BY sort_order;", this.WVM.SelectedBookVM.Id, (int)this.WVM.SelectedBalanceKind);
                }
                else {
                    reader = dao.ExecQuery(@"
SELECT item_id, item_name FROM mst_item I 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{0} AND RBI.item_id = I.item_id AND del_flg = 0)
  AND category_id = @{1}
ORDER BY sort_order;", this.WVM.SelectedBookVM.Id, (int)this.WVM.SelectedCategoryVM.Id);
                }
                reader.ExecWholeRow((count, record) => {
                    ItemViewModel vm = new ItemViewModel() { Id = record.ToInt("item_id"), Name = record["item_name"] };
                    itemVMList.Add(vm);
                    if (selectedItemVM == null || vm.Id == itemId) {
                        selectedItemVM = vm;
                    }
                    return true;
                });
            }
            this.WVM.ItemVMList = itemVMList;
            this.WVM.SelectedItemVM = selectedItemVM;
        }

        /// <summary>
        /// 店舗リストを更新する
        /// </summary>
        /// <param name="shopName">選択対象の店舗名</param>
        private void UpdateShopList(string shopName = null)
        {
            ObservableCollection<string> shopNameVMList = new ObservableCollection<string>() {
                    string.Empty
            };
            string selectedShopName = shopName ?? this.WVM.SelectedShopName ?? shopNameVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT shop_name FROM hst_shop 
WHERE del_flg = 0 AND item_id = @{0} 
ORDER BY used_time DESC;", this.WVM.SelectedItemVM.Id);
                reader.ExecWholeRow((count, record) => {
                    string tmp = record["shop_name"];
                    shopNameVMList.Add(tmp);
                    return true;
                });
            }

            this.WVM.ShopNameList = shopNameVMList;
            this.WVM.SelectedShopName = selectedShopName;
        }

        /// <summary>
        /// 備考リストを更新する
        /// </summary>
        /// <param name="remark">選択対象の備考</param>
        private void UpdateRemarkList(string remark = null)
        {
            ObservableCollection<string> remarkVMList = new ObservableCollection<string>() {
                    string.Empty
            };
            string selectedRemark = remark ?? this.WVM.SelectedRemark ?? remarkVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT remark FROM hst_remark 
WHERE del_flg = 0 AND item_id = @{0} 
ORDER BY used_time DESC;", this.WVM.SelectedItemVM.Id);
                reader.ExecWholeRow((count, record) => {
                    string tmp = record["remark"];
                    remarkVMList.Add(tmp);
                    return true;
                });
            }
            
            this.WVM.RemarkList = remarkVMList;
            this.WVM.SelectedRemark = selectedRemark;
        }
        #endregion

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目ID</returns>
        private int? RegisterToDb()
        {
            if(this.WVM.DateValueVMList.Count < 1) {
                MessageBox.Show(this, MessageText.IllegalValue, MessageTitle.Exclamation, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return null;
            }

            BalanceKind balanceKind = this.WVM.SelectedBalanceKind; // 収支種別
            int bookId = this.WVM.SelectedBookVM.Id.Value; // 帳簿ID
            int itemId = this.WVM.SelectedItemVM.Id; // 帳簿項目ID
            string shopName = this.WVM.SelectedShopName; // 店舗名
            string remark = this.WVM.SelectedRemark; // 備考

            DateTime lastActTime = this.WVM.DateValueVMList[0].ActDate;
            using (DaoBase dao = builder.Build()) {
                #region 帳簿項目を追加する
                foreach(DateValueViewModel vm in this.WVM.DateValueVMList) {
                    if (vm.ActValue.HasValue) {
                        DateTime actTime = vm.ActDate; // 日付
                        int actValue = (balanceKind == BalanceKind.Income ? 1 : -1) * vm.ActValue.Value; // 金額
                        DaoReader reader = dao.ExecQuery(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, shop_name, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, 0, 'now', @{6}, 'now', @{7}) RETURNING action_id;",
                            bookId, itemId, actTime, actValue, shopName, remark, Updater, Inserter);
                    }
                }
                #endregion

                if (shopName != string.Empty) {
                    #region 店舗を追加する
                    dao.ExecTransaction(() => {
                        DaoReader reader = dao.ExecQuery(@"
SELECT shop_name FROM hst_shop
WHERE item_id = @{0} AND shop_name = @{1};", itemId, shopName);

                        if (reader.Count == 0) {
                            dao.ExecNonQuery(@"
INSERT INTO hst_shop (item_id, shop_name, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, 0, 'now', @{3}, 'now', @{4});", itemId, shopName, lastActTime, Updater, Inserter);
                        }
                        else {
                            dao.ExecNonQuery(@"
UPDATE hst_shop
SET used_time = @{0}, del_flg = 0, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND shop_name = @{3} AND used_time < @{0};", lastActTime, Updater, itemId, shopName);
                        }
                    });
                    #endregion
                }

                if (remark != string.Empty) {
                    #region 備考を追加する
                    dao.ExecTransaction(() => {
                        DaoReader reader = dao.ExecQuery(@"
SELECT remark FROM hst_remark
WHERE item_id = @{0} AND remark = @{1};", itemId, remark);

                        if (reader.Count == 0) {
                            dao.ExecNonQuery(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, 0, @{2}, 0, 'now', @{3}, 'now', @{4});", itemId, remark, lastActTime, Updater, Inserter);
                        }
                        else {
                            dao.ExecNonQuery(@"
UPDATE hst_remark
SET used_time = @{0}, del_flg = 0, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND remark = @{3} AND used_time < @{0};", lastActTime, Updater, itemId, remark);
                        }
                    });
                    #endregion
                }
            }

            return null;
        }

        #region 設定反映用の関数
        /// <summary>
        /// 設定を読み込む
        /// </summary>
        private void LoadSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.ActionListRegistrationWindow_Left != -1) {
                Left = settings.ActionListRegistrationWindow_Left;
            }
            if (settings.ActionListRegistrationWindow_Top != -1) {
                Top = settings.ActionListRegistrationWindow_Top;
            }
            if (settings.ActionListRegistrationWindow_Width != -1) {
                Width = settings.ActionListRegistrationWindow_Width;
            }
            if (settings.ActionListRegistrationWindow_Height != -1) {
                Height = settings.ActionListRegistrationWindow_Height;
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        private void SaveSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (this.WindowState == WindowState.Normal) {
                settings.ActionListRegistrationWindow_Left = Left;
                settings.ActionListRegistrationWindow_Top = Top;
                settings.ActionListRegistrationWindow_Width = Width;
                settings.ActionListRegistrationWindow_Height = Height;
                settings.Save();
            }
        }
        #endregion
    }
}
