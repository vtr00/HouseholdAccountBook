using HouseholdAccountBook.Adapters.Dto.Abstract;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Abstract
{
    /// <summary>
    /// シーケンスをもつDTOのDAOインターフェース
    /// </summary>
    /// <typeparam name="DTO">シーケンスをもつDTO</typeparam>
    public interface ISequentialIDDao<DTO> where DTO : ISequentialIDDto
    {
        /// <summary>
        /// 引数のレコードにおける最大のIDでシーケンスを更新する
        /// </summary>
        /// <param name="dtoList">シーケンスをもつレコードのリスト</param>
        public async Task SetIdSequenceAsync(IEnumerable<DTO> dtoList) => await this.SetIdSequenceAsync(dtoList.Max(d => d.GetId()));

        /// <summary>
        /// シーケンスを更新する
        /// </summary>
        /// <param name="idSeq">ID</param>
        public abstract Task SetIdSequenceAsync(int idSeq);
    }
}
