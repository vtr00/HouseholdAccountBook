using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Dto.Abstract;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Abstract
{
    /// <summary>
    /// 単一の主キーを持つテーブル向けのDAOのベースクラス
    /// </summary>
    /// <typeparam name="DTO"><see cref="TableDtoBase"/>の派生クラス</typeparam>
    /// <typeparam name="T">主キーの型</typeparam>
    public abstract class PrimaryKeyDaoBase<DTO, T>(DbHandlerBase dbHandler) : ReadWriteTableDaoBase<DTO>(dbHandler), ISequentialIDDao<DTO> where DTO : TableDtoBase, ISequentialIDDto
    {
        public async Task SetIdSequenceAsync(IEnumerable<DTO> dtoList)
        {
            await this.SetIdSequenceAsync(dtoList.Max(d => d.GetId()));
        }

        public abstract Task SetIdSequenceAsync(int idSeq);

        /// <summary>
        /// 主キーでレコードを取得する
        /// </summary>
        /// <param name="pkey">主キー</param>
        /// <returns>DTO</returns>
        public abstract Task<DTO> FindByIdAsync(T pkey);

        /// <summary>
        /// レコードを挿入し、主キーを返す
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>主キー</returns>
        public abstract Task<T> InsertReturningIdAsync(DTO dto);

        /// <summary>
        /// 主キーでレコードを削除する
        /// </summary>
        /// <param name="pkey">主キー</param>
        /// <returns>成功した場合はtrue</returns>
        public abstract Task<int> DeleteByIdAsync(T pkey);
    }
}
