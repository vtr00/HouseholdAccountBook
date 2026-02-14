using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandlers;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;
using HouseholdAccountBook.Adapters.Logger;
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
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            return await this.mDbHandler.QueryAsync<CbrBookDto>(@"SELECT * FROM CBR_BOOK;");
        }
    }
}
