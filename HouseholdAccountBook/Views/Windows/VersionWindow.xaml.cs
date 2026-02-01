using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Utilities;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
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
            using FuncLog funcLog = new();

            this.Owner = owner;
            this.Name = UiConstants.WindowNameStr[nameof(VersionWindow)];
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();
            this.AddCommonEventHandlersToVM();

            this.Loaded += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.Loaded));

                this.WVM.AddEventHandlers();

                this.SetHistoryLog();
            };
        }

        /// <summary>
        /// 更新履歴を設定する
        /// </summary>
        private void SetHistoryLog()
        {
            FlowDocument doc = new();
            Paragraph p = new();

            string text = Properties.Resources.UpdateLog;
            string pattern = @"refs\s+#(\d+)";
            MatchCollection matches = Regex.Matches(text, pattern);

            int lastIndex = 0;

            foreach (Match match in matches) {
                // リンクテキスト手前のテキストを追加
                if (match.Groups[1].Index > lastIndex) {
                    p.Inlines.Add(text[lastIndex..(match.Groups[1].Index - 1)]);
                }

                // リンクテキストを追加
                string uri = $"https://github.com/vtr00/HouseholdAccountBook/issues/{match.Groups[1].Value}";
                Hyperlink link = new(new Run($"#{match.Groups[1].Value}")) {
                    NavigateUri = new Uri(uri)
                };
                p.Inlines.Add(link);

                lastIndex = match.Index + match.Length;
            }

            // 残りのテキストを追加
            if (lastIndex < text.Length) {
                p.Inlines.Add(text[lastIndex..]);
            }

            doc.Blocks.Add(p);
            doc.PageWidth = 1500;
            this.HistoryLog.Document = doc;
        }

        /// <summary>
        /// 更新履歴のリンククリック時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RichTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not Run pointer || pointer.Parent is not Hyperlink parent) {
                return;
            }

            var uri = parent.NavigateUri;
            if (uri != null) {
                _ = Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
                e.Handled = true;
            }
        }
    }
}
