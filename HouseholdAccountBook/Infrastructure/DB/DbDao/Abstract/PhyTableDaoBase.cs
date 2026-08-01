using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract
{
    /// <summary>
    /// 物理テーブルDAOのベースクラス
    /// </summary>
    /// <typeparam name="DTO"><see cref="PhyTableDtoBase"/>の派生クラス</typeparam>
    /// <param name="dbHandler">DBハンドラ</param>
    /// <param name="tableName">テーブル名</param>
    public abstract class PhyTableDaoBase<DTO>(DbHandlerBase dbHandler, string tableName) :
        TableDaoBase(dbHandler), IReadTableDao<DTO>, IWriteTableDao<DTO> where DTO : PhyTableDtoBase
    {
        private string mTableName = tableName;

        /// <summary>
        /// テーブルを作成する
        /// </summary>
        /// <returns></returns>
        public abstract Task CreateTableAsync();

        public abstract Task<IEnumerable<DTO>> FindAllAsync();

        public abstract Task<int> InsertAsync(DTO dto);
        public async Task<int> BulkInsertAsync(IEnumerable<DTO> dtoList)
        {
            using FuncLog funcLog = new(new { dtoList }, Log.LogLevel.Trace);

            int count = 0;
            foreach (DTO dto in dtoList) {
                count += await this.InsertAsync(dto);
            }
            return count;
        }
        public abstract Task<int> UpdateAsync(DTO dto);
        public abstract Task<int> UpsertAsync(DTO dto);

        public abstract Task<int> DeleteAllAsync();

        public async Task AnalizeAsync()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            _ = await this.mDbHandler.ExecuteAsync(@$"ANALYZE {this.mTableName};", null, DBKindMask.PostgreSQL);
        }
    }
}
