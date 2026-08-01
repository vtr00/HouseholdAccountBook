using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract
{
    /// <summary>
    /// 汎用テーブル向けのDAOのベースクラス
    /// </summary>
    /// <typeparam name="DTO"><see cref="CommonTableDtoBase"/>の派生クラス</typeparam>
    /// <param name="dbHandler">DBハンドラ</param>
    /// <param name="tableName">テーブル名</param>
    public abstract class CommonTableDaoBase<DTO>(DbHandlerBase dbHandler, string tableName) :
        PhyTableDaoBase<DTO>(dbHandler, tableName) where DTO : CommonTableDtoBase
    {
        public abstract override Task<int> CreateTableAsync();

        public abstract override Task<IEnumerable<DTO>> FindAllAsync();

        public abstract override Task<int> InsertAsync(DTO dto);

        public abstract override Task<int> DeleteAllAsync();

        public abstract override Task<int> UpdateAsync(DTO dto);

        public abstract override Task<int> UpsertAsync(DTO dto);
    }
}
