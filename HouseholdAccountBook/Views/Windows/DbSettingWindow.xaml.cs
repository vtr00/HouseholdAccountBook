using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Extensions;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static HouseholdAccountBook.Models.DbConstants;

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
            this.passwordBox.Password = settings.App_Postgres_Password == string.Empty ? 
                ProtectedDataExtension.DecryptPassword(settings.App_Postgres_EncryptedPassword) : settings.App_Postgres_Password;
#if DEBUG
            this.WVM.PostgreSQLDBSettingVM.DatabaseName = settings.App_Postgres_DatabaseName_Debug;
#else
            this.WVM.PostgreSQLDBSettingVM.DatabaseName = settings.App_Postgres_DatabaseName;
#endif
            this.WVM.PostgreSQLDBSettingVM.Role = settings.App_Postgres_Role;
            this.WVM.PostgreSQLDBSettingVM.DumpExePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), settings.App_Postgres_DumpExePath);
            this.WVM.PostgreSQLDBSettingVM.RestoreExePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), settings.App_Postgres_RestoreExePath);

            // SQLite
            this.WVM.SQLiteSettingVM.DBFilePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), settings.App_SQLite_DBFilePath);

            // Access
            this.WVM.AccessSettingVM.ProviderNameDic.Clear();
            OleDbHandler.GetOleDbProvider().ForEach(this.WVM.AccessSettingVM.ProviderNameDic.Add);
            this.WVM.AccessSettingVM.SelectedProviderName = settings.App_Access_Provider;
            this.WVM.AccessSettingVM.DBFilePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), settings.App_Access_DBFilePath);
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
            (string directory, string fileName) = PathExtensions.GetSeparatedPath(this.WVM.PostgreSQLDBSettingVM.DumpExePath, App.GetCurrentDir());
            if (string.IsNullOrWhiteSpace(directory)) {
                directory = App.GetCurrentDir();
            }

            OpenFileDialog ofd = new() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = "pg_dump.exe|pg_dump.exe"
            };

            if (ofd.ShowDialog(this) == true) {
                this.WVM.PostgreSQLDBSettingVM.DumpExePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), ofd.FileName);
            }
        }

        /// <summary>
        /// pg_restore.exeを指定するダイアログを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestorePathDialogCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            (string directory, string fileName) = PathExtensions.GetSeparatedPath(this.WVM.PostgreSQLDBSettingVM.RestoreExePath, App.GetCurrentDir());
            if (string.IsNullOrWhiteSpace(directory)) {
                directory = App.GetCurrentDir();
            }

            OpenFileDialog ofd = new() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = "pg_restore.exe|pg_restore.exe"
            };

            if (ofd.ShowDialog(this) == true) {
                this.WVM.PostgreSQLDBSettingVM.RestoreExePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), ofd.FileName);
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
                    (string directory, string fileName) = PathExtensions.GetSeparatedPath(this.WVM.AccessSettingVM.DBFilePath, App.GetCurrentDir());
                    OpenFileDialog ofd = new() {
                        CheckFileExists = false,
                        InitialDirectory = directory,
                        FileName = fileName,
                        Title = Properties.Resources.Title_FileSelection,
                        Filter = $"{Properties.Resources.FileSelectFilter_AccessFile}|*.mdb;*.accdb"
                    };
                    if (ofd.ShowDialog(this) == true) {
                        this.WVM.AccessSettingVM.DBFilePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), ofd.FileName);
                    }
                    break;
                }
                case DBKind.SQLite: {
                    (string directory, string fileName) = PathExtensions.GetSeparatedPath(this.WVM.SQLiteSettingVM.DBFilePath, App.GetCurrentDir());
                    OpenFileDialog ofd = new() {
                        CheckFileExists = false,
                        InitialDirectory = directory,
                        FileName = fileName,
                        Title = Properties.Resources.Title_FileSelection,
                        Filter = $"{Properties.Resources.FileSelectFilter_SQLiteFile}|*.db;*.sqlite;*.sqlite3"
                    };
                    if (ofd.ShowDialog(this) == true) {
                        this.WVM.SQLiteSettingVM.DBFilePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), ofd.FileName);
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
            switch (this.WVM.SelectedDBKind) {
                case DBKind.SQLite: {
                    e.CanExecute = this.WVM.SQLiteSettingVM.DBFilePath != string.Empty;
                    break;
                }
                case DBKind.PostgreSQL: {
                    e.CanExecute = this.WVM.PostgreSQLDBSettingVM.Host != string.Empty && this.WVM.PostgreSQLDBSettingVM.Port != null &&
                                   this.WVM.PostgreSQLDBSettingVM.UserName != string.Empty && this.passwordBox.Password != string.Empty &&
                                   this.WVM.PostgreSQLDBSettingVM.DatabaseName != string.Empty && this.WVM.PostgreSQLDBSettingVM.Role != string.Empty;
                    break;
                }
                case DBKind.Access:
                case DBKind.Undefined:
                default:
                    e.CanExecute = false;
                    break;
            }
        }

        /// <summary>
        /// OKボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool result = false;
            Properties.Settings settings = Properties.Settings.Default;
            settings.App_SelectedDBKind = (int)this.WVM.SelectedDBKind;

            switch (this.WVM.SelectedDBKind) {
                case DBKind.PostgreSQL: {
                    settings.App_Postgres_Host = this.WVM.PostgreSQLDBSettingVM.Host;
                    settings.App_Postgres_Port = this.WVM.PostgreSQLDBSettingVM.Port.Value;
                    settings.App_Postgres_UserName = this.WVM.PostgreSQLDBSettingVM.UserName;
                    settings.App_Postgres_Password = string.Empty; // パスワードは暗号化して保存するので、空にしておく
                    settings.App_Postgres_EncryptedPassword = ProtectedDataExtension.EncryptPassword(this.passwordBox.Password);
                    settings.App_Postgres_DatabaseName = this.WVM.PostgreSQLDBSettingVM.DatabaseName;
                    settings.App_Postgres_Role = this.WVM.PostgreSQLDBSettingVM.Role;
                    settings.App_Postgres_DumpExePath = Path.GetFullPath(this.WVM.PostgreSQLDBSettingVM.DumpExePath, App.GetCurrentDir());
                    settings.App_Postgres_RestoreExePath = Path.GetFullPath(this.WVM.PostgreSQLDBSettingVM.RestoreExePath, App.GetCurrentDir());

                    result = true;
                    break;
                }
                case DBKind.SQLite: {
                    string sqliteFilePath = Path.GetFullPath(this.WVM.SQLiteSettingVM.DBFilePath);
                    bool exists = File.Exists(sqliteFilePath);
                    if (!exists) {
                        if (MessageBox.Show(Properties.Resources.Message_NotFoundFileDoYouCreateNew, Properties.Resources.Title_Conformation, MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                            byte[] sqliteBinary = Properties.Resources.SQLiteTemplateFile;
                            try {
                                File.WriteAllBytes(sqliteFilePath, sqliteBinary);
                                exists = true;
                            }
                            catch { }
                        }
                    }
                    if (exists) {
                        settings.App_SQLite_DBFilePath = Path.GetFullPath(sqliteFilePath, App.GetCurrentDir());
                        result = true;
                    }
                    break;
                }
                case DBKind.Access: {
                    settings.App_Access_DBFilePath = Path.GetFullPath(this.WVM.AccessSettingVM.DBFilePath, App.GetCurrentDir());
                    result = true;
                    break;
                }
            }

            if (result) {
                settings.Save();
                this.DialogResult = true;
                this.Close();
            }
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
        /// ウィンドウ初期化完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DbSettingWindow_Initialized(object sender, EventArgs e)
        {
            this.LoadWindowSetting();
        }

        /// <summary>
        /// ウィンドウクローズ後
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DbSettingWindow_Closed(object sender, EventArgs e)
        {
            this.SaveWindowSetting();
        }

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

        #region 設定反映用の関数
        /// <summary>
        /// ウィンドウ設定を読み込む
        /// </summary>
        public void LoadWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.DbSettingWindow_Width != -1 && settings.DbSettingWindow_Height != -1) {
                this.Width = settings.DbSettingWindow_Width;
                this.Height = settings.DbSettingWindow_Height;
            }

            if (settings.App_IsPositionSaved && -10 <= settings.DbSettingWindow_Left && 0 <= settings.DbSettingWindow_Top) {
                this.Left = settings.DbSettingWindow_Left;
                this.Top = settings.DbSettingWindow_Top;
            }
            else {
                this.MoveOwnersCenter();
            }
        }

        /// <summary>
        /// ウィンドウ設定を保存する
        /// </summary>
        public void SaveWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (this.WindowState == WindowState.Normal) {
                if (settings.App_IsPositionSaved) {
                    settings.DbSettingWindow_Left = this.Left;
                    settings.DbSettingWindow_Top = this.Top;
                }
                settings.DbSettingWindow_Width = this.Width;
                settings.DbSettingWindow_Height = this.Height;
                settings.Save();
            }
        }
        #endregion
    }
}
