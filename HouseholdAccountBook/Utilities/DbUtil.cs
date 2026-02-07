using HouseholdAccountBook.Adapters;
using HouseholdAccountBook.Adapters.Dao.DbTable;
using HouseholdAccountBook.Adapters.DbHandlers;
using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Logger;
using System;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Utilities
{
    public static class DbUtil
    {
        #region マイグレーション
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

        /// <summary>
        /// PostgreSQLのアップマイグレーションを実行する
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <returns>成功/失敗</returns>
        private static async Task<bool> UpMigratePostgreSQLAsync(DbHandlerBase dbHandler)
        {
            using FuncLog funcLog = new();

            if (dbHandler is NpgsqlDbHandler npgsqlDbHandler) {
                MtdSchemaVersionDao dao = new(dbHandler);
                await dbHandler.ExecTransactionAsync(dao.CreateTableAsync);

                int requiredSchemaVersion = 0; // アプリが想定しているバージョン
                int currentSchemaVersion = await dao.GetSchemaVersionAsync();
                while (currentSchemaVersion < requiredSchemaVersion) {
                    switch (currentSchemaVersion) {
                        case 0:
                            // バージョン1へアップグレード
                            break;
                        default:
                            break;
                    }
                    currentSchemaVersion++;
                    _ = await dao.UpdateSchemaVersionAsync(currentSchemaVersion);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// SQLiteのアップマイグレーションを実行する
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <returns>成功/失敗</returns>
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
                    _ = await sqliteDbHandler.SetUserVersion(currentSchemaVersion);
                }

                return true;
            }
            return false;
        }
        #endregion
    }
}
