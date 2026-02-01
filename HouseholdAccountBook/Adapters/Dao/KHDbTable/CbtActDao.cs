using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandlers;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.KHDbTable
{
    /// <summary>
    /// 花鳥風月の帳簿項目テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class CbtActDao(OleDbHandler dbHandler) : KHReadDaoBase<CbtActDto>(dbHandler)
    {
        public override async Task<IEnumerable<CbtActDto>> FindAllAsync() => await this.mDbHandler.QueryAsync<CbtActDto>(@"SELECT * FROM CBT_ACT ORDER BY ACT_ID;");

        /// <summary>
        /// <see cref="CbtActDto.GROUP_ID"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="groupId">グループID</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<CbtActDto>> FindByGroupIdAsync(int groupId)
            => await this.mDbHandler.QueryAsync<CbtActDto>(@"SELECT * FROM CBT_ACT WHERE GROUP_ID = @GROUP_ID;", new CbtActDto { GROUP_ID = groupId });
    }
}
