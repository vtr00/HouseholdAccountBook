using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Adapters.Dto.Others;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Compositions
{
    /// <summary>
    /// 備考情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class RemarkInfoDao(DbHandlerBase dbHandler) : ReadDaoBase(dbHandler)
    {
        /// <summary>
        /// <see cref="HstRemarkDto.ItemId"/> に基づいて、<see cref="RemarkInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="itemId">項目ID</param>
        /// <returns>備考情報リスト</returns>
        public async Task<IEnumerable<RemarkInfoDto>> FindByItemIdAsync(int itemId)
        {
            var dtoList = await this.dbHandler.QueryAsync<RemarkInfoDto>(@"
SELECT R.remark, COUNT(A.remark) AS count, COALESCE(MAX(A.act_time), '1970-01-01') AS sort_time, COALESCE(MAX(A.act_time), null) AS used_time
FROM hst_remark R
LEFT OUTER JOIN (SELECT * FROM hst_action WHERE del_flg = 0) A ON A.remark = R.remark AND A.item_id = R.item_id
WHERE R.del_flg = 0 AND R.item_id = @ItemId
GROUP BY R.remark
ORDER BY sort_time DESC, count DESC;",
new { ItemId = itemId });

            return dtoList;
        }
    }
}
