using HouseholdAccountBook.Adapters.Dto.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Abstract
{
    /// <summary>
    /// 書き込み用テーブルのDAOインターフェース
    /// </summary>
    /// <typeparam name="DTO"><see cref="DtoBase"/>の派生クラス</typeparam>
    public interface IWriteTableDao<DTO> where DTO : DtoBase
    {
        /// <summary>
        /// テーブルの全てのレコードを削除する
        /// </summary>
        /// <returns>削除件数</returns>
        public Task<int> DeleteAllAsync();

        /// <summary>
        /// レコードをテーブルに追加する
        /// </summary>
        /// <param name="dto">追加するレコード</param>
        /// <returns>追加件数</returns>
        public Task<int> InsertAsync(DTO dto);

        /// <summary>
        /// 複数のレコードをテーブルに追加する
        /// </summary>
        /// <param name="dtoList">追加するレコードのリスト</param>
        /// <returns>追加件数</returns>
        public Task<int> BulkInsertAsync(IEnumerable<DTO> dtoList);

        /// <summary>
        /// レコードを更新する
        /// </summary>
        /// <param name="dto">対象のレコード</param>
        /// <returns>更新件数</returns>
        public Task<int> UpdateAsync(DTO dto);

        /// <summary>
        /// レコードを挿入または更新する
        /// </summary>
        /// <param name="dto">対象のレコード</param>
        /// <returns>挿入/更新件数</returns>
        public Task<int> UpsertAsync(DTO dto);
    }
}
