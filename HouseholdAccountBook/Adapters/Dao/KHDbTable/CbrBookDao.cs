using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;
using HouseholdAccountBook.DbHandler;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.KHDbTable
{
    /// <summary>
    /// 花鳥風月の帳簿-項目関連テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class CbrBookDao(OleDbHandler dbHandler) : KHReadDaoBase<CbrBookDto>(dbHandler)
    {
        public override async Task<IEnumerable<CbrBookDto>> FindAllAsync()
        {
            return await this.dbHandler.QueryAsync<CbrBookDto>(@"SELECT * FROM CBR_BOOK;");
        }
    }
}
