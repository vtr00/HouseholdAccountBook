using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Loaders;
using HouseholdAccountBook.ViewModels.Settings;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    /// <summary>
    /// アセット設定タブVM
    /// </summary>
    public class SettingsWindowAssetTabViewModel : WindowPartViewModelBase
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
        /// アセットセレクタVM
        /// </summary>
        public SelectorViewModel<AssetModel, AssetIdObj> AssetSelectorVM => field ??= new(static vm => vm?.Id, this.mBusyService);
        /// <summary>
        /// 表示されたアセット設定VM
        /// </summary>
        public AssetSettingViewModel DisplayedAssetSettingVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        #region コマンド
        /// <summary>
        /// アセット追加コマンド
        /// </summary>
        public ICommand AddAssetCommand => field ??= new AsyncRelayCommand(this.AddAssetCommand_ExecuteAsync, null, this.mBusyService);
        /// <summary>
        /// アセット追加コマンド処理
        /// </summary>
        /// <returns></returns>
        private async Task AddAssetCommand_ExecuteAsync()
        {
            AssetIdObj assetId = await this.mSettingService.AddAssetAsync();
            await this.LoadAsync(assetId);

            // アセットリストを更新する
            await AssetService.Instance.UpdateAssets(this.mDbHandlerFactory);

            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// アセット削除コマンド
        /// </summary>
        public ICommand DeleteAssetCommand => field ??= new AsyncRelayCommand(this.DeleteAssetCommand_ExecuteAsync,
            () => this.AssetSelectorVM.SelectedItem != null && 1 < this.AssetSelectorVM.Count && !this.AssetSelectorVM.SelectedItem.IsDefault, this.mBusyService);
        /// <summary>
        /// アセット削除コマンド処理
        /// </summary>
        /// <returns></returns>
        private async Task DeleteAssetCommand_ExecuteAsync()
        {
            if (await this.mSettingService.DeleteAssetAsync(this.AssetSelectorVM.SelectedKey)) {
                await this.LoadAsync();

                // アセットリストを更新する
                await AssetService.Instance.UpdateAssets(this.mDbHandlerFactory);

                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseAssetIsUsed, Properties.Resources.Title_Error);
            }
        }

        /// <summary>
        /// アセット表示順上昇コマンド
        /// </summary>
        public ICommand RaiseAssetSortOrderCommand => field ??= new AsyncRelayCommand(
            this.RaiseAssetSortOrderCommand_ExecuteAsync,
            () => 0 < this.AssetSelectorVM.SelectedIndex, this.mBusyService);
        /// <summary>
        /// アセット表示順上昇コマンド処理
        /// </summary>
        /// <returns></returns>
        private async Task RaiseAssetSortOrderCommand_ExecuteAsync()
        {
            int index = this.AssetSelectorVM.SelectedIndex;
            AssetIdObj changingId = this.AssetSelectorVM.ItemList[index].Id;
            AssetIdObj changedId = this.AssetSelectorVM.ItemList[index - 1].Id;

            await this.mSettingService.SwapAssetSortOrderAsync(changingId, changedId);
            await this.LoadAsync(changingId);

            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// アセット表示順下降コマンド
        /// </summary>
        public ICommand DropAssetSortOrderCommand => field ??= new AsyncRelayCommand(
            this.DropAssetSortOrderCommand_ExecuteAsync,
            () => this.AssetSelectorVM.SelectedIndex != -1 && this.AssetSelectorVM.SelectedIndex < this.AssetSelectorVM.Count - 1, this.mBusyService);
        /// <summary>
        /// アセット表示順下降コマンド処理
        /// </summary>
        /// <returns></returns>
        private async Task DropAssetSortOrderCommand_ExecuteAsync()
        {
            int index = this.AssetSelectorVM.SelectedIndex;
            AssetIdObj changingId = this.AssetSelectorVM.ItemList[index].Id;
            AssetIdObj changedId = this.AssetSelectorVM.ItemList[index + 1].Id;

            await this.mSettingService.SwapAssetSortOrderAsync(changingId, changedId);
            await this.LoadAsync(changingId);

            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// デフォルトアセット設定コマンド
        /// </summary>
        public ICommand SetDefaultAssetCommand => field ??= new AsyncRelayCommand(
            this.SetDefaultAssetCommand_ExecuteAsync,
            () => this.AssetSelectorVM.SelectedItem != null && !this.AssetSelectorVM.SelectedItem.IsDefault);
        /// <summary>
        /// デフォルトアセット設定コマンド処理
        /// </summary>
        /// <returns></returns>
        private async Task SetDefaultAssetCommand_ExecuteAsync()
        {
            AssetIdObj assetId = this.AssetSelectorVM.SelectedKey;
            UserSettingService.Instance.DefaultAssetId = assetId;

            await this.LoadAsync(assetId);

            // アセットリストを更新する
            await AssetService.Instance.UpdateAssets(this.mDbHandlerFactory);

            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// アセット情報保存コマンド
        /// </summary>
        public ICommand SaveAssetInfoCommand => field ??= new AsyncRelayCommand(
            this.SaveAssetInfoCommand_ExecuteAsync,
            () => !string.IsNullOrWhiteSpace(this.DisplayedAssetSettingVM?.InputedName), this.mBusyService);
        /// <summary>
        /// アセット情報保存コマンド処理
        /// </summary>
        /// <returns></returns>
        private async Task SaveAssetInfoCommand_ExecuteAsync()
        {
            AssetSettingViewModel vm = this.DisplayedAssetSettingVM;
            AssetModel account = new(vm.Id, vm.InputedName) {
                Name = vm.InputedName,
                SubunitName = vm.InputedSubunitName,
                AssetCode = vm.InputedAssetCode,
                AssetKind = vm.AssetKindSelectorVM.SelectedKey,
                Scale = vm.InputedScale,
                Prefix = vm.InputedPrefix,
                Suffix = vm.InputedSuffix,
                SubPrefix = vm.InputedSubPrefix,
                SubSuffix = vm.InputedSubSuffix,
                BaseRate = vm.InputedBaseRate
            };
            await this.mSettingService.SaveAssetAsync(account);
            await this.LoadAsync(vm.Id);

            // アセットリストを更新する
            await AssetService.Instance.UpdateAssets(this.mDbHandlerFactory);

            _ = MessageBox.Show(Properties.Resources.Message_CompletedToSave, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information);
            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="busyService">処理中状態サービス</param>
        public SettingsWindowAssetTabViewModel(BusyService busyService) : base(busyService)
        {
            using FuncLog funcLog = new();
        }

        public override void Initialize(DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(dbHandlerFactory);

            this.mAppService = new(this.mDbHandlerFactory);
            this.mSettingService = new(this.mDbHandlerFactory);

            this.AssetSelectorVM.SetLoader(async _ => await this.mAppService.LoadAssetListAsync());
        }

        public override async Task LoadAsync() => await this.LoadAsync(null);

        /// <summary>
        /// アセット設定タブに表示するデータを読み込む
        /// </summary>
        /// <param name="assetId">選択対象のアセットID</param>
        public async Task LoadAsync(AssetIdObj assetId = null)
        {
            using FuncLog funcLog = new(new { assetId });
            using IDisposable disposable = this.mBusyService.Enter();

            // InitializeComponent内で呼ばれる場合があるため、nullチェックを行う
            if (this.mAppService == null) {
                return;
            }

            await this.AssetSelectorVM.LoadAsync(assetId);
            // この時点では選択時イベントハンドラは未登録なので明示的に読み込む
            if (this.AssetSelectorVM.SelectedItem != null) {
                SettingViewModelLoader loader = new(this.mAppService, this.mSettingService);
                this.DisplayedAssetSettingVM = await loader.LoadAssetSettingViewModelAsync(this.AssetSelectorVM.SelectedKey);
            }
        }

        public override void AddEventHandlers()
        {
            this.AssetSelectorVM.SelectionChanged += async (sender, e) => {
                if (e.NewValue != null) {
                    using IDisposable disposable = this.mBusyService.Enter();

                    SettingViewModelLoader loader = new(this.mAppService, this.mSettingService);
                    this.DisplayedAssetSettingVM = await loader.LoadAssetSettingViewModelAsync(e.NewValue);
                }
                else {
                    this.DisplayedAssetSettingVM = null;
                }
            };
        }
    }
}
