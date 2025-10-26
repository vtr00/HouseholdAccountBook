using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;
using HouseholdAccountBook.DbHandler;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.KHDbTable
{
    /// <summary>
    /// 花鳥風月の帳簿テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class CbmBookDao(OleDbHandler dbHandler) : KHReadDaoBase<CbmBookDto>(dbHandler)
    {
        public override async Task<IEnumerable<CbmBookDto>> FindAllAsync()
        {
            return await this.dbHandler.QueryAsync<CbmBookDto>(@"SELECT * FROM CBM_BOOK ORDER BY BOOK_ID;");
        }
    }
}
