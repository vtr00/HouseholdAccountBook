using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Dto.KHDbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.KHDbTable
{
    public class CbrBookDao(OleDbHandler dbHandler) : KHReadDaoBase<CbrBookDto>(dbHandler)
    {
        public override async Task<IEnumerable<CbrBookDto>> FindAllAsync()
        {
            return await this.dbHandler.QueryAsync<CbrBookDto>(@"SELECT * FROM CBR_BOOK;");
        }
    }
}
