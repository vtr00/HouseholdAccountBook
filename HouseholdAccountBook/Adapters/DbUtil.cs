using HouseholdAccountBook.Adapters.DbHandler;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Utilites;
using System;
using System.IO;
using System.Threading.Tasks;
using static HouseholdAccountBook.Adapters.FileConstants;

namespace HouseholdAccountBook.Adapters
{
    public static class DbUtil
    {
        #region ダンプ/リストア
        /// <summary>
        /// バックアップファイルを作成する
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="notifyResult">実行結果を通知する</param>
        /// <param name="waitForFinish">完了を待つか</param>
        /// <param name="backUpNum">バックアップ数</param>
        /// <param name="backUpFolderPath">バックアップ用フォルダパス</param>
        /// <returns>成功/失敗</returns>
        public static async Task<bool> CreateBackUpFileAsync(DbHandlerFactory dbHandlerFactory, bool notifyResult = false, bool waitForFinish = true, int? backUpNum = null, string backUpFolderPath = null)
        {
            using FuncLog funcLog = new(new { notifyResult, waitForFinish, backUpNum, backUpFolderPath });

            Properties.Settings settings = Properties.Settings.Default;

            int tmpBackUpNum = backUpNum ?? settings.App_BackUpNum;
            string tmpBackUpFolderPath = backUpFolderPath ?? settings.App_BackUpFolderPath;

            if (tmpBackUpFolderPath == string.Empty) { return false; }

            bool result;
            string backUpFileNamePattern = PostgreSQLBackupFileNamePattern;
            if (0 < tmpBackUpNum) {
                Log.Debug("Create backup file.");
                // フォルダが存在しなければ作成する
                if (!Directory.Exists(tmpBackUpFolderPath)) {
                    _ = Directory.CreateDirectory(tmpBackUpFolderPath);
                }

                DBKind selectedDBKind = (DBKind)settings.App_SelectedDBKind;
                switch (selectedDBKind) {
                    case DBKind.PostgreSQL: {
                        string backupFilePath = Path.Combine(tmpBackUpFolderPath, PostgreSQLBackupFileName);
                        backUpFileNamePattern = PostgreSQLBackupFileNamePattern;
                        int? exitCode = null;

                        if (settings.App_Postgres_DumpExePath != string.Empty) {
                            exitCode = await ExecuteDumpPostgreSQL(dbHandlerFactory, backupFilePath, PostgresFormat.Custom, notifyResult, waitForFinish);
                        }

                        result = exitCode == 0;
                        break;
                    }
                    case DBKind.SQLite: {
                        string backupFilePath = Path.Combine(tmpBackUpFolderPath, SQLiteBackupFileName);
                        backUpFileNamePattern = SQLiteBackupFileNamePattern;
                        try {
                            File.Copy(settings.App_SQLite_DBFilePath, backupFilePath, true);
                            result = true;
                        }
                        catch {
                            result = false;
                        }
                        break;
                    }
                    default:
                        result = false;
                        break;
                }
            }
            else {
                result = true;
            }

            // 古いバックアップファイルを削除する
            FileUtil.DeleteOldFiles(tmpBackUpFolderPath, backUpFileNamePattern, tmpBackUpNum);

            return result;
        }

        /// <summary>
        /// PostgreSQLのダンプを実行する
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="backupFilePath">バックアップファイルパス</param>
        /// <param name="format">ダンプフォーマット</param>
        /// <param name="notifyResult">実行結果を通知する</param>
        /// <param name="waitForFinish">処理の完了を待機する</param>
        /// <returns>成功/失敗/不明</returns>
        public static async Task<int?> ExecuteDumpPostgreSQL(DbHandlerFactory dbHandlerFactory, string backupFilePath, PostgresFormat format, bool notifyResult = false, bool waitForFinish = true)
        {
            using FuncLog funcLog = new(new { backupFilePath, format, notifyResult, waitForFinish });

            Properties.Settings settings = Properties.Settings.Default;

            int? result = -1;
            await using (DbHandlerBase dbHandler = await dbHandlerFactory.CreateAsync()) {
                if (dbHandler is NpgsqlDbHandler npgsqlDbHandler) {
                    result = notifyResult
                        ? await npgsqlDbHandler.ExecuteDump(backupFilePath, settings.App_Postgres_DumpExePath, (PostgresPasswordInput)settings.App_Postgres_Password_Input, format,
                            exitCode => {
                                // ダンプ結果を通知する
                                if (exitCode == 0) {
                                    NotificationUtil.NotifyFinishingToBackup();
                                }
                                else if (exitCode != null) {
                                    NotificationUtil.NotifyFailingToBackup();
                                }
                            }, waitForFinish)
                        : await npgsqlDbHandler.ExecuteDump(backupFilePath, settings.App_Postgres_DumpExePath, (PostgresPasswordInput)settings.App_Postgres_Password_Input, format, null, waitForFinish);
                }
            }
            return result;
        }

