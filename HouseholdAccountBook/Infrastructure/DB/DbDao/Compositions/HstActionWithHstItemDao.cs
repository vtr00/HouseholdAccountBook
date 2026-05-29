using HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions
{
    /// <summary>
    /// 分類内帳簿項目DTO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class HstActionWithHstItemDao(DbHandlerBase dbHandler) : TableDaoBase(dbHandler)
    {
        /// <summary>
        /// <see cref="MstCategoryDto.CategoryId"/> に基づいて、<see cref="HstActionDto"/> リストを取得します
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<HstActionDto>> FindByCategoryIdAsync(int categoryId)
        {
            using FuncLog funcLog = new(new { categoryId }, Log.LogLevel.Trace);

            IEnumerable<HstActionDto> dtoList = await this.mDbHandler.QueryAsync<HstActionDto>(@"
SELECT *
FROM hst_action A
INNER JOIN mst_item I ON A.item_id = I.item_id AND I.category_id = @CategoryId AND I.del_flg = 0
WHERE A.del_flg = 0;",
new MstItemDto { CategoryId = categoryId });

            return dtoList;
        }
    }
}
