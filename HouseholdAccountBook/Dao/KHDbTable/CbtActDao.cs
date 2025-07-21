using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Dto.KHDbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.KHDbTable
{
    public class CbtActDao : KHReadDaoBase<CbtActDto>
    {
        public CbtActDao(OleDbHandler dbHandler) : base(dbHandler) { }

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
