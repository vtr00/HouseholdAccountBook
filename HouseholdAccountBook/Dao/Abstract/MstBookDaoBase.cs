using HouseholdAccountBook.Dao.Interfaces;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Abstract
{
    public abstract class MstBookDaoBase : IDao<MstBookDto>
    {
        /// <summary>
        /// DBハンドラ
        /// </summary>
        protected readonly DbHandlerBase handler;

        /// <summary>
        /// MstBookDaoBase クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="handler"></param>
        public MstBookDaoBase(DbHandlerBase handler) {
            this.handler = handler;
        }

        /// <summary>
        /// 全ての帳簿を取得する
        /// </summary>
        /// <returns></returns>
        public abstract Task<IEnumerable<MstBookDto>> FindAllAsync();

        /// <summary>
        /// 帳簿IDで帳簿を取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns></returns>
        public abstract Task<MstBookDto> FindByIdAsync(int bookId);

        /// <summary>
        /// 帳簿を挿入する
        /// </summary>
        /// <param name="dto">帳簿DTO</param>
        /// <returns>帳簿ID</returns>
        public abstract Task<int> InsertAsync(MstBookDto dto);

        /// <summary>
        /// 複数の帳簿を挿入する
        /// </summary>
        /// <param name="dtos">帳簿DTOリスト</param>
        /// <returns>帳簿IDリスト</returns>
        public abstract Task<IEnumerable<int>> InsertAsync(IEnumerable<MstBookDto> dtos);

        /// <summary>
        /// 帳簿を更新する
        /// </summary>
        /// <param name="dto">帳簿DTO</param>
        /// <returns>成功した場合はtrue</returns>
        public abstract Task<bool> UpdateAsync(MstBookDto dto);

        /// <summary>
        /// 帳簿IDで帳簿を削除する
        /// </summary>
        /// <param name="dto">帳簿DTO</param>
        /// <returns>成功した場合はtrue</returns>
        public abstract Task<bool> DeleteAsync(MstBookDto dto);
    }
}
