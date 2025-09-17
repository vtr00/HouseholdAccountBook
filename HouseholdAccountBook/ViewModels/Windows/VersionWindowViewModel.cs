using HouseholdAccountBook.ViewModels.Abstract;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// バージョンウィンドウVM
    /// </summary>
    public class VersionWindowViewModel : WindowViewModelBase
    {
        /// <summary>
        /// サポートサイト
        /// </summary>
        public string SupportSiteUri { get; } = "https://github.com/vtr00/";
        /// <summary>
        /// SNSサイト
        /// </summary>
        public string SnsSiteUri { get; } = "https://twitter.com/toresebu";

        /// <summary>
        /// 対象のURIを開く
        /// </summary>
        public ICommand OpenUriCommand => new RelayCommand<string>(this.OpenUriCommand_Execute);

        #region ウィンドウ設定プロパティ
        public override Rect WindowRectSetting
        {
            set {
                Properties.Settings settings = Properties.Settings.Default;

                if (settings.App_IsPositionSaved) {
                    settings.VersionWindow_Left = value.Left;
                    settings.VersionWindow_Top = value.Top;
                }

                settings.Save();
            }
        }

        public override Size? WindowSizeSetting => null;

        public override Point? WindowPointSetting
        {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return WindowPointSettingImpl(settings.VersionWindow_Left, settings.VersionWindow_Top, settings.App_IsPositionSaved);
            }
        }
        #endregion

        private void OpenUriCommand_Execute(string uri)
        {
            ProcessStartInfo info = new() {
                FileName = uri,
                UseShellExecute = true
            };
            _ = Process.Start(info);
        }
    }
}
