using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.DbHandlers;
using HouseholdAccountBook.ViewModels.Abstract;
using System.IO;
using System.Windows;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// SQLite設定VM
    /// </summary>
    public class SQLiteSettingViewModel : BindableBase
    {
        /// <summary>
        /// 入力されたDBファイルパス
        /// </summary>
        public string InputedDBFilePath {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        public void Load()
        {
            SQLiteDbHandler.ConnectInfo connectInfo = UserSettingService.Instance.SQLiteConnectInfo;
            this.InputedDBFilePath = PathUtil.GetSmartPath(App.GetCurrentDir(), connectInfo.FilePath);
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <returns>設定の保存成否</returns>
        public bool Save()
        {
            bool result = false;

            string sqliteFilePath = Path.GetFullPath(this.InputedDBFilePath);
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
            // ファイルが存在する場合、設定を保存する
            if (exists) {
                SQLiteDbHandler.ConnectInfo connectInfo = new() {
                    FilePath = Path.GetFullPath(sqliteFilePath, App.GetCurrentDir())
                };
                UserSettingService.Instance.SQLiteConnectInfo = connectInfo;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 設定を保存可能か
        /// </summary>
        /// <returns>設定の保存可否</returns>
        public bool CanSave() => !string.IsNullOrWhiteSpace(this.InputedDBFilePath);
    }
}
