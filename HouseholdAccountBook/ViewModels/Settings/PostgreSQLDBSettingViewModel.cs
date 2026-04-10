using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.DbHandlers;
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
        public string InputedHost {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力されたポート
        /// </summary>
        public int? InputedPort {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力されたユーザ名
        /// </summary>
        public string InputedUserName {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力されたパスワード
        /// </summary>
        public string InputedPassword {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力されたデータベース名
        /// </summary>
        public string InputedDatabaseName {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力されたロール名
        /// </summary>
        public string InputedRole {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力されたpg_dump.exeパス
        /// </summary>
        public string InputedDumpExePath {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 入力されたpg_restore.exeパス
        /// </summary>
        public string InputedRestoreExePath {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 選択されたパスワード入力方法
        /// </summary>
        public PostgresPasswordInput SelectedPasswordInput {
            get;
            set => this.SetProperty(ref field, value);
        } = PostgresPasswordInput.InputWindow;
        #endregion

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        /// <param name="setPassword">パスワードを設定するデリゲート</param>
        public void Load(Action<string> setPassword)
        {
            NpgsqlDbHandler.ConnectInfo connectInfo = UserSettingService.Instance.NpgsqlConnectInfo;
            this.InputedHost = connectInfo.Host;
            this.InputedPort = connectInfo.Port;
            this.InputedUserName = connectInfo.UserName;
            setPassword?.Invoke(connectInfo.Password);
            this.InputedDatabaseName = connectInfo.DatabaseName;
            this.InputedRole = connectInfo.Role;

            NpgsqlDbHandler.BackupConfiguration config = UserSettingService.Instance.PostgreSQLBackupConfig;
            this.InputedDumpExePath = PathUtil.GetSmartPath(App.GetCurrentDir(), config.DumpExePath);
            this.InputedRestoreExePath = PathUtil.GetSmartPath(App.GetCurrentDir(), config.RestoreExePath);
            this.SelectedPasswordInput = config.PasswordInput;
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <param name="getPassword">パスワードを取得するデリゲート</param>
        /// <returns>設定の保存成否</returns>
        public bool Save(Func<string> getPassword)
        {
            UserSettingService.Instance.NpgsqlConnectInfo = new() {
                Host = this.InputedHost,
                Port = this.InputedPort.Value,
                UserName = this.InputedUserName,
                Password = getPassword?.Invoke() ?? string.Empty,
                DatabaseName = this.InputedDatabaseName,
                Role = this.InputedRole
            };

            UserSettingService.Instance.PostgreSQLBackupConfig = new() {
                DumpExePath = this.InputedRestoreExePath,
                RestoreExePath = this.InputedRestoreExePath,
                PasswordInput = this.SelectedPasswordInput
            };

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
