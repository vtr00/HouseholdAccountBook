using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// DbSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DbSettingWindow : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="message">表示メッセージ</param>
        public DbSettingWindow(string message)
        {
            InitializeComponent();

            Properties.Settings settings = Properties.Settings.Default;
            this.DbSettingWindowVM.Message = message;

            this.DbSettingWindowVM.Host = settings.App_Postgres_Host;
            this.DbSettingWindowVM.Port = settings.App_Postgres_Port;
            this.DbSettingWindowVM.UserName = settings.App_Postgres_UserName;
            this.passwordBox.Password = settings.App_Postgres_Password;
#if DEBUG
            this.DbSettingWindowVM.DatabaseName = settings.App_Postgres_DatabaseName_Debug;
#else
            this.DbSettingWindowVM.DatabaseName = settings.App_Postgres_DatabaseName;
#endif
            this.DbSettingWindowVM.Role = settings.App_Postgres_Role;
            this.DbSettingWindowVM.DumpExePath = settings.App_Postgres_DumpExePath;
            this.DbSettingWindowVM.RestoreExePath = settings.App_Postgres_RestoreExePath;
        }

        #region コマンド
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DumpExePathDialogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                FileName = "pg_dump.exe",
                Title = "ファイル選択",
                Filter = "pg_dump.exe|pg_dump.exe"
            };

            if (ofd.ShowDialog() == true) {
                this.DbSettingWindowVM.DumpExePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestorePathDialogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                FileName = "pg_restore.exe",
                Title = "ファイル選択",
                Filter = "pg_restore.exe|pg_restore.exe"
            };

            if (ofd.ShowDialog() == true) {
                this.DbSettingWindowVM.RestoreExePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            }
        }

        /// <summary>
        /// OKボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Properties.Settings.Default.App_Postgres_Host = this.DbSettingWindowVM.Host;
            Properties.Settings.Default.App_Postgres_Port = this.DbSettingWindowVM.Port;
            Properties.Settings.Default.App_Postgres_UserName = this.DbSettingWindowVM.UserName;
            Properties.Settings.Default.App_Postgres_Password = this.passwordBox.Password;
            Properties.Settings.Default.App_Postgres_DatabaseName = this.DbSettingWindowVM.DatabaseName;
            Properties.Settings.Default.App_Postgres_Role = this.DbSettingWindowVM.Role;
            Properties.Settings.Default.App_Postgres_DumpExePath = this.DbSettingWindowVM.DumpExePath;
            Properties.Settings.Default.App_Postgres_RestoreExePath = this.DbSettingWindowVM.RestoreExePath;
            Properties.Settings.Default.Save();

            DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// キャンセルボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
        #endregion

        #region イベントハンドラ
        /// <summary>
        /// 入力された値を表示前にチェックする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            bool yes_parse = false;
            if (sender != null) {
                // 既存のテキストボックス文字列に、新規に一文字追加された時、その文字列が数値として意味があるかどうかをチェック
                {
                    int xx;
                    string tmp = textBox.Text + e.Text;
                    if (tmp == string.Empty) {
                        yes_parse = true;
                    }
                    else {
                        yes_parse = Int32.TryParse(tmp, out xx);

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
