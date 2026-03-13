using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.Utilities;
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
        /// 入力されたホスト
        /// </summary>
        #region InputedHost
        public string InputedHost {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 入力されたポート
        /// </summary>
        #region InputedPort
        public int? InputedPort {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 入力されたユーザ名
        /// </summary>
        #region InputedUserName
        public string InputedUserName {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 入力されたパスワード
        /// </summary>
        #region InputedPassword
        public string InputedPassword {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 入力されたデータベース名
        /// </summary>
        #region InoutedDatabaseName
        public string InputedDatabaseName {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 入力されたロール名
        /// </summary>
        #region InoutedRole
        public string InputedRole {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 入力されたpg_dump.exeパス
        /// </summary>
        #region InputedDumpExePath
        public string InputedDumpExePath {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 入力されたpg_restore.exeパス
        /// </summary>
        #region InputedRestoreExePath
        public string InputedRestoreExePath {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 選択されたパスワード入力方法
        /// </summary>
        #region SelectedPasswordInput
        public PostgresPasswordInput SelectedPasswordInput {
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

            this.InputedHost = settings.App_Postgres_Host;
            this.InputedPort = settings.App_Postgres_Port;
            this.InputedUserName = settings.App_Postgres_UserName;
            setPassword?.Invoke(settings.App_Postgres_Password == string.Empty ?
                ProtectedDataUtil.DecryptPassword(settings.App_Postgres_EncryptedPassword) : settings.App_Postgres_Password);
#if DEBUG
            this.InputedDatabaseName = settings.App_Postgres_DatabaseName_Debug;
#else
            this.DatabaseName = settings.App_Postgres_DatabaseName;
#endif
            this.InputedRole = settings.App_Postgres_Role;
            this.InputedDumpExePath = PathUtil.GetSmartPath(App.GetCurrentDir(), settings.App_Postgres_DumpExePath);
            this.InputedRestoreExePath = PathUtil.GetSmartPath(App.GetCurrentDir(), settings.App_Postgres_RestoreExePath);
            this.SelectedPasswordInput = (PostgresPasswordInput)settings.App_Postgres_Password_Input;
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <param name="getPassword">パスワードを取得するデリゲート</param>
        /// <returns>設定の保存成否</returns>
        public bool Save(Func<string> getPassword)
        {
            Properties.Settings settings = Properties.Settings.Default;

            settings.App_Postgres_Host = this.InputedHost;
            settings.App_Postgres_Port = this.InputedPort.Value;
            settings.App_Postgres_UserName = this.InputedUserName;
            settings.App_Postgres_Password = string.Empty; // パスワードは暗号化して保存するので、空にしておく
            if (getPassword != null) {
                settings.App_Postgres_EncryptedPassword = ProtectedDataUtil.EncryptPassword(getPassword?.Invoke());
            }
            settings.App_Postgres_DatabaseName = this.InputedDatabaseName;
            settings.App_Postgres_Role = this.InputedRole;
            settings.App_Postgres_DumpExePath = Path.GetFullPath(this.InputedDumpExePath, App.GetCurrentDir());
            settings.App_Postgres_RestoreExePath = Path.GetFullPath(this.InputedRestoreExePath, App.GetCurrentDir());
            settings.App_Postgres_Password_Input = (int)this.SelectedPasswordInput;

            return true;
        }

        /// <summary>
        /// 設定を保存可能か
        /// </summary>
        /// <param name="getPassword">パスワードを取得するデリゲート</param>
        /// <returns>設定の保存可否</returns>
        public bool CanSave(Func<string> getPassword) => !(
            string.IsNullOrWhiteSpace(this.InputedHost) || !this.InputedPort.HasValue || this.InputedPort < 1 || this.InputedPort > 65535
            || string.IsNullOrWhiteSpace(this.InputedUserName) || string.IsNullOrWhiteSpace(getPassword?.Invoke())
            || string.IsNullOrWhiteSpace(this.InputedDatabaseName)
            || string.IsNullOrWhiteSpace(this.InputedDumpExePath) || !File.Exists(this.InputedDumpExePath)
            || string.IsNullOrWhiteSpace(this.InputedRestoreExePath) || !File.Exists(this.InputedRestoreExePath)
            );
    }
}
