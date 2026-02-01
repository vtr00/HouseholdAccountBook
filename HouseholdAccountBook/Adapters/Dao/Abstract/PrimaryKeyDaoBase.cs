using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Dto.Abstract;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Abstract
{
    /// <summary>
    /// 単一の主キーを持つテーブル向けのDAOのベースクラス
    /// </summary>
    /// <typeparam name="DTO"><see cref="CommonTableDtoBase"/>の派生クラス かつ シーケンスを持つDTO</typeparam>
    /// <typeparam name="T">主キーの型</typeparam>
    public abstract class PrimaryKeyDaoBase<DTO, T>(DbHandlerBase dbHandler) : CommonTableDaoBase<DTO>(dbHandler), ISequentialIDDao<DTO> where DTO : CommonTableDtoBase, ISequentialIDDto
    {
        public async Task SetIdSequenceAsync(IEnumerable<DTO> dtoList) => await this.SetIdSequenceAsync(dtoList.Max(d => d.GetId()));

        public abstract Task SetIdSequenceAsync(int idSeq);

        /// <summary>
        /// 主キーに基づいて、レコードを取得する
        /// </summary>
        /// <param name="pkey">主キー</param>
        /// <returns>取得したレコード</returns>
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
        /// <returns>削除件数</returns>
        public abstract Task<int> DeleteByIdAsync(T pkey);
    }
}
