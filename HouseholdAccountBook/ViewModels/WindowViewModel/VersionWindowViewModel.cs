using Prism.Mvvm;

namespace HouseholdAccountBook.ViewModels
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
    }
}
