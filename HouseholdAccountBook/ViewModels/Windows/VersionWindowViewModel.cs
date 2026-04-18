using HouseholdAccountBook.Models.AppServices;
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
        public ICommand OpenUriCommand => field ??= new RelayCommand<string>(this.OpenUriCommand_Execute);
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
            get => UserSettingService.Instance.VersionWindowSize;
            set => UserSettingService.Instance.VersionWindowSize = value;
        }

        public override Point WindowPointSetting {
            get => UserSettingService.Instance.VersionWindowPoint;
            set => UserSettingService.Instance.VersionWindowPoint = value;
        }
        #endregion

        public override Task LoadAsync() => throw new NotImplementedException();

        public override void AddEventHandlers()
        {
            // NOP
        }
    }
}
