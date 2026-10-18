using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.DbHandlers;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Common;
using System;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// PostgreSQL設定VM
    /// </summary>
    public class PostgreSQLSettingViewModel : ViewModelBase
    {
        #region プロパティ
        /// <summary>
        /// ホストVM
        /// </summary>
        public InputableViewModel<string> HostVM { get; } = new();
        /// <summary>
        /// ポートVM
        /// </summary>
        public InputableViewModel<int?> PortVM { get; } = new();
        /// <summary>
        /// ユーザ名VM
        /// </summary>
        public InputableViewModel<string> UserNameVM { get; } = new();
        /// <summary>
        /// データベース名VM
        /// </summary>
        public InputableViewModel<string> DatabaseNameVM { get; } = new();
        /// <summary>
        /// ロール名VM
        /// </summary>
        public InputableViewModel<string> RoleVM { get; } = new();
        /// <summary>
        /// pg_dump.exe パスVM
        /// </summary>
        public InputableViewModel<string> DumpExePathVM { get; } = new();
        /// <summary>
        /// pg_restore.exe パスVM
        /// </summary>
        public InputableViewModel<string> RestoreExePathVM { get; } = new();
        /// <summary>
        /// パスワード入力方法VM
        /// </summary>
        public InputableViewModel<PostgresPasswordInput> PasswordInputVM { get; } = new();

        /// <summary>
        /// 未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager DirtyStatus => field ??=
            new(this.HostVM, this.PortVM, this.UserNameVM, this.DatabaseNameVM, this.RoleVM, this.DumpExePathVM, this.RestoreExePathVM, this.PasswordInputVM);
        #endregion

        /// <summary>
        /// 設定をVMに読み込む
        /// </summary>
        /// <param name="setPassword">パスワードを設定するデリゲート</param>
        public void Load(Action<string> setPassword)
        {
            NpgsqlDbHandler.ConnectInfo connectInfo = UserSettingService.Instance.NpgsqlConnectInfo;
            this.HostVM.LoadedValue = connectInfo.Host;
            this.PortVM.LoadedValue = connectInfo.Port;
            this.UserNameVM.LoadedValue = connectInfo.UserName;
            setPassword?.Invoke(connectInfo.Password);
            this.DatabaseNameVM.LoadedValue = connectInfo.DatabaseName;
            this.RoleVM.LoadedValue = connectInfo.Role;

            NpgsqlDbHandler.BackupConfiguration config = UserSettingService.Instance.PostgreSQLBackupConfig;
            this.DumpExePathVM.LoadedValue = PathUtil.GetSmartPath(App.GetCurrentDir(), config.DumpExePath);
            this.RestoreExePathVM.LoadedValue = PathUtil.GetSmartPath(App.GetCurrentDir(), config.RestoreExePath);
            this.PasswordInputVM.LoadedValue = config.PasswordInput;
        }

        /// <summary>
        /// 接続情報の入力値を得る
        /// </summary>
        /// <param name="getPassword">パスワードを取得するデリゲート</param>
        /// <returns>接続情報</returns>
        private NpgsqlDbHandler.ConnectInfo GetConnectInfo(Func<string> getPassword)
        {
            return new() {
                Host = this.HostVM.InputedValue,
                Port = this.PortVM.InputedValue.Value,
                UserName = this.UserNameVM.InputedValue,
                Password = getPassword?.Invoke() ?? string.Empty,
                DatabaseName = this.DatabaseNameVM.InputedValue,
                Role = this.RoleVM.InputedValue
            };
        }

        /// <summary>
        /// バックアップ設定の入力値を得る
        /// </summary>
        /// <returns>バックアップ設定</returns>
        private NpgsqlDbHandler.BackupConfiguration GetBackupConfiguration()
        {
            return new() {
                DumpExePath = this.DumpExePathVM.InputedValue,
                RestoreExePath = this.RestoreExePathVM.InputedValue,
                PasswordInput = this.PasswordInputVM.InputedValue
            };
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <param name="getPassword">パスワードを取得するデリゲート</param>
        /// <returns>設定の保存成否</returns>
        public bool Save(Func<string> getPassword)
        {
            NpgsqlDbHandler.ConnectInfo connInfo = this.GetConnectInfo(getPassword);
            if (!connInfo.CheckValidation()) {
                return false;
            }
            NpgsqlDbHandler.BackupConfiguration config = this.GetBackupConfiguration();
            if (!config.CheckValidation()) {
                return false;
            }

            UserSettingService.Instance.NpgsqlConnectInfo = connInfo;
            UserSettingService.Instance.PostgreSQLBackupConfig = config;

            this.HostVM.AcceptChanges();
            this.PortVM.AcceptChanges();
            this.UserNameVM.AcceptChanges();
            this.DatabaseNameVM.AcceptChanges();
            this.RoleVM.AcceptChanges();
            this.DumpExePathVM.AcceptChanges();
            this.RestoreExePathVM.AcceptChanges();
            this.PasswordInputVM.AcceptChanges();

            return true;
        }

        /// <summary>
        /// 設定を保存可能か
        /// </summary>
        /// <param name="getPassword">パスワードを取得するデリゲート</param>
        /// <returns>設定の保存可否</returns>
        public bool CanSave(Func<string> getPassword) => this.PortVM.InputedValue.HasValue && this.GetConnectInfo(getPassword).CheckValidation() && this.GetBackupConfiguration().CheckValidation();
    }
}
