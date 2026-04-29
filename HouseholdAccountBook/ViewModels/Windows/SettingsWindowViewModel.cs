using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.WindowsParts;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// 設定ウィンドウVM
    /// </summary>
    public class SettingsWindowViewModel : WindowViewModelBase
    {
        #region イベント
        /// <summary>
        /// 更新必要時イベント
        /// </summary>
        public event EventHandler NeedToUpdateChanged {
            add {
                this.BookTabVM.NeedToUpdateChanged += value;
                this.ItemTabVM.NeedToUpdateChanged += value;
                this.OtherTabVM.NeedToUpdateChanged += value;
            }
            remove {
                this.BookTabVM.NeedToUpdateChanged -= value;
                this.ItemTabVM.NeedToUpdateChanged -= value;
                this.OtherTabVM.NeedToUpdateChanged -= value;
            }
        }
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 選択された設定タブインデックス
        /// </summary>
        public int SelectedTabIndex {
            get => (int)this.SelectedTab;
            set => this.SelectedTab = EnumUtil.SafeParseEnum(value, SettingsTabs.BookSettingsTab);
        }
        /// <summary>
        /// 選択された設定タブ種別
        /// </summary>
        public SettingsTabs SelectedTab {
            get;
            set {
                if (this.SetProperty(ref field, value)) {
                    this.RaisePropertyChanged(nameof(this.SelectedTabIndex));
                }
            }
        }

        /// <summary>
        /// 帳簿設定タブVM
        /// </summary>
        public SettingsWindowBookTabViewModel BookTabVM => field ??= new();

        /// <summary>
        /// 項目設定タブVM
        /// </summary>
        public SettingsWindowItemTabViewModel ItemTabVM => field ??= new();

        /// <summary>
        /// DB設定タブVM
        /// </summary>
        public SettingsWindowDbTabViewModel DbTabVM => field ??= new();

        /// <summary>
        /// その他設定タブVM
        /// </summary>
        public SettingsWindowOtherTabViewModel OtherTabVM => field ??= new();
        #endregion

        #region ウィンドウ設定プロパティ
        protected override (double, double) WindowSizeSettingRaw {
            get => UserSettingService.Instance.SettingsWindowSize;
            set => UserSettingService.Instance.SettingsWindowSize = value;
        }

        public override Point WindowPointSetting {
            get => UserSettingService.Instance.SettingsWindowPoint;
            set => UserSettingService.Instance.SettingsWindowPoint = value;
        }
        #endregion

        /// <summary>
        /// <see cref="SettingsWindowViewModel"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public SettingsWindowViewModel()
        {
            this.mChildrenVM.AddRange(
                [
                    this.BookTabVM,
                    this.ItemTabVM,
                    this.DbTabVM,
                    this.OtherTabVM
                ]
            );
        }

        public override async Task LoadAsync()
        {
            using FuncLog funcLog = new();

            switch (this.SelectedTab) {
                case SettingsTabs.BookSettingsTab:
                    await this.BookTabVM.LoadAsync();
                    break;
                case SettingsTabs.ItemSettingsTab:
                    await this.ItemTabVM.LoadAsync();
                    break;
                case SettingsTabs.DbSettingsTab:
                    await this.DbTabVM.LoadAsync();
                    break;
                case SettingsTabs.OtherSettingsTab:
                    await this.OtherTabVM.LoadAsync();
                    break;
            }
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            this.mChildrenVM.ForEach(childVM => {
                childVM.OpenFolderDialogRequested += (sender, e) => this.OpenFolderDialogRequest(e);
                childVM.OpenFileDialogRequested += (sender, e) => this.OpenFileDialogRequest(e);
                childVM.SaveFileDialogRequested += (sender, e) => this.SaveFileDialogRequest(e);
                childVM.AddEventHandlers();
            });
        }
    }
}
