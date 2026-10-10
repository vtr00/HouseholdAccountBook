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
SELECT A.book_id, A.action_id, A.item_id, A.act_time, A.asset_id, A.remark, I.item_kind, I.move_flg,
       (A.act_value / POWER(10, AA.scale)) * AA.base_rate / DA.base_rate AS main_act_value,        DA.asset_id AS act_asset_id,
       (A.act_value / POWER(10, AA.scale)) * AA.base_rate / BA.base_rate AS main_act_value_book,   BA.asset_id AS act_asset_id_book,
       (A.act_value / POWER(10, AA.scale)) * AA.base_rate / IA.base_rate AS main_act_value_item,   IA.asset_id AS act_asset_id_item,
       (A.act_value / POWER(10, AA.scale))                               AS main_act_value_action, AA.asset_id AS act_asset_id_action
FROM hst_action A
INNER JOIN mst_item I ON I.item_id = A.item_id AND I.del_flg = 0
INNER JOIN mst_book B ON B.book_id = A.book_id AND B.del_flg = 0
INNER JOIN mst_asset DA ON DA.asset_id =                                              @DefaultAssetId  AND DA.del_flg = 0 -- デフォルトアセット
INNER JOIN mst_asset BA ON BA.asset_id = COALESCE(                        B.asset_id, @DefaultAssetId) AND BA.del_flg = 0 -- 帳簿に紐づくアセット
INNER JOIN mst_asset IA ON IA.asset_id = COALESCE(            I.asset_id, B.asset_id, @DefaultAssetId) AND IA.del_flg = 0 -- 項目に紐づくアセット
INNER JOIN mst_asset AA ON AA.asset_id = COALESCE(A.asset_id, I.asset_id, B.asset_id, @DefaultAssetId) AND AA.del_flg = 0 -- 帳簿項目に紐づくアセット
WHERE A.del_flg = 0 AND A.group_id = @GroupId
ORDER BY I.move_flg DESC;",
new { DefaultAssetId = defaultAssetId, GroupId = groupId });

            return dtoList;
        }
    }
}
