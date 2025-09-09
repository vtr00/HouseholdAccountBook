using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.ViewModels.Abstract;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// OleDb設定VM
    /// </summary>
    public class OleDbSettingViewModel : BindableBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public OleDbSettingViewModel() { }

        /// <summary>
        /// プロバイダ名
        /// </summary>
        /// <remarks>動的に指定するため<see cref="ObservableCollection{T}"/>を用いる</remarks>
        public ObservableCollection<KeyValuePair<string, string>> ProviderNameDic { get; } = [];
        /// <summary>
        /// 選択されたプロバイダ名
        /// </summary>
        #region SelectedProviderName
        public string SelectedProviderName
        {
            get => this._SelectedProviderName;
            set => this.SetProperty(ref this._SelectedProviderName, value);
        }
        private string _SelectedProviderName = default;
        #endregion

        /// <summary>
        /// DBファイルパス
        /// </summary>
        #region DBFilePath
        public string DBFilePath
        {
            get => this._DBFilePath;
            set => this.SetProperty(ref this._DBFilePath, value);
        }
        private string _DBFilePath = default;
        #endregion

        public void Load()
        {
            Properties.Settings settings = Properties.Settings.Default;

            this.ProviderNameDic.Clear();
            OleDbHandler.GetOleDbProvider().ForEach(this.ProviderNameDic.Add);
            this.SelectedProviderName = settings.App_Access_Provider;
            this.DBFilePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), settings.App_Access_DBFilePath);
        }

        public bool Save()
        {
            Properties.Settings settings = Properties.Settings.Default;

            settings.App_Access_Provider = this.SelectedProviderName;
            settings.App_Access_DBFilePath = Path.GetFullPath(this.DBFilePath, App.GetCurrentDir());
            return true;
        }

        public bool CanSave()
        {
            if (string.IsNullOrWhiteSpace(this.SelectedProviderName)) return false;
            if (string.IsNullOrWhiteSpace(this.DBFilePath)) return false;
            return true;
        }
    }
}
