using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.DbHandlers;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Common;
using System.IO;
using System.Windows;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// SQLite設定VM
    /// </summary>
    public class SQLiteSettingViewModel : ViewModelBase
    {
        #region プロパティ
        /// <summary>
        /// DBファイルパスVM
        /// </summary>
        public InputableViewModel<string> DBFilePathVM { get; } = new();
        /// <summary>
        /// キャッシュサイズ[MB] VM
        /// </summary>
        public InputableViewModel<uint?> CacheSizeVM { get; } = new();
        /// <summary>
        /// メモリマップドI/Oサイズ[MB] VM
        /// </summary>
        public InputableViewModel<uint?> MmapSizeVM { get; } = new();

        /// <summary>
        /// 未保存状態マネージャ
        /// </summary>
        public DirtyStatusManager DirtyStatus => field ??= new(this.DBFilePathVM, this.CacheSizeVM, this.MmapSizeVM);
        #endregion

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        public void Load()
        {
            SQLiteDbHandler.ConnectInfo connectInfo = UserSettingService.Instance.SQLiteConnectInfo;
            this.DBFilePathVM.LoadedValue = PathUtil.GetSmartPath(App.GetCurrentDir(), connectInfo.FilePath);

            SQLiteDbHandler.PragmaConfiguration config = UserSettingService.Instance.SQLiteConfig;
            this.CacheSizeVM.LoadedValue = config.CacheSize;
            this.MmapSizeVM.LoadedValue = config.MmapSize;
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <returns>設定の保存成否</returns>
        public bool Save()
        {
            bool result = false;

            string sqliteFilePath = Path.GetFullPath(this.DBFilePathVM.InputedValue);
            bool exists = File.Exists(sqliteFilePath);
            if (!exists) {
                // ファイルが存在しない場合、新規作成するか確認する
                if (MessageBox.Show(Properties.Resources.Message_NotFoundFileDoYouCreateNew, Properties.Resources.Title_Conformation, MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    // SQLiteのテンプレートファイルをコピーして新規作成する
                    byte[] sqliteBinary = Properties.Resources.SQLiteTemplateFile;
                    if (SQLiteDbHandler.CreateTemplateFile(sqliteFilePath, sqliteBinary)) {
                        exists = true;
                    }
                }
            }
            // ファイルが存在する場合、接続設定を保存する
            if (exists) {
                SQLiteDbHandler.ConnectInfo connectInfo = new() {
                    FilePath = Path.GetFullPath(sqliteFilePath, App.GetCurrentDir())
                };
                UserSettingService.Instance.SQLiteConnectInfo = connectInfo;
                this.DBFilePathVM.AcceptChanges();
                result = true;
            }

            SQLiteDbHandler.PragmaConfiguration config = new() {
                CacheSize = this.CacheSizeVM.InputedValue.Value,
                MmapSize = this.MmapSizeVM.InputedValue.Value
            };
            UserSettingService.Instance.SQLiteConfig = config;
            this.CacheSizeVM.AcceptChanges();
            this.MmapSizeVM.AcceptChanges();

            return result;
        }

        /// <summary>
        /// 設定を保存可能か
        /// </summary>
        /// <returns>設定の保存可否</returns>
        public bool CanSave() => !string.IsNullOrWhiteSpace(this.DBFilePathVM.InputedValue) && this.CacheSizeVM.InputedValue.HasValue && this.MmapSizeVM.InputedValue.HasValue;
    }
}
