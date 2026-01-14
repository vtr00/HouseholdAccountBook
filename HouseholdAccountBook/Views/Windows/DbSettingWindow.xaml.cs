using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Utilites;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// DbSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DbSettingWindow : Window
    {
        /// <summary>
        /// <see cref="DbSettingWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="message">表示メッセージ</param>
        public DbSettingWindow(Window owner, string message)
        {
            using FuncLog funcLog = new(new { message });

            this.Owner = owner;
            this.Name = UiConstants.WindowNameStr[nameof(DbSettingWindow)];
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();
            this.AddCommonEventHandlersToVM();
            this.WVM.SetPassword = password => this.passwordBox.Password = password;
            this.WVM.GetPassword = () => this.passwordBox.Password;

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), null);
            this.WVM.Message = message;

            this.Loaded += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.Loaded));

                using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create(methodName: nameof(this.Loaded))) {
                    this.WVM.Load();
                }

                this.WVM.AddEventHandlers();
            };
        }

        #region イベントハンドラ
        /// <summary>
        /// 入力された値を表示前にチェックする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            using FuncLog funcLog = new();

            TextBox textBox = sender as TextBox;
            bool yes_parse = false;
            if (sender != null) {
                // 既存のテキストボックス文字列に、新規に一文字追加された時、その文字列が数値として意味があるかどうかをチェック
                {
                    string tmp = textBox.Text + e.Text;
                    if (tmp == string.Empty) {
                        yes_parse = true;
                    }
                    else {
                        yes_parse = int.TryParse(tmp, out int xx);

                        // 範囲内かどうかチェック
                        if (yes_parse) {
                            if (xx < 0 || (int)Math.Pow(2, 16) - 1 < xx) {
                                yes_parse = false;
                            }
                        }
                    }
                }
            }
            // 更新したい場合は false, 更新したくない場合は true
            e.Handled = !yes_parse;
        }
        #endregion
    }
}
