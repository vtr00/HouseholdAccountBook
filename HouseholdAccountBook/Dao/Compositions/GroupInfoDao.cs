using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.DbTable;
using HouseholdAccountBook.Dto.Others;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Compositions
{
    /// <summary>
    /// グループ情報DAO
    /// </summary>
    public class GroupInfoDao : ReadDaoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dbHandler"></param>
        public GroupInfoDao(DbHandlerBase dbHandler) : base(dbHandler) { }

        /// <summary>
        /// <see cref="HstActionDto.ActionId"/>に基づいて、<see cref="GroupInfoDto"/> を取得する
        /// </summary>
        /// <param name="actionId">帳簿ID</param>
        /// <returns>グループ情報</returns>
        public async Task<GroupInfoDto> FindByActionId(int actionId)
        {
            var dto = await this.dbHandler.QuerySingleAsync<GroupInfoDto>(@"
SELECT A.group_id, G.group_kind
FROM hst_action A
LEFT JOIN (SELECT * FROM hst_group WHERE del_flg = 0) G ON G.group_id = A.group_id
WHERE A.action_id = @ActionId AND A.del_flg = 0;",
new HstActionDto { ActionId = actionId });

            return dto;
        }
    }
}
