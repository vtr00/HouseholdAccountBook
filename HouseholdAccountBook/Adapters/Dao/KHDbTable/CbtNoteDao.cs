using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;
using HouseholdAccountBook.DbHandler;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.KHDbTable
{
    public class CbtNoteDao(OleDbHandler dbHandler) : KHReadDaoBase<CbtNoteDto>(dbHandler)
    {
        public override async Task<IEnumerable<CbtNoteDto>> FindAllAsync()
        {
            return await this.dbHandler.QueryAsync<CbtNoteDto>(@"SELECT * FROM CBT_NOTE;");
        }
    }
}
