using HouseholdAccountBook.Extensions;
using Notification.Wpf;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// VersionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VersionWindow : Window
    {
        /// <summary>
        /// <see cref="VersionWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        public VersionWindow(Window owner)
        {
            this.InitializeComponent();

            this.Owner = owner;
            this.LoadWindowSetting();
            this.Loaded += (sender, e) => this.HistoryLog.ScrollToEnd();

            this.AddCommonEventHandlers();
        }
    }
}
