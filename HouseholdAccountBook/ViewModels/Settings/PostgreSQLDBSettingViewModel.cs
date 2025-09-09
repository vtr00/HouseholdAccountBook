using HouseholdAccountBook.Enums;
using HouseholdAccountBook.ViewModels.Abstract;

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
    }
}
