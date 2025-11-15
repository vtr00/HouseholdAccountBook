using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.ViewModels.Abstract;
using System.IO;
using System.Windows;

namespace HouseholdAccountBook.ViewModels.Settings
{
    public class FileDbSettingViewModel : BindableBase
    {
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
            this.DBFilePath = PathExtensions.GetSmartPath(App.GetCurrentDir(), settings.App_SQLite_DBFilePath);
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        /// <returns>設定の保存成否</returns>
        public bool Save()
        {
            bool result = false;

            string sqliteFilePath = Path.GetFullPath(this.DBFilePath);
            bool exists = File.Exists(sqliteFilePath);
            if (!exists) {
                if (MessageBox.Show(Properties.Resources.Message_NotFoundFileDoYouCreateNew, Properties.Resources.Title_Conformation, MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    byte[] sqliteBinary = Properties.Resources.SQLiteTemplateFile;
                    try {
                        File.WriteAllBytes(sqliteFilePath, sqliteBinary);
                        exists = true;
                    }
                    catch { }
                }
            }
            if (exists) {
                Properties.Settings settings = Properties.Settings.Default;
                settings.App_SQLite_DBFilePath = Path.GetFullPath(sqliteFilePath, App.GetCurrentDir());
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 設定を保存可能か
        /// </summary>
        /// <returns>設定の保存可否</returns>
        public bool CanSave() => !string.IsNullOrWhiteSpace(this.DBFilePath);
    }
}
