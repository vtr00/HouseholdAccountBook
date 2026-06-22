using HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Properties;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using static HouseholdAccountBook.Infrastructure.DB.DbConstants;

namespace HouseholdAccountBook.Infrastructure.DB.DbDao.DbTable
{
    /// <summary>
    /// アセットテーブルDAO
    /// </summary>
    /// <param name="dbHandler"></param>
    public class MstAssetDao(DbHandlerBase dbHandler) : PrimaryKeyDaoBase<MstAssetDto, int>(dbHandler)
    {
        public override async Task<int> CreateTableAsync()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            int count = 0;

            switch (this.mDbHandler.Kind) {
                case DBKind.PostgreSQL:
                    count = await this.mDbHandler.ExecuteAsync(@"
CREATE SEQUENCE IF NOT EXISTS mst_asset_asset_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 2147483647
    CACHE 1;

CREATE TABLE IF NOT EXISTS mst_asset
(
    asset_id integer NOT NULL DEFAULT nextval('mst_asset_asset_id_seq'::regclass),
    asset_name text COLLATE pg_catalog.""default"" NOT NULL,
    subunit_name text COLLATE pg_catalog.""default"",
    asset_code text COLLATE pg_catalog.""default"",
    asset_kind integer NOT NULL,
    scale integer NOT NULL,
    prefix text COLLATE pg_catalog.""default"" NOT NULL,
    suffix text COLLATE pg_catalog.""default"" NOT NULL,
    sub_prefix text COLLATE pg_catalog.""default"",
    sub_suffix text COLLATE pg_catalog.""default"",
    base_rate numeric NOT NULL,
    json_code text COLLATE pg_catalog.""default"",
    sort_order integer NOT NULL,
    del_flg integer NOT NULL,
    update_time timestamp without time zone NOT NULL,
    updater text COLLATE pg_catalog.""default"" NOT NULL,
    insert_time timestamp without time zone NOT NULL,
    inserter text COLLATE pg_catalog.""default"" NOT NULL,
    CONSTRAINT mst_asset_pkey PRIMARY KEY (asset_id),
    CONSTRAINT mst_asset_del_flg_check CHECK (del_flg = 0 OR del_flg = 1) NOT VALID,
    CONSTRAINT mst_asset_sort_order_check CHECK (sort_order >= 0) NOT VALID
);");
                    break;
                case DBKind.SQLite:
                    count = await this.mDbHandler.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS mst_asset
(
    asset_id      INTEGER PRIMARY KEY,
    asset_name    TEXT NOT NULL,
    subunit_name  TEXT,
    asset_code    TEXT,
    asset_kind    INTEGER NOT NULL,
    scale         INTEGER NOT NULL,
    prefix        TEXT NOT NULL,
    suffix        TEXT NOT NULL,
    sub_prefix    TEXT,
    sub_suffix    TEXT,
    base_rate     NUMERIC NOT NULL,
    json_code     TEXT,
    sort_order    INTEGER NOT NULL,
    del_flg       INTEGER NOT NULL,
    update_time   TEXT NOT NULL,
    updater       TEXT NOT NULL,
    insert_time   TEXT NOT NULL,
    inserter      TEXT NOT NULL,

    CONSTRAINT mst_asset_del_flg_check CHECK (del_flg IN (0, 1)),
    CONSTRAINT mst_asset_sort_order_check CHECK (sort_order >= 0)
);");
                    break;
            }

            return count;
        }

        public override async Task<IEnumerable<MstAssetDto>> FindAllAsync()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            IEnumerable<MstAssetDto> dtoList = await this.mDbHandler.QueryAsync<MstAssetDto>(@"
SELECT * 
FROM mst_asset
WHERE del_flg = 0
ORDER BY sort_order;");

            return dtoList;
        }

        /// <summary>
        /// <see cref="MstAssetDto.AssetId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="assetId">アセットID</param>
        /// <returns>取得したレコード</returns>
        public override async Task<MstAssetDto> FindByIdAsync(int assetId)
        {
            using FuncLog funcLog = new(new { assetId }, Log.LogLevel.Trace);

            MstAssetDto dto = await this.mDbHandler.QuerySingleOrDefaultAsync<MstAssetDto>(@"
SELECT *
FROM mst_asset
WHERE asset_id = @AssetId AND del_flg = 0;",
new MstAssetDto { AssetId = assetId });

            return dto;
        }

        public override async Task SetIdSequenceAsync(int idSeq)
        {
            using FuncLog funcLog = new(new { idSeq }, Log.LogLevel.Trace);

            _ = await this.mDbHandler.ExecuteAsync(@"SELECT setval('mst_asset_asset_id_seq', @AssetIdSeq);", new { AssetIdSeq = idSeq }, DBKindMask.PostgreSQL);
        }

