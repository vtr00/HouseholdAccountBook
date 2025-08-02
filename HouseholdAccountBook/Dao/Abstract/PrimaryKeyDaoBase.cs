using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.Abstract;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Abstract
{
    /// <summary>
    /// 単一の主キーを持つDTO向けのDAOのベースクラス
    /// </summary>
    /// <typeparam name="DTO"><see cref="TableDtoBase"/>の派生クラス</typeparam>
    /// <typeparam name="T">主キーの型</typeparam>
    public abstract class PrimaryKeyDaoBase<DTO, T>(DbHandlerBase dbHandler) : ReadWriteDaoBase<DTO>(dbHandler) where DTO : TableDtoBase
    {
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
        /// シーケンスを更新する
        /// </summary>
        /// <param name="idSeq">シーケンスID</param>
        /// <returns>-</returns>
        public abstract Task SetIdSequenceAsync(int idSeq);

        /// <summary>
        /// 主キーでレコードを削除する
        /// </summary>
        /// <param name="pkey">主キー</param>
        /// <returns>成功した場合はtrue</returns>
        public abstract Task<int> DeleteByIdAsync(T pkey);
    }
}
