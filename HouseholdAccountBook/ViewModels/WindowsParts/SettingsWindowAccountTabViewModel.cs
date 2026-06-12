using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Loaders;
using HouseholdAccountBook.ViewModels.Settings;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    /// <summary>
    /// 帳簿設定タブVM
    /// </summary>
    public class SettingsWindowAccountTabViewModel : WindowPartViewModelBase
    {
        #region フィールド
        /// <summary>
        /// アプリサービス
        /// </summary>
        private AppCommonService mAppService;
        /// <summary>
        /// 設定サービス
        /// </summary>
        private MasterSettingService mSettingService;
        #endregion

        #region イベント
        /// <summary>
        /// 更新必要時イベント
        /// </summary>
        public event EventHandler NeedToUpdateChanged;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 帳簿セレクタVM
        /// </summary>
        public SelectorViewModel<AccountModel, AccountIdObj> AccountSelectorVM => field ??= new(static vm => vm?.Id, this.mBusyService);
        /// <summary>
        /// 表示された帳簿設定VM
        /// </summary>
        public AccountSettingViewModel DisplayedAccountSettingVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        #region コマンド
        /// <summary>
        /// 帳簿追加コマンド
        /// </summary>
        public ICommand AddAccountCommand => field ??= new AsyncRelayCommand(this.AddAccountCommand_ExecuteAsync, null, this.mBusyService);
        /// <summary>
        /// 帳簿追加コマンド処理
        /// </summary>
        private async Task AddAccountCommand_ExecuteAsync()
        {
            AccountIdObj accountId = await this.mSettingService.AddAccountAsync();
            await this.LoadAsync(accountId);

            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 帳簿削除コマンド
        /// </summary>
        public ICommand DeleteAccountCommand => field ??= new AsyncRelayCommand(this.DeleteAccountCommand_ExecuteAsync, () => this.AccountSelectorVM.SelectedItem != null, this.mBusyService);
        /// <summary>
        /// 帳簿削除コマンド処理
        /// </summary>
        private async Task DeleteAccountCommand_ExecuteAsync()
        {
            if (await this.mSettingService.DeleteAccountAsync(this.AccountSelectorVM.SelectedKey)) {
                await this.LoadAsync();
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInAccount, Properties.Resources.Title_Error);
            }
        }

        /// <summary>
        /// 帳簿表示順上昇コマンド
        /// </summary>
        public ICommand RaiseAccountSortOrderCommand => field ??= new AsyncRelayCommand(this.RaiseAccountSortOrderCommand_ExecuteAsync, () => 0 < this.AccountSelectorVM.SelectedIndex, this.mBusyService);
        /// <summary>
        /// 帳簿表示順上昇コマンド処理
        /// </summary>
        private async Task RaiseAccountSortOrderCommand_ExecuteAsync()
        {
            int index = this.AccountSelectorVM.SelectedIndex;
            AccountIdObj changingId = this.AccountSelectorVM.ItemList[index].Id;
            AccountIdObj changedId = this.AccountSelectorVM.ItemList[index - 1].Id;

            await this.mSettingService.SwapAccountSortOrderAsync(changingId, changedId);
            await this.LoadAsync(changingId);

            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 帳簿表示順下降コマンド
        /// </summary>
        public ICommand DropAccountSortOrderCommand => field ??= new AsyncRelayCommand(
            this.DropAccountSortOrderCommand_ExecuteAsync,
            () => this.AccountSelectorVM.SelectedIndex != -1 && this.AccountSelectorVM.SelectedIndex < this.AccountSelectorVM.Count - 1, this.mBusyService);
        /// <summary>
        /// 帳簿表示順下降コマンド処理
        /// </summary>
        private async Task DropAccountSortOrderCommand_ExecuteAsync()
        {
            int index = this.AccountSelectorVM.SelectedIndex;
            AccountIdObj changingId = this.AccountSelectorVM.ItemList[index].Id;
            AccountIdObj changedId = this.AccountSelectorVM.ItemList[index + 1].Id;

            await this.mSettingService.SwapAccountSortOrderAsync(changingId, changedId);
            await this.LoadAsync(changingId);

            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 帳簿情報保存コマンド
        /// </summary>
        public ICommand SaveAccountInfoCommand => field ??= new AsyncRelayCommand(
            this.SaveAccountInfoCommand_ExecuteAsync,
            () => !string.IsNullOrWhiteSpace(this.DisplayedAccountSettingVM?.InputedName), this.mBusyService);
        /// <summary>
        /// 帳簿情報保存コマンド処理
        /// </summary>
        private async Task SaveAccountInfoCommand_ExecuteAsync()
        {
            AccountSettingViewModel vm = this.DisplayedAccountSettingVM;
            AccountModel account = new(vm.Id, vm.InputedName) {
                AccountKind = vm.AccountKindSelectorVM.SelectedKey,
                Remark = vm.InputedRemark ?? string.Empty,
                InitialValue = vm.InputedInitialValue,
                StartDateExists = vm.SelectedIfStartDateExists,
                EndDateExists = vm.SelectedIfEndDateExists,
                Period = vm.InputedPeriod,
                DebitAccountId = vm.DebitAccountSelectorVM.SelectedKey,
                PayDay = vm.InputedPayDay,
                CsvFolderPath = vm.InputedCsvFolderPath != string.Empty ? Path.GetFullPath(vm.InputedCsvFolderPath, App.GetCurrentDir()) : null,
                TextEncoding = vm.TextEncodingSelectorVM.SelectedKey,
                ActDateIndex = vm.InputedActDateIndex,
                ExpensesIndex = vm.InputedExpensesIndex,
                ItemNameIndex = vm.InputedItemNameIndex
            };
            await this.mSettingService.SaveAccountAsync(account);
            await this.LoadAsync(vm.Id);

            _ = MessageBox.Show(Properties.Resources.Message_CompletedToSave, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information);
            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// CSVフォルダパス選択コマンド
        /// </summary>
        public ICommand SelectCsvFolderPathCommand => field ??= new RelayCommand(this.SelectCsvFolderPathCommand_Execute);
        /// <summary>
        /// CSVフォルダパス選択コマンド処理
        /// </summary>
        private void SelectCsvFolderPathCommand_Execute()
        {
            string folderFullPath;
            if (string.IsNullOrWhiteSpace(this.DisplayedAccountSettingVM.InputedCsvFolderPath)) {
                folderFullPath = Path.GetDirectoryName(UserSettingService.Instance.CsvCompFile);
            }
            else {
                (string folderPath, string fileName) = PathUtil.GetSeparatedPath(this.DisplayedAccountSettingVM.InputedCsvFolderPath, App.GetCurrentDir());
                folderFullPath = Path.Combine(folderPath, fileName);
            }

            OpenFolderDialogRequestEventArgs e = new() {
                InitialDirectory = folderFullPath,
                Title = Properties.Resources.Title_CsvFolderSelection
            };
            if (this.OpenFolderDialogRequest(e)) {
                this.DisplayedAccountSettingVM.InputedCsvFolderPath = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FolderName);
            }
        }

        /// <summary>
        /// 帳簿-項目関係変更コマンド
        /// </summary>
        public ICommand ChangeAccountRelationCommand => field ??= new AsyncRelayCommand<object>(this.ChangeAccountRelationCommand_ExecuteAsync, null, this.mBusyService);
        /// <summary>
        /// 帳簿-項目関係変更コマンド処理
        /// </summary>
        /// <param name="viewModel">チェックされた対象の<see cref="RelationModel"/></param>
        private async Task ChangeAccountRelationCommand_ExecuteAsync(object viewModel)
        {
            AccountSettingViewModel vm = this.DisplayedAccountSettingVM;
            vm.RelationSelectorVM.SelectedItem = viewModel as RelationViewModel; // チェックボックスを変更しただけでは変更されないため、引数で受け取る

            if (await this.mSettingService.SaveAccountItemRemationAsync(vm.Id, (int)vm.RelationSelectorVM.SelectedKey, vm.RelationSelectorVM.SelectedItem.IsRelated)) {
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
            else {
                vm.RelationSelectorVM.SelectedItem.IsRelated = !vm.RelationSelectorVM.SelectedItem.IsRelated; // 選択前の状態に戻す
                _ = MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInItemWithinAccount, Properties.Resources.Title_Error);
            }
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="busyService">処理中状態サービス</param>
        public SettingsWindowAccountTabViewModel(BusyService busyService) : base(busyService)
        {
            using FuncLog funcLog = new();
        }

        public override void Initialize(DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(dbHandlerFactory);

            this.mAppService = new(this.mDbHandlerFactory);
            this.mSettingService = new(this.mDbHandlerFactory);

            this.AccountSelectorVM.SetLoader(async _ => await this.mAppService.LoadAccountListAsync());
        }

        public override async Task LoadAsync() => await this.LoadAsync(null);

        /// <summary>
        /// 帳簿設定タブに表示するデータを読み込む
        /// </summary>
        /// <param name="accountId">選択対象の帳簿ID</param>
        public async Task LoadAsync(AccountIdObj accountId = null)
        {
            using FuncLog funcLog = new(new { accountId });
            using IDisposable disposable = this.mBusyService.Enter();

            // InitializeComponent内で呼ばれる場合があるため、nullチェックを行う
            if (this.mAppService == null) {
                return;
            }

            await this.AccountSelectorVM.LoadAsync(accountId);
            // この時点では選択時イベントハンドラは未登録なので明示的に読み込む
            if (this.AccountSelectorVM.SelectedItem != null) {
                SettingViewModelLoader loader = new(this.mAppService, this.mSettingService);
                this.DisplayedAccountSettingVM = await loader.LoadAccountSettingViewModelAsync(this.AccountSelectorVM.SelectedKey);
            }
        }

        public override void AddEventHandlers()
        {
            this.AccountSelectorVM.SelectionChanged += async (sender, e) => {
                if (e.NewValue != null) {
                    using IDisposable disposable = this.mBusyService.Enter();

                    SettingViewModelLoader loader = new(this.mAppService, this.mSettingService);
                    this.DisplayedAccountSettingVM = await loader.LoadAccountSettingViewModelAsync(e.NewValue);
                }
                else {
                    this.DisplayedAccountSettingVM = null;
                }
            };
        }
    }
}
