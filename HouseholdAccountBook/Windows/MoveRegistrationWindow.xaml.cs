using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Extentions;
using HouseholdAccountBook.UserEventArgs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// MoveRegistrationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MoveRegistrationWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DAOビルダ
        /// </summary>
        private DaoBuilder builder;
        /// <summary>
        /// 選択されていた帳簿ID
        /// </summary>
        private int? selectedBookId = null;
        /// <summary>
        /// 移動元帳簿項目ID
        /// </summary>
        private int? fromActionId = null;
        /// <summary>
        /// 移動先帳簿項目ID
        /// </summary>
        private int? toActionId = null;
        /// <summary>
        /// 手数料帳簿項目ID
        /// </summary>
        private int? commissionActionId = null;
        /// <summary>
        /// グループID
        /// </summary>
        private int? groupId = null;
        #endregion

        /// <summary>
        /// 登録時のイベント
        /// </summary>
        public event EventHandler<RegistrateEventArgs> Registrated = null;

        /// <summary>
        /// 帳簿項目追加ウィンドウ(移動)
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="selectedBookId">帳簿ID</param>
        /// <param name="selectedDateTime">選択された日時</param>
        public MoveRegistrationWindow(DaoBuilder builder, int? selectedBookId, DateTime? selectedDateTime)
        {
            InitializeComponent();
            Title = "追加";

            this.builder = builder;
            this.selectedBookId = selectedBookId;
            this.fromActionId = null;
            this.toActionId = null;
            this.commissionActionId = null;
            this.groupId = null;

            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            BookViewModel selectedBookVM = null;
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery("SELECT book_id, book_name FROM mst_book WHERE del_flg = 0;");
                reader.ExecWholeRow((count, record) => {
                    BookViewModel vm = new BookViewModel() { BookId = record.ToInt("book_id"), BookName = record["book_name"] };
                    bookVMList.Add(vm);
                    if (selectedBookVM == null || selectedBookId == vm.BookId) {
                        selectedBookVM = vm;
                    }
                });
            }

            this.MoveRegistrationWindowVM.SelectedDate = selectedDateTime != null ? selectedDateTime.Value : DateTime.Today;
            this.MoveRegistrationWindowVM.BookVMList = bookVMList;
            this.MoveRegistrationWindowVM.SelectedFromBookVM = selectedBookVM;
            this.MoveRegistrationWindowVM.SelectedToBookVM = selectedBookVM;
            this.MoveRegistrationWindowVM.SelectedCommissionKindVM = this.MoveRegistrationWindowVM.CommissionKindVMList[0];

            UpdateItemList();
            UpdateRemarkList();

            LoadSetting();

            #region イベントハンドラの設定
            this.MoveRegistrationWindowVM.OnFromBookChanged += () => {
                UpdateItemList();
                UpdateRemarkList();
            };
            this.MoveRegistrationWindowVM.OnToBookChanged += () => {
                UpdateItemList();
                UpdateRemarkList();
            };
            this.MoveRegistrationWindowVM.OnCommissionKindChanged += () => {
                UpdateItemList();
                UpdateRemarkList();
            };
            this.MoveRegistrationWindowVM.OnItemChanged += () => {
                UpdateRemarkList();
            };
            #endregion
        }

        /// <summary>
        /// 帳簿項目編集ウィンドウ(移動)
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        /// <param name="selectedBookId">帳簿ID</param>
        /// <param name="groupId">グループID</param>
        public MoveRegistrationWindow(DaoBuilder builder, int? selectedBookId, int groupId)
        {
            InitializeComponent();
            Title = "編集";

            this.builder = builder;
            this.selectedBookId = selectedBookId;
            this.groupId = groupId;

            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>();
            int fromBookId = -1;
            BookViewModel selectedFromBookVM = null;
            int toBookId = -1;
            BookViewModel selectedToBookVM = null;
            DateTime actDate = DateTime.Now;
            int moveValue = -1;
            int commissionKindIndex = 0;
            int? commissionItemId = null;
            int commissionValue = 0;
            string commissionRemark = null;
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT A.book_id, A.action_id, A.item_id, A.act_time, A.act_value, A.remark, I.move_flg
FROM hst_action A
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = A.item_id
WHERE A.del_flg = 0 AND A.group_id = @{0}
ORDER BY move_flg DESC;", groupId);
                reader.ExecWholeRow((count, record) => {
                    int bookId = record.ToInt("book_id");
                    int actionId = record.ToInt("action_id");
                    int itemId = record.ToInt("item_id");
                    actDate = DateTime.Parse(record["act_time"]);
                    int actValue = record.ToInt("act_value");
                    int moveFlg = record.ToInt("move_flg");
                    string remark = record["remark"];
                    
                    if (moveFlg == 1) {
                        if(actValue < 0) {
                            fromBookId = bookId;
                            fromActionId = actionId;
                        }
                        else {
                            toBookId = bookId;
                            toActionId = actionId;
                            moveValue = actValue;
                        }
                    }
                    else {
                        if (bookId == fromBookId) { // 移動元負担
                            commissionKindIndex = 0;
                        }
                        else if (bookId == toBookId) { // 移動先負担
                            commissionKindIndex = 1;
                        }
                        commissionActionId = actionId;
                        commissionItemId = itemId;
                        commissionValue = Math.Abs(actValue);
                        commissionRemark = remark;
                    }
                });

                reader = dao.ExecQuery(@"
SELECT book_id, book_name FROM mst_book WHERE del_flg = 0;");
                reader.ExecWholeRow((count, record) => {
                    BookViewModel vm = new BookViewModel() { BookId = record.ToInt("book_id"), BookName = record["book_name"] };
                    bookVMList.Add(vm);
                    if (selectedFromBookVM == null || fromBookId == vm.BookId) {
                        selectedFromBookVM = vm;
                    }
                    if (selectedToBookVM == null || toBookId == vm.BookId) {
                        selectedToBookVM = vm;
                    }
                });
            }
            this.MoveRegistrationWindowVM.SelectedDate = actDate;
            this.MoveRegistrationWindowVM.BookVMList = bookVMList;
            this.MoveRegistrationWindowVM.SelectedFromBookVM = selectedFromBookVM;
            this.MoveRegistrationWindowVM.SelectedToBookVM = selectedToBookVM;
            this.MoveRegistrationWindowVM.Value = moveValue;
            this.MoveRegistrationWindowVM.SelectedCommissionKindVM = this.MoveRegistrationWindowVM.CommissionKindVMList[commissionKindIndex];
            this.MoveRegistrationWindowVM.Commission = commissionValue;

            UpdateItemList(commissionItemId);
            UpdateRemarkList(commissionRemark);

            LoadSetting();

            #region イベントハンドラの設定
            this.MoveRegistrationWindowVM.OnFromBookChanged += () => {
                UpdateItemList();
                UpdateRemarkList();
            };
            this.MoveRegistrationWindowVM.OnToBookChanged += () => {
                UpdateItemList();
                UpdateRemarkList();
            };
            this.MoveRegistrationWindowVM.OnCommissionKindChanged += () => {
                UpdateItemList();
                UpdateRemarkList();
            };
            this.MoveRegistrationWindowVM.OnItemChanged += () => {
                UpdateRemarkList();
            };
            #endregion
        }

        #region コマンド
        /// <summary>
        /// 登録コマンド判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.MoveRegistrationWindowVM.Value.HasValue;
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

            Registrated?.Invoke(this, new RegistrateEventArgs(id));

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
        #endregion

        #region イベントハンドラ
        /// <summary>
        /// 読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveRegistrationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 処理なし
        }

        /// <summary>
        /// フォーム終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveRegistrationWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveSetting();
        }

        /// <summary>
        /// キー押下時の処理(Escキーでフォームを閉じる)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveRegistrationWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) {
                case Key.Enter:
                    if (!this.MoveRegistrationWindowVM.Value.HasValue) break;

                    // DB登録
                    RegisterToDb();
                    
                    this.DialogResult = true;
                    this.Close();
                    break;
                case Key.Escape:
                    this.DialogResult = false;
                    this.Close();
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// 手数料項目リストを更新する
        /// </summary>
        /// <param name="itemId">選択対象の項目</param>
        private void UpdateItemList(int? itemId = null)
        {
            ObservableCollection<ItemViewModel> itemVMList = new ObservableCollection<ItemViewModel>();
            ItemViewModel selectedItemVM = null;
            using (DaoBase dao = builder.Build()) {
                int bookId = -1;
                switch (this.MoveRegistrationWindowVM.SelectedCommissionKindVM.CommissionKind) {
                    case CommissionKind.FromBook:
                        bookId = this.MoveRegistrationWindowVM.SelectedFromBookVM.BookId.Value;
                        break;
                    case CommissionKind.ToBook:
                        bookId = this.MoveRegistrationWindowVM.SelectedToBookVM.BookId.Value;
                        break;
                }
                DaoReader reader = dao.ExecQuery(@"
SELECT item_id, item_name FROM mst_item I 
WHERE del_flg = 0 AND EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @{0} AND RBI.item_id = I.item_id AND del_flg = 0)
  AND EXISTS (SELECT * FROM mst_category C WHERE C.category_id = I.category_id AND balance_kind = @{1} AND del_flg = 0)
ORDER BY sort_order;", bookId, (int)BalanceKind.Outgo);

                reader.ExecWholeRow((count, record) => {
                    ItemViewModel vm = new ItemViewModel() { ItemId = record.ToInt("item_id"), ItemName = record["item_name"] };
                    itemVMList.Add(vm);
                    if (selectedItemVM == null || vm.ItemId == itemId) {
                        selectedItemVM = vm;
                    }
                });
            }
            this.MoveRegistrationWindowVM.ItemVMList = itemVMList;
            this.MoveRegistrationWindowVM.SelectedItemVM = selectedItemVM;
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
            string selectedRemark = remark ?? this.MoveRegistrationWindowVM.SelectedRemark ?? remarkVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT remark FROM hst_remark 
WHERE del_flg = 0 AND item_id = @{0} 
ORDER BY used_time DESC;", this.MoveRegistrationWindowVM.SelectedItemVM.ItemId);
                reader.ExecWholeRow((count, record) => {
                    string tmp = record["remark"];
                    remarkVMList.Add(tmp);
                });
            }

            this.MoveRegistrationWindowVM.RemarkList = remarkVMList;
            this.MoveRegistrationWindowVM.SelectedRemark = selectedRemark;
        }
        #endregion

        /// <summary>
        /// DBに登録する
        /// </summary>
        /// <returns>登録された帳簿項目ID</returns>
        private int? RegisterToDb()
        {
            if(this.MoveRegistrationWindowVM.SelectedFromBookVM == this.MoveRegistrationWindowVM.SelectedToBookVM) {
                MessageBox.Show(this, Message.IllegalSameBook, MessageTitle.Exclamation, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return null;
            }
            if(!this.MoveRegistrationWindowVM.Value.HasValue && this.MoveRegistrationWindowVM.Value <= 0) {
                MessageBox.Show(this, Message.IllegalValue, MessageTitle.Exclamation, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return null;
            }

            DateTime actTime = this.MoveRegistrationWindowVM.SelectedDate;
            int fromBookId = this.MoveRegistrationWindowVM.SelectedFromBookVM.BookId.Value;
            int toBookId = this.MoveRegistrationWindowVM.SelectedToBookVM.BookId.Value;
            int actValue = this.MoveRegistrationWindowVM.Value.Value;
            CommissionKind commissionKind = this.MoveRegistrationWindowVM.SelectedCommissionKindVM.CommissionKind;
            int commissionItemId = this.MoveRegistrationWindowVM.SelectedItemVM.ItemId;
            int commission = (int)this.MoveRegistrationWindowVM.Commission;
            string remark = this.MoveRegistrationWindowVM.SelectedRemark;

            int? resActionId = null;

            using (DaoBase dao = builder.Build()) {
                dao.ExecTransaction(() => {
                    if (groupId == null) { // 追加
                        int tmpGroupId = -1;
                        // グループIDを取得する
                        DaoReader reader = dao.ExecQuery(@"
INSERT INTO hst_group (group_kind, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, 0, 'now', @{1}, 'now', @{2}) RETURNING group_id;", (int)GroupKind.Move, Updater, Inserter);
                        reader.ExecARow((record) => {
                            tmpGroupId = record.ToInt("group_id");
                        });

                        reader = dao.ExecQuery(@"
-- 移動元
INSERT INTO hst_action (book_id, item_id, act_time, act_value, group_id, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, (
  SELECT item_id FROM mst_item I 
  INNER JOIN (SELECT * FROM mst_category WHERE balance_kind = @{6}) C ON C.category_id = I.category_id
  WHERE move_flg = 1
), @{1}, @{2}, @{3}, 0, 'now', @{4}, 'now', @{5}) RETURNING action_id;",
                            fromBookId, actTime, - actValue, tmpGroupId, Updater, Inserter, (int)BalanceKind.Outgo);
                        if(selectedBookId == fromBookId) {
                            reader.ExecARow((record) => {
                                resActionId = record.ToInt("action_id");
                            });
                        }

                        reader = dao.ExecQuery(@"
-- 移動先
INSERT INTO hst_action (book_id, item_id, act_time, act_value, group_id, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, (
  SELECT item_id FROM mst_item I
  INNER JOIN (SELECT * FROM mst_category WHERE balance_kind = @{6}) C ON C.category_id = I.category_id
  WHERE move_flg = 1
), @{1}, @{2}, @{3}, 0, 'now', @{4}, 'now', @{5}) RETURNING action_id;",
                            toBookId, actTime, actValue, tmpGroupId, Updater, Inserter, (int)BalanceKind.Income);
                        if (selectedBookId == toBookId) {
                            reader.ExecARow((record) => {
                                resActionId = record.ToInt("action_id");
                            });
                        }
                    }
                    else { // 編集
                        dao.ExecNonQuery(@"
-- 移動元
UPDATE hst_action
SET book_id = @{0}, act_time = @{1}, act_value = @{2}, update_time = 'now', updater = @{3}
WHERE action_id = @{4};", fromBookId, actTime, - actValue, Updater, fromActionId);
                        if(selectedBookId == fromBookId) {
                            resActionId = fromActionId;
                        }

                        dao.ExecNonQuery(@"
-- 移動先
UPDATE hst_action
SET book_id = @{0}, act_time = @{1}, act_value = @{2}, update_time = 'now', updater = @{3}
WHERE action_id = @{4};", toBookId, actTime, actValue, Updater, toActionId);
                        if(selectedBookId == toBookId) {
                            resActionId = toActionId;
                        }
                    }

                    if (commission != 0) {
                        #region 手数料あり
                        int bookId = -1;
                        switch (commissionKind) {
                            case CommissionKind.FromBook:
                                bookId = fromBookId;
                                break;
                            case CommissionKind.ToBook:
                                bookId = toBookId;
                                break;
                        }
                        if (commissionActionId != null) {
                            dao.ExecNonQuery(@"
UPDATE hst_action
SET book_id = @{0}, item_id = @{1}, act_time = @{2}, act_value = @{3}, remark = @{4}, update_time = 'now', updater = @{5}
WHERE action_id = @{6};", bookId, commissionItemId, actTime, - commission, remark, Updater, commissionActionId);
                        }
                        else {
                            dao.ExecNonQuery(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, group_id, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, 0, 'now', @{6}, 'now', @{7});", bookId, commissionItemId, actTime, - commission, groupId, remark, Updater, Inserter);
                        }
                        #endregion
                    }
                    else {
                        #region 手数料なし
                        if (commissionActionId != null) {
                            dao.ExecNonQuery(@"
UPDATE hst_action
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE action_id = @{1};", Updater, commissionActionId);
                        }
                        #endregion
                    }
                });

                if (remark != string.Empty) {
                    #region 備考を追加する
                    DaoReader reader = dao.ExecQuery(@"
SELECT remark FROM hst_remark
WHERE item_id = @{0} AND remark = @{1};", commissionItemId, remark);

                    if (reader.Count == 0) {
                        dao.ExecNonQuery(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, 0, @{2}, 0, 'now', @{3}, 'now', @{4});", commissionItemId, remark, actTime, Updater, Inserter);
                    }
                    else {
                        dao.ExecNonQuery(@"
UPDATE hst_remark
SET used_time = @{0}, del_flg = 0, update_time = 'now', updater = @{1}
WHERE item_id = @{2} AND remark = @{3};", actTime, Updater, commissionItemId, remark);
                    }
                    #endregion
                }

                return resActionId;
            }
        }

        #region 設定反映用の関数
        /// <summary>
        /// 設定を読み込む
        /// </summary>
        private void LoadSetting()
        {
            if (Properties.Settings.Default.MoveRegistrationWindow_Left != -1) {
                Left = Properties.Settings.Default.MoveRegistrationWindow_Left;
            }
            if (Properties.Settings.Default.MoveRegistrationWindow_Top != -1) {
                Top = Properties.Settings.Default.MoveRegistrationWindow_Top;
            }
            if (Properties.Settings.Default.MoveRegistrationWindow_Width != -1) {
                Width = Properties.Settings.Default.MoveRegistrationWindow_Width;
            }
            if (Properties.Settings.Default.MoveRegistrationWindow_Height != -1) {
                Height = Properties.Settings.Default.MoveRegistrationWindow_Height;
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        private void SaveSetting()
        {
            if (this.WindowState == WindowState.Normal) {
                Properties.Settings.Default.MoveRegistrationWindow_Left = Left;
                Properties.Settings.Default.MoveRegistrationWindow_Top = Top;
                Properties.Settings.Default.MoveRegistrationWindow_Width = Width;
                Properties.Settings.Default.MoveRegistrationWindow_Height = Height;
                Properties.Settings.Default.Save();
            }
        }
        #endregion
    }

    #region ViewModel
    /// <summary>
    /// 帳簿VM
    /// </summary>
    public partial class BookViewModel { }

    /// <summary>
    /// 手数料項目VM
    /// </summary>
    public partial class ItemViewModel { }

    /// <summary>
    /// 手数料種別
    /// </summary>
    public class CommissionKindViewModel
    {
        /// <summary>
        /// 手数料種別
        /// </summary>
        public CommissionKind CommissionKind { get; set; }
        /// <summary>
        /// 手数料種別名
        /// </summary>
        public string CommissionKindName { get; set; }
    }

    /// <summary>
    /// 帳簿項目登録ウィンドウ(移動)VM
    /// </summary>
    public class MoveRegistrationWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 変更時処理重複防止用フラグ
        /// </summary>
        private bool UpdateOnChanged = false;

        /// <summary>
        /// 移動元帳簿変更時イベント
        /// </summary>
        public event Action OnFromBookChanged = default(Action);
        /// <summary>
        /// 移動先帳簿変更時イベント
        /// </summary>
        public event Action OnToBookChanged = default(Action);
        /// <summary>
        /// 手数料種別変更時イベント
        /// </summary>
        public event Action OnCommissionKindChanged = default(Action);
        /// <summary>
        /// 項目変更時イベント
        /// </summary>
        public event Action OnItemChanged = default(Action);

        /// <summary>
        /// 選択された日時
        /// </summary>
        #region SelectedDate
        public DateTime SelectedDate {
            get { return _SelectedDate; }
            set {
                if (_SelectedDate != value) {
                    _SelectedDate = value;
                    PropertyChanged?.Raise(this, _nameSelectedDate);
                }
            }
        }
        private DateTime _SelectedDate = default(DateTime);
        internal static readonly string _nameSelectedDate = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.SelectedDate);
        #endregion
        
        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList {
            get { return _BookVMList; }
            set {
                if (_BookVMList != value) {
                    _BookVMList = value;
                    PropertyChanged?.Raise(this, _nameBookVMList);
                }
            }
        }
        private ObservableCollection<BookViewModel> _BookVMList = default(ObservableCollection<BookViewModel>);
        internal static readonly string _nameBookVMList = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.BookVMList);
        #endregion
        /// <summary>
        /// 選択された移動元帳簿VM
        /// </summary>
        #region SelectedFromBookVM
        public BookViewModel SelectedFromBookVM {
            get { return _SelectedFromBookVM; }
            set {
                if (_SelectedFromBookVM != value) {
                    _SelectedFromBookVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedFromBookVM);
                    
                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnFromBookChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private BookViewModel _SelectedFromBookVM = default(BookViewModel);
        internal static readonly string _nameSelectedFromBookVM = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.SelectedFromBookVM);
        #endregion
        /// <summary>
        /// 選択された移動先帳簿VM
        /// </summary>
        #region SelectedToBookVM
        public BookViewModel SelectedToBookVM {
            get { return _SelectedToBookVM; }
            set {
                if (_SelectedToBookVM != value) {
                    _SelectedToBookVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedToBookVM);

                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnToBookChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private BookViewModel _SelectedToBookVM = default(BookViewModel);
        internal static readonly string _nameSelectedToBookVM = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.SelectedToBookVM);
        #endregion

        /// <summary>
        /// 金額
        /// </summary>
        #region Value
        public int? Value {
            get { return _Value; }
            set {
                if (_Value != value) {
                    _Value = value;
                    PropertyChanged?.Raise(this, _nameValue);
                }
            }
        }
        private int? _Value = null;
        internal static readonly string _nameValue = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.Value);
        #endregion

        /// <summary>
        /// 手数料種別VMリスト
        /// </summary>
        #region CommissionKindVMList
        public ObservableCollection<CommissionKindViewModel> CommissionKindVMList {
            get { return _CommissionKindVMList; }
            set {
                if(_CommissionKindVMList != value) {
                    _CommissionKindVMList = value;
                    PropertyChanged?.Raise(this, _nameCommissionKindVMList);
                }
            }
        }
        private ObservableCollection<CommissionKindViewModel> _CommissionKindVMList = new ObservableCollection<CommissionKindViewModel> {
            new CommissionKindViewModel() { CommissionKind = CommissionKind.FromBook, CommissionKindName = CommissionStr[CommissionKind.FromBook] },
            new CommissionKindViewModel() { CommissionKind = CommissionKind.ToBook, CommissionKindName = CommissionStr[CommissionKind.ToBook] }
        };
        internal static readonly string _nameCommissionKindVMList = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.CommissionKindVMList);
        #endregion
        /// <summary>
        /// 選択された手数料種別VM
        /// </summary>
        #region SelectedCommissionKindVM
        public CommissionKindViewModel SelectedCommissionKindVM {
            get { return _SelectedCommissionKindVM; }
            set {
                if(_SelectedCommissionKindVM != value) {
                    _SelectedCommissionKindVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedCommissionKindVM);

                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnCommissionKindChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private CommissionKindViewModel _SelectedCommissionKindVM = default(CommissionKindViewModel);
        internal static readonly string _nameSelectedCommissionKindVM = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.SelectedCommissionKindVM);
        #endregion

        /// <summary>
        /// 手数料項目VMリスト
        /// </summary>
        #region ItemVMList
        public ObservableCollection<ItemViewModel> ItemVMList {
            get { return _ItemVMList; }
            set {
                if (_ItemVMList != value) {
                    _ItemVMList = value;
                    PropertyChanged?.Raise(this, _nameItemVMList);
                }
            }
        }
        private ObservableCollection<ItemViewModel> _ItemVMList = default(ObservableCollection<ItemViewModel>);
        internal static readonly string _nameItemVMList = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.ItemVMList);
        #endregion
        /// <summary>
        /// 選択された手数料項目VM
        /// </summary>
        #region SelectedItemVM
        public ItemViewModel SelectedItemVM {
            get { return _SelectedItemVM; }
            set {
                if (_SelectedItemVM != value) {
                    _SelectedItemVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedItemVM);

                    if (!UpdateOnChanged) {
                        UpdateOnChanged = true;
                        OnItemChanged?.Invoke();
                        UpdateOnChanged = false;
                    }
                }
            }
        }
        private ItemViewModel _SelectedItemVM = default(ItemViewModel);
        internal static readonly string _nameSelectedItemVM = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.SelectedItemVM);
        #endregion
        
        /// <summary>
        /// 手数料
        /// </summary>
        #region Commission
        public int? Commission {
            get { return _Commission; }
            set {
                if (_Commission != value) {
                    _Commission = value;
                    PropertyChanged?.Raise(this, _nameCommission);
                }
            }
        }
        private int? _Commission = null;
        internal static readonly string _nameCommission = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.Commission);
        #endregion

        /// <summary>
        /// 備考リスト
        /// </summary>
        #region RemarkList
        public ObservableCollection<String> RemarkList {
            get { return _RemarkList; }
            set {
                if (_RemarkList != value) {
                    _RemarkList = value;
                    PropertyChanged?.Raise(this, _nameRemarkList);
                }
            }
        }
        private ObservableCollection<String> _RemarkList = default(ObservableCollection<String>);
        internal static readonly string _nameRemarkList = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.RemarkList);
        #endregion
        /// <summary>
        /// 選択された備考
        /// </summary>
        #region SelectedRemark
        public String SelectedRemark {
            get { return _SelectedRemark; }
            set {
                if (_SelectedRemark != value) {
                    _SelectedRemark = value;
                    PropertyChanged?.Raise(this, _nameSelectedRemark);
                }
            }
        }
        private String _SelectedRemark = default(String);
        internal static readonly string _nameSelectedRemark = PropertyName<MoveRegistrationWindowViewModel>.Get(x => x.SelectedRemark);
        #endregion

        /// <summary>
        /// プロパティ変更イベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
    #endregion
}
