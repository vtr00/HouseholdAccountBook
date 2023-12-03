using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.UserControls;
using HouseholdAccountBook.UserEventArgs;
using HouseholdAccountBook.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly DaoBuilder builder;
        /// <summary>
        /// <see cref="MainWindow"/>で表示された帳簿ID
        /// </summary>
        private readonly int? selectedBookId;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された表示月日
        /// </summary>
        private readonly DateTime? selectedMonth;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された帳簿項目の日付
        /// </summary>
        private readonly DateTime? selectedDate;
        /// <summary>
        /// <see cref="CsvComparisonWindow"/>で選択されたCSVレコードリスト
        /// </summary>
        private readonly List<CsvComparisonViewModel.CsvRecord> selectedRecordList;
        /// <summary>
        /// <see cref="MainWindow"/>で選択された帳簿項目のグループID
        /// </summary>
        private readonly int? selectedGroupId;
        /// <summary>
        /// グループに属する帳簿項目の帳簿項目ID
        /// </summary>
        private readonly List<int> groupedActionIdList = new List<int>();
        /// <summary>
        /// 金額列の最後に選択したセル
        /// </summary>
        private DataGridCell lastDataGridCell;
        /// <summary>
        /// 最後に選択したセルのVM
        /// </summary>
        private DateValueViewModel lastDateValueVM;
        #endregion

        #region イベント
        /// <summary>
        /// 登録時のイベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> Registrated = null;
        /// <summary>
        /// 帳簿変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<int?>> BookChanged = null;
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 複数の帳簿項目の新規登録のために <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedMonth">選択された年月</param>
        /// <param name="selectedDate">選択された日付</param>
        public ActionListRegistrationWindow(DaoBuilder builder, int? selectedBookId, DateTime? selectedMonth, DateTime? selectedDate = null)
        {
            this.builder = builder;
            this.selectedBookId = selectedBookId;
            this.selectedMonth = selectedMonth;
            this.selectedDate = selectedDate;
            this.selectedGroupId = null;

            this.InitializeComponent();

            this.WVM.RegMode = RegistrationMode.Add;
        }

        /// <summary>
        /// 複数の帳簿項目の新規登録のために <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedRecordList">選択されたCSVレコードリスト</param>
        public ActionListRegistrationWindow(DaoBuilder builder, int selectedBookId, List<CsvComparisonViewModel.CsvRecord> selectedRecordList)
        {
            this.builder = builder;
            this.selectedBookId = selectedBookId;
            this.selectedRecordList = selectedRecordList;
            this.selectedMonth = null;
            this.selectedDate = null;
            this.selectedGroupId = null;

            this.InitializeComponent();

            this.WVM.RegMode = RegistrationMode.Add;
        }

        /// <summary>
        /// 複数の帳簿項目の編集(複製)のために <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="selectedGroupId">選択されたグループID</param>
        /// <param name="mode">登録モード</param>
        public ActionListRegistrationWindow(DaoBuilder builder, int selectedGroupId, RegistrationMode mode = RegistrationMode.Edit)
        {
            this.builder = builder;
            this.selectedBookId = null;
            this.selectedMonth = null;
            this.selectedDate = null;
            switch (mode) {
                case RegistrationMode.Edit:
                case RegistrationMode.Copy:
                    this.selectedGroupId = selectedGroupId;
                    break;
            }

            this.InitializeComponent();

            this.WVM.RegMode = mode;
        }
        #endregion

        #region イベントハンドラ
        #region コマンド
        /// <summary>
        /// 登録コマンド判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.DateValueVMList.Count((vm) => vm.ActValue.HasValue) >= 1;
        }

        /// <summary>
        /// 登録コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RegisterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // DB登録
            List<int> idList = await this.RegisterToDbAsync();

            // MainWindow更新
            this.Registrated?.Invoke(this, new EventArgs<List<int>>(idList ?? new List<int>()));

            try {
                this.DialogResult = true;
            }
            catch (InvalidOperationException) { }
            
            this.Close();
        }

        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try {
                this.DialogResult = false;
            }
            catch (InvalidOperationException) { }
            this.Close();
        }

        /// <summary>
        /// 数値入力ボタン押下コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonInputCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextBox textBox = this._popup.PlacementTarget as TextBox;
            if (!(textBox.DataContext is DateValueViewModel vm)) vm = this.lastDateValueVM; // textBoxのDataContextが取得できないため応急処置

            switch (this.WVM.InputedKind) {
                case NumericInputButton.InputKind.Number:
                    int value = this.WVM.InputedValue.Value;
                    if (vm.ActValue == null) {
                        vm.ActValue = value;
                        textBox.Text = string.Format("{0}", vm.ActValue);
                        textBox.SelectionStart = 1;
                    }
                    else {
                        // 選択された位置に値を挿入する
                        int selectionStart = textBox.SelectionStart;
                        int selectionEnd = selectionStart + textBox.SelectionLength;
                        string forwardText = textBox.Text.Substring(0, selectionStart);
                        string backwardText = textBox.Text.Substring(selectionEnd, textBox.Text.Length - selectionEnd);

                        if (int.TryParse(string.Format("{0}{1}{2}", forwardText, value, backwardText), out int outValue)) {
                            vm.ActValue = outValue;
                            textBox.Text = string.Format("{0}", vm.ActValue);
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
                            if (int.TryParse(string.Format("{0}{1}", forwardText, backwardText), out int outValue)) {
                                vm.ActValue = outValue;
                                textBox.Text = string.Format("{0}", outValue);
                                textBox.SelectionStart = selectionStart;
                            }
                        }
                        else if (selectionStart != 0) {
                            string newText = string.Format("{0}{1}", forwardText.Substring(0, selectionStart - 1), backwardText);
                            if (string.Empty == newText || int.TryParse(newText, out int outValue)) {
                                vm.ActValue = string.Empty == newText ? (int?)null : int.Parse(newText);
                                textBox.Text = string.Format("{0}", vm.ActValue);
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
            this.lastDataGridCell.IsEditing = true; // セルを編集モードにする - 画面がちらつくがやむを得ない？
            textBox.Focus();
            this.lastDataGridCell.Focus(); // Enterキーでの入力完了を有効にする
            //Keyboard.Focus(textBox); // キーでの数値入力を有効にする - 意図した動作にならない

            e.Handled = true;
        }
        #endregion

        #region ウィンドウ
        /// <summary>
        /// ウィンドウ読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ActionListRegistrationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            int? bookId = null;
            BalanceKind balanceKind = BalanceKind.Outgo;

            int? itemId = null;
            string shopName = null;
            string remark = null;

            // DBから値を読み込む
            switch (this.WVM.RegMode) {
                case RegistrationMode.Add: {
                    bookId = this.selectedBookId;
                    balanceKind = BalanceKind.Outgo;
                    DateTime actDate = this.selectedDate ?? ((this.selectedMonth == null || this.selectedMonth?.Month == DateTime.Today.Month) ? DateTime.Today : this.selectedMonth.Value);

                    if (this.selectedRecordList == null) {
                        this.WVM.DateValueVMList.Add(new DateValueViewModel() { ActDate = actDate });
                    }
                    else {
                        foreach (CsvComparisonViewModel.CsvRecord record in this.selectedRecordList) {
                            this.WVM.DateValueVMList.Add(new DateValueViewModel() { ActDate = record.Date, ActValue = record.Value });
                        }
                    }
                }
                break;
                case RegistrationMode.Edit:
                case RegistrationMode.Copy: {
                    using (DaoBase dao = this.builder.Build()) {
                        DaoReader reader = await dao.ExecQueryAsync(@"
SELECT book_id, item_id, action_id, act_time, act_value, shop_name, remark
FROM hst_action 
WHERE del_flg = 0 AND group_id = @{0};", this.selectedGroupId);
                        reader.ExecWholeRow((count, record) => {
                            int actionId = -1;
                            DateTime actDate = DateTime.Now;
                            int actValue = -1;

                            actionId = record.ToInt("action_id");
                            actDate = record.ToDateTime("act_time");
                            actValue = record.ToInt("act_value");
                            bookId = record.ToInt("book_id");
                            itemId = record.ToInt("item_id");
                            shopName = record["shop_name"];
                            remark = record["remark"];

                            DateValueViewModel vm = new DateValueViewModel() {
                                ActionId = actionId,
                                ActDate = actDate,
                                ActValue = Math.Abs(actValue)
                            };

                            balanceKind = Math.Sign(actValue) > 0 ? BalanceKind.Income : BalanceKind.Outgo; // 収入 / 支出

                            this.groupedActionIdList.Add(vm.ActionId.Value);
                            this.WVM.DateValueVMList.Add(vm);
                            return true;
                        });
                    }
                }
                break;
            }

            // WVMに値を設定する
            this.WVM.GroupId = this.WVM.RegMode == RegistrationMode.Edit ? this.selectedGroupId : null;
            this.WVM.SelectedBalanceKind = balanceKind;

            // リストを更新する
            await this.UpdateBookListAsync(bookId);
            await this.UpdateCategoryListAsync();
            await this.UpdateItemListAsync(itemId);
            await this.UpdateShopListAsync(shopName);
            await this.UpdateRemarkListAsync(remark);

            // イベントハンドラを登録する
            this.RegisterEventHandlerToWVM();
        }

        /// <summary>
        /// ウィンドウ終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionListRegistrationWindow_Closed(object sender, EventArgs e)
        {
            this.SaveWindowSetting();
        }
        #endregion

        /// <summary>
        /// 日付金額リスト追加時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            if (this.WVM.DateValueVMList.Count == 0) {
                e.NewItem = new DateValueViewModel() { ActDate = this.selectedDate ?? DateTime.Today, ActValue = null };
            }
            else {
                // リストに入力済の末尾のデータの日付を追加時に採用する
                DateValueViewModel lastVM = this.WVM.DateValueVMList[this.WVM.DateValueVMList.Count - 1];
                e.NewItem = new DateValueViewModel() { ActDate = this.WVM.IsDateAutoIncrement ? lastVM.ActDate.AddDays(1) : lastVM.ActDate, ActValue = null };
            }
        }

        /// <summary>
        /// 日付金額リスト選択変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            if (dataGrid.SelectedIndex != -1) {
                dataGrid.BeginEdit();
            }
        }

        /// <summary>
        /// 金額列セルフォーカス取得時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            DataGridCell dataGridCell = sender as DataGridCell;

            // 新しいセルに移動していたら数値入力ボタンを非表示にする
            if (dataGridCell != this.lastDataGridCell) {
                this.WVM.IsEditing = false;
            }
            this.lastDataGridCell = dataGridCell;
        }

        /// <summary>
        /// テキストボックステキスト入力確定前
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
        /// テキストボックスフォーカス取得時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!this.WVM.IsEditing) {
                TextBox textBox = sender as TextBox;
                this._popup.PlacementTarget = textBox;
                this.WVM.IsEditing = true;

                this.lastDateValueVM = textBox.DataContext as DateValueViewModel;
            }
            e.Handled = true;
        }
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        /// <returns></returns>
        private async Task UpdateBookListAsync(int? bookId = null)
        {
            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            BookViewModel selectedBookVM = null;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT book_id, book_name FROM mst_book WHERE del_flg = 0 ORDER BY sort_order;");
                reader.ExecWholeRow((count, record) => {
                    BookViewModel vm = new BookViewModel() { Id = record.ToInt("book_id"), Name = record["book_name"] };
                    bookVMList.Add(vm);
                    if (selectedBookVM == null || bookId == vm.Id) {
                        selectedBookVM = vm;
                    }
                    return true;
                });
            }

            this.WVM.BookVMList = bookVMList;
            this.WVM.SelectedBookVM = selectedBookVM;
        }

        /// <summary>
        /// 分類リストを更新する
        /// </summary>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <returns></returns>
        private async Task UpdateCategoryListAsync(int? categoryId = null)
        {
            ObservableCollection<CategoryViewModel> categoryVMList = new ObservableCollection<CategoryViewModel>() {
                new CategoryViewModel() { Id = -1, Name = "(指定なし)" }
            };
            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryVM?.Id ?? categoryVMList[0].Id;
            CategoryViewModel selectedCategoryVM = categoryVMList[0];
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT category_id, category_name FROM mst_category C 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM mst_item I WHERE I.category_id = C.category_id AND balance_kind = @{0} AND del_flg = 0 
  AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{1} AND RBI.item_id = I.item_id)) 
