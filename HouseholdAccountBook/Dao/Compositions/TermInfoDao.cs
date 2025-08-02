using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.Others;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Compositions
{
    /// <summary>
    /// 期間情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class TermInfoDao(DbHandlerBase dbHandler) : ReadDaoBase(dbHandler)
    {
        /// <summary>
        /// 全帳簿の <see cref="TermInfoDto"/> を取得する
        /// </summary>
        /// <returns></returns>
        public async Task<TermInfoDto> Find()
        {
            TermInfoDto dto = await this.dbHandler.QuerySingleAsync<TermInfoDto>(@"
SELECT MIN(act_time) as first_time, MAX(act_time) as last_time
FROM hst_action
WHERE del_flg = 0;");

            return dto;
        }
    }
}
