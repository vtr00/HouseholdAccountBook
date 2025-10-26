using HouseholdAccountBook.Adapters.Dto.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Abstract
{
    /// <summary>
    /// 読み込み用テーブルのDAOインターフェース
    /// </summary>
    /// <typeparam name="DTO"><see cref="DtoBase"/>の派生クラス</typeparam>
    public interface IReadTableDao<DTO> where DTO : DtoBase
    {
        /// <summary>
        /// テーブルの全てのレコードを取得する
        /// </summary>
        /// <returns>取得したレコードリスト</returns>
        public Task<IEnumerable<DTO>> FindAllAsync();
    }
}
