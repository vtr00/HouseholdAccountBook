using System;
using System.Windows;

namespace HouseholdAccountBook.Adapters
{
    /// <summary>
    /// ファイル関連の定数定義
    /// </summary>
    public static class FileConstants
    {
        #region ログファイル
        /// <summary>
        /// ログファイルのフォルダパス
        /// </summary>
        public static string LogFolderPath => @".\Logs";
        /// <summary>
        /// ログファイルパス
        /// </summary>
        public static string LogFilePath {
            get {
                App app = Application.Current as App;
                DateTime dt = app.StartupTime;
                return string.Format($@"{LogFolderPath}\{dt:yyyyMMdd_HHmmss}.txt");
            }
        }
        #endregion

        #region DBバックアップファイル
        /// <summary>
        /// PostgreSQL バックアップファイル ポストフィックス
        /// </summary>
        public static string PostgreSQLBackupFileExt => "backup";
        /// <summary>
        /// PostgreSQL バックアップファイル名
        /// </summary>
        public static string PostgreSQLBackupFileName {
            get {
                DateTime dt = DateTime.Now;
                return string.Format($"{dt:yyyyMMdd_HHmmss}.{PostgreSQLBackupFileExt}");
            }
        }
        /// <summary>
        /// SQLite バックアップファイル ポストフィックス
        /// </summary>
        public static string SQLiteBackupFileExt => "sqlite3";
        /// <summary>
        /// SQLite バックアップファイル名
        /// </summary>
        public static string SQLiteBackupFileName {
            get {
                DateTime dt = DateTime.Now;
                return string.Format($"{dt:yyyyMMdd_HHmmss}.{SQLiteBackupFileExt}");
            }
        }
        #endregion

        #region 捕捉されない例外情報ファイル
        /// <summary>
        /// 捕捉されない例外情報のファイルのフォルダパス
        /// </summary>
        public static string UnhandledExceptionInfoFolderPath => @".\UnhandledExceptions";
        /// <summary>
        /// 捕捉されない例外情報のファイルパス
        /// </summary>
        public static string UnhandledExceptionInfoFilePath {
            get {
                DateTime dt = DateTime.Now;
                return string.Format($@"{UnhandledExceptionInfoFolderPath}\{dt:yyyyMMdd_HHmmss}.txt");
            }
        }
        #endregion

        #region ウィンドウ情報ファイル
        /// <summary>
        /// ウィンドウ情報のファイルのフォルダパス
        /// </summary>
        public static string WindowLocationFolderPath => @".\WindowLocations";
        /// <summary>
        /// ウィンドウ情報のファイルパス
        /// </summary>
        public static string WindowLocationFilePath(string windowName)
        {
            App app = Application.Current as App;
            DateTime dt = app.StartupTime;
            return string.Format($@"{WindowLocationFolderPath}\{windowName}_{dt:yyyyMMdd_HHmmss}.txt");
        }
        #endregion

        /// <summary>
        /// 設定ファイルのファイルパス
        /// </summary>
        public static string SettingsJsonFilePath => @".\Settings.json";
    }
}
