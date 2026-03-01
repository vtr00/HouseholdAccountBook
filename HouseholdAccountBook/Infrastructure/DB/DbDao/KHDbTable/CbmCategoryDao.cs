using HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDto.KHDbTable;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models.DbHandlers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.DB.DbDao.KHDbTable
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
