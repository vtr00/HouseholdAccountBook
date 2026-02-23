using HouseholdAccountBook.Models.DbHandlers;
using HouseholdAccountBook.Models.Infrastructure.DbDao.Abstract;
using HouseholdAccountBook.Models.Infrastructure.DbDto.KHDbTable;
using HouseholdAccountBook.Models.Infrastructure.Logger;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Infrastructure.DbDao.KHDbTable
{
    /// <summary>
    /// 花鳥風月の項目テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class CbmItemDao(OleDbHandler dbHandler) : KHReadDaoBase<CbmItemDto>(dbHandler)
    {
        public override async Task<IEnumerable<CbmItemDto>> FindAllAsync()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            return await this.mDbHandler.QueryAsync<CbmItemDto>(@"SELECT * FROM CBM_ITEM ORDER BY ITEM_ID;");
        }
    }
}
