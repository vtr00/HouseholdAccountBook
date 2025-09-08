using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Models.Dao.Abstract;
using HouseholdAccountBook.Models.Dto.KHDbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Dao.KHDbTable
{
    public class CbmCategoryDao(OleDbHandler dbHandler) : KHReadDaoBase<CbmCategoryDto>(dbHandler)
    {
        public override async Task<IEnumerable<CbmCategoryDto>> FindAllAsync()
        {
            return await this.dbHandler.QueryAsync<CbmCategoryDto>(@"SELECT * FROM CBM_CATEGORY ORDER BY CATEGORY_ID;");
        }
    }
}
