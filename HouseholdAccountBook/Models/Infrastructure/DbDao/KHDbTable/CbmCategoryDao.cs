using HouseholdAccountBook.Models.DbHandlers;
using HouseholdAccountBook.Models.Infrastructure.DbDao.Abstract;
using HouseholdAccountBook.Models.Infrastructure.DbDto.KHDbTable;
using HouseholdAccountBook.Models.Infrastructure.Logger;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Infrastructure.DbDao.KHDbTable
{
    /// <summary>
    /// 花鳥風月の分類テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class CbmCategoryDao(OleDbHandler dbHandler) : KHReadDaoBase<CbmCategoryDto>(dbHandler)
    {
        public override async Task<IEnumerable<CbmCategoryDto>> FindAllAsync()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            return await this.mDbHandler.QueryAsync<CbmCategoryDto>(@"SELECT * FROM CBM_CATEGORY ORDER BY CATEGORY_ID;");
        }
    }
}
