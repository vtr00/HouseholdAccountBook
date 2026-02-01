using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandlers;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.KHDbTable
{
    /// <summary>
    /// 花鳥風月の項目テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class CbmItemDao(OleDbHandler dbHandler) : KHReadDaoBase<CbmItemDto>(dbHandler)
    {
        public override async Task<IEnumerable<CbmItemDto>> FindAllAsync() => await this.mDbHandler.QueryAsync<CbmItemDto>(@"SELECT * FROM CBM_ITEM ORDER BY ITEM_ID;");
    }
}
