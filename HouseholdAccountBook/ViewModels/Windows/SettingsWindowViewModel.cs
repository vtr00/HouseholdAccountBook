using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
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
        public event EventHandler NeedToUpdateChanged
        {
            add {
                this.ItemTabVM.NeedToUpdateChanged += value;
                this.BookTabVM.NeedToUpdateChanged += value;
                this.OtherTabVM.NeedToUpdateChanged += value;
            }
            remove {
                this.ItemTabVM.NeedToUpdateChanged -= value;
                this.BookTabVM.NeedToUpdateChanged -= value;
                this.OtherTabVM.NeedToUpdateChanged -= value;
            }
        }
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 選択された設定タブインデックス
        /// </summary>
        #region SelectedTabIndex
        public int SelectedTabIndex
        {
            get => this._SelectedTabIndex;
            set {
                if (this.SetProperty(ref this._SelectedTabIndex, value)) {
                    this._SelectedTab = (SettingsTabs)value;
                }
            }
        }
        private int _SelectedTabIndex = default;
        #endregion
        /// <summary>
        /// 選択された設定タブ種別
        /// </summary>
        #region SelectedTab
        public SettingsTabs SelectedTab
        {
            get => this._SelectedTab;
            set {
                if (this.SetProperty(ref this._SelectedTab, value)) {
                    this._SelectedTabIndex = (int)value;
                }
            }
        }
        private SettingsTabs _SelectedTab = default;
        #endregion

        /// <summary>
        /// 項目設定タブVM
        /// </summary>
        public SettingsWindowItemTabViewModel ItemTabVM { get; } = new();

        /// <summary>
        /// 帳簿設定タブVM
        /// </summary>
        public SettingsWindowBookTabViewModel BookTabVM { get; } = new();

        /// <summary>
        /// その他設定タブVM
        /// </summary>
        public SettingsWindowOtherTabViewModel OtherTabVM { get; } = new();
        #endregion

        #region ウィンドウ設定プロパティ
        public override Size WindowSizeSetting
        {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return new Size(settings.SettingsWindow_Width, settings.SettingsWindow_Height);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.SettingsWindow_Width = value.Width;
                settings.SettingsWindow_Height = value.Height;
                settings.Save();
            }
        }

        public override Point WindowPointSetting
        {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return new Point(settings.SettingsWindow_Left, settings.SettingsWindow_Top);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.SettingsWindow_Left = value.X;
                settings.SettingsWindow_Top = value.Y;
                settings.Save();
            }
        }
        #endregion

        public SettingsWindowViewModel()
        {
            this.childrenVM.AddRange(
                [
                    this.ItemTabVM,
                    this.BookTabVM,
                    this.OtherTabVM
                ]
            );
        }

        public override async Task LoadAsync()
        {
            using FuncLog funcLog = new();

            switch (this.SelectedTab) {
                case SettingsTabs.ItemSettingsTab:
                    await this.ItemTabVM.LoadAsync();
                    break;
                case SettingsTabs.BookSettingsTab:
                    await this.BookTabVM.LoadAsync();
                    break;
                case SettingsTabs.OtherSettingsTab:
                    this.OtherTabVM.Load();
                    break;
            }
        }

        public override void AddEventHandlers()
        {
            using FuncLog funcLog = new();

            this.childrenVM.ForEach(childVM => {
                childVM.OpenFolderDialogRequested += (sender, e) => this.OpenFolderDialogRequest(e);
                childVM.OpenFileDialogRequested += (sender, e) => this.OpenFileDialogRequest(e);
            });

            this.childrenVM.ForEach(childVM => childVM.AddEventHandlers());
        }
    }
}
