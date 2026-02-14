using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Dto.Abstract;
using HouseholdAccountBook.Adapters.Logger;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Abstract
{
    /// <summary>
    /// 物理テーブルDAOのベースクラス
    /// </summary>
    /// <typeparam name="DTO"><see cref="PhyTableDtoBase"/>の派生クラス</typeparam>
    /// <param name="dbHandler">DBハンドラ</param>
    public abstract class PhyTableDaoBase<DTO>(DbHandlerBase dbHandler) : TableDaoBase(dbHandler), IReadTableDao<DTO>, IWriteTableDao<DTO> where DTO : PhyTableDtoBase
    {
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
            foreach (var dto in dtoList) {
                count += await this.InsertAsync(dto);
            }
            return count;
        }
        public abstract Task<int> UpdateAsync(DTO dto);
        public abstract Task<int> UpsertAsync(DTO dto);

        public abstract Task<int> DeleteAllAsync();
    }
}
