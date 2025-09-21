using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.Others;
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

        #region プロパティ
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
        public override Rect WindowRectSetting
        {
            set {
                Properties.Settings settings = Properties.Settings.Default;

                if (settings.App_IsPositionSaved) {
                    settings.SettingsWindow_Left = value.Left;
                    settings.SettingsWindow_Top = value.Top;
                }

                settings.SettingsWindow_Width = value.Width;
                settings.SettingsWindow_Height = value.Height;
                settings.Save();
            }
        }

        public override Size? WindowSizeSetting
        {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return WindowSizeSettingImpl(settings.SettingsWindow_Width, settings.SettingsWindow_Height);
            }
        }

        public override Point? WindowPointSetting
        {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return WindowPointSettingImpl(settings.SettingsWindow_Left, settings.SettingsWindow_Top, settings.App_IsPositionSaved);
            }
        }
        #endregion

        public override void Initialize(WaitCursorManagerFactory waitCursorManagerFactory, DbHandlerFactory dbHandlerFactory)
        {
            this.ItemTabVM.Initialize(waitCursorManagerFactory, dbHandlerFactory);
            this.BookTabVM.Initialize(waitCursorManagerFactory, dbHandlerFactory);
            this.OtherTabVM.Initialize(waitCursorManagerFactory, dbHandlerFactory);

            this.ItemTabVM.OpenFolderDialogRequested += (sender, e) => this.OpenFolderDialogRequest(e, (folder) => { });
            this.ItemTabVM.OpenFileDialogRequested += (sender, e) => this.OpenFileDialogRequest(e, (file) => { });
            this.BookTabVM.OpenFolderDialogRequested += (sender, e) => this.OpenFolderDialogRequest(e, (folder) => { });
            this.BookTabVM.OpenFileDialogRequested += (sender, e) => this.OpenFileDialogRequest(e, (file) => { });
            this.OtherTabVM.OpenFolderDialogRequested += (sender, e) => this.OpenFolderDialogRequest(e, (folder) => { });
            this.OtherTabVM.OpenFileDialogRequested += (sender, e) => this.OpenFileDialogRequest(e, (file) => { });

            base.Initialize(waitCursorManagerFactory, dbHandlerFactory);
        }

        /// <summary>
        /// DB等から読み込む
        /// </summary>
        public async Task LoadSettingsInfoAsync()
        {
            switch (this.SelectedTab) {
                case SettingsTabs.ItemSettingsTab:
                    await this.ItemTabVM.LoadItemInfoAsync();
                    break;
                case SettingsTabs.BookSettingsTab:
                    await this.BookTabVM.LoadBookInfoAsync();
                    break;
                case SettingsTabs.OtherSettingsTab:
                    this.OtherTabVM.LoadOthersSettings();
                    break;
            }
        }
    }
}
