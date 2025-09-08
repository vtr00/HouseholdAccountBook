using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Models.Dao.Abstract;
using HouseholdAccountBook.Models.Dto.KHDbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Dao.KHDbTable
{
    public class CbtNoteDao(OleDbHandler dbHandler) : KHReadDaoBase<CbtNoteDto>(dbHandler)
    {
        public override async Task<IEnumerable<CbtNoteDto>> FindAllAsync()
        {
            return await this.dbHandler.QueryAsync<CbtNoteDto>(@"SELECT * FROM CBT_NOTE;");
        }
    }
}
