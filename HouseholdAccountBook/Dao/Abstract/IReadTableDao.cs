using HouseholdAccountBook.Dto.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Abstract
{
    /// <summary>
    /// 読み込み用テーブルのDAOインターフェース
    /// </summary>
    /// <typeparam name="DTO"></typeparam>
    public interface IReadTableDao<DTO> where DTO : DtoBase
    {
        /// <summary>
        /// テーブルの全てのデータを取得する
        /// </summary>
        /// <returns>取得データ</returns>
        public Task<IEnumerable<DTO>> FindAllAsync();
    }
}
