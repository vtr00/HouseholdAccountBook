using HouseholdAccountBook.Adapters.Dao.Compositions;
using HouseholdAccountBook.Adapters.Dao.DbTable;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.Others.RequestEventArgs;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    public class MainWindowBookTabViewModel(MainWindowViewModel parent, Tabs tab) : WindowPartViewModelBase
    {
        private MainWindowViewModel Parent { get; } = parent;

        private Tabs Tab { get; } = tab;

        #region イベント
        /// <summary>
        /// スクロール要求時イベント
        /// </summary>
        public event EventHandler<EventArgs<int>> ScrollRequested;

        /// <summary>
        /// 移動追加要求時イベント
        /// </summary>
        public event EventHandler<AddMoveRequestEventArgs> AddMoveRequested;
        /// <summary>
        /// 帳簿項目追加要求時イベント
        /// </summary>
        public event EventHandler<AddActionRequestEventArgs> AddActionRequested;
        /// <summary>
        /// 帳簿項目リスト追加要求時イベント
        /// </summary>
        public event EventHandler<AddActionListRequestEventArgs> AddActionListRequested;
        /// <summary>
        /// 移動複製要求時イベント
        /// </summary>
        public event EventHandler<CopyMoveRequestEventArgs> CopyMoveRequested;
        /// <summary>
        /// 帳簿項目複製要求時イベント
        /// </summary>
        public event EventHandler<CopyActionRequestEventArgs> CopyActionRequested;
        /// <summary>
        /// 移動編集要求時イベント
        /// </summary>
        public event EventHandler<EditMoveRequestEventArgs> EditMoveRequested;
        /// <summary>
        /// 帳簿項目編集要求時イベント
        /// </summary>
        public event EventHandler<EditActionRequestEventArgs> EditActionRequested;
        /// <summary>
        /// 帳簿項目リスト編集要求時イベント
        /// </summary>
        public event EventHandler<EditActionListRequestEventArgs> EditActionListRequested;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 帳簿項目VMリスト
        /// </summary>
        #region ActionVMList
        public ObservableCollection<ActionViewModel> ActionVMList
        {
            get => this._ActionVMList;
            set {
                _ = this.SetProperty(ref this._ActionVMList, value);
                this.UpdateDisplayedActionVMList();
            }
        }
        private ObservableCollection<ActionViewModel> _ActionVMList = default;
        #endregion
        /// <summary>
        /// 表示対象の帳簿項目VMリスト
        /// </summary>
        #region DisplayedActionVMList
        public ObservableCollection<ActionViewModel> DisplayedActionVMList
        {
            get => this._DisplayedActionVMList;
            set => this.SetProperty(ref this._DisplayedActionVMList, value);
        }
        private ObservableCollection<ActionViewModel> _DisplayedActionVMList = default;
        #endregion

        /// <summary>
        /// 選択された帳簿項目VM(先頭)
        /// </summary>
        #region SelectedActionVM
        public ActionViewModel SelectedActionVM
        {
            get => this._SelectedActionVM;
            set => this.SetProperty(ref this._SelectedActionVM, value);
        }
        private ActionViewModel _SelectedActionVM = default;
        #endregion
        /// <summary>
        /// 選択された帳簿項目VMリスト
        /// </summary>
        #region SelectedActionVMList
        public ObservableCollection<ActionViewModel> SelectedActionVMList
        {
            get => this._SelectedActionVMList;
            set {
                if (value != null) {
                    //Log.Debug($"Old SelectedActionVMList.Count: {this._SelectedActionVMList.Count}");
                    //Log.Debug($"New SelectedActionVMList.Count: {value.Count}");

                    List<ActionViewModel> added = [.. value.Except(this._SelectedActionVMList)];
                    List<ActionViewModel> removed = [.. this._SelectedActionVMList.Except(value)];

                    //Log.Debug($"added.Count: {added.Count}");
                    //Log.Debug($"removed.Count: {removed.Count}");

                    foreach (ActionViewModel vm in added) {
                        this._SelectedActionVMList.Add(vm);
                    }
                    foreach (ActionViewModel vm in removed) {
                        _ = this._SelectedActionVMList.Remove(vm);
                    }
                }
                else {
                    //Log.Debug($"Old SelectedActionVMList.Count: {this._SelectedActionVMList.Count}");
                    //Log.Debug($"New SelectedActionVMList.Count: 0(null)");

                    // null の場合はリストを空にする(ClearだとBehaviorが意図した挙動にならない)
                    while (this._SelectedActionVMList.Count > 0) {
                        this._SelectedActionVMList.RemoveAt(0);
                    }
                }
            }
        }
        private readonly ObservableCollection<ActionViewModel> _SelectedActionVMList = [];
        #endregion

        /// <summary>
        /// CSVと一致したか
        /// </summary>
        #region IsMatch
        public bool? IsMatch
        {
            get {
                int count = this.SelectedActionVMList.Count(vm => vm.IsMatch);
                if (count == 0) return false;
                else if (count == this.SelectedActionVMList.Count) return true;
                return null;
            }
            set {
                if (value.HasValue) {
                    foreach (ActionViewModel vm in this.SelectedActionVMList) {
                        vm.IsMatch = value.Value;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 最後に操作した帳簿項目の日付
        /// </summary>
        #region ActDateLastEdited
        public DateTime? ActDateLastEdited
        {
            get => this._ActDateLastEdited;
            set => this.SetProperty(ref this._ActDateLastEdited, value);
        }
        private DateTime? _ActDateLastEdited = null;
        #endregion

        /// <summary>
        /// 選択されたデータの平均値
        /// </summary>
        #region AverageValue
        public double? AverageValue => this.Count != 0 ? (double?)this.SumValue / this.Count : null;
        #endregion
        /// <summary>
        /// 選択されたデータの個数
        /// </summary>
        #region Count
        public int Count => this.SelectedActionVMList.Count(vm => vm.Income != null || vm.Expenses != null);
        #endregion
        /// <summary>
        /// 選択されたデータの合計値
        /// </summary>
        #region SumValue
        public int SumValue => this.IncomeSumValue + this.ExpensesSumValue;
        #endregion
        /// <summary>
        /// 選択されたデータの収入合計値
        /// </summary>
        #region IncomeSumValue
        public int IncomeSumValue => this.SelectedActionVMList.Sum(vm => vm.Income ?? 0);
        #endregion
        /// <summary>
        /// 選択されたデータの支出合計値
        /// </summary>
        #region ExpensesSumValue
        public int ExpensesSumValue => this.SelectedActionVMList.Sum(vm => -vm.Expenses ?? 0);
        #endregion

        /// <summary>
        /// 概要VMリスト
        /// </summary>
        #region SummaryVMList
        public ObservableCollection<SummaryViewModel> SummaryVMList
        {
            get => this._SummaryVMList;
            set {
                _ = this.SetProperty(ref this._SummaryVMList, value);
                this.UpdateDisplayedActionVMList();
            }
        }
        private ObservableCollection<SummaryViewModel> _SummaryVMList = default;
        #endregion
        /// <summary>
        /// 選択された概要VM
        /// </summary>
        #region SelectedSummaryVM
        public SummaryViewModel SelectedSummaryVM
        {
            get => this._SelectedSummaryVM;
            set {
                if (this.SetProperty(ref this._SelectedSummaryVM, value)) {
                    this.Parent.SelectedBalanceKind = value?.BalanceKind;
                    this.Parent.SelectedCategoryId = value?.CategoryId;
                    this.Parent.SelectedItemId = value?.ItemId;

                    this.UpdateDisplayedActionVMList();
                }
            }
        }
        private SummaryViewModel _SelectedSummaryVM = default;
        #endregion

        /// <summary>
        /// 帳簿項目を概要によって絞り込むか
        /// </summary>
        #region UseFilter
        public bool UseFilter
        {
            get => this._UseFilter;
            set {
                if (this.SetProperty(ref this._UseFilter, value)) {
                    this.UpdateDisplayedActionVMList();
                }
            }
        }
        private bool _UseFilter = false;
        #endregion

        /// <summary>
        /// 選択された検索種別
        /// </summary>
        #region SelectedFindKind
        public FindKind SelectedFindKind
        {
            get => this._SelectedFindKind;
            set {
                if (this.SetProperty(ref this._SelectedFindKind, value)) {
                    this.RaisePropertyChanged(nameof(this.ShowFindBox));
                    this.RaisePropertyChanged(nameof(this.ShowReplaceBox));
                }
            }
        }
        private FindKind _SelectedFindKind = FindKind.None;
        #endregion
        /// <summary>
        /// 検索欄を表示するか
        /// </summary>
        #region ShowFindBox
        public bool ShowFindBox => this.SelectedFindKind != FindKind.None;
        #endregion
        /// <summary>
        /// 検索入力テキスト
        /// </summary>
        #region FindInputText
        public string FindInputText
        {
            get => this._FindInputText;
            set => this.SetProperty(ref this._FindInputText, value);
        }
        private string _FindInputText = string.Empty;
        #endregion
        /// <summary>
        /// 検索テキスト(設定時に絞り込み)
        /// </summary>
        #region FindText
        public string FindText
        {
            get => this._FindText;
            set {
                if (this.SetProperty(ref this._FindText, value)) {
                    this.UpdateDisplayedActionVMList();
                }
            }
        }
        private string _FindText = string.Empty;
        #endregion
        /// <summary>
        /// 置換欄を表示するか
        /// </summary>
        #region ShowReplaceBox
        public bool ShowReplaceBox => this.SelectedFindKind == FindKind.Replace;
        #endregion
        /// <summary>
        /// 置換テキスト
        /// </summary>
        #region ReplaceText
        public string ReplaceText
        {
            get => this._ReplaceText;
            set => this.SetProperty(ref this._ReplaceText, value);
        }
        private string _ReplaceText = string.Empty;
        #endregion

        #region コマンド
        /// <summary>
        /// 編集コマンド
        /// </summary>
        public ICommand EditMenuCommand => new RelayCommand(null, this.EditMenuCommand_CanExecute);
        /// <summary>
        /// 検索欄表示コマンド
        /// </summary>
        public ICommand ShowFindBoxCommand => new RelayCommand(this.ShowFindBoxCommand_Executed, this.ShowFindBoxCommand_CanExecute);
        /// <summary>
        /// 検索欄非表示コマンド
        /// </summary>
        public ICommand HideFindBoxCommand => new RelayCommand(this.HideFindBoxCommand_Executed);
        /// <summary>
        /// 置換欄表示コマンド
        /// </summary>
        public ICommand ShowReplaceBoxCommand => new RelayCommand(this.ShowReplaceBoxCommand_Executed, this.ShowReplaceBoxCommand_CanExecute);
        /// <summary>
        /// 置換欄非表示コマンド
        /// </summary>
        public ICommand HideReplaceBoxCommand => new RelayCommand(this.HideReplaceBoxCommand_Executed);
        /// <summary>
        /// 帳簿項目検索コマンド
        /// </summary>
        public ICommand FindActionCommand => new RelayCommand(this.FindActionCommand_Executed, this.FindActionCommand_CanExecute);
        /// <summary>
        /// 帳簿項目置換コマンド
        /// </summary>
        public ICommand ReplaceActionCommand => new RelayCommand(this.ReplaceActionCommand_Executed, this.ReplaceActionCommand_CanExecute);
        /// <summary>
        /// 移動追加コマンド
        /// </summary>
        public ICommand AddMoveCommand => new RelayCommand(this.AddMoveCommand_Executed, this.AddMoveCommand_CanExecute);
        /// <summary>
        /// 帳簿項目追加コマンド
        /// </summary>
        public ICommand AddActionCommand => new RelayCommand(this.AddActionCommand_Executed, this.AddActionCommand_CanExecute);
        /// <summary>
        /// 帳簿項目リスト追加コマンド
        /// </summary>
        public ICommand AddActionListCommand => new RelayCommand(this.AddActionListCommand_Executed, this.AddActionListCommand_CanExecute);
        /// <summary>
        /// 複製コマンド
        /// </summary>
        public ICommand CopyCommand => new RelayCommand(this.CopyCommand_Executed, this.CopyCommand_CanExecute);
        /// <summary>
        /// 編集コマンド
        /// </summary>
        public ICommand EditCommand => new RelayCommand(this.EditCommand_Executed, this.EditCommand_CanExecute);
        /// <summary>
        /// 削除コマンド
        /// </summary>
        public ICommand DeleteCommand => new RelayCommand(this.DeleteCommand_Executed, this.DeleteCommand_CanExecute);
        /// <summary>
        /// 一致フラグ変更コマンド
        /// </summary>
        public ICommand ChangeIsMatchCommand => new RelayCommand(this.ChangeIsMatchCommand_Executed, this.ChangeIsMatchCommand_CanExecute);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// 編集コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool EditMenuCommand_CanExecute() => !this.Parent.IsChildrenWindowOpened();

        /// <summary>
        /// 検索欄表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>帳簿タブを選択しているか</remarks>
        private bool ShowFindBoxCommand_CanExecute() => this.Parent.SelectedTab == Tabs.BooksTab;
        /// <summary>
        /// 検索欄表示コマンド処理
        /// </summary>
        private void ShowFindBoxCommand_Executed() => this.SelectedFindKind = FindKind.Find;

        /// <summary>
        /// 検索欄非表示コマンド処理
        /// </summary>
        private void HideFindBoxCommand_Executed()
        {
            this.FindText = string.Empty;
            this.SelectedFindKind = FindKind.None;
        }

        /// <summary>
        /// 置換欄表示コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>帳簿タブを選択しているか</remarks>
        private bool ShowReplaceBoxCommand_CanExecute() => this.Parent.SelectedTab == Tabs.BooksTab;
        /// <summary>
        /// 置換欄表示コマンド処理
        /// </summary>
        private void ShowReplaceBoxCommand_Executed() => this.SelectedFindKind = FindKind.Replace;

        /// <summary>
        /// 置換欄非表示コマンド処理
        /// </summary>
        private void HideReplaceBoxCommand_Executed() => this.SelectedFindKind = FindKind.Find;

        /// <summary>
        /// 帳簿項目検索コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool FindActionCommand_CanExecute() => !string.IsNullOrWhiteSpace(this.FindInputText);
        /// <summary>
        /// 帳簿項目検索コマンド処理
        /// </summary>
        private void FindActionCommand_Executed() => this.FindText = this.FindInputText;
        /// <summary>
        /// 帳簿項目置換コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool ReplaceActionCommand_CanExecute() => !string.IsNullOrWhiteSpace(this.FindInputText) && this.FindInputText != this.ReplaceText;
        /// <summary>
        /// 帳簿項目置換コマンド処理
        /// </summary>
        private async void ReplaceActionCommand_Executed()
        {
            this.FindText = this.FindInputText;

            if (MessageBox.Show(string.Format(Properties.Resources.Message_ReplaceShopNameRemarkNotification, this.FindText, this.ReplaceText), Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                this.FindText = string.Empty; // 検索をクリアしておく
                return;
            }

            List<int> actionIdList = [];
            foreach (ActionViewModel vm in this.DisplayedActionVMList) {
                actionIdList.Add(vm.ActionId);

                string shopName = vm.ShopName.Replace(this.FindText, this.ReplaceText);
                string remark = vm.Remark.Replace(this.FindText, this.ReplaceText);

                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    HstActionDao hstActionDao = new(dbHandler);
                    _ = await hstActionDao.UpdateShopNameAndRemarkByIdAsync(vm.ActionId, shopName, remark);
                }
            }

            this.FindText = string.Empty; // 検索をクリアしておく
            await this.LoadAsync(actionIdList);
        }

        /// <summary>
        /// 移動追加コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>帳簿タブを選択 かつ 帳簿が2つ以上存在 かつ 選択されている帳簿が存在 かつ 子ウィンドウが開いていない</remarks>
        private bool AddMoveCommand_CanExecute() => this.Parent.SelectedTab == Tabs.BooksTab && this.Parent.BookVMList?.Count >= 2 && this.Parent.SelectedBookVM != null && !this.Parent.IsChildrenWindowOpened();
        /// <summary>
        /// 移動追加コマンド処理
        /// </summary>
        private void AddMoveCommand_Executed()
        {
            AddMoveRequestEventArgs e = new() {
                DbHandlerFactory = this.dbHandlerFactory,
                BookId = this.Parent.SelectedBookVM?.Id,
                Month = this.Parent.DisplayedTermKind == TermKind.Monthly ? this.Parent.DisplayedMonth : null,
                Date = this.SelectedActionVM?.ActTime,
                Registered = async (sender, e2) => {
                    // 帳簿一覧タブを更新する
                    await this.LoadAsync(e2.Value, isUpdateActDateLastEdited: true);
                }
            };
            this.AddMoveRequested?.Invoke(this, e);
        }

        /// <summary>
        /// 帳簿項目追加コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>帳簿タブを選択 かつ 選択されている帳簿が存在 かつ 子ウィンドウが開いていない</remarks>
        private bool AddActionCommand_CanExecute() => this.Parent.SelectedTab == Tabs.BooksTab && this.Parent.SelectedBookVM != null && !this.Parent.IsChildrenWindowOpened();
        /// <summary>
        /// 帳簿項目追加コマンド処理
        /// </summary>
        private void AddActionCommand_Executed()
        {
            AddActionRequestEventArgs e = new() {
                DbHandlerFactory = this.dbHandlerFactory,
                BookId = this.Parent.SelectedBookVM?.Id,
                Month = this.Parent.DisplayedTermKind == TermKind.Monthly ? this.Parent.DisplayedMonth : null,
                Date = this.SelectedActionVM?.ActTime,
                Registered = async (sender, e2) => {
                    // 帳簿一覧タブを更新する
                    await this.LoadAsync(e2.Value, isUpdateActDateLastEdited: true);
                }
            };
            this.AddActionRequested?.Invoke(this, e);
        }

        /// <summary>
        /// 帳簿項目リスト追加コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>帳簿タブを選択 かつ 選択されている帳簿が存在 かつ 子ウィンドウを開いていない</remarks>
        private bool AddActionListCommand_CanExecute() => this.Parent.SelectedTab == Tabs.BooksTab && this.Parent.SelectedBookVM != null && !this.Parent.IsChildrenWindowOpened();
        /// <summary>
        /// 帳簿項目リスト追加コマンド処理
        /// </summary>
        private void AddActionListCommand_Executed()
        {
            AddActionListRequestEventArgs e = new() {
                DbHandlerFactory = this.dbHandlerFactory,
                BookId = this.Parent.SelectedBookVM?.Id,
                Month = this.Parent.DisplayedTermKind == TermKind.Monthly ? this.Parent.DisplayedMonth : null,
                Date = this.SelectedActionVM?.ActTime,
                Registered = async (sender, e2) => {
                    // 帳簿一覧タブを更新する
                    await this.LoadAsync(e2.Value, isUpdateActDateLastEdited: true);
                }
            };
            this.AddActionListRequested?.Invoke(this, e);
        }

        /// <summary>
        /// 複製コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>帳簿タブを選択 かつ 選択されている帳簿項目が1つだけ存在 かつ 選択している帳簿項目のIDが0より大きい かつ 子ウィンドウを開いていない</remarks>
        private bool CopyCommand_CanExecute() => this.Parent.SelectedTab == Tabs.BooksTab &&
                   this.SelectedActionVMList.Count == 1 && this.SelectedActionVMList[0].ActionId > 0 && !this.Parent.IsChildrenWindowOpened();
        /// <summary>
        /// 複製コマンド処理
        /// </summary>
        private async void CopyCommand_Executed()
        {
            // グループ種別を特定する
            int? groupKind = null;
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                GroupInfoDao groupInfoDao = new(dbHandler);
                var dto = await groupInfoDao.FindByActionId(this.SelectedActionVM.ActionId);
                groupKind = dto.GroupKind;
            }

            if (groupKind == null || groupKind == (int)GroupKind.Repeat) {
                // 移動以外の帳簿項目の複製時の処理
                CopyActionRequestEventArgs e = new() {
                    DbHandlerFactory = this.dbHandlerFactory,
                    ActionId = this.SelectedActionVM.ActionId,
                    Registered = async (sender, e) => {
                        // 帳簿一覧タブを更新する
                        await this.LoadAsync(e.Value, isUpdateActDateLastEdited: true);
                    }
                };
                this.CopyActionRequested?.Invoke(this, e);
            }
            else {
                // 移動の複製時の処理
                CopyMoveRequestEventArgs e = new() {
                    DbHandlerFactory = this.dbHandlerFactory,
                    GroupId = this.SelectedActionVM.GroupId.Value,
                    Registered = async (sender, e) => {
                        // 帳簿一覧タブを更新する
                        await this.LoadAsync(e.Value, isUpdateActDateLastEdited: true);
                    }
                };
                this.CopyMoveRequested?.Invoke(this, e);
            }
        }

        /// <summary>
        /// 編集コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>帳簿タブを選択 かつ 選択されている帳簿項目が1つだけ存在 かつ 選択している帳簿項目のIDが0より大きい かつ 子ウィンドウを開いていない</remarks>
        private bool EditCommand_CanExecute() => this.Parent.SelectedTab == Tabs.BooksTab &&
                   this.SelectedActionVMList.Count == 1 && 0 < this.SelectedActionVMList[0].ActionId && !this.Parent.IsChildrenWindowOpened();
        /// <summary>
        /// 編集コマンド処理
        /// </summary>
        private async void EditCommand_Executed()
        {
            // グループ種別を特定する
            int? groupKind = null;
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                GroupInfoDao groupInfoDao = new(dbHandler);
                var dto = await groupInfoDao.FindByActionId(this.SelectedActionVM.ActionId);
                groupKind = dto.GroupKind;
            }

            switch (groupKind) {
                case (int)GroupKind.Move: {
                    // 移動の編集時の処理
                    EditMoveRequestEventArgs e = new() {
                        DbHandlerFactory = this.dbHandlerFactory,
                        GroupId = this.SelectedActionVM.GroupId.Value,
                        Registered = async (sender, e) => {
                            // 帳簿一覧タブを更新する
                            await this.LoadAsync(e.Value, isUpdateActDateLastEdited: true);
                        }
                    };
                    this.EditMoveRequested?.Invoke(this, e);
                    break;
                }
                case (int)GroupKind.ListReg: {
                    // リスト登録された帳簿項目の編集時の処理
                    EditActionListRequestEventArgs e = new() {
                        DbHandlerFactory = this.dbHandlerFactory,
                        GroupId = this.SelectedActionVM.GroupId.Value,
                        Registered = async (sender, e) => {
                            // 帳簿一覧タブを更新する
                            await this.LoadAsync(e.Value, isUpdateActDateLastEdited: true);
                        }
                    };
                    this.EditActionListRequested?.Invoke(this, e);
                    break;
                }
                case (int)GroupKind.Repeat:
                default: {
                    // 移動・リスト登録以外の帳簿項目の編集時の処理
                    EditActionRequestEventArgs e = new() {
                        DbHandlerFactory = this.dbHandlerFactory,
                        ActionId = this.SelectedActionVM.ActionId,
                        Registered = async (sender, e) => {
                            // 帳簿一覧タブを更新する
                            await this.LoadAsync(e.Value, isUpdateActDateLastEdited: true);
                        }
                    };
                    this.EditActionRequested?.Invoke(this, e);
                    break;
                }
            }
        }

        /// <summary>
        /// 削除コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>帳簿タブを選択 かつ 選択している帳簿項目が存在 かつ 選択している帳簿項目にIDが0より大きいものが存在 かつ 子ウィンドウが開いていない</remarks>
        private bool DeleteCommand_CanExecute() => this.Parent.SelectedTab == Tabs.BooksTab &&
                   this.SelectedActionVMList.Where(vm => vm.ActionId > 0).Any() && !this.Parent.IsChildrenWindowOpened();
        /// <summary>
        /// 削除コマンド処理
        /// </summary>
        private async void DeleteCommand_Executed()
        {
            if (MessageBox.Show(Properties.Resources.Message_DeleteNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    await dbHandler.ExecTransactionAsync(async () => {
                        HstActionDao hstActionDao = new(dbHandler);
                        HstGroupDao hstGroupDao = new(dbHandler);

                        List<int> groupIdList = [];
                        // 帳簿項目IDが0を超える項目についてループ
                        foreach (ActionViewModel vm in this.SelectedActionVMList.Where(vm => 0 < vm.ActionId)) {
                            int actionId = vm.ActionId;
                            int? groupId = vm.GroupId;

                            // 対象となる帳簿項目を削除する
                            _ = await hstActionDao.DeleteByIdAsync(actionId);

                            // グループIDがない場合は次の項目へ
                            if (!groupId.HasValue) { continue; }

                            var groupDto = await hstGroupDao.FindByIdAsync(groupId.Value);
                            groupIdList.Add(groupId.Value);
                            int groupKind = groupDto.GroupKind;

                            switch (groupKind) {
                                case (int)GroupKind.Move: {
                                    // 移動の場合、削除項目と同じグループIDを持つ帳簿項目を削除する
                                    _ = await hstActionDao.DeleteByGroupIdAsync(groupId.Value);
                                }
                                break;
                                case (int)GroupKind.Repeat: {
                                    // 繰返しの場合、削除項目の日時以降の同じグループIDを持つ帳簿項目を削除する
                                    _ = await hstActionDao.DeleteInGroupAfterDateByIdAsync(actionId, false);
                                }
                                break;
                            }

                            // 削除対象と同じグループIDを持つ帳簿項目が1つだけの場合にグループIDをクリアする(移動以外の場合に該当する)
                            var actionDtoList = await hstActionDao.FindByGroupIdAsync(groupId.Value);
                            if (actionDtoList.Count() == 1) {
                                _ = await hstActionDao.ClearGroupIdByIdAsync(actionDtoList.First().ActionId);
                            }
                        }

                        foreach (int groupId in groupIdList) {
                            // 同じグループIDを持つ帳簿項目が存在しなくなる場合にグループを削除する
                            var actionDtoList = await hstActionDao.FindByGroupIdAsync(groupId);
                            if (!actionDtoList.Any()) {
                                _ = await hstGroupDao.DeleteByIdAsync(groupId);
                            }
                        }
                    });
                }

                // 帳簿一覧タブを更新する
                await this.LoadAsync(isUpdateActDateLastEdited: true);
            }
        }

        /// <summary>
        /// 一致フラグ変更コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        /// <remarks>帳簿タブを選択 かつ 選択している帳簿項目が存在 かつ 選択している帳簿項目にIDが0より大きいものが存在 かつ 登録ウィンドウが開いていない</remarks>
        private bool ChangeIsMatchCommand_CanExecute() => this.Parent.SelectedTab == Tabs.BooksTab &&
                   this.SelectedActionVMList.Where(vm => vm.ActionId > 0).Any() && !this.Parent.IsRegistrationWindowOpened();
        /// <summary>
        /// 一致ブラグ変更コマンド処理
        /// </summary>
        private async void ChangeIsMatchCommand_Executed()
        {
            await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                // 帳簿項目IDが0を超える項目についてループ
                HstActionDao hstActionDao = new(dbHandler);
                foreach (ActionViewModel vm in this.SelectedActionVMList.Where(vm => 0 < vm.ActionId)) {
                    _ = await hstActionDao.UpdateIsMatchByIdAsync(vm.ActionId, vm.IsMatch ? 1 : 0);
                }
            }
        }
        #endregion

        /// <summary>
        /// <see cref="DisplayedActionVMList"/> を更新する
        /// </summary>
        /// <remarks>表示されている項目のみ選択する</remarks>
        private void UpdateDisplayedActionVMList()
        {
            using FuncLog funcLog = new();

            ObservableCollection<ActionViewModel> tmp = this.ActionVMList;

            if (this._UseFilter) {   // フィルタ有効の場合
                // 概要が選択されている場合
                if (this._SelectedSummaryVM != null) {
                    // 収支が選択されている場合
                    if ((BalanceKind)this.SelectedSummaryVM.BalanceKind != BalanceKind.Others) {
                        if (this._SelectedSummaryVM.ItemId != -1) {             // 項目名が選択されている
                            tmp = [.. tmp.Where(vm => vm.ItemId == this.SelectedSummaryVM.ItemId || vm.ActionId == -1)];
                        }
                        else if (this._SelectedSummaryVM.CategoryId != -1) {    // 分類名が選択されている
                            tmp = [.. tmp.Where(vm => vm.CategoryId == this.SelectedSummaryVM.CategoryId || vm.ActionId == -1)];
                        }
                        else {                                                  // 収支種別が選択されている
                            tmp = [.. tmp.Where(vm => vm.BalanceKind == (BalanceKind)this.SelectedSummaryVM.BalanceKind || vm.ActionId == -1)];
                        }
                    }
                }
            }

            // 検索テキストで絞り込む
            if (this.FindText != string.Empty) {
                tmp = [.. tmp.Where(vm => (vm.ShopName?.Contains(this.FindText) ?? false) || (vm.Remark?.Contains(this.FindText) ?? false))];
            }

            this.DisplayedActionVMList = tmp;

            // 選択項目を表示項目に限定する
            this.SelectedActionVMList = [.. this.SelectedActionVMList.Where(vm => this.DisplayedActionVMList.Contains(vm))];
        }

        public override async Task LoadAsync()
        {
            await this.LoadAsync(null, null, null, null, false, false);
        }

        /// <summary>
        /// 帳簿タブに表示するデータを読み込む
        /// </summary>
        /// <param name="actionIdList">選択対象の帳簿項目IDリスト</param>
        /// <param name="balanceKind">選択対象の収支種別</param>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <param name="itemId">選択対象の項目ID</param>
        /// <param name="isScroll">帳簿項目一覧をスクロールするか</param>
        /// <param name="isUpdateActDateLastEdited">最後に操作した帳簿項目を更新するか</param>
        public async Task LoadAsync(List<int> actionIdList = null, int? balanceKind = null, int? categoryId = null, int? itemId = null,
                                      bool isScroll = false, bool isUpdateActDateLastEdited = false)
        {
            if (this.Parent.SelectedTab != Tabs.BooksTab) return;

            using FuncLog funcLog = new(new { actionIdList, balanceKind, categoryId, itemId, isScroll, isUpdateActDateLastEdited });

            // 指定がなければ、更新前の帳簿項目の選択を維持する
            List<int> tmpActionIdList = actionIdList ?? [.. this.SelectedActionVMList.Select(tmp => tmp.ActionId)];
            // 指定がなければ、更新前のサマリーの選択を維持する
            int? tmpBalanceKind = balanceKind ?? this.Parent.SelectedBalanceKind;
            int? tmpCategoryId = categoryId ?? this.Parent.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.Parent.SelectedItemId;
            Log.Vars(vars: new { tmpActionIdList, tmpBalanceKind, tmpCategoryId, tmpItemId });

            // 表示するデータを指定する
            var loader = new ViewModelLoader(this.dbHandlerFactory);
            switch (this.Parent.DisplayedTermKind) {
                case TermKind.Monthly:
                    var (tmp1, tmp2) = await (
                        loader.LoadActionViewModelListWithinMonthAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedMonth.Value),
                        loader.LoadSummaryViewModelListWithinMonthAsync(this.Parent.SelectedBookVM?.Id, this.Parent.DisplayedMonth.Value)).WhenAll();
                    this.ActionVMList = tmp1;
                    this.SummaryVMList = tmp2;
                    break;
                case TermKind.Selected:
                    var (tmp3, tmp4) = await (
                        loader.LoadActionViewModelListAsync(this.Parent.SelectedBookVM?.Id, this.Parent.StartDate, this.Parent.EndDate),
                        loader.LoadSummaryViewModelListAsync(this.Parent.SelectedBookVM?.Id, this.Parent.StartDate, this.Parent.EndDate)).WhenAll();
                    this.ActionVMList = tmp3;
                    this.SummaryVMList = tmp4;
                    break;
            }

            // 帳簿項目を選択する(サマリーの選択はこの段階では無視して処理する)
            this.SelectedActionVMList = new ObservableCollection<ActionViewModel>(this.ActionVMList.Where(vm => tmpActionIdList.Contains(vm.ActionId)));

            // サマリーを選択する
            this.SelectedSummaryVM = this.SummaryVMList.FirstOrDefault(vm => vm.BalanceKind == tmpBalanceKind && vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId);

            if (isScroll) {
                if (this.Parent.DisplayedTermKind == TermKind.Monthly &&
                    this.Parent.DisplayedMonth.Value.GetFirstDateOfMonth() <= DateTime.Today && DateTime.Today <= this.Parent.DisplayedMonth.Value.GetLastDateOfMonth()) {
                    // 今月の場合は、末尾が表示されるようにする
                    this.ScrollRequested?.Invoke(this, new EventArgs<int>(1));
                }
                else {
                    // 今月でない場合は、先頭が表示されるようにする
                    this.ScrollRequested?.Invoke(this, new EventArgs<int>(-1));
                }
            }

            // 最後に操作した帳簿項目の日付を更新する
            if (isUpdateActDateLastEdited) {
                if (actionIdList != null && actionIdList.Count != 0) {
                    await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                        HstActionDao hstActionDao = new(dbHandler);
                        var dto = await hstActionDao.FindByIdAsync(actionIdList[0]);
                        this.ActDateLastEdited = dto.ActTime;
                    }
                }
                else {
                    this.ActDateLastEdited = null;
                }
            }
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            // 帳簿項目選択変更時
            this.SelectedActionVMList.CollectionChanged += (sender, e) => {
                using FuncLog funcLog = new(new { ActionIdList = this.SelectedActionVMList.Select(vm => vm.ActionId) }, methodName: nameof(this.SelectedActionVMList.CollectionChanged));

                this.RaisePropertyChanged(nameof(this.AverageValue));
                this.RaisePropertyChanged(nameof(this.Count));
                this.RaisePropertyChanged(nameof(this.SumValue));
                this.RaisePropertyChanged(nameof(this.IncomeSumValue));
                this.RaisePropertyChanged(nameof(this.ExpensesSumValue));

                this.RaisePropertyChanged(nameof(this.IsMatch));
            };
        }
    }
}
