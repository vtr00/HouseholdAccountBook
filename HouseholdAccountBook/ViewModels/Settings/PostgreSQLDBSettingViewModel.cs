using HouseholdAccountBook.Adapters;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.ViewModels.Abstract;
using System;
using System.IO;

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
        public string Host {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// ポート
        /// </summary>
        #region Port
        public int? Port {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// ユーザ名
        /// </summary>
        #region UserName
        public string UserName {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// パスワード
        /// </summary>
        #region Password
        public string Password {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// データベース名
        /// </summary>
        #region DatabaseName
        public string DatabaseName {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// ロール名
        /// </summary>
        #region Role
        public string Role {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// pg_dump.exeパス
        /// </summary>
        #region DumpExePath
        public string DumpExePath {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// pg_restore.exeパス
        /// </summary>
        #region RestoreExePath
        public string RestoreExePath {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// パスワード入力方法
        /// </summary>
        #region PasswordInput
        public PostgresPasswordInput PasswordInput {
            get;
            set => this.SetProperty(ref field, value);
        } = PostgresPasswordInput.InputWindow;
        #endregion
        #endregion

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        /// <param name="setPassword">パスワードを設定するデリゲート</param>
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
            this.PasswordInput = (PostgresPasswordInput)settings.App_Postgres_Password_Input;
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <param name="getPassword">パスワードを取得するデリゲート</param>
        /// <returns>設定の保存成否</returns>
        public bool Save(Func<string> getPassword)
        {
            Properties.Settings settings = Properties.Settings.Default;

            settings.App_Postgres_Host = this.Host;
            settings.App_Postgres_Port = this.Port.Value;
            settings.App_Postgres_UserName = this.UserName;
            settings.App_Postgres_Password = string.Empty; // パスワードは暗号化して保存するので、空にしておく
            if (getPassword != null) {
                settings.App_Postgres_EncryptedPassword = ProtectedDataExtension.EncryptPassword(getPassword?.Invoke());
            }
            settings.App_Postgres_DatabaseName = this.DatabaseName;
            settings.App_Postgres_Role = this.Role;
            settings.App_Postgres_DumpExePath = Path.GetFullPath(this.DumpExePath, App.GetCurrentDir());
            settings.App_Postgres_RestoreExePath = Path.GetFullPath(this.RestoreExePath, App.GetCurrentDir());
            settings.App_Postgres_Password_Input = (int)this.PasswordInput;

            return true;
        }

        /// <summary>
        /// 設定を保存可能か
        /// </summary>
        /// <param name="getPassword">パスワードを取得するデリゲート</param>
        /// <returns>設定の保存可否</returns>
        public bool CanSave(Func<string> getPassword) => !(
            string.IsNullOrWhiteSpace(this.Host) || !this.Port.HasValue || this.Port < 1 || this.Port > 65535
            || string.IsNullOrWhiteSpace(this.UserName) || string.IsNullOrWhiteSpace(getPassword?.Invoke())
            || string.IsNullOrWhiteSpace(this.DatabaseName)
            || string.IsNullOrWhiteSpace(this.DumpExePath) || !File.Exists(this.DumpExePath)
            || string.IsNullOrWhiteSpace(this.RestoreExePath) || !File.Exists(this.RestoreExePath)
            );
    }
}