        public override async Task<int> InsertAsync(MstAssetDto dto)
        {
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
INSERT INTO mst_asset
(asset_id, asset_code, asset_name, asset_kind, scale, prefix, suffix, sub_prefix, sub_suffix, base_rate, json_code, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@AssetId, @AssetCode, @AssetName, @AssetKind, @Scale, @Prefix, @Suffix, @SubPrefix, @SubSuffix, @BaseRate, @JsonCode, @SortOrder, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter);", dto);

            return count;
        }

        public override async Task<int> InsertReturningIdAsync(MstAssetDto dto)
        {
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

            int assetId = await this.mDbHandler.QuerySingleAsync<int>(@"
INSERT INTO mst_asset
(asset_code, asset_name, asset_kind, scale, prefix, suffix, sub_prefix, sub_suffix, base_rate, json_code, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@AssetCode, @AssetName, @AssetKind, @Scale, @Prefix, @Suffix, @SubPrefix, @SubSuffix, @BaseRate, @JsonCode, (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_asset), @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter)
RETURNING asset_id;", dto);

            return assetId;
        }

        /// <summary>
        /// デフォルトデータを挿入し、主キーを返す
        /// </summary>
        /// <returns>主キー</returns>
        public async Task<int> InsertDefaultReturningIdAsync()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            MstAssetDto dto = new() {
                AssetName = Resources.Unit_DefaultMoney,
                SubunitName = null,
                AssetCode = "JPY",
                AssetKind = (int)AssetKind.Currency,
                Scale = 0,
                Prefix = @"\",
                Suffix = string.Empty,
                SubPrefix = null,
                SubSuffix = null,
                BaseRate = 1
            };

            return await this.InsertReturningIdAsync(dto);
        }

        public override Task<int> UpdateAsync(MstAssetDto dto) => throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");

        /// <summary>
        /// <see cref="MstAssetDto.AssetId"/> に基づいて、 <see cref="MstAssetDto.AssetName"/> 等を更新する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>更新件数</returns>
        public async Task<int> UpdateSetableAsync(MstAssetDto dto)
        {
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE mst_asset
SET asset_name = @AssetName, subunit_name = @SubunitName, asset_code = @AssetCode, asset_kind = @AssetKind, scale = @Scale, 
    prefix = @Prefix, suffix = @Suffix, sub_prefix = @SubPrefix, sub_suffix = @SubSuffix, base_rate = @BaseRate, update_time = @UpdateTime, updater = @Updater
WHERE asset_id = @AssetId;", dto);

            return count;
        }

        /// <summary>
        /// レコードのソート順を入れ替える
        /// </summary>
        /// <param name="assetId1">アセットID1</param>
        /// <param name="assetId2">アセットID2</param>
        /// <returns>更新件数</returns>
        /// <remarks>PostgreSQLとSQLiteで挙動が変わる可能性があるため変更時は要動作確認</remarks>
        public async Task<int> SwapSortOrderAsync(int assetId1, int assetId2)
        {
            using FuncLog funcLog = new(new { assetId1, assetId2 }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@" 
WITH original AS (
  SELECT
    (SELECT sort_order FROM mst_asset WHERE asset_id = @AssetId1) AS sort_order1,
    (SELECT sort_order FROM mst_asset WHERE asset_id = @AssetId2) AS sort_order2
)
UPDATE mst_asset
SET sort_order = CASE
  WHEN asset_id = @AssetId1 THEN (SELECT sort_order2 FROM original)
  WHEN asset_id = @AssetId2 THEN (SELECT sort_order1 FROM original)
  ELSE sort_order
END, update_time = @UpdateTime, updater = @Updater
WHERE asset_id IN (@AssetId1, @AssetId2);",
new { AssetId1 = assetId1, AssetId2 = assetId2, UpdateTime = DateTime.Now, Updater });

            return count;
        }

        public override Task<int> UpsertAsync(MstAssetDto dto) => throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");

        public override async Task<int> DeleteAllAsync()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"DELETE FROM mst_asset;");

            return count;
        }

        /// <summary>
        /// <see cref="MstAssetDto.AssetId"/> に基づいて、レコードを削除する
        /// </summary>
        /// <param name="assetId">アセットID</param>
        /// <returns>削除件数</returns>
        public override async Task<int> DeleteByIdAsync(int assetId)
        {
            using FuncLog funcLog = new(new { assetId }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE mst_asset
SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE asset_id = @AssetId;",
new MstAssetDto { AssetId = assetId });

            return count;
        }
    }
}
