using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto;
using HouseholdAccountBook.Dto.DbTable;
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
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;
using static HouseholdAccountBook.Extensions.FrameworkElementExtensions;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// ActionListRegistrationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ActionListRegistrationWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory dbHandlerFactory;
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
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedMonth">選択された年月</param>
        /// <param name="selectedDate">選択された日付</param>
        public ActionListRegistrationWindow(DbHandlerFactory dbHandlerFactory, int? selectedBookId, DateTime? selectedMonth, DateTime? selectedDate = null)
        {
            this.dbHandlerFactory = dbHandlerFactory;
            this.selectedBookId = selectedBookId;
            this.selectedMonth = selectedMonth;
            this.selectedDate = selectedDate;
            this.selectedGroupId = null;

            this.InitializeComponent();

            this.WVM.RegMode = RegistrationKind.Add;
        }

        /// <summary>
        /// 複数の帳簿項目の新規登録のために <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedRecordList">選択されたCSVレコードリスト</param>
        public ActionListRegistrationWindow(DbHandlerFactory dbHandlerFactory, int selectedBookId, List<CsvComparisonViewModel.CsvRecord> selectedRecordList)
        {
            this.dbHandlerFactory = dbHandlerFactory;
            this.selectedBookId = selectedBookId;
            this.selectedRecordList = selectedRecordList;
            this.selectedMonth = null;
            this.selectedDate = null;
            this.selectedGroupId = null;

            this.InitializeComponent();

            this.WVM.RegMode = RegistrationKind.Add;
        }

        /// <summary>
        /// 複数の帳簿項目の編集(複製)のために <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedGroupId">選択されたグループID</param>
        /// <param name="mode">登録種別</param>
        public ActionListRegistrationWindow(DbHandlerFactory dbHandlerFactory, int selectedGroupId, RegistrationKind mode = RegistrationKind.Edit)
        {
            this.dbHandlerFactory = dbHandlerFactory;
            this.selectedBookId = null;
            this.selectedMonth = null;
            this.selectedDate = null;
            switch (mode) {
                case RegistrationKind.Edit:
                case RegistrationKind.Copy:
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
            List<int> idList = null;
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                idList = await this.RegisterToDbAsync();
            }

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
                        textBox.Text = string.Format($"{vm.ActValue}");
                        textBox.SelectionStart = 1;
                    }
                    else {
                        // 選択された位置に値を挿入する
                        int selectionStart = textBox.SelectionStart;
                        int selectionEnd = selectionStart + textBox.SelectionLength;
                        string forwardText = textBox.Text.Substring(0, selectionStart);
                        string backwardText = textBox.Text.Substring(selectionEnd, textBox.Text.Length - selectionEnd);

                        if (int.TryParse(string.Format($"{forwardText}{value}{backwardText}"), out int outValue)) {
                            vm.ActValue = outValue;
                            textBox.Text = string.Format($"{vm.ActValue}");
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
                            if (int.TryParse(string.Format($"{forwardText}{backwardText}"), out int outValue)) {
                                vm.ActValue = outValue;
                                textBox.Text = string.Format($"{outValue}");
                                textBox.SelectionStart = selectionStart;
                            }
                        }
                        else if (selectionStart != 0) {
                            string newText = string.Format($"{forwardText.Substring(0, selectionStart - 1)}{backwardText}");
                            if (string.Empty == newText || int.TryParse(newText, out int outValue)) {
                                vm.ActValue = string.Empty == newText ? (int?)null : int.Parse(newText);
                                textBox.Text = string.Format($"{vm.ActValue}");
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
            BalanceKind balanceKind = BalanceKind.Expenses;

            int? itemId = null;
            string shopName = null;
            string remark = null;

            // DBから値を読み込む
            switch (this.WVM.RegMode) {
                case RegistrationKind.Add: {
                    bookId = this.selectedBookId;
                    balanceKind = BalanceKind.Expenses;
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
                case RegistrationKind.Edit:
                case RegistrationKind.Copy: {
                    using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                        var dtoList = await dbHandler.QueryAsync<HstActionDto>(@"
SELECT book_id, item_id, action_id, act_time, act_value, shop_name, remark
FROM hst_action 
WHERE del_flg = 0 AND group_id = @GroupId;",
new HstActionDto { GroupId = this.selectedGroupId });
                        foreach (HstActionDto dto in dtoList) {
                            // 日付金額リストに追加
                            DateValueViewModel vm = new DateValueViewModel() {
                                ActionId = dto.ActionId,
                                ActDate = dto.ActTime,
                                ActValue = Math.Abs(dto.ActValue)
                            };

                            bookId = dto.BookId;
                            itemId = dto.ItemId;
                            shopName = dto.ShopName;
                            remark = dto.Remark;

                            this.groupedActionIdList.Add(vm.ActionId.Value);
                            this.WVM.DateValueVMList.Add(vm);

                            balanceKind = Math.Sign(dto.ActValue) > 0 ? BalanceKind.Income : BalanceKind.Expenses; // 収入 / 支出

                            this.groupedActionIdList.Add(vm.ActionId.Value);
                            this.WVM.DateValueVMList.Add(vm);
                        }
                    }
                }
                break;
            }

            // WVMに値を設定する
            this.WVM.GroupId = this.WVM.RegMode == RegistrationKind.Edit ? this.selectedGroupId : null;
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
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                var dtoList = await dbHandler.QueryAsync<MstBookDto>(@"
SELECT book_id, book_name FROM mst_book WHERE del_flg = 0 ORDER BY sort_order;");
                foreach (MstBookDto dto in dtoList) {
                    BookViewModel vm = new BookViewModel() { Id = dto.BookId, Name = dto.BookName };
                    bookVMList.Add(vm);
                    if (selectedBookVM == null || bookId == vm.Id) {
                        selectedBookVM = vm;
                    }
                }
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
                new CategoryViewModel() { Id = -1, Name = Properties.Resources.ListName_NoSpecification }
            };
            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryVM?.Id ?? categoryVMList[0].Id;
            CategoryViewModel selectedCategoryVM = categoryVMList[0];
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                var dtoList = await dbHandler.QueryAsync<MstCategoryDto>(@"
SELECT category_id, category_name
FROM mst_category C 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM mst_item I WHERE I.category_id = C.category_id AND balance_kind = @BalanceKind AND del_flg = 0 
  AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @BookId AND RBI.item_id = I.item_id)) 
ORDER BY sort_order;",
new { BalanceKind = (int)this.WVM.SelectedBalanceKind, BookId = this.WVM.SelectedBookVM.Id });
                foreach (MstCategoryDto dto in dtoList) {
                    CategoryViewModel vm = new CategoryViewModel() { Id = dto.CategoryId, Name = dto.CategoryName };
                    categoryVMList.Add(vm);
                    if (vm.Id == tmpCategoryId) {
                        selectedCategoryVM = vm;
                    }
                }
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
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                IEnumerable<CategoryItemInfoDto> dtoList;
                if (this.WVM.SelectedCategoryVM.Id == -1) {
                    dtoList = await dbHandler.QueryAsync<CategoryItemInfoDto>(@"
SELECT I.item_id, I.item_name, C.category_name
FROM mst_item I
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
WHERE I.del_flg = 0 AND C.balance_kind = @BalanceKind AND
      EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @BookId AND RBI.item_id = I.item_id AND del_flg = 0)
ORDER BY C.sort_order, I.sort_order;",
new { BalanceKind = (int)this.WVM.SelectedBalanceKind, BookId = this.WVM.SelectedBookVM.Id });
                }
                else {
                    dtoList = await dbHandler.QueryAsync<CategoryItemInfoDto>(@"
SELECT I.item_id, I.item_name, C.category_name
FROM mst_item I
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
WHERE I.del_flg = 0 AND C.category_id = @CategoryId AND
      EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @BookId AND RBI.item_id = I.item_id AND del_flg = 0)
ORDER BY I.sort_order;",
new { CategoryId = (int)this.WVM.SelectedCategoryVM.Id, BookId = this.WVM.SelectedBookVM.Id });
                }
                foreach (CategoryItemInfoDto dto in dtoList) {
                    ItemViewModel vm = new ItemViewModel() {
                        Id = dto.ItemId,
                        Name = dto.ItemName,
                        CategoryName = this.WVM.SelectedCategoryVM.Id == -1 ? dto.CategoryName : ""
                    };
                    itemVMList.Add(vm);
                    if (selectedItemVM == null || vm.Id == tmpItemId) {
                        selectedItemVM = vm;
                    }
                }
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
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                var dtoList = await dbHandler.QueryAsync<ShopInfoDto>(@"
SELECT S.shop_name, COUNT(A.shop_name) AS count, COALESCE(MAX(A.act_time), '1970-01-01') AS sort_time, COALESCE(MAX(A.act_time), null) AS used_time
FROM hst_shop S
LEFT OUTER JOIN (SELECT * FROM hst_action WHERE del_flg = 0) A ON A.shop_name = S.shop_name AND A.item_id = S.item_id
WHERE S.del_flg = 0 AND S.item_id = @ItemId
GROUP BY S.shop_name
ORDER BY sort_time DESC, count DESC;",
new { ItemId = this.WVM.SelectedItemVM.Id });
                foreach (ShopInfoDto dto in dtoList) {
                    ShopViewModel svm = new ShopViewModel() {
                        Name = dto.ShopName,
                        UsedCount = dto.Count,
                        UsedTime = dto.UsedTime
                    };
                    shopVMList.Add(svm);
                    if (selectedShopVM == null || svm.Name == selectedShopName) {
                        selectedShopVM = svm;
                    }
                }
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
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                var dtoList = await dbHandler.QueryAsync<RemarkInfoDto>(@"
SELECT R.remark, COUNT(A.remark) AS count, COALESCE(MAX(A.act_time), '1970-01-01') AS sort_time, COALESCE(MAX(A.act_time), null) AS used_time
FROM hst_remark R
LEFT OUTER JOIN (SELECT * FROM hst_action WHERE del_flg = 0) A ON A.remark = R.remark AND A.item_id = R.item_id
WHERE R.del_flg = 0 AND R.item_id = @ItemId
GROUP BY R.remark
ORDER BY sort_time DESC, count DESC;",
new { ItemId = this.WVM.SelectedItemVM.Id });
                foreach (RemarkInfoDto dto in dtoList) {
                    RemarkViewModel rvm = new RemarkViewModel() {
                        Remark = dto.Remark,
                        UsedCount = dto.Count,
                        UsedTime = dto.UsedTime
                    };
                    remarkVMList.Add(rvm);
                    if (selectedRemarkVM == null || rvm.Remark == selectedRemark) {
                        selectedRemarkVM = rvm;
                    }
                }
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
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    await this.UpdateCategoryListAsync();
                    await this.UpdateItemListAsync();
                    await this.UpdateShopListAsync();
                    await this.UpdateRemarkListAsync();
                }
                this.BookChanged?.Invoke(this, new EventArgs<int?>(bookId));
            };
            this.WVM.BalanceKindChanged += async (_) => {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    await this.UpdateCategoryListAsync();
                    await this.UpdateItemListAsync();
                    await this.UpdateShopListAsync();
                    await this.UpdateRemarkListAsync();
                }
            };
            this.WVM.CategoryChanged += async (_) => {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    await this.UpdateItemListAsync();
                    await this.UpdateShopListAsync();
                    await this.UpdateRemarkListAsync();
                }
            };
            this.WVM.ItemChanged += async (_) => {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    await this.UpdateShopListAsync();
                    await this.UpdateRemarkListAsync();
                }
            };
        }

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目IDリスト</returns>
        private async Task<List<int>> RegisterToDbAsync()
        {
            List<int> resActionIdList = new List<int>();

            BalanceKind balanceKind = this.WVM.SelectedBalanceKind; // 収支種別
            int bookId = this.WVM.SelectedBookVM.Id.Value;          // 帳簿ID
            int itemId = this.WVM.SelectedItemVM.Id;                // 帳簿項目ID
            string shopName = this.WVM.SelectedShopName;            // 店舗名
            string remark = this.WVM.SelectedRemark;                // 備考

            DateTime lastActTime = this.WVM.DateValueVMList.Max((tmp) => tmp.ActDate);
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                switch (this.WVM.RegMode) {
                    case RegistrationKind.Add: {
                        #region 帳簿項目を追加する
                        await dbHandler.ExecTransactionAsync(async () => {
                            int tmpGroupId = -1;
                            // グループIDを取得する
                            ReturningDto dto = await dbHandler.QuerySingleAsync<ReturningDto>(@"
INSERT INTO hst_group (group_kind, del_flg, update_time, updater, insert_time, inserter)
VALUES (@GroupKind, 0, 'now', @Updater, 'now', @Inserter)
RETURNING group_id AS Id;",
new HstGroupDto { GroupKind = (int)GroupKind.ListReg });
                            tmpGroupId = dto.Id;

                            foreach (DateValueViewModel vm in this.WVM.DateValueVMList) {
                                if (vm.ActValue.HasValue) {
                                    DateTime actTime = vm.ActDate; // 日付
                                    int actValue = (balanceKind == BalanceKind.Income ? 1 : -1) * vm.ActValue.Value; // 金額
                                    dto = await dbHandler.QuerySingleAsync<ReturningDto>(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, shop_name, group_id, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@BookId, @ItemId, @ActTime, @ActValue, @ShopName, @GroupId, @Remark, 0, 'now', @Updater, 'now', @Inserter)
RETURNING action_id AS Id;",
new HstActionDto { BookId = bookId, ItemId = itemId, ActTime = actTime, ActValue = actValue, ShopName = shopName, GroupId = tmpGroupId, Remark = remark });
                                    resActionIdList.Add(dto.Id);
                                }
                            }
                        });
                        #endregion
                        break;
                    }
                    case RegistrationKind.Edit: {
                        #region 帳簿項目を編集する
                        await dbHandler.ExecTransactionAsync(async () => {
                            foreach (DateValueViewModel vm in this.WVM.DateValueVMList) {
                                if (vm.ActValue.HasValue) {
                                    int? actionId = vm.ActionId;
                                    DateTime actTime = vm.ActDate; // 日付
                                    int actValue = (balanceKind == BalanceKind.Income ? 1 : -1) * vm.ActValue.Value; // 金額

                                    if (actionId.HasValue) {
                                        await dbHandler.ExecuteAsync(@"
UPDATE hst_action
SET book_id = @BookId, item_id = @ItemId, act_time = @ActTime, act_value = @ActValue, shop_name = @ShopName, remark = @Remark, update_time = 'now', updater = @Updater
WHERE action_id = @ActionId AND NOT (book_id = @BookId AND item_id = @ItemId AND act_time = @ActTime AND act_value = @ActValue AND shop_name = @ShopName AND remark = @Remark);",
new HstActionDto { BookId = bookId, ItemId = itemId, ActTime = actTime, ActValue = actValue, ShopName = shopName, Remark = remark, ActionId = actionId.Value });

                                        resActionIdList.Add(actionId.Value);
                                    }
                                    else {
                                        ReturningDto dto = await dbHandler.QuerySingleAsync<ReturningDto>(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, shop_name, group_id, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@BookId, @ItemId, @ActTime, @ActValue, @ShopName, @GroupId, @Remark, 0, 'now', @Updater, 'now', @Inserter)
RETURNING action_id AS Id;",
new HstActionDto { BookId = bookId, ItemId = itemId, ActTime = actTime, ActValue = actValue, ShopName = shopName, GroupId = this.selectedGroupId, Remark = remark });
                                        resActionIdList.Add(dto.Id);
                                    }
                                }
                            }

                            IEnumerable<int> expected = this.groupedActionIdList.Except(resActionIdList);
                            foreach (int actionId in expected) {
                                await dbHandler.ExecuteAsync(@"
UPDATE hst_action SET del_flg = 1, update_time = 'now', updater = @Updater 
WHERE action_id = @ActionId;",
new HstActionDto { ActionId = actionId } );
                            }
                        });
                        #endregion
                        break;
                    }
                }


                if (shopName != string.Empty) {
                    #region 店舗を追加する
                    await dbHandler.ExecTransactionAsync(async () => {
                        var dtoList = await dbHandler.QueryAsync<HstShopDto>(@"
SELECT * FROM hst_shop
WHERE item_id = @ItemId AND shop_name = @ShopName;",
new HstShopDto { ItemId = itemId, ShopName = shopName });

                        if (dtoList.Count() == 0) {
                            await dbHandler.ExecuteAsync(@"
INSERT INTO hst_shop (item_id, shop_name, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemId, @ShopName, @UsedTime, 0, 'now', @Updater, 'now', @Inserter);",
new HstShopDto { ItemId = itemId, ShopName = shopName, UsedTime = lastActTime });
                        }
                        else {
                            await dbHandler.ExecuteAsync(@"
UPDATE hst_shop
SET used_time = @UsedTime, del_flg = 0, update_time = 'now', updater = @Updater
WHERE item_id = @ItemId AND shop_name = @ShopName AND used_time < @UsedTime;",
new HstShopDto { UsedTime = lastActTime, ItemId = itemId, ShopName = shopName });
                        }
                    });
                    #endregion
                }

                if (remark != string.Empty) {
                    #region 備考を追加する
                    await dbHandler.ExecTransactionAsync(async () => {
                        var dtoList = await dbHandler.QueryAsync<HstRemarkDto>(@"
SELECT remark FROM hst_remark
WHERE item_id = @ItemId AND remark = @Remark;",
new HstRemarkDto { ItemId = itemId, Remark = remark });

                        if (dtoList.Count() == 0) {
                            await dbHandler.ExecuteAsync(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemId, @Remark, 0, @UsedTime, 0, 'now', @Updater, 'now', @Inserter);", 
new HstRemarkDto { ItemId = itemId, Remark = remark, UsedTime = lastActTime });
                        }
                        else {
                            await dbHandler.ExecuteAsync(@"
UPDATE hst_remark
SET used_time = @UsedTime, del_flg = 0, update_time = 'now', updater = @Updater
WHERE item_id = @ItemId AND remark = @Remark AND used_time < @UsedTime;", 
new HstRemarkDto { UsedTime = lastActTime, ItemId = itemId, Remark = remark });
                        }
                    });
                    #endregion
                }
            }

            return resActionIdList;
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
