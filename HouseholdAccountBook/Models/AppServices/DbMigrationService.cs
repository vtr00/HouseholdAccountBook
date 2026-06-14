using HouseholdAccountBook.Infrastructure.DB;
using HouseholdAccountBook.Infrastructure.DB.DbDao.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models.DbHandlers;
using System;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.AppServices
{
    /// <summary>
    /// DBマイグレーションサービス
    /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
    public class DbMigrationService(DbHandlerFactory dbHandlerFactory)
    {
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory mDbHandlerFactory = dbHandlerFactory;

        /// <summary>
        /// スキーマバージョンを取得する
        /// </summary>
        /// <returns>スキーマバージョン</returns>
        public async Task<int> GetSchemaVersionAsync()
        {
            using FuncLog funcLog = new();

            int version = 0;
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                version = await GetSchemaVersionCoreAsync(dbHandler);
            }

            return version;
        }

        /// <summary>
        /// スキーマバージョンを取得する(本処理)
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <returns>取得したスキーマバージョン</returns>
        private static async Task<int> GetSchemaVersionCoreAsync(DbHandlerBase dbHandler)
        {
            int version = 0;
            switch (dbHandler.Kind) {
                case DBKind.PostgreSQL:
                    MtdSchemaVersionDao dao = new(dbHandler);
                    await dbHandler.ExecTransactionAsync(dao.CreateTableAsync);
                    version = await dao.SelectSchemaVersionAsync();
                    break;
                case DBKind.SQLite:
                    if (dbHandler is SQLiteDbHandler sqliteDbHandler) {
                        version = await sqliteDbHandler.GetUserVersion();
                    }
                    break;
                default:
                    throw new NotSupportedException("Unsupported DB kind.");
            }

            return version;
        }

        /// <summary>
        /// スキーマバージョンを設定する(本処理)
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="version">設定するスキーマバージョン</param>
        /// <returns></returns>
        private static async Task SetSchemaVersionCoreAsync(DbHandlerBase dbHandler, int version)
        {
            switch (dbHandler.Kind) {
                case DBKind.PostgreSQL:
                    MtdSchemaVersionDao dao = new(dbHandler);
                    _ = await dao.UpsertSchemaVersionAsync(version);
                    break;
                case DBKind.SQLite:
                    if (dbHandler is SQLiteDbHandler sqliteDbHandler) {
                        _ = await sqliteDbHandler.SetUserVersion(version);
                    }
                    break;
            }
        }

        #region アップマイグレーション
        /// <summary>
        /// アップマイグレーションを実行する
        /// </summary>
        /// <returns>成功/失敗</returns>
        public async Task<bool> UpMigrateAsync()
        {
            using FuncLog funcLog = new();

            bool result = true;
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                int currentSchemaVersion = await GetSchemaVersionCoreAsync(dbHandler);
                int requiredSchemaVersion = 1; // アプリが想定しているバージョン
                while (currentSchemaVersion < requiredSchemaVersion) {
                    switch (currentSchemaVersion) {
                        case 0: {
                            // バージョン1へアップグレード
                            MstAssetDao dao = new(dbHandler);
                            _ = await dao.CreateTableAsync();
                            int assetId = await dao.InsertDefaultReturningIdAsync();
                            UserSettingService.Instance.DefaultAssetId = assetId;
                            break;
                        }
                        case 1:
                            // バージョン2へアップグレード
                            break;
                        default:
                            break;
                    }
                    currentSchemaVersion++;
                    await SetSchemaVersionCoreAsync(dbHandler, currentSchemaVersion);
                }
            }

            return result;
        }
        #endregion
    }
}
