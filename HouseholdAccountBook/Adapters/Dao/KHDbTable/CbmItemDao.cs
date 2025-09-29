using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;
using HouseholdAccountBook.DbHandler;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.KHDbTable
{
    public class CbmItemDao(OleDbHandler dbHandler) : KHReadDaoBase<CbmItemDto>(dbHandler)
    {
        public override async Task<IEnumerable<CbmItemDto>> FindAllAsync()
        {
            return await this.dbHandler.QueryAsync<CbmItemDto>(@"SELECT * FROM CBM_ITEM ORDER BY ITEM_ID;");
        }
    }
}
