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
            this.WVM.Message = message;

            this.WVM.Host = settings.App_Postgres_Host;
            this.WVM.Port = settings.App_Postgres_Port;
            this.WVM.UserName = settings.App_Postgres_UserName;
            this.passwordBox.Password = settings.App_Postgres_Password;
#if DEBUG
            this.WVM.DatabaseName = settings.App_Postgres_DatabaseName_Debug;
#else
            this.WVM.DatabaseName = settings.App_Postgres_DatabaseName;
#endif
            this.WVM.Role = settings.App_Postgres_Role;
            this.WVM.DumpExePath = settings.App_Postgres_DumpExePath;
            this.WVM.RestoreExePath = settings.App_Postgres_RestoreExePath;
        }

        #region コマンド
        /// <summary>
        /// pg_dump.exeを指定するダイアログを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DumpExePathDialogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string directory = string.Empty;
            string fileName = string.Empty;
            if (this.WVM.DumpExePath != string.Empty) {
                directory = Path.GetDirectoryName(this.WVM.DumpExePath);
                fileName = Path.GetFileName(this.WVM.DumpExePath);
            }

            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = "ファイル選択",
                Filter = "pg_dump.exe|pg_dump.exe"
            };

            if (ofd.ShowDialog() == true) {
                this.WVM.DumpExePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            }
        }

        /// <summary>
        /// pg_restore.exeを指定するダイアログを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestorePathDialogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string directory = string.Empty;
            string fileName = string.Empty;
            if (this.WVM.RestoreExePath != string.Empty) {
                directory = Path.GetDirectoryName(this.WVM.RestoreExePath);
                fileName = Path.GetFileName(this.WVM.RestoreExePath);
            }

            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = "ファイル選択",
                Filter = "pg_restore.exe|pg_restore.exe"
            };

            if (ofd.ShowDialog() == true) {
                this.WVM.RestoreExePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
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
            Properties.Settings settings = Properties.Settings.Default;
            settings.App_Postgres_Host = this.WVM.Host;
            settings.App_Postgres_Port = this.WVM.Port;
            settings.App_Postgres_UserName = this.WVM.UserName;
            settings.App_Postgres_Password = this.passwordBox.Password;
            settings.App_Postgres_DatabaseName = this.WVM.DatabaseName;
            settings.App_Postgres_Role = this.WVM.Role;
            settings.App_Postgres_DumpExePath = this.WVM.DumpExePath;
            settings.App_Postgres_RestoreExePath = this.WVM.RestoreExePath;
            settings.Save();

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
