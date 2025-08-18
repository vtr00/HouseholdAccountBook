using System.Collections.Generic;
using static HouseholdAccountBook.Others.DbConstants;
using static HouseholdAccountBook.Others.UiConstants;

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
            get => this._Message;
            set => this.SetProperty(ref this._Message, value);
        }
        private string _Message = default;
        #endregion

        /// <summary>
        /// DB種別辞書
        /// </summary>
        #region DBKindDic
        public Dictionary<DBKind, string> DBKindDic { get; } = DBKindStr;
        #endregion
        /// <summary>
        /// 選択されたDB種別
        /// </summary>
        #region SelectedDBKind
        public DBKind SelectedDBKind
        {
            get => this._SelectedDBKind;
            set => this.SetProperty(ref this._SelectedDBKind, value);
        }
        private DBKind _SelectedDBKind = DBKind.PostgreSQL;
        #endregion

        /// <summary>
        /// PostgreSQL設定
        /// </summary>
        #region PostgreSQLDBSettingVM
        public PostgreSQLDBSettingViewModel PostgreSQLDBSettingVM
        {
            get => this._PostgreSQLDBSettingVM;
            set => this.SetProperty(ref this._PostgreSQLDBSettingVM, value);
        }
        private PostgreSQLDBSettingViewModel _PostgreSQLDBSettingVM = new();
        #endregion

        /// <summary>
        /// Access設定
        /// </summary>
        #region AccessSettingVM
        public OleDbSettingViewModel AccessSettingVM
        {
            get => this._AccessSettingVM;
            set => this.SetProperty(ref this._AccessSettingVM, value);
        }
        private OleDbSettingViewModel _AccessSettingVM = new();
        #endregion

        /// <summary>
        /// SQLite設定
        /// </summary>
        #region SQLiteSettingVM
        public FileDbSettingViewModel SQLiteSettingVM
        {
            get => this._SQLiteSettingVM;
            set => this.SetProperty(ref this._SQLiteSettingVM, value);
        }
        private FileDbSettingViewModel _SQLiteSettingVM = new();
        #endregion
        #endregion
    }
}
