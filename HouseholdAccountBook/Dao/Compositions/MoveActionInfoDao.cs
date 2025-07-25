﻿using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.DbTable;
using HouseholdAccountBook.Dto.Others;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Compositions
{
    /// <summary>
    /// 移動情報DAO
    /// </summary>
    public class MoveActionInfoDao : ReadDaoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        public MoveActionInfoDao(DbHandlerBase dbHandler) : base(dbHandler) { }

        /// <summary>
        /// <see cref="HstGroupDto.GroupId"/> に基づいて、<see cref="MoveActionInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="groupId">グループID</param>
        /// <returns>DTO</returns>
        public async Task<IEnumerable<MoveActionInfoDto>> GetAllAsync(int groupId)
        {
            var dtoList = await this.dbHandler.QueryAsync<MoveActionInfoDto>(@"
SELECT A.book_id, A.action_id, A.item_id, A.act_time, A.act_value, A.remark, I.move_flg
FROM hst_action A
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = A.item_id
WHERE A.del_flg = 0 AND A.group_id = @GroupId
ORDER BY I.move_flg DESC;",
new { GroupId = groupId });

            return dtoList;
        }
    }
}
