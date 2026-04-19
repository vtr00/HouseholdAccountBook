using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.DbHandlers;
using HouseholdAccountBook.ViewModels.Abstract;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// OleDb設定VM
    /// </summary>
    public class OleDbSettingViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// プロバイダ名セレクタVM
        /// </summary>
        public SelectorViewModel<KeyValuePair<string, string>, string> ProviderNameSelectorVM => field ??= new(static p => p.Key);

        /// <summary>
        /// 入力されたDBファイルパス
        /// </summary>
        public string InputedDBFilePath {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        public async Task LoadAsync()
        {
            OleDbHandler.ConnectInfo connectInfo = UserSettingService.Instance.AccessConnectInfo;

            this.ProviderNameSelectorVM.SetLoader(static () => [.. OleDbHandler.GetOleDbProvider()]);
            await this.ProviderNameSelectorVM.LoadAsync(connectInfo.Provider);

            this.InputedDBFilePath = PathUtil.GetSmartPath(App.GetCurrentDir(), connectInfo.DataSource);
        }

        /// <summary>
        /// 記帳風月向け設定を読み込む
        /// </summary>
        public async Task LoadForKichoFugetsuAsync()
        {
            OleDbHandler.ConnectInfo connectInfo = UserSettingService.Instance.KichoFugetsuConnectInfo;

            this.ProviderNameSelectorVM.SetLoader(static () => new List<KeyValuePair<string, string>>(OleDbHandler.GetOleDbProvider()).FindAll(static pair => pair.Key.Contains(OleDbHandler.ConnectInfo.AccessProviderHeader)));
            await this.ProviderNameSelectorVM.LoadAsync(connectInfo.Provider);
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <returns>設定の保存成否</returns>
        public bool Save()
        {
            UserSettingService.Instance.AccessConnectInfo = new(this.ProviderNameSelectorVM.SelectedKey) {
                DataSource = Path.GetFullPath(this.InputedDBFilePath, App.GetCurrentDir())
            };

            return true;
        }

        /// <summary>
        /// 記帳風月向け設定を保存する
        /// </summary>
        /// <returns>設定の保存成否</returns>
        public bool SaveForKichoFugetsu()
        {
            UserSettingService.Instance.KichoFugetsuConnectInfo = new(this.ProviderNameSelectorVM.SelectedKey);

            return true;
        }

        /// <summary>
        /// 設定を保存可能か
        /// </summary>
        /// <returns>設定の保存可否</returns>
        public bool CanSave() => !(string.IsNullOrWhiteSpace(this.ProviderNameSelectorVM.SelectedKey) || string.IsNullOrWhiteSpace(this.InputedDBFilePath));
    }
}
