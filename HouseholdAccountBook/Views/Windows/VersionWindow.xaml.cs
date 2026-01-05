using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Others;
using System.Windows;

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
            using FuncLog funcLog = new();

            this.Owner = owner;
            this.Name = UiConstants.WindowNameStr[nameof(VersionWindow)];
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();
            this.AddCommonEventHandlersToVM();

            this.Loaded += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.Loaded));

                this.WVM.AddEventHandlers();
            };
        }
    }
}
