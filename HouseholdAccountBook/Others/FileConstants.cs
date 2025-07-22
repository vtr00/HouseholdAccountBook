using System;

namespace HouseholdAccountBook.Others
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
        public static string LogFolderPath = @".\Logs";
        /// <summary>
        /// ログファイルパス
        /// </summary>
        public static string LogFilePath
        {
            get {
                DateTime dt = App.StartupTime;
                return string.Format($@"{LogFolderPath}\{dt:yyyyMMdd_HHmmss}.txt");
            }
        }
        #endregion

        #region バックアップファイル
        /// <summary>
        /// バックアップファイル名
        /// </summary>
        public static string BackupFileName
        {
            get {
                DateTime dt = DateTime.Now;
                return string.Format($"{dt:yyyyMMdd_HHmmss}.backup");
            }
        }
        #endregion

        #region 捕捉されない例外情報ファイル
        /// <summary>
        /// 捕捉されない例外情報のファイルのフォルダパス
        /// </summary>
        public static string UnhandledExceptionInfoFolderPath = @".\UnhandledExceptions";
        /// <summary>
        /// 捕捉されない例外情報のファイルパス
        /// </summary>
        public static string UnhandledExceptionInfoFilePath
        {
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
        public static string WindowLocationFolderPath = @".\WindowLocations";
        /// <summary>
        /// ウィンドウ情報のファイルパス
        /// </summary>
        public static string WindowLocationFilePath(string windowName)
        {
            DateTime dt = App.StartupTime;
            return string.Format($@"{WindowLocationFolderPath}\{windowName}_{dt:yyyyMMdd_HHmmss}.txt");
        }
        #endregion
    }
}
