using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Dto.Others;
using HouseholdAccountBook.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Compositions
{
    public class TableInfoDao(DbHandlerBase dbHandler) : TableDaoBase(dbHandler)
    {
        /// <summary>
        /// 指定のカラムを含むテーブルの <see cref="TableInfoDto"/> を取得する
        /// </summary>
        /// <param name="columnName">検索するカラム名</param>
        /// <returns>取得したレコード</returns>
        public async Task<IEnumerable<TableInfoDto>> FindByColumnName(string columnName)
        {
            if (!columnName.IsValidDBIdentifier()) {
                throw new ArgumentException($"Invalid column name: {columnName}");
            }

            IEnumerable<TableInfoDto> dtoList;
            switch (this.mDbHandler.DBKind) {
                case DBKind.PostgreSQL: {
                    dtoList = await this.mDbHandler.QueryAsync<TableInfoDto>($@"
SELECT table_schema, table_name
FROM information_schema.columns
WHERE column_name = '{columnName}'
  AND table_schema NOT IN ('pg_catalog', 'information_schema')
ORDER BY table_schema, table_name;");
                    break;
                }
                case DBKind.SQLite: {
                    dtoList = await this.mDbHandler.QueryAsync<TableInfoDto>($@"
SELECT '' AS table_schema, m.name AS table_name
FROM sqlite_master AS m
JOIN pragma_table_info(m.name) AS p ON 1=1
WHERE m.type = 'table'
  AND m.name NOT LIKE 'sqlite_%'
  AND p.name = '{columnName}'
ORDER BY m.name;");
                    break;
                }
                default: {
                    throw new NotSupportedException($"DB Kind: {this.mDbHandler.DBKind} is not supported.");
                }
            }

            return dtoList;
        }
    }
}
