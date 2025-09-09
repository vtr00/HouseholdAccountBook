using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.ViewModels.Abstract;
using System;
using System.IO;
using System.Windows.Navigation;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// PostgreSQL設定VM
    /// </summary>
    public class PostgreSQLDBSettingViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// ホスト
        /// </summary>
        #region Host
        public string Host
        {
            get => this._Host;
            set => this.SetProperty(ref this._Host, value);
        }
        private string _Host = default;
        #endregion

        /// <summary>
        /// ポート
        /// </summary>
        #region Port
        public int? Port
        {
            get => this._Port;
            set => this.SetProperty(ref this._Port, value);
        }
        private int? _Port = default;
        #endregion

        /// <summary>
        /// ユーザ名
        /// </summary>
        #region UserName
        public string UserName
        {
            get => this._UserName;
            set => this.SetProperty(ref this._UserName, value);
        }
        private string _UserName = default;
        #endregion

        /// <summary>
        /// パスワード
        /// </summary>
        #region Password
        public string Password
        {
            get => this._Password;
            set => this.SetProperty(ref this._Password, value);
        }
        private string _Password = default;
        #endregion

        /// <summary>
        /// データベース名
        /// </summary>
        #region DatabaseName
        public string DatabaseName
        {
            get => this._DatabaseName;
            set => this.SetProperty(ref this._DatabaseName, value);
        }
        private string _DatabaseName = default;
        #endregion

        /// <summary>
        /// ロール名
        /// </summary>
        #region Role
        public string Role
        {
            get => this._Role;
            set => this.SetProperty(ref this._Role, value);
        }
        private string _Role = default;
        #endregion

        /// <summary>
        /// pg_dump.exeパス
        /// </summary>
        #region DumpExePath
        public string DumpExePath
        {
            get => this._DumpExePath;
            set => this.SetProperty(ref this._DumpExePath, value);
        }
        private string _DumpExePath = default;
        #endregion

        /// <summary>
        /// pg_restore.exeパス
        /// </summary>
        #region RestoreExePath
        public string RestoreExePath
        {
            get => this._RestoreExePath;
            set => this.SetProperty(ref this._RestoreExePath, value);
        }
        private string _RestoreExePath = default;
        #endregion

        /// <summary>
        /// パスワード入力方法
        /// </summary>
        #region PasswordInput
        public PostgresPasswordInput PasswordInput
        {
            get => this._PasswordInput;
            set => this.SetProperty(ref this._PasswordInput, value);
        }
        private PostgresPasswordInput _PasswordInput = PostgresPasswordInput.InputWindow;
        #endregion
        #endregion

        public void Load(Action<string> setPassword)
        {
            Properties.Settings settings = Properties.Settings.Default;

            this.Host = settings.App_Postgres_Host;
            this.Port = settings.App_Postgres_Port;
            this.UserName = settings.App_Postgres_UserName;
            setPassword?.Invoke(settings.App_Postgres_Password == string.Empty ?
                ProtectedDataExtension.DecryptPassword(settings.App_Postgres_EncryptedPassword) : settings.App_Postgres_Password);
#if DEBUG
            this.DatabaseName = settings.App_Postgres_DatabaseName_Debug;
#else
            this.DatabaseName = settings.App_Postgres_DatabaseName;
#endif
            this.Role = settings.App_Postgres_Role;
            this.DumpExePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), settings.App_Postgres_DumpExePath);
            this.RestoreExePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), settings.App_Postgres_RestoreExePath);
        }

        public bool Save(Func<string> getPassword)
        {
            Properties.Settings settings = Properties.Settings.Default;

            settings.App_Postgres_Host = this.Host;
            settings.App_Postgres_Port = this.Port.Value;
            settings.App_Postgres_UserName = this.UserName;
            settings.App_Postgres_Password = string.Empty; // パスワードは暗号化して保存するので、空にしておく
            settings.App_Postgres_EncryptedPassword = ProtectedDataExtension.EncryptPassword(getPassword?.Invoke());
            settings.App_Postgres_DatabaseName = this.DatabaseName;
            settings.App_Postgres_Role = this.Role;
            settings.App_Postgres_DumpExePath = Path.GetFullPath(this.DumpExePath, App.GetCurrentDir());
            settings.App_Postgres_RestoreExePath = Path.GetFullPath(this.RestoreExePath, App.GetCurrentDir());

            return true;
        }

        public bool CanSave(Func<string> getPassword)
        {
            if (string.IsNullOrWhiteSpace(this.Host)) return false;
            if (!this.Port.HasValue || this.Port < 1 || this.Port > 65535) return false;
            if (string.IsNullOrWhiteSpace(this.UserName)) return false;
            if (string.IsNullOrWhiteSpace(getPassword?.Invoke())) return false;
            if (string.IsNullOrWhiteSpace(this.DatabaseName)) return false;
            if (string.IsNullOrWhiteSpace(this.DumpExePath) || !File.Exists(this.DumpExePath)) return false;
            if (string.IsNullOrWhiteSpace(this.RestoreExePath) || !File.Exists(this.RestoreExePath)) return false;
            return true;
        }
    }
}