        /// <summary>
        /// PostgreSQLのリストアを実行する
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="backupFilePath">バックアップファイルパス</param>
        /// <returns>成功/失敗</returns>
        public static async Task<int> ExecuteRestorePostgreSQL(DbHandlerFactory dbHandlerFactory, string backupFilePath)
        {
            using FuncLog funcLog = new(new { backupFilePath });

            Properties.Settings settings = Properties.Settings.Default;

            int result = -1;
            await using (DbHandlerBase dbHandler = await dbHandlerFactory.CreateAsync()) {
                if (dbHandler is NpgsqlDbHandler npgsqlDbHandler) {
                    result = await npgsqlDbHandler.ExecuteRestore(backupFilePath, settings.App_Postgres_RestoreExePath, (PostgresPasswordInput)settings.App_Postgres_Password_Input);
                }
            }
            return result;
        }
        #endregion

        /// <summary>
        /// アップマイグレーションを実行する
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <returns>成功/失敗</returns>
        public static async Task<bool> UpMigrateAsync(DbHandlerFactory dbHandlerFactory)
        {
            using FuncLog funcLog = new();

            bool result = true;
            await using (DbHandlerBase dbHandler = await dbHandlerFactory.CreateAsync()) {
                result = dbHandler.DBKind switch {
                    DBKind.PostgreSQL => await UpMigratePostgreSQLAsync(dbHandler),
                    DBKind.SQLite => await UpMigrateSQLiteAsync(dbHandler),
                    _ => throw new NotSupportedException("Unsupported DB kind."),
                };
            }

            return result;
        }

        private static async Task<bool> UpMigratePostgreSQLAsync(DbHandlerBase dbHandler)
        {
            using FuncLog funcLog = new();

            if (dbHandler is NpgsqlDbHandler npgsqlDbHandler) {
                // バージョン管理テーブル作成
                await npgsqlDbHandler.ExecTransactionAsync(async () => {
                    // テーブルがなければ、バージョン管理テーブルを作成する
                    _ = await npgsqlDbHandler.ExecuteAsync($@"
CREATE TABLE IF NOT EXISTS mtd_schema_version (
    version INTEGER NOT NULL,
    update_time timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT mtd_schema_version_check CHECK (version >= 0)
) TABLESPACE pg_default;

ALTER TABLE IF EXISTS mtd_schema_version
    OWNER to ""{npgsqlDbHandler.GetDbCreationRoll()}"";
");

                    // レコードがなければ、初期レコードを挿入する
                    _ = await npgsqlDbHandler.ExecuteAsync(@"
INSERT INTO mtd_schema_version (version)
SELECT 0
WHERE NOT EXISTS (SELECT 1 FROM mtd_schema_version);");
                });

                int requiredSchemaVersion = 0; // アプリが想定しているバージョン
                int currentSchemaVersion = await npgsqlDbHandler.QuerySingleAsync<int>("SELECT version FROM mtd_schema_version;");
                while (currentSchemaVersion < requiredSchemaVersion) {
                    switch (currentSchemaVersion) {
                        case 0:
                            // バージョン1へアップグレード
                            break;
                        default:
                            break;
                    }
                    currentSchemaVersion++;
                }

                return true;
            }
            return false;
        }

        private static async Task<bool> UpMigrateSQLiteAsync(DbHandlerBase dbHandler)
        {
            using FuncLog funcLog = new();

            if (dbHandler is SQLiteDbHandler sqliteDbHandler) {
                int requiredSchemaVersion = 0; // アプリが想定しているバージョン
                int currentSchemaVersion = await sqliteDbHandler.GetUserVersion();
                while (currentSchemaVersion < requiredSchemaVersion) {
                    switch (currentSchemaVersion) {
                        case 0:
                            // バージョン1へアップグレード
                            break;
                        default:
                            break;
                    }
                    currentSchemaVersion++;
                }

                return true;
            }
            return false;
        }
    }
}
