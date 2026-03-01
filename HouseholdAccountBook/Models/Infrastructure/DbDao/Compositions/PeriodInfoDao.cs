using HouseholdAccountBook.Models.Infrastructure.DbDao.Abstract;
using HouseholdAccountBook.Models.Infrastructure.DbHandlers.Abstract;
using HouseholdAccountBook.Models.Infrastructure.DbDto.Others;
using HouseholdAccountBook.Models.Infrastructure.Logger;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Infrastructure.DbDao.Compositions
{
    /// <summary>
    /// 期間情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class PeriodInfoDao(DbHandlerBase dbHandler) : TableDaoBase(dbHandler)
    {
        /// <summary>
        /// 全帳簿の <see cref="PeriodInfoDto"/> を取得する
        /// </summary>
        /// <returns>取得したレコード</returns>
        public async Task<PeriodInfoDto> Find()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            PeriodInfoDto dto = await this.mDbHandler.QuerySingleAsync<PeriodInfoDto>(@"
SELECT MIN(act_time) as first_time, MAX(act_time) as last_time
FROM hst_action
WHERE del_flg = 0;");

            return dto;
        }
    }
}
