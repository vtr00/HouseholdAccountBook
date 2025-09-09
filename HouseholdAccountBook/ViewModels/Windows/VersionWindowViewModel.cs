using HouseholdAccountBook.ViewModels.Abstract;
using System.Diagnostics;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Windows
{
    /// <summary>
    /// バージョンウィンドウVM
    /// </summary>
    public class VersionWindowViewModel : BindableBase
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
        public ICommand OpenUriCommand { get; private set; }

        public VersionWindowViewModel()
        {
            this.OpenUriCommand = new RelayCommand<string>(this.OpenUriCommand_Execute);
        }

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
