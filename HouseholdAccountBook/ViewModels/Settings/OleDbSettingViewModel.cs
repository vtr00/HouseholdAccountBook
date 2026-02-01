using HouseholdAccountBook.Adapters.DbHandlers;
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
        public string SelectedProviderName {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// DBファイルパス
        /// </summary>
        #region DBFilePath
        public string DBFilePath {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        public void Load()
        {
            Properties.Settings settings = Properties.Settings.Default;

            this.ProviderNameDic.Clear();
            OleDbHandler.GetOleDbProvider().ForEach(this.ProviderNameDic.Add);
            this.SelectedProviderName = settings.App_Access_Provider;
            this.DBFilePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), settings.App_Access_DBFilePath);
        }

        /// <summary>
        /// 記帳風月向け設定を読み込む
        /// </summary>
        public void LoadForKichoFugetsu()
        {
            Properties.Settings settings = Properties.Settings.Default;

            this.ProviderNameDic.Clear();
            OleDbHandler.GetOleDbProvider().FindAll(pair => pair.Key.Contains(OleDbHandler.AccessProviderHeader)).ForEach(this.ProviderNameDic.Add);
            this.SelectedProviderName = settings.App_Import_KichoFugetsu_Provider;
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <returns>設定の保存成否</returns>
        public bool Save()
        {
            Properties.Settings settings = Properties.Settings.Default;

            settings.App_Access_Provider = this.SelectedProviderName;
            settings.App_Access_DBFilePath = Path.GetFullPath(this.DBFilePath, App.GetCurrentDir());

            return true;
        }

        /// <summary>
        /// 記帳風月向け設定を保存する
        /// </summary>
        /// <returns>設定の保存成否</returns>
        public bool SaveForKichoFugetsu()
        {
            Properties.Settings settings = Properties.Settings.Default;

            settings.App_Import_KichoFugetsu_Provider = this.SelectedProviderName;

            return true;
        }

        /// <summary>
        /// 設定を保存可能か
        /// </summary>
        /// <returns>設定の保存可否</returns>
        public bool CanSave() => !(string.IsNullOrWhiteSpace(this.SelectedProviderName) || string.IsNullOrWhiteSpace(this.DBFilePath));
    }
}
