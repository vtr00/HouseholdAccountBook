using HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.DbHandlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HouseholdAccountBook.Infrastructure.DB
{
    /// <summary>
    /// DBバックアップマネージャ
    /// </summary>
    public class DbBackUpManager : SingletonBase<DbBackUpManager>
    {
        /// <summary>
        /// バックアップ設定
        /// </summary>
        public class Configuration
        {
            /// <summary>
            /// メインウィンドウを閉じるときにバックアップを行うか
            /// </summary>
            public bool ExecuteAtClosing { get; set; }
            /// <summary>
            /// メインウィンドウを最小化したときにバックアップを行うか
            /// </summary>
            public bool ExecuteAtMinimizing { get; set; }
            /// <summary>
            /// メインウィンドウを最小化したときにバックアップを行う間隔[min.]
            /// </summary>
            public int IntervalMinAtMinimizing { get; set; }
            /// <summary>
            /// メインウィンドウを最小化したときにバックアップを行ったあと通知するか
            /// </summary>
            public bool NotifyAtMinimizing { get; set; }
            /// <summary>
            /// バックアップの個数
            /// </summary>
            public int Amount { get; set; }
            /// <summary>
            /// バックアップを行うフォルダパス
            /// </summary>
            public string TargetFolderPath { get; set; }
            /// <summary>
            /// バックアップ条件
            /// </summary>
            public BackUpCondition Condition { get; set; }
        }

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
        /// バックアップ設定
        /// </summary>
        public Configuration Config { get; set; }
        /// <summary>
        /// Npgsqlバックアップ設定
        /// </summary>
        public NpgsqlDbHandler.BackupConfiguration NpgsqlBackupConfig { get; set; }

        /// <summary>
        /// メインウィンドウを最小化したときにバックアップを行った時刻
        /// </summary>
        public DateTime BackUpCurrentAtMinimizing { get; set; }
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
        public async Task<bool> ExecuteAtMainWindowClosing(DateTime? lastBackupTime)
        {
            using FuncLog funcLog = new(new { lastBackupTime });

            if (this.Config.Condition == BackUpCondition.Updated && lastBackupTime.HasValue && lastBackupTime.Value >= await this.GetDbRowUpdateTime()) { return false; }

            bool result = false;
            if (this.Config.ExecuteAtClosing) {
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
        public async Task<bool> ExecuteAtMainWindowStateChanged(WindowState windowState, DateTime? lastBackupTime)
        {
            using FuncLog funcLog = new(new { windowState, lastBackupTime });

            if (this.Config.Condition == BackUpCondition.Updated && lastBackupTime.HasValue && lastBackupTime.Value >= await this.GetDbRowUpdateTime()) { return false; }

            bool result = false;
            if (windowState == WindowState.Minimized) {
                if (this.Config.ExecuteAtMinimizing) {
                    Log.Info(string.Format($"BackUpCurrentAtMinimizing: {this.BackUpCurrentAtMinimizing}"));
                    DateTime nextBackup = this.BackUpCurrentAtMinimizing.AddMinutes(this.Config.IntervalMinAtMinimizing);
                    Log.Info(string.Format($"NextBackup: {nextBackup}"));

                    if (nextBackup <= DateTime.Now) {
                        result = await this.CreateBackUpFileAsync(this.Config.NotifyAtMinimizing);
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
        /// <returns>成功/失敗 または 未実施</returns>
        public async Task<bool> CreateBackUpFileAsync(bool notifyResult = false, bool waitForFinish = true, int? backUpNum = null, string backUpFolderPath = null)
        {
            using FuncLog funcLog = new(new { notifyResult, waitForFinish, backUpNum, backUpFolderPath });

            int tmpBackUpNum = backUpNum ?? this.Config.Amount;
            string tmpBackUpFolderPath = backUpFolderPath ?? this.Config.TargetFolderPath;

            if (tmpBackUpFolderPath == string.Empty) { return false; }

            bool result;

            DBKind selectedDBKind;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                selectedDBKind = dbHandler.Kind;
            }
            if (tmpBackUpNum is -1 or > 0) {
                Log.Debug("Create backup file.");
                // フォルダが存在しなければ作成する
                if (!Directory.Exists(tmpBackUpFolderPath)) {
                    _ = Directory.CreateDirectory(tmpBackUpFolderPath);
                }

                string backupFilePath = Path.Combine(tmpBackUpFolderPath, selectedDBKind switch {
                    DBKind.PostgreSQL => PostgreSQLBackupFileName,
                    DBKind.SQLite => SQLiteBackupFileName,
                    _ => string.Empty
                });
                result = selectedDBKind switch {
                    DBKind.PostgreSQL => await this.ExecuteDumpPostgreSQLAsync(backupFilePath, PostgresFormat.Custom, notifyResult, waitForFinish) == true,
                    DBKind.SQLite => await this.ExecuteCopySQLiteAsync(backupFilePath, notifyResult),
                    _ => false
                };
            }
            else {
                result = false;
            }

            if (tmpBackUpNum != -1) {
                // 古いバックアップファイルを削除する
                string backUpFileNamePattern = selectedDBKind switch {
                    DBKind.PostgreSQL => PostgreSQLBackupFileNamePattern,
                    DBKind.SQLite => SQLiteBackupFileNamePattern,
                    _ => string.Empty
                };
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

            if (this.NpgsqlBackupConfig.DumpExePath == string.Empty) { return false; }

            int? errorCode = -1;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                if (dbHandler is NpgsqlDbHandler npgsqlDbHandler) {
                    errorCode = notifyResult
                        ? await npgsqlDbHandler.ExecuteDump(backupFilePath, this.NpgsqlBackupConfig, format,
                            exitCode => {
                                // ダンプ結果を通知する
                                if (exitCode == 0) {
                                    NotificationService.NotifyFinishingToBackup();
                                }
                                else if (exitCode != null) {
                                    NotificationService.NotifyFailingToBackup();
                                }
                            }, waitForFinish)
                        : await npgsqlDbHandler.ExecuteDump(backupFilePath, this.NpgsqlBackupConfig, format, null, waitForFinish);
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

            if (this.NpgsqlBackupConfig.RestoreExePath == string.Empty) { return false; }

            int errorCode = -1;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                if (dbHandler is NpgsqlDbHandler npgsqlDbHandler) {
                    errorCode = await npgsqlDbHandler.ExecuteRestore(backupFilePath, this.NpgsqlBackupConfig);
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
                    NotificationService.NotifyFinishingToBackup();
                }
                else {
                    NotificationService.NotifyFailingToBackup();
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
