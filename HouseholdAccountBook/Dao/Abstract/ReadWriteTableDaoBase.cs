using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Abstract
{
    /// <summary>
    /// 読み書き可能なテーブル向けのDAOのベースクラス
    /// </summary>
    /// <typeparam name="DTO"><see cref="TableDtoBase"/>の派生クラス</typeparam>
    /// <param name="dbHandler">DBハンドラ</param>
    public abstract class ReadWriteTableDaoBase<DTO>(DbHandlerBase dbHandler) : ReadDaoBase(dbHandler), IReadTableDao<DTO>, IWriteTableDao<DTO> where DTO : TableDtoBase
    {
        public abstract Task<IEnumerable<DTO>> FindAllAsync();

        public abstract Task<int> InsertAsync(DTO dto);

        public abstract Task<int> DeleteAllAsync();

        public async Task<int> BulkInsertAsync(IEnumerable<DTO> dtoList)
        {
            int count = 0;
            foreach (var dto in dtoList) {
                count += await this.InsertAsync(dto);
            }
            return count;
        }

        /// <summary>
        /// レコードを更新する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>更新行数</returns>
        public abstract Task<int> UpdateAsync(DTO dto);

        /// <summary>
        /// レコードを挿入または更新する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>挿入/更新行数</returns>
        public abstract Task<int> UpsertAsync(DTO dto);
    }
}
