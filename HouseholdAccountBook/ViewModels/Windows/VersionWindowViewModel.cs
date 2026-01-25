using HouseholdAccountBook.ViewModels.Abstract;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// バージョンウィンドウVM
    /// </summary>
    public class VersionWindowViewModel : WindowViewModelBase
    {
        #region Bindingプロパティ
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
        /// URIオープンコマンド
        /// </summary>
        public ICommand OpenUriCommand => new RelayCommand<string>(this.OpenUriCommand_Execute);
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// URIオープンコマンド処理
        /// </summary>
        /// <param name="uri">対象URI</param>
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
        protected override (double, double) WindowSizeSettingRaw {
            get {
                Properties.Settings settings = Properties.Settings.Default;
                return (settings.VersionWindow_Width, settings.VersionWindow_Height);
            }
            set {
                Properties.Settings settings = Properties.Settings.Default;
                settings.VersionWindow_Width = value.Item1;
                settings.VersionWindow_Height = value.Item2;
                settings.Save();
            }
        }

        public override Point WindowPointSetting {
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

        public override Task LoadAsync() => throw new NotImplementedException();

        public override void AddEventHandlers()
        {
            // NOP
        }
    }
}
