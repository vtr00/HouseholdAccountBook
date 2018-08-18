using Prism.Mvvm;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// DB設定ウィンドウVM
    /// </summary>
    public class DbSettingWindowViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 表示メッセージ
        /// </summary>
        #region Message
        public string Message
        {
            get { return this._Message; }
            set { this.SetProperty(ref this._Message, value); }
        }
        private string _Message = default(string);
        #endregion

        /// <summary>
        /// ホスト
        /// </summary>
        #region Host
        public string Host
        {
            get { return this._Host; }
            set { this.SetProperty(ref this._Host, value); }
        }
        private string _Host = default(string);
        #endregion

        /// <summary>
        /// ポート
        /// </summary>
        #region Port
        public int Port
        {
            get { return this._Port; }
            set { this.SetProperty(ref this._Port, value); }
        }
        private int _Port = default(int);
        #endregion

        /// <summary>
        /// ユーザ名
        /// </summary>
        #region UserName
        public string UserName
        {
            get { return this._UserName; }
            set { this.SetProperty(ref this._UserName, value); }
        }
        private string _UserName = default(string);
        #endregion

        /// <summary>
        /// パスワード
        /// </summary>
        #region Password
        public string Password
        {
            get { return this._Password; }
            set { this.SetProperty(ref this._Password, value); }
        }
        private string _Password = default(string);
        #endregion

        /// <summary>
        /// データベース名
        /// </summary>
        #region DatabaseName
        public string DatabaseName
        {
            get { return this._DatabaseName; }
            set { this.SetProperty(ref this._DatabaseName, value); }
        }
        private string _DatabaseName = default(string);
        #endregion

        /// <summary>
        /// ロール名
        /// </summary>
        #region Role
        public string Role
        {
            get { return this._Role; }
            set { this.SetProperty(ref this._Role, value); }
        }
        private string _Role = default(string);
        #endregion

        /// <summary>
        /// pg_dump.exeパス
        /// </summary>
        #region DumpExePath
        public string DumpExePath
        {
            get { return this._DumpExePath; }
            set { this.SetProperty(ref this._DumpExePath, value); }
        }
        private string _DumpExePath = default(string);
        #endregion

        /// <summary>
        /// pg_restore.exeパス
        /// </summary>
        #region RestoreExePath
        public string RestoreExePath
        {
            get { return this._RestoreExePath; }
            set { this.SetProperty(ref this._RestoreExePath, value); }
        }
        private string _RestoreExePath = default(string);
        #endregion
        #endregion
    }
}
