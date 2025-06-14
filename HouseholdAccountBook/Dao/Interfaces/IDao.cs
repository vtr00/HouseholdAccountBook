using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Interfaces
{
    /// <summary>
    /// DAOのインターフェース
    /// </summary>
    /// <typeparam name="T">エンティティ</typeparam>
    public interface IDao<T>
    {
        /// <summary>
        /// <see cref="T"/> の全てのレコードを取得する
        /// </summary>
        /// <param name="dtos">DTOリスト</param>
        /// <returns>成功した場合はtrue</returns>
        Task<IEnumerable<T>> FindAllAsync();

        /// <summary>
        /// IDで <see cref="T"/> を取得する
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="dto">DTO</param>
        /// <returns>成功した場合はtrue</returns>
        Task<T> FindByIdAsync(int id);

        /// <summary>
        /// <see cref="T"/> を挿入する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>ID</returns>
        Task<int> InsertAsync(T dto);

        /// <summary>
        /// <see cref="T"/> の複数のレコードを挿入する
        /// </summary>
        /// <param name="dtos">DTOリスト</param>
        /// <returns>IDリスト</returns>
        Task<IEnumerable<int>> InsertAsync(IEnumerable<T> dtos);

        /// <summary>
        /// <see cref="T"/> を更新する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>成功した場合はtrue</returns>
        Task<bool> UpdateAsync(T dto);

        /// <summary>
        /// IDで <see cref="T"/> を削除する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>成功した場合はtrue</returns>
        Task<bool> DeleteAsync(T dto);
    }
}
