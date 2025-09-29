using HouseholdAccountBook.Adapters.Dto.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Abstract
{
    /// <summary>
    /// 書き込み用テーブルのDAOインターフェース
    /// </summary>
    /// <typeparam name="DTO"></typeparam>
    public interface IWriteTableDao<DTO> where DTO : DtoBase
    {
        /// <summary>
        /// テーブルの全てのデータを削除する
        /// </summary>
        /// <returns>削除件数</returns>
        public Task<int> DeleteAllAsync();

        /// <summary>
        /// データをテーブルに追加する
        /// </summary>
        /// <param name="dto">追加するデータ</param>
        /// <returns>追加件数</returns>
        public Task<int> InsertAsync(DTO dto);

        /// <summary>
        /// 全てのデータをテーブルに追加する
        /// </summary>
        /// <param name="dtoList">追加するデータのリスト</param>
        /// <returns>追加件数</returns>
        public Task<int> BulkInsertAsync(IEnumerable<DTO> dtoList);
    }
}
