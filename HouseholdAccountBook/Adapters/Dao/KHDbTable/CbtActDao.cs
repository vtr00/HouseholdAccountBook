using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;
using HouseholdAccountBook.DbHandler;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.KHDbTable
{
    public class CbtActDao(OleDbHandler dbHandler) : KHReadDaoBase<CbtActDto>(dbHandler)
    {
        public override async Task<IEnumerable<CbtActDto>> FindAllAsync()
        {
            return await this.dbHandler.QueryAsync<CbtActDto>(@"SELECT * FROM CBT_ACT ORDER BY ACT_ID;");
        }

        public async Task<IEnumerable<CbtActDto>> FindByGroupIdAsync(int groupId)
        {
            return await this.dbHandler.QueryAsync<CbtActDto>(@"SELECT * FROM CBT_ACT WHERE GROUP_ID = @GROUP_ID;", new CbtActDto { GROUP_ID = groupId });
        }
    }
}
