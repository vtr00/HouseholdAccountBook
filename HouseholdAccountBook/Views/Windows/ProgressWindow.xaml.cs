using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// ProgressWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public IProgress<int> Progress { get; private set; }

        /// <summary>
        /// コード側からの閉じ要求か
        /// </summary>
        private bool mAllowClose = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ProgressWindow(CancellationTokenSource cts)
        {
            this.InitializeComponent();

            if (cts != null) {
                this.WVM.Canceled += (sender, e) => cts?.Cancel();
                CommandManager.InvalidateRequerySuggested();
            }
            this.Progress = new Progress<int>(value => this.WVM.ProgressValue = value);
            this.Progress.Report(-1);
        }

        /// <summary>
        /// コード側からウィンドウを閉じる
        /// </summary>
        public void CloseOnCode()
        {
            this.mAllowClose = true;
            base.Close();
        }

        /// <summary>
        /// Alt+F4等によるユーザー操作での閉じを防止する
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!this.mAllowClose) {
                e.Cancel = true;
            }
        }
    }
}
