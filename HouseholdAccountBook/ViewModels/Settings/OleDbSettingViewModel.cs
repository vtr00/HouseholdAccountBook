using HouseholdAccountBook.Infrastructure.Utilities;
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
        public SelectorViewModel<KeyValuePair<string, string>, string> ProviderNameSelectorVM { get; } = new(static p => p.Key);

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
            Properties.Settings settings = Properties.Settings.Default;

            this.ProviderNameSelectorVM.SetLoader(static () => [.. OleDbHandler.GetOleDbProvider()]);
            await this.ProviderNameSelectorVM.LoadAsync(settings.App_Access_Provider);

            this.InputedDBFilePath = PathUtil.GetSmartPath(App.GetCurrentDir(), settings.App_Access_DBFilePath);
        }

        /// <summary>
        /// 記帳風月向け設定を読み込む
        /// </summary>
        public async Task LoadForKichoFugetsuAsync()
        {
            Properties.Settings settings = Properties.Settings.Default;

            this.ProviderNameSelectorVM.SetLoader(static () => new List<KeyValuePair<string, string>>(OleDbHandler.GetOleDbProvider()).FindAll(static pair => pair.Key.Contains(OleDbHandler.ConnectInfo.AccessProviderHeader)));
            await this.ProviderNameSelectorVM.LoadAsync(settings.App_Import_KichoFugetsu_Provider);
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <returns>設定の保存成否</returns>
        public bool Save()
        {
            Properties.Settings settings = Properties.Settings.Default;

            settings.App_Access_Provider = this.ProviderNameSelectorVM.SelectedKey;
            settings.App_Access_DBFilePath = Path.GetFullPath(this.InputedDBFilePath, App.GetCurrentDir());

            return true;
        }

        /// <summary>
        /// 記帳風月向け設定を保存する
        /// </summary>
        /// <returns>設定の保存成否</returns>
        public bool SaveForKichoFugetsu()
        {
            Properties.Settings settings = Properties.Settings.Default;

            settings.App_Import_KichoFugetsu_Provider = this.ProviderNameSelectorVM.SelectedKey;

            return true;
        }

        /// <summary>
        /// 設定を保存可能か
        /// </summary>
        /// <returns>設定の保存可否</returns>
        public bool CanSave() => !(string.IsNullOrWhiteSpace(this.ProviderNameSelectorVM.SelectedKey) || string.IsNullOrWhiteSpace(this.InputedDBFilePath));
    }
}
