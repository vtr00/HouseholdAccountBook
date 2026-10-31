using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Services;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Common;
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
        /// アセットリストVM
        /// </summary>
        public SelectableViewModel<AssetModel, AssetIdObj> AssetListVM => field ??= new(static vm => vm?.Id, this.mBusyService);
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
            () => this.AssetListVM.SelectedItem != null && 1 < this.AssetListVM.Count && !this.AssetListVM.SelectedItem.IsDefault, this.mBusyService);
        /// <summary>
        /// アセット削除コマンド処理
        /// </summary>
        /// <returns></returns>
        private async Task DeleteAssetCommand_ExecuteAsync()
        {
            if (await this.mSettingService.DeleteAssetAsync(this.AssetListVM.SelectedKey)) {
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
            () => 0 < this.AssetListVM.SelectedIndex, this.mBusyService);
        /// <summary>
        /// アセット表示順上昇コマンド処理
        /// </summary>
        /// <returns></returns>
        private async Task RaiseAssetSortOrderCommand_ExecuteAsync()
        {
            int index = this.AssetListVM.SelectedIndex;
            AssetIdObj changingId = this.AssetListVM.ItemList[index].Id;
            AssetIdObj changedId = this.AssetListVM.ItemList[index - 1].Id;

            await this.mSettingService.SwapAssetSortOrderAsync(changingId, changedId);
            await this.LoadAsync(changingId);

            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// アセット表示順下降コマンド
        /// </summary>
        public ICommand DropAssetSortOrderCommand => field ??= new AsyncRelayCommand(
            this.DropAssetSortOrderCommand_ExecuteAsync,
            () => this.AssetListVM.SelectedIndex != -1 && this.AssetListVM.SelectedIndex < this.AssetListVM.Count - 1, this.mBusyService);
        /// <summary>
        /// アセット表示順下降コマンド処理
        /// </summary>
        /// <returns></returns>
        private async Task DropAssetSortOrderCommand_ExecuteAsync()
        {
            int index = this.AssetListVM.SelectedIndex;
            AssetIdObj changingId = this.AssetListVM.ItemList[index].Id;
            AssetIdObj changedId = this.AssetListVM.ItemList[index + 1].Id;

            await this.mSettingService.SwapAssetSortOrderAsync(changingId, changedId);
            await this.LoadAsync(changingId);

            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// デフォルトアセット設定コマンド
        /// </summary>
        public ICommand SetDefaultAssetCommand => field ??= new AsyncRelayCommand(
            this.SetDefaultAssetCommand_ExecuteAsync,
            () => this.AssetListVM.SelectedItem != null && !this.AssetListVM.SelectedItem.IsDefault);
        /// <summary>
        /// デフォルトアセット設定コマンド処理
        /// </summary>
        /// <returns></returns>
        private async Task SetDefaultAssetCommand_ExecuteAsync()
        {
            AssetIdObj assetId = this.AssetListVM.SelectedKey;
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
            () => !string.IsNullOrWhiteSpace(this.DisplayedAssetSettingVM?.NameVM.InputedValue) && (this.DisplayedAssetSettingVM?.DirtyState.IsDirty ?? false), this.mBusyService);
        /// <summary>
        /// アセット情報保存コマンド処理
        /// </summary>
        /// <returns></returns>
        private async Task SaveAssetInfoCommand_ExecuteAsync()
        {
            AssetSettingViewModel vm = this.DisplayedAssetSettingVM;
            AssetModel account = new(vm.Id, vm.NameVM.InputedValue) {
                Name = vm.NameVM.InputedValue,
                SubunitName = vm.SubunitNameVM.InputedValue,
                AssetCode = vm.AssetCodeVM.InputedValue,
                AssetKind = vm.AssetKindListVM.SelectedKey,
                Scale = vm.ScaleVM.InputedValue,
                Prefix = vm.PrefixVM.InputedValue,
                Suffix = vm.SuffixVM.InputedValue,
                SubPrefix = vm.SubPrefixVM.InputedValue,
                SubSuffix = vm.SubSuffixVM.InputedValue,
                BaseRate = vm.BaseRateVM.InputedValue
            };
            // アセット情報を保存する
            await this.mSettingService.SaveAssetAsync(account);
            // アセット情報を再読込する
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

            this.AssetListVM.SetLoader(async () => await this.mAppService.LoadAssetListAsync());
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

            await this.AssetListVM.LoadAsync(assetId);
            // この時点では選択時イベントハンドラは未登録なので明示的に読み込む
            if (this.AssetListVM.SelectedItem != null) {
                SettingViewModelLoader loader = new(this.mAppService, this.mSettingService);
                this.DisplayedAssetSettingVM = await loader.LoadAssetSettingViewModelAsync(this.AssetListVM.SelectedKey);
            }
        }

        public override void AddEventHandlers()
        {
            this.AssetListVM.SelectionChanged += async (sender, e) => {
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
