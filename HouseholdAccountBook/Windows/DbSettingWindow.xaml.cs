using HouseholdAccountBook.Extentions;
using Microsoft.Win32;
using System;
using System.ComponentModel;
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

            this.DbSettingWindowVM.Message = message;

            this.DbSettingWindowVM.Host = Properties.Settings.Default.App_Postgres_Host;
            this.DbSettingWindowVM.Port = Properties.Settings.Default.App_Postgres_Port;
            this.DbSettingWindowVM.UserName = Properties.Settings.Default.App_Postgres_UserName;
            this.passwordBox.Password = Properties.Settings.Default.App_Postgres_Password;
#if DEBUG
            this.DbSettingWindowVM.DatabaseName = Properties.Settings.Default.App_Postgres_DatabaseName_Debug;
#else
            this.DbSettingWindowVM.DatabaseName = Properties.Settings.Default.App_Postgres_DatabaseName;
#endif
            this.DbSettingWindowVM.Role = Properties.Settings.Default.App_Postgres_Role;
            this.DbSettingWindowVM.DumpExePath = Properties.Settings.Default.App_Postgres_DumpExePath;
            this.DbSettingWindowVM.RestoreExePath = Properties.Settings.Default.App_Postgres_RestoreExePath;
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

    #region ViewModel
    /// <summary>
    /// DB設定ウィンドウVM
    /// </summary>
    public class DbSettingWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 表示メッセージ
        /// </summary>
        #region Message
        public string Message
        {
            get { return _Message; }
            set {
                if (_Message != value) {
                    _Message = value;
                    PropertyChanged.Raise(this, _nameMessage);
                }
            }
        }
        private string _Message = default(string);
        internal static readonly string _nameMessage = PropertyName<DbSettingWindowViewModel>.Get(x => x.Message);
        #endregion
        
        /// <summary>
        /// ホスト
        /// </summary>
        #region Host
        public string Host
        {
            get { return _Host; }
            set {
                if (_Host != value) {
                    _Host = value;
                    PropertyChanged.Raise(this, _nameHost);
                }
            }
        }
        private string _Host = default(string);
        internal static readonly string _nameHost = PropertyName<DbSettingWindowViewModel>.Get(x => x.Host);
        #endregion

        /// <summary>
        /// ポート
        /// </summary>
        #region Port
        public int Port
        {
            get { return _Port; }
            set {
                if (_Port != value) {
                    _Port = value;
                    PropertyChanged.Raise(this, _namePort);
                }
            }
        }
        private int _Port = default(int);
        internal static readonly string _namePort = PropertyName<DbSettingWindowViewModel>.Get(x => x.Port);
        #endregion

        /// <summary>
        /// ユーザ名
        /// </summary>
        #region UserName
        public string UserName
        {
            get { return _UserName; }
            set {
                if (_UserName != value) {
                    _UserName = value;
                    PropertyChanged.Raise(this, _nameUserName);
                }
            }
        }
        private string _UserName = default(string);
        internal static readonly string _nameUserName = PropertyName<DbSettingWindowViewModel>.Get(x => x.UserName);
        #endregion

        /// <summary>
        /// パスワード
        /// </summary>
        #region Password
        public string Password
        {
            get { return _Password; }
            set {
                if (_Password != value) {
                    _Password = value;
                    PropertyChanged.Raise(this, _namePassword);
                }
            }
        }
        private string _Password = default(string);
        internal static readonly string _namePassword = PropertyName<DbSettingWindowViewModel>.Get(x => x.Password);
        #endregion

        /// <summary>
        /// データベース名
        /// </summary>
        #region DatabaseName
        public string DatabaseName
        {
            get { return _DatabaseName; }
            set {
                if (_DatabaseName != value) {
                    _DatabaseName = value;
                    PropertyChanged.Raise(this, _nameDatabaseName);
                }
            }
        }
        private string _DatabaseName = default(string);
        internal static readonly string _nameDatabaseName = PropertyName<DbSettingWindowViewModel>.Get(x => x.DatabaseName);
        #endregion
        
        /// <summary>
        /// ロール名
        /// </summary>
        #region Role
        public string Role
        {
            get { return _Role; }
            set {
                if (_Role != value) {
                    _Role = value;
                    PropertyChanged.Raise(this, _nameRole);
                }
            }
        }
        private string _Role = default(string);
        internal static readonly string _nameRole = PropertyName<DbSettingWindowViewModel>.Get(x => x.Role);
        #endregion

        /// <summary>
        /// dump.exeパス
        /// </summary>
        #region DumpExePath
        public string DumpExePath
        {
            get { return _DumpExePath; }
            set {
                if (_DumpExePath != value) {
                    _DumpExePath = value;
                    PropertyChanged.Raise(this, _nameDumpExePath);
                }
            }
        }
        private string _DumpExePath = default(string);
        internal static readonly string _nameDumpExePath = PropertyName<DbSettingWindowViewModel>.Get(x => x.DumpExePath);
        #endregion

        /// <summary>
        /// restore.exeパス
        /// </summary>
        #region RestoreExePath
        public string RestoreExePath
        {
            get { return _RestoreExePath; }
            set {
                if (_RestoreExePath != value) {
                    _RestoreExePath = value;
                    PropertyChanged.Raise(this, _nameRestoreExePath);
                }
            }
        }
        private string _RestoreExePath = default(string);
        internal static readonly string _nameRestoreExePath = PropertyName<DbSettingWindowViewModel>.Get(x => x.RestoreExePath);
        #endregion
        
        /// <summary>
        /// プロパティ変更イベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
    #endregion
}
