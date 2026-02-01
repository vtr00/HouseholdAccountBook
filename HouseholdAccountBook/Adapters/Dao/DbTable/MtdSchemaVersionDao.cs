using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandlers;
using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.DbTable
{
    /// <summary>
    /// スキーマバージョンテーブルDAO
    /// </summary>
    /// <param name="dbHandler"></param>
    public class MtdSchemaVersionDao(DbHandlerBase dbHandler) : PhyTableDaoBase<MtdSchemaVersionDto>(dbHandler)
    {
        public override async Task CreateTableAsync()
        {
            if (this.mDbHandler.DBKind != DBKind.PostgreSQL) {
                throw new NotSupportedException("This method is only supported for PostgreSQL.");
            }
            if (!this.mDbHandler.InTransaction()) {
                throw new InvalidOperationException("Cannot create table during a transaction.");
            }

            // テーブルがなければ作成する
            _ = await this.mDbHandler.ExecuteAsync($@"
CREATE TABLE IF NOT EXISTS mtd_schema_version (
    version INTEGER NOT NULL,
    update_time timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT mtd_schema_version_check CHECK (version >= 0)
) TABLESPACE pg_default;");

            if (this.mDbHandler is NpgsqlDbHandler npgsqlDbHandler) {
                string roll = npgsqlDbHandler.GetDbCreationRoll();
                // オーナーを設定する
                _ = await npgsqlDbHandler.ExecuteAsync($@"
ALTER TABLE IF EXISTS mtd_schema_version
    OWNER to ""{roll}"";
");
            }

            // レコードがなければ、初期レコードを挿入する
            _ = await this.mDbHandler.ExecuteAsync(@"
INSERT INTO mtd_schema_version (version)
SELECT 0
WHERE NOT EXISTS (SELECT 1 FROM mtd_schema_version);");
        }

        public override async Task<IEnumerable<MtdSchemaVersionDto>> FindAllAsync()
        {
            var dtoList = await this.mDbHandler.QueryAsync<MtdSchemaVersionDto>(@"
SELECT * 
FROM mtd_schema_version;");

            return dtoList;
        }

        public override Task<int> InsertAsync(MtdSchemaVersionDto dto) => throw new NotImplementedException();
        public override Task<int> UpdateAsync(MtdSchemaVersionDto dto) => throw new NotImplementedException();
        public override Task<int> UpsertAsync(MtdSchemaVersionDto dto) => throw new NotImplementedException();

        public override Task<int> DeleteAllAsync() => throw new NotImplementedException();

        public async Task<int> GetSchemaVersionAsync() => await this.mDbHandler.QuerySingleAsync<int>("SELECT version FROM mtd_schema_version;");

        public async Task<int> UpdateSchemaVersionAsync(int version)
        {
            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE mtd_schema_version
SET version = @Version, update_time = CURRENT_TIMESTAMP;",
new { Version = version });
            return count;
        }
    }
}
