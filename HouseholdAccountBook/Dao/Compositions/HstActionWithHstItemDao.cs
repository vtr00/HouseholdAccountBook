using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.DbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Compositions
{
    /// <summary>
    /// 
    /// </summary>
    public class HstActionWithHstItemDao : ReadDaoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        public HstActionWithHstItemDao(DbHandlerBase dbHandler) : base(dbHandler) { }

        /// <summary>
        /// <see cref="MstCategoryDto.CategoryId"/> に基づいて、<see cref="HstActionDto"/> リストを取得します
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <returns>DTOリスト</returns>
        public async Task<IEnumerable<HstActionDto>> FindByCategoryIdAsync(int categoryId)
        {
            var dtoList = await this.dbHandler.QueryAsync<HstActionDto>(@"
SELECT *
FROM hst_action A
INNER JOIN (SELECT item_id FROM mst_item WHERE del_flg = 0 AND category_id = @CategoryId) I ON A.item_id = I.item_id
WHERE A.del_flg = 0;",
new MstItemDto { CategoryId = categoryId });

            return dtoList;
        }
    }
}
