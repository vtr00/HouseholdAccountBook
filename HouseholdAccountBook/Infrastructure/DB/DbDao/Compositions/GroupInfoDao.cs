using System.Threading.Tasks;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;

namespace HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions
{
    /// <summary>
    /// グループ情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class GroupInfoDao(DbHandlerBase dbHandler) : TableDaoBase(dbHandler)
    {
        /// <summary>
        /// <see cref="HstActionDto.ActionId"/>に基づいて、<see cref="GroupInfoDto"/> を取得する
        /// </summary>
        /// <param name="actionId">帳簿ID</param>
        /// <returns>取得したレコード</returns>
        public async Task<GroupInfoDto> FindByActionId(int actionId)
        {
            using FuncLog funcLog = new(new { actionId }, Log.LogLevel.Trace);

            var dto = await this.mDbHandler.QuerySingleAsync<GroupInfoDto>(@"
SELECT A.group_id, G.group_kind
FROM hst_action A
LEFT JOIN (SELECT * FROM hst_group WHERE del_flg = 0) G ON G.group_id = A.group_id
WHERE A.action_id = @ActionId AND A.del_flg = 0;",
new HstActionDto { ActionId = actionId });

            return dto;
        }
    }
}
