using HouseholdAccountBook.Models.Dao.Abstract;
using HouseholdAccountBook.Models.DbHandler.Abstract;
using HouseholdAccountBook.Models.Dto.DbTable;
using HouseholdAccountBook.Models.Dto.Others;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Dao.Compositions
{
    /// <summary>
    /// 店舗情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class ShopInfoDao(DbHandlerBase dbHandler) : ReadDaoBase(dbHandler)
    {
        /// <summary>
        /// <see cref="HstShopDto.ItemId"> に基づいて、<see cref="ShopInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="itemId">項目ID</param>
        /// <returns>店舗情報リスト</returns>
        public async Task<IEnumerable<ShopInfoDto>> FindByItemIdAsync(int itemId)
        {
            var dtoList = await this.dbHandler.QueryAsync<ShopInfoDto>(@"
SELECT S.shop_name, COUNT(A.shop_name) AS count, COALESCE(MAX(A.act_time), '1970-01-01') AS sort_time, COALESCE(MAX(A.act_time), null) AS used_time
FROM hst_shop S
LEFT OUTER JOIN (SELECT * FROM hst_action WHERE del_flg = 0) A ON A.shop_name = S.shop_name AND A.item_id = S.item_id
WHERE S.del_flg = 0 AND S.item_id = @ItemId
GROUP BY S.shop_name
ORDER BY sort_time DESC, count DESC;",
new { ItemId = itemId });

            return dtoList;
        }
    }
}
