using HouseholdAccountBook.Adapters;
using HouseholdAccountBook.Adapters.Dao.Compositions;
using HouseholdAccountBook.Adapters.DbHandlers;
using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Dto.Others;
using HouseholdAccountBook.Adapters.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HouseholdAccountBook.Utilities
{
    public class DbBackUpManager : SingletonBase<DbBackUpManager>
    {
        #region プロパティ
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

        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        public DbHandlerFactory DbHandlerFactory { get; set; }

        /// <summary>
        /// メインウィンドウを閉じるときにバックアップを行うか
        /// </summary>
        public bool BackUpFlagAtClosing { get; set; }
        /// <summary>
        /// メインウィンドウを最小化したときにバックアップを行うか
        /// </summary>
        public bool BackUpFlagAtMinimizing { get; set; }
        /// <summary>
        /// メインウィンドウを最小化したときにバックアップを行った時刻
        /// </summary>
        public DateTime BackUpCurrentAtMinimizing { get; set; }
        /// <summary>
        /// メインウィンドウを最小化したときにバックアップを行う間隔[ms]
        /// </summary>
        public int BackUpIntervalMinAtMinimizing { get; set; }
        /// <summary>
        /// メインウィンドウを最小化したときにバックアップを行ったあと通知するか
        /// </summary>
        public bool BackUpNotifyAtMinimizing { get; set; }
        /// <summary>
        /// バックアップの個数
        /// </summary>
        public int BackUpNum { get; set; }
        /// <summary>
        /// バックアップを行うフォルダパス
        /// </summary>
        public string BackUpFolderPath { get; set; }
        /// <summary>
        /// pg_dump.exeのパス
        /// </summary>
        public string PostgresDumpExePath { get; set; }
        /// <summary>
        /// pg_restore.exeのパス
        /// </summary>
        public string PostgresRestoreExePath { get; set; }
        /// <summary>
        /// PostgreSQL パスワード入力方法
        /// </summary>
        public PostgresPasswordInput PostgresPasswordInput { get; set; }
        #endregion

        /// <summary>
        /// スタティックコンストラクタ
        /// </summary>
        static DbBackUpManager() => Register(static () => new DbBackUpManager());
        /// <summary>
        /// プライベートコンストラクタ
        /// </summary>
        private DbBackUpManager() { }

        /// <summary>
        /// メインウィンドウクローズ時のバックアップを実行する
        /// </summary>
        /// <param name="lastBackupTime">最後にバックアップした日時</param>
        /// <returns>成功/失敗 または 未実施</returns>
        public async Task<bool> ExecuteAtMainWindowClosing(DateTime? lastBackupTime = null)
        {
            using FuncLog funcLog = new();

            if (lastBackupTime.HasValue && lastBackupTime.Value >= await this.GetDbRowUpdateTime()) { return false; }

            bool result = false;
            if (this.BackUpFlagAtClosing) {
                // 通知しても即座に終了するため通知しない
                result = await this.CreateBackUpFileAsync();
            }
            return result;
        }

        /// <summary>
        /// メインウィンドウ状態変更時のバックアップを実行する
        /// </summary>
        /// <param name="windowState">ウィンドウの状態</param>
        /// <param name="lastBackupTime">最後にバックアップした日時</param>
        /// <returns>成功/失敗 または 未実施</returns>
        public async Task<bool> ExecuteAtMainWindowStateChanged(WindowState windowState, DateTime? lastBackupTime = null)
        {
            using FuncLog funcLog = new(new { windowState });

            if (lastBackupTime.HasValue && lastBackupTime.Value >= await this.GetDbRowUpdateTime()) { return false; }

            bool result = false;
            if (windowState == WindowState.Minimized) {
                if (this.BackUpFlagAtMinimizing) {
                    Log.Info(string.Format($"BackUpCurrentAtMinimizing: {this.BackUpCurrentAtMinimizing}"));
                    DateTime nextBackup = this.BackUpCurrentAtMinimizing.AddMinutes(this.BackUpIntervalMinAtMinimizing);
                    Log.Info(string.Format($"NextBackup: {nextBackup}"));

                    if (nextBackup <= DateTime.Now) {
                        result = await this.CreateBackUpFileAsync(this.BackUpNotifyAtMinimizing);
                    }
                }
            }
            return result;
        }

        #region ダンプ/リストア
        /// <summary>
        /// バックアップファイルを作成する
        /// </summary>
        /// <param name="notifyResult">実行結果を通知する</param>
        /// <param name="waitForFinish">完了を待つか</param>
        /// <param name="backUpNum">バックアップ数。-1は無制限</param>
        /// <param name="backUpFolderPath">バックアップ用フォルダパス</param>
        /// <returns>成功/失敗</returns>
        public async Task<bool> CreateBackUpFileAsync(bool notifyResult = false, bool waitForFinish = true, int? backUpNum = null, string backUpFolderPath = null)
        {
            using FuncLog funcLog = new(new { notifyResult, waitForFinish, backUpNum, backUpFolderPath });

            int tmpBackUpNum = backUpNum ?? this.BackUpNum;
            string tmpBackUpFolderPath = backUpFolderPath ?? this.BackUpFolderPath;

            if (tmpBackUpFolderPath == string.Empty) { return false; }

            bool result;
            string backUpFileNamePattern = PostgreSQLBackupFileNamePattern;
            if (tmpBackUpNum is (-1) or > 0) {
                Log.Debug("Create backup file.");
                // フォルダが存在しなければ作成する
                if (!Directory.Exists(tmpBackUpFolderPath)) {
                    _ = Directory.CreateDirectory(tmpBackUpFolderPath);
                }

                DBKind selectedDBKind;
                await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                    selectedDBKind = dbHandler.DBKind;
                }

                switch (selectedDBKind) {
                    case DBKind.PostgreSQL: {
                        string backupFilePath = Path.Combine(tmpBackUpFolderPath, PostgreSQLBackupFileName);
                        backUpFileNamePattern = PostgreSQLBackupFileNamePattern;
                        result = await this.ExecuteDumpPostgreSQLAsync(backupFilePath, PostgresFormat.Custom, notifyResult, waitForFinish) == true;
                        break;
                    }
                    case DBKind.SQLite: {
                        string backupFilePath = Path.Combine(tmpBackUpFolderPath, SQLiteBackupFileName);
                        backUpFileNamePattern = SQLiteBackupFileNamePattern;
                        result = await this.ExecuteCopySQLiteAsync(backupFilePath, notifyResult);
                        break;
                    }
                    default: {
                        result = false;
                        break;
                    }
                }
            }
            else {
                result = true;
            }

            if (tmpBackUpNum != -1) {
                // 古いバックアップファイルを削除する
                FileUtil.DeleteOldFiles(tmpBackUpFolderPath, backUpFileNamePattern, tmpBackUpNum);
            }

            return result;
        }

        /// <summary>
        /// PostgreSQLのダンプを実行する
        /// </summary>
        /// <param name="backupFilePath">バックアップファイルパス</param>
        /// <param name="format">ダンプフォーマット</param>
        /// <param name="notifyResult">実行結果を通知する</param>
        /// <param name="waitForFinish">処理の完了を待機する</param>
        /// <returns>成功/失敗/不明</returns>
        public async Task<bool?> ExecuteDumpPostgreSQLAsync(string backupFilePath, PostgresFormat format, bool notifyResult = false, bool waitForFinish = true)
        {
            using FuncLog funcLog = new(new { backupFilePath, format, notifyResult, waitForFinish });

            if (this.PostgresDumpExePath == string.Empty) { return false; }

            int? errorCode = -1;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                if (dbHandler is NpgsqlDbHandler npgsqlDbHandler) {
                    errorCode = notifyResult
                        ? await npgsqlDbHandler.ExecuteDump(backupFilePath, this.PostgresDumpExePath, this.PostgresPasswordInput, format,
                            exitCode => {
                                // ダンプ結果を通知する
                                if (exitCode == 0) {
                                    NotificationUtil.NotifyFinishingToBackup();
                                }
                                else if (exitCode != null) {
                                    NotificationUtil.NotifyFailingToBackup();
                                }
                            }, waitForFinish)
                        : await npgsqlDbHandler.ExecuteDump(backupFilePath, this.PostgresDumpExePath, this.PostgresPasswordInput, format, null, waitForFinish);
                }
            }
            return errorCode == null ? null : (errorCode == 0);
        }

        /// <summary>
        /// PostgreSQLのリストアを実行する
        /// </summary>
        /// <param name="backupFilePath">バックアップファイルパス</param>
        /// <returns>成功/失敗</returns>
        public async Task<bool> ExecuteRestorePostgreSQLAsync(string backupFilePath)
        {
            using FuncLog funcLog = new(new { backupFilePath });

            if (this.PostgresRestoreExePath == string.Empty) { return false; }

            int errorCode = -1;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                if (dbHandler is NpgsqlDbHandler npgsqlDbHandler) {
                    errorCode = await npgsqlDbHandler.ExecuteRestore(backupFilePath, this.PostgresRestoreExePath, this.PostgresPasswordInput);
                }
            }
            return errorCode == 0;
        }

        /// <summary>
        /// SQLiteのDBファイルのコピーを実行する
        /// </summary>
        /// <param name="backupFilePath">バックアップファイルパス</param>
        /// <param name="notifyResult">実行結果を通知する</param>
        /// <returns>成功/失敗</returns>
        public async Task<bool> ExecuteCopySQLiteAsync(string backupFilePath, bool notifyResult = false)
        {
            using FuncLog funcLog = new(new { backupFilePath, notifyResult });

            bool result = false;
            string sourceFilePath = string.Empty;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                if (dbHandler is SQLiteDbHandler sqliteDbHandler) {
                    sourceFilePath = sqliteDbHandler.DbFilePath;
                }
            }

            if (sourceFilePath != string.Empty) {
                try {
                    File.Copy(sourceFilePath, backupFilePath, true);
                    result = true;
                }
                catch {
                    result = false;
                }
            }

            if (notifyResult) {
                if (result) {
                    NotificationUtil.NotifyFinishingToBackup();
                }
                else {
                    NotificationUtil.NotifyFailingToBackup();
                }
            }

            return result;
        }
        #endregion

        /// <summary>
        /// DBのデータが最後に更新された日時を取得する
        /// </summary>
        /// <returns>最後に更新された日時</returns>
        private async Task<DateTime> GetDbRowUpdateTime()
        {
            using FuncLog funcLog = new();

            DateTime updateTime = DateTime.MinValue;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                TableInfoDao tableInfoDao = new(dbHandler);
                IEnumerable<TableInfoDto> tableInfoDtoList = await tableInfoDao.FindByColumnName("update_time");

                foreach (TableInfoDto tableInfoDto in tableInfoDtoList) {
                    DateTimeInfoDao dateTimeInfoDao = new(dbHandler);
                    DateTime dt = await dateTimeInfoDao.GetUpdateTime(tableInfoDto);
                    updateTime = new[] { dt, updateTime }.Max();
                }
            }
            return updateTime;
        }
    }
}
