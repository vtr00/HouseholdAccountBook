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
        /// ログファイル ポストフィックス
        /// </summary>
        private static string LogFileExt => "txt";
        /// <summary>
        /// ログファイルパス
        /// </summary>
        public static string LogFilePath {
            get {
                App app = Application.Current as App;
                DateTime dt = app.StartupTime;
                return string.Format($@"{LogFolderPath}\{dt:yyyyMMdd_HHmmss}.{LogFileExt}");
            }
        }
        /// <summary>
        /// ログファイル名パターン
        /// </summary>
        public static string LogFileNamePattern => $"*_*.{LogFileExt}";
        #endregion

        #region DBバックアップファイル
        /// <summary>
        /// PostgreSQL バックアップファイル ポストフィックス
        /// </summary>
        private static string PostgreSQLBackupFileExt => "backup";
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
        /// PostgreSQL バックアップファイル名パターン
        /// </summary>
        public static string PostgreSQLBackupFileNamePattern => $"*_*.{PostgreSQLBackupFileExt}";
        /// <summary>
        /// SQLite バックアップファイル ポストフィックス
        /// </summary>
        private static string SQLiteBackupFileExt => "sqlite3";
        /// <summary>
        /// SQLite バックアップファイル名
        /// </summary>
        public static string SQLiteBackupFileName {
            get {
                DateTime dt = DateTime.Now;
                return string.Format($"{dt:yyyyMMdd_HHmmss}.{SQLiteBackupFileExt}");
            }
        }
        /// <summary>
        /// SQLite バックアップファイル名パターン
        /// </summary>
        public static string SQLiteBackupFileNamePattern => $"*_*.{SQLiteBackupFileExt}";
        #endregion

        #region 捕捉されない例外情報ファイル
        /// <summary>
        /// 捕捉されない例外情報のファイルのフォルダパス
        /// </summary>
        public static string UnhandledExceptionInfoFolderPath => @".\UnhandledExceptions";
        /// <summary>
        /// 捕捉されない例外情報のファイル ポストフィックス
        /// </summary>
        private static string UnhandledExceptionInfoFileExt => "txt";
        /// <summary>
        /// 捕捉されない例外情報のファイルパス
        /// </summary>
        public static string UnhandledExceptionInfoFilePath {
            get {
                DateTime dt = DateTime.Now;
                return string.Format($@"{UnhandledExceptionInfoFolderPath}\{dt:yyyyMMdd_HHmmss}.{UnhandledExceptionInfoFileExt}");
            }
        }
        /// <summary>
        /// 捕捉されない例外情報のファイル名パターン
        /// </summary>
        public static string UnhandledExceptionInfoFileNamePattern => $"*_*.{UnhandledExceptionInfoFileExt}";
        #endregion

        #region ウィンドウ情報ファイル
        /// <summary>
        /// ウィンドウ情報のファイルのフォルダパス
        /// </summary>
        public static string WindowLocationFolderPath => @".\WindowLocations";
        /// <summary>
        /// ウィンドウ情報のファイル ポストフィックス
        /// </summary>
        private static string WindowLocationFileExt => "txt";
        /// <summary>
        /// ウィンドウ情報のファイルパス
        /// </summary>
        public static string WindowLocationFilePath(string windowName)
        {
            App app = Application.Current as App;
            DateTime dt = app.StartupTime;
            return string.Format($@"{WindowLocationFolderPath}\{windowName}_{dt:yyyyMMdd_HHmmss}.{WindowLocationFileExt}");
        }
        /// <summary>
        /// ウィンドウ情報のファイル名パターン
        /// </summary>
        public static string WindowLocationFileNamePattern(string windowName) => $"{windowName}_*_*.{WindowLocationFileExt}";
        #endregion

        /// <summary>
        /// 設定ファイルのファイルパス
        /// </summary>
        public static string SettingsJsonFilePath => @".\Settings.json";
    }
}
