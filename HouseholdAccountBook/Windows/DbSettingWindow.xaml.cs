using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// DbSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DbSettingWindow : Window
    {
        /// <summary>
        /// <see cref="DbSettingWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="message">表示メッセージ</param>
        public DbSettingWindow(string message)
        {
            this.InitializeComponent();

            Properties.Settings settings = Properties.Settings.Default;
            this.WVM.Message = message;

            this.WVM.SelectedDBKind = (DBKind)settings.App_SelectedDBKind;

            // PostgreSQL
            this.WVM.PostgreSQLDBSettingVM.Host = settings.App_Postgres_Host;
            this.WVM.PostgreSQLDBSettingVM.Port = settings.App_Postgres_Port;
            this.WVM.PostgreSQLDBSettingVM.UserName = settings.App_Postgres_UserName;
            this.passwordBox.Password = settings.App_Postgres_Password;
#if DEBUG
            this.WVM.PostgreSQLDBSettingVM.DatabaseName = settings.App_Postgres_DatabaseName_Debug;
#else
            this.WVM.PostgreSQLDBSettingVM.DatabaseName = settings.App_Postgres_DatabaseName;
#endif
            this.WVM.PostgreSQLDBSettingVM.Role = settings.App_Postgres_Role;
            this.WVM.PostgreSQLDBSettingVM.DumpExePath = settings.App_Postgres_DumpExePath;
            this.WVM.PostgreSQLDBSettingVM.RestoreExePath = settings.App_Postgres_RestoreExePath;

            // Access
            this.WVM.AccessSettingVM.ProviderNameDic.Clear();
            OleDbEnumerator enumerator = new OleDbEnumerator();
            DataTable table = enumerator.GetElements();
            foreach (DataRow row in table.Rows) {
                string sourcesName = row["SOURCES_NAME"].ToString();
                string sourcesDescription = row["SOURCES_DESCRIPTION"].ToString();

                this.WVM.AccessSettingVM.ProviderNameDic.Add(new KeyValuePair<string, string>(sourcesName, $"{sourcesName} ({sourcesDescription})"));
            }
            this.WVM.AccessSettingVM.SelectedProviderName = settings.App_Access_Provider;
            this.WVM.AccessSettingVM.DBFilePath = settings.App_Access_DBFilePath;

            // SQLite
            this.WVM.SQLiteSettingVM.DBFilePath = settings.App_SQLite_DBFilePath;
        }

        #region イベントハンドラ
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
            if (this.WVM.PostgreSQLDBSettingVM.DumpExePath != string.Empty) {
                directory = Path.GetDirectoryName(this.WVM.PostgreSQLDBSettingVM.DumpExePath);
                fileName = Path.GetFileName(this.WVM.PostgreSQLDBSettingVM.DumpExePath);
            }

            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = "pg_dump.exe|pg_dump.exe"
            };

            if (ofd.ShowDialog() == true) {
                this.WVM.PostgreSQLDBSettingVM.DumpExePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
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
            if (this.WVM.PostgreSQLDBSettingVM.RestoreExePath != string.Empty) {
                directory = Path.GetDirectoryName(this.WVM.PostgreSQLDBSettingVM.RestoreExePath);
                fileName = Path.GetFileName(this.WVM.PostgreSQLDBSettingVM.RestoreExePath);
            }

            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = "pg_restore.exe|pg_restore.exe"
            };

            if (ofd.ShowDialog() == true) {
                this.WVM.PostgreSQLDBSettingVM.RestoreExePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            }
        }

        /// <summary>
        /// DBファイルパスを指定するダイアログを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DBFilePathDialogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        { 
            switch (this.WVM.SelectedDBKind) {
                case DBKind.Access: {
                    string directory = string.Empty;
                    string fileName = string.Empty;
                    if (this.WVM.AccessSettingVM.DBFilePath != string.Empty) {
                        directory = Path.GetDirectoryName(this.WVM.AccessSettingVM.DBFilePath);
                        fileName = Path.GetFileName(this.WVM.AccessSettingVM.DBFilePath);
                    }
                    OpenFileDialog ofd = new OpenFileDialog() {
                        CheckFileExists = true,
                        InitialDirectory = directory,
                        FileName = fileName,
                        Title = Properties.Resources.Title_FileSelection,
                        Filter = "Access DBファイル|*.mdb;*.accdb"
                    };
                    if (ofd.ShowDialog() == true) {
                        this.WVM.AccessSettingVM.DBFilePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
                    }
                    break;
                }
                case DBKind.SQLite: {
                    string directory = string.Empty;
                    string fileName = string.Empty;
                    if (this.WVM.SQLiteSettingVM.DBFilePath != string.Empty) {
                        directory = Path.GetDirectoryName(this.WVM.SQLiteSettingVM.DBFilePath);
                        fileName = Path.GetFileName(this.WVM.SQLiteSettingVM.DBFilePath);
                    }
                    OpenFileDialog ofd = new OpenFileDialog() {
                        CheckFileExists = true,
                        InitialDirectory = directory,
                        FileName = fileName,
                        Title = Properties.Resources.Title_FileSelection,
                        Filter = "SQLite DBファイル|*.db;*.sqlite;*.sqlite3"
                    };
                    if (ofd.ShowDialog() == true) {
                        this.WVM.SQLiteSettingVM.DBFilePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// OKボタン押下可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// OKボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;
            settings.App_SelectedDBKind = (int)this.WVM.SelectedDBKind;

            switch (this.WVM.SelectedDBKind) {
                case DBKind.PostgreSQL: {
                    settings.App_Postgres_Host = this.WVM.PostgreSQLDBSettingVM.Host;
                    settings.App_Postgres_Port = this.WVM.PostgreSQLDBSettingVM.Port;
                    settings.App_Postgres_UserName = this.WVM.PostgreSQLDBSettingVM.UserName;
                    settings.App_Postgres_Password = this.passwordBox.Password;
                    settings.App_Postgres_DatabaseName = this.WVM.PostgreSQLDBSettingVM.DatabaseName;
                    settings.App_Postgres_Role = this.WVM.PostgreSQLDBSettingVM.Role;
                    settings.App_Postgres_DumpExePath = this.WVM.PostgreSQLDBSettingVM.DumpExePath;
                    settings.App_Postgres_RestoreExePath = this.WVM.PostgreSQLDBSettingVM.RestoreExePath;
                    break;
                }
                case DBKind.Access: {
                    settings.App_Access_DBFilePath = this.WVM.AccessSettingVM.DBFilePath;
                    break;
                }
                case DBKind.SQLite: {
                    settings.App_SQLite_DBFilePath = this.WVM.SQLiteSettingVM.DBFilePath;
                    break;
                }
            }
            settings.Save();

            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// キャンセルボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        #endregion

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
                    string tmp = textBox.Text + e.Text;
                    if (tmp == string.Empty) {
                        yes_parse = true;
                    }
                    else {
                        yes_parse = Int32.TryParse(tmp, out int xx);

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
