using HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions
{
    /// <summary>
    /// 移動情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class MoveActionInfoDao(DbHandlerBase dbHandler) : TableDaoBase(dbHandler)
    {
        /// <summary>
        /// <see cref="HstGroupDto.GroupId"/> に基づいて、<see cref="MoveActionInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="defaultAssetId">デフォルトアセットID</param>
        /// <param name="groupId">グループID</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<MoveActionInfoDto>> GetAllAsync(int defaultAssetId, int groupId)
        {
            using FuncLog funcLog = new(new { defaultAssetId, groupId }, Log.LogLevel.Trace);

            IEnumerable<MoveActionInfoDto> dtoList = await this.mDbHandler.QueryAsync<MoveActionInfoDto>(@"
SELECT A.book_id, A.action_id, A.item_id, A.act_time, A.act_value / POWER(10, MA.scale) AS act_main_value, A.remark, I.move_flg
FROM hst_action A
INNER JOIN mst_item I ON I.item_id = A.item_id AND I.del_flg = 0
INNER JOIN mst_book B ON B.book_id = A.book_id AND B.del_flg = 0
INNER JOIN mst_asset MA ON MA.asset_id = @DefaultAssetId AND MA.del_flg = 0
WHERE A.del_flg = 0 AND A.group_id = @GroupId
ORDER BY I.move_flg DESC;",
new { DefaultAssetId = defaultAssetId, GroupId = groupId });

            return dtoList;
        }
    }
}
