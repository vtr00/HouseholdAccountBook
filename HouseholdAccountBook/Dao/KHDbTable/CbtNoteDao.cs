using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Dto.KHDbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.KHDbTable
{
    public class CbtNoteDao : KHReadDaoBase<CbtNoteDto>
    {
        public CbtNoteDao(OleDbHandler dbHandler) : base(dbHandler) { }

        public override async Task<IEnumerable<CbtNoteDto>> FindAllAsync()
        {
            return await this.dbHandler.QueryAsync<CbtNoteDto>(@"SELECT * FROM CBT_NOTE;");
        }
    }
}
