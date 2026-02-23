using HouseholdAccountBook.Models.DbHandlers;
using HouseholdAccountBook.Models.Infrastructure.DbDao.Abstract;
using HouseholdAccountBook.Models.Infrastructure.DbDto.KHDbTable;
using HouseholdAccountBook.Models.Infrastructure.Logger;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Infrastructure.DbDao.KHDbTable
{
    /// <summary>
    /// 花鳥風月の備考テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class CbtNoteDao(OleDbHandler dbHandler) : KHReadDaoBase<CbtNoteDto>(dbHandler)
    {
        public override async Task<IEnumerable<CbtNoteDto>> FindAllAsync()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            return await this.mDbHandler.QueryAsync<CbtNoteDto>(@"SELECT * FROM CBT_NOTE;");
        }
    }
}
