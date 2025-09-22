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
        #region プロパティ
        /// <summary>
        /// サポートサイト
        /// </summary>
        public string SupportSiteUri { get; } = "https://github.com/vtr00/";
        /// <summary>
        /// SNSサイト
        /// </summary>
        public string SnsSiteUri { get; } = "https://x.com/toresebu";
        #endregion

        #region コマンド
        /// <summary>
        /// 対象のURIを開く
        /// </summary>
        public ICommand OpenUriCommand => new RelayCommand<string>(this.OpenUriCommand_Execute);
        #endregion

        #region コマンドイベントハンドラ
        private void OpenUriCommand_Execute(string uri)
        {
            ProcessStartInfo info = new() {
                FileName = uri,
                UseShellExecute = true
            };
            _ = Process.Start(info);
        }
        #endregion

        #region ウィンドウ設定プロパティ
        public override Point WindowPointSetting
        {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return new Point(settings.VersionWindow_Left, settings.VersionWindow_Top);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.VersionWindow_Left = value.X;
                settings.VersionWindow_Top = value.Y;
                settings.Save();
            }
        }
        #endregion
    }
}
