using HouseholdAccountBook.Models.Infrastructure.DbDto.Abstract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Infrastructure.DbDao.Abstract
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
