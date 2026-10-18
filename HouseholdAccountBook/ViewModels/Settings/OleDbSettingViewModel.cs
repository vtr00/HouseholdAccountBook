using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.DbHandlers;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// OleDb設定VM
    /// </summary>
    public class OleDbSettingViewModel : ViewModelBase
    {
        #region プロパティ
        /// <summary>
        /// プロバイダ名リストVM
        /// </summary>
        public SelectableViewModel<KeyValuePair<string, string>, string> ProviderNameListVM => field ??= new(static p => p.Key);
        /// <summary>
        /// DBファイルパスVM
        /// </summary>
        public InputableViewModel<string> DBFilePathVM { get; } = new();

        /// <summary>
        /// 未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager DirtyStatus => field ??= new(this.ProviderNameListVM, this.DBFilePathVM);
        #endregion

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        public async Task LoadAsync()
        {
            OleDbHandler.ConnectInfo connectInfo = UserSettingService.Instance.AccessConnectInfo;

            this.ProviderNameListVM.SetLoader(static () => [.. OleDbHandler.GetOleDbProvider()]);
            await this.ProviderNameListVM.LoadAsync(connectInfo.Provider);

            this.DBFilePathVM.LoadedValue = PathUtil.GetSmartPath(App.GetCurrentDir(), connectInfo.DataSource);
        }

        /// <summary>
        /// 記帳風月向け設定を読み込む
        /// </summary>
        public async Task LoadForKichoFugetsuAsync()
        {
            OleDbHandler.ConnectInfo connectInfo = UserSettingService.Instance.KichoFugetsuConnectInfo;

            this.ProviderNameListVM.SetLoader(static () => new List<KeyValuePair<string, string>>(OleDbHandler.GetOleDbProvider()).FindAll(static pair => pair.Key.Contains(OleDbHandler.ConnectInfo.AccessProviderHeader)));
            await this.ProviderNameListVM.LoadAsync(connectInfo.Provider);
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <returns>設定の保存成否</returns>
        public bool Save()
        {
            UserSettingService.Instance.AccessConnectInfo = new(this.ProviderNameListVM.SelectedKey) {
                DataSource = Path.GetFullPath(this.DBFilePathVM.InputedValue, App.GetCurrentDir())
            };
            this.DirtyStatus.AcceptChanges();

            return true;
        }

        /// <summary>
        /// 記帳風月向け設定を保存する
        /// </summary>
        /// <returns>設定の保存成否</returns>
        public bool SaveForKichoFugetsu()
        {
            UserSettingService.Instance.KichoFugetsuConnectInfo = new(this.ProviderNameListVM.SelectedKey);
            this.DirtyStatus.AcceptChanges();

            return true;
        }

        /// <summary>
        /// 設定を保存可能か
        /// </summary>
        /// <returns>設定の保存可否</returns>
        public bool CanSave() => !(string.IsNullOrWhiteSpace(this.ProviderNameListVM.SelectedKey) || string.IsNullOrWhiteSpace(this.DBFilePathVM.InputedValue));
    }
}