ORDER BY sort_order;", (int)this.WVM.SelectedBalanceKind, this.WVM.SelectedBookVM.Id);
                reader.ExecWholeRow((count, record) => {
                    CategoryViewModel vm = new CategoryViewModel() { Id = record.ToInt("category_id"), Name = record["category_name"] };
                    categoryVMList.Add(vm);
                    if (vm.Id == tmpCategoryId) {
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
        /// <param name="itemId">選択対象の項目ID</param>
        /// <returns></returns>
        private async Task UpdateItemListAsync(int? itemId = null)
        {
            if (this.WVM.SelectedCategoryVM == null) return;

            ObservableCollection<ItemViewModel> itemVMList = new ObservableCollection<ItemViewModel>();
            int? tmpItemId = itemId ?? this.WVM.SelectedItemVM?.Id;
            ItemViewModel selectedItemVM = null;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader;
                if (this.WVM.SelectedCategoryVM.Id == -1) {
                    reader = await dao.ExecQueryAsync(@"
SELECT I.item_id, I.item_name, C.category_name
FROM mst_item I
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
WHERE I.del_flg = 0 AND C.balance_kind = @{0} AND
      EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{1} AND RBI.item_id = I.item_id AND del_flg = 0)
ORDER BY C.sort_order, I.sort_order;", (int)this.WVM.SelectedBalanceKind, this.WVM.SelectedBookVM.Id);
                }
                else {
                    reader = await dao.ExecQueryAsync(@"
SELECT I.item_id, I.item_name, C.category_name
FROM mst_item I
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
WHERE I.del_flg = 0 AND C.category_id = @{0} AND
      EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{1} AND RBI.item_id = I.item_id AND del_flg = 0)
ORDER BY I.sort_order;", (int)this.WVM.SelectedCategoryVM.Id, this.WVM.SelectedBookVM.Id);
                }
                reader.ExecWholeRow((count, record) => {
                    ItemViewModel vm = new ItemViewModel() {
                        Id = record.ToInt("item_id"),
                        Name = record["item_name"],
                        CategoryName = this.WVM.SelectedCategoryVM.Id == -1 ? record["category_name"] : ""
                    };
                    itemVMList.Add(vm);
                    if (selectedItemVM == null || vm.Id == tmpItemId) {
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
        /// <returns></returns>
        private async Task UpdateShopListAsync(string shopName = null)
        {
            if (this.WVM.SelectedItemVM == null) return;

            ObservableCollection<ShopViewModel> shopVMList = new ObservableCollection<ShopViewModel>() {
                new ShopViewModel() { Name = string.Empty }
            };
            string selectedShopName = shopName ?? this.WVM.SelectedShopName ?? shopVMList[0].Name;
            ShopViewModel selectedShopVM = shopVMList[0]; // UNUSED
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT S.shop_name, COUNT(A.shop_name) AS shop_count, COALESCE(MAX(A.act_time), '1970-01-01') AS sort_time, COALESCE(MAX(A.act_time), null) AS used_time
FROM hst_shop S
LEFT OUTER JOIN (SELECT * FROM hst_action WHERE del_flg = 0) A ON A.shop_name = S.shop_name AND A.item_id = S.item_id
WHERE S.del_flg = 0 AND S.item_id = @{0}
GROUP BY S.shop_name
ORDER BY sort_time DESC, shop_count DESC;", this.WVM.SelectedItemVM.Id);
                reader.ExecWholeRow((count, record) => {
                    ShopViewModel svm = new ShopViewModel() { Name = record["shop_name"], UsedCount = record.ToInt("shop_count"), UsedTime = record.ToNullableDateTime("used_time") };
                    shopVMList.Add(svm);
                    if (selectedShopVM == null || svm.Name == selectedShopName) {
                        selectedShopVM = svm;
                    }
                    return true;
                });
            }

            this.WVM.ShopVMList = shopVMList;
            this.WVM.SelectedShopName = selectedShopName;
        }

        /// <summary>
        /// 備考リストを更新する
        /// </summary>
        /// <param name="remark">選択対象の備考</param>
        /// <returns></returns>
        private async Task UpdateRemarkListAsync(string remark = null)
        {
            if (this.WVM.SelectedItemVM == null) return;

            ObservableCollection<RemarkViewModel> remarkVMList = new ObservableCollection<RemarkViewModel>() {
                new RemarkViewModel() { Remark = string.Empty }
            };
            string selectedRemark = remark ?? this.WVM.SelectedRemark ?? remarkVMList[0].Remark;
            RemarkViewModel selectedRemarkVM = remarkVMList[0]; // UNUSED
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT R.remark, COUNT(A.remark) AS remark_count, COALESCE(MAX(A.act_time), '1970-01-01') AS sort_time, COALESCE(MAX(A.act_time), null) AS used_time
FROM hst_remark R
LEFT OUTER JOIN (SELECT * FROM hst_action WHERE del_flg = 0) A ON A.remark = R.remark AND A.item_id = R.item_id
WHERE R.del_flg = 0 AND R.item_id = @{0}
GROUP BY R.remark
ORDER BY sort_time DESC, remark_count DESC;", this.WVM.SelectedItemVM.Id);
                reader.ExecWholeRow((count, record) => {
                    RemarkViewModel rvm = new RemarkViewModel() { Remark = record["remark"], UsedCount = record.ToInt("remark_count"), UsedTime = record.ToNullableDateTime("used_time") };
                    remarkVMList.Add(rvm);
                    if (selectedRemarkVM == null || rvm.Remark == selectedRemark) {
                        selectedRemarkVM = rvm;
                    }
                    return true;
                });
            }

            this.WVM.RemarkVMList = remarkVMList;
            this.WVM.SelectedRemark = selectedRemark;
        }
        #endregion

        /// <summary>
        /// イベントハンドラをWVMに登録する
        /// </summary>
        private void RegisterEventHandlerToWVM()
        {
            this.WVM.BookChanged += async (bookId) => {
                await this.UpdateCategoryListAsync();
                await this.UpdateItemListAsync();
                await this.UpdateShopListAsync();
                await this.UpdateRemarkListAsync();
                this.BookChanged?.Invoke(this, new EventArgs<int?>(bookId));
            };
            this.WVM.BalanceKindChanged += async (_) => {
                await this.UpdateCategoryListAsync();
                await this.UpdateItemListAsync();
                await this.UpdateShopListAsync();
                await this.UpdateRemarkListAsync();
            };
            this.WVM.CategoryChanged += async (_) => {
                await this.UpdateItemListAsync();
                await this.UpdateShopListAsync();
                await this.UpdateRemarkListAsync();
            };
            this.WVM.ItemChanged += async (_) => {
                await this.UpdateShopListAsync();
                await this.UpdateRemarkListAsync();
            };
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目IDリスト</returns>
        private async Task<List<int>> RegisterToDbAsync()
        {
            List<int> tmpActionIdList = new List<int>();
            BalanceKind balanceKind = this.WVM.SelectedBalanceKind; // 収支種別
            int bookId = this.WVM.SelectedBookVM.Id.Value; // 帳簿ID
            int itemId = this.WVM.SelectedItemVM.Id; // 帳簿項目ID
            string shopName = this.WVM.SelectedShopName; // 店舗名
            string remark = this.WVM.SelectedRemark; // 備考

            DateTime lastActTime = this.WVM.DateValueVMList.Max((tmp) => tmp.ActDate);
            using (DaoBase dao = this.builder.Build()) {
                switch (this.WVM.RegMode) {
                    case RegistrationMode.Add: {
                        #region 帳簿項目を追加する
                        await dao.ExecTransactionAsync(async () => {
                            int tmpGroupId = -1;
                            // グループIDを取得する
                            DaoReader reader = await dao.ExecQueryAsync(@"
INSERT INTO hst_group (group_kind, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, 0, 'now', @{1}, 'now', @{2}) RETURNING group_id;", (int)GroupKind.ListReg, Updater, Inserter);
                            reader.ExecARow((record) => {
                                tmpGroupId = record.ToInt("group_id");
                            });

                            foreach (DateValueViewModel vm in this.WVM.DateValueVMList) {
                                if (vm.ActValue.HasValue) {
                                    DateTime actTime = vm.ActDate; // 日付
                                    int actValue = (balanceKind == BalanceKind.Income ? 1 : -1) * vm.ActValue.Value; // 金額
                                    reader = await dao.ExecQueryAsync(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, shop_name, group_id, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, @{6}, 0, 'now', @{7}, 'now', @{8}) RETURNING action_id;",
                                        bookId, itemId, actTime, actValue, shopName, tmpGroupId, remark, Updater, Inserter);

                                    reader.ExecARow((record) => {
                                        tmpActionIdList.Add(record.ToInt("action_id"));
                                    });
                                }
                            }
                        });
                        #endregion
                        break;
                    }
                    case RegistrationMode.Edit: {
                        #region 帳簿項目を編集する
                        await dao.ExecTransactionAsync(async () => {
                            foreach (DateValueViewModel vm in this.WVM.DateValueVMList) {
                                if (vm.ActValue.HasValue) {
                                    int? actionId = vm.ActionId;
                                    DateTime actTime = vm.ActDate; // 日付
                                    int actValue = (balanceKind == BalanceKind.Income ? 1 : -1) * vm.ActValue.Value; // 金額

                                    if (actionId.HasValue) {
                                        await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET book_id = @{0}, item_id = @{1}, act_time = @{2}, act_value = @{3}, shop_name = @{4}, remark = @{5}, update_time = 'now', updater = @{6}
WHERE action_id = @{7} AND NOT (book_id = @{0} AND item_id = @{1} AND act_time = @{2} AND act_value = @{3} AND shop_name = @{4} AND remark = @{5});",
                                            bookId, itemId, actTime, actValue, shopName, remark, Updater, actionId.Value);


                                        tmpActionIdList.Add(actionId.Value);
                                    }
                                    else {
                                        DaoReader reader = await dao.ExecQueryAsync(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, shop_name, group_id, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, @{6}, 0, 'now', @{7}, 'now', @{8}) RETURNING action_id;",
                                            bookId, itemId, actTime, actValue, shopName, this.selectedGroupId, remark, Updater, Inserter);

                                        reader.ExecARow((record) => {
                                            tmpActionIdList.Add(record.ToInt("action_id"));
                                        });
                                    }
                                }
                            }

                            IEnumerable<int> expected = this.groupedActionIdList.Except(tmpActionIdList);
                            foreach (int actionId in expected) {
                                await dao.ExecNonQueryAsync(@"
UPDATE hst_action SET del_flg = 1, update_time = 'now', updater = @{1} 
WHERE action_id = @{0};", actionId, Updater);
                            }
                        });
                        #endregion
                        break;
                    }
                }


                if (shopName != string.Empty) {
                    #region 店舗を追加する
                    await dao.ExecTransactionAsync(async () => {
                        DaoReader reader = await dao.ExecQueryAsync(@"
SELECT shop_name FROM hst_shop
WHERE item_id = @{0} AND shop_name = @{1};", itemId, shopName);

                        if (reader.Count == 0) {
                            await dao.ExecNonQueryAsync(@"
INSERT INTO hst_shop (item_id, shop_name, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, 0, 'now', @{3}, 'now', @{4});", itemId, shopName, lastActTime, Updater, Inserter);
                        }
                        else {
                            await dao.ExecNonQueryAsync(@"
UPDATE hst_shop
SET used_time = @{0}, del_flg = 0, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND shop_name = @{3} AND used_time < @{0};", lastActTime, Updater, itemId, shopName);
                        }
                    });
                    #endregion
                }

                if (remark != string.Empty) {
                    #region 備考を追加する
                    await dao.ExecTransactionAsync(async () => {
                        DaoReader reader = await dao.ExecQueryAsync(@"
SELECT remark FROM hst_remark
WHERE item_id = @{0} AND remark = @{1};", itemId, remark);

                        if (reader.Count == 0) {
                            await dao.ExecNonQueryAsync(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, 0, @{2}, 0, 'now', @{3}, 'now', @{4});", itemId, remark, lastActTime, Updater, Inserter);
                        }
                        else {
                            await dao.ExecNonQueryAsync(@"
UPDATE hst_remark
SET used_time = @{0}, del_flg = 0, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND remark = @{3} AND used_time < @{0};", lastActTime, Updater, itemId, remark);
                        }
                    });
                    #endregion
                }
            }

            return tmpActionIdList;
        }

        #region 設定反映用の関数
        /// <summary>
        /// ウィンドウ設定を読み込む
        /// </summary>
        public void LoadWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.ActionListRegistrationWindow_Width != -1 && settings.ActionListRegistrationWindow_Height != -1) {
                this.Width = settings.ActionListRegistrationWindow_Width;
                this.Height = settings.ActionListRegistrationWindow_Height;
            }

            if (settings.App_IsPositionSaved && (-10 <= settings.ActionListRegistrationWindow_Left && 0 <= settings.ActionListRegistrationWindow_Top)) {
                this.Left = settings.ActionListRegistrationWindow_Left;
                this.Top = settings.ActionListRegistrationWindow_Top;
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
                    settings.ActionListRegistrationWindow_Left = this.Left;
                    settings.ActionListRegistrationWindow_Top = this.Top;
                }
                settings.ActionListRegistrationWindow_Width = this.Width;
                settings.ActionListRegistrationWindow_Height = this.Height;
                settings.Save();
            }
        }
        #endregion
    }
}
