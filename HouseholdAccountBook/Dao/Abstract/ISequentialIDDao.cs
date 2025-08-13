using HouseholdAccountBook.Dto.Abstract;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Abstract
{
    /// <summary>
    /// シーケンスのDAOインターフェース
    /// </summary>
    /// <typeparam name="DTO">シーケンスをもつDTO</typeparam>
    public interface ISequentialIDDao<DTO> where DTO : ISequentialIDDto
    {
        /// <summary>
        /// シーケンスを更新する
        /// </summary>
        /// <param name="dtoList">シーケンスをもつDTOのリスト</param>
        /// <returns></returns>
        public async Task SetIdSequenceAsync(IEnumerable<DTO> dtoList)
        {
            await this.SetIdSequenceAsync(dtoList.Max(d => d.GetId()));
        }

        /// <summary>
        /// シーケンスを更新する
        /// </summary>
        /// <param name="idSeq">ID</param>
        /// <returns></returns>
        public abstract Task SetIdSequenceAsync(int idSeq);
    }
}
