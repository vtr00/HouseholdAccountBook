using HouseholdAccountBook.Extentions;
using System.ComponentModel;

namespace HouseholdAccountBook.ViewModels
{
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
        /// pg_dump.exeパス
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
        /// pg_restore.exeパス
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
}
