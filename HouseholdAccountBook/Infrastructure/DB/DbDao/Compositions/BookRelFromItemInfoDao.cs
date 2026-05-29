using HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions
{
    /// <summary>
    /// 項目-帳簿関連情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class BookRelFromItemInfoDao(DbHandlerBase dbHandler) : TableDaoBase(dbHandler)
    {
        /// <summary>
        /// <see cref="MstItemDto.ItemId"/> に基づいて、<see cref="BookRelFromItemInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="itemId">項目ID</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<BookRelFromItemInfoDto>> FindByItemIdAsync(int itemId)
        {
            using FuncLog funcLog = new(new { itemId }, Log.LogLevel.Trace);

            IEnumerable<BookRelFromItemInfoDto> dtoList = await this.mDbHandler.QueryAsync<BookRelFromItemInfoDto>(@"
SELECT B.book_id AS book_id, B.book_name, RBI.book_id IS NOT NULL AS is_related
FROM mst_book B
LEFT JOIN rel_book_item RBI ON RBI.book_id = B.book_id AND RBI.item_id = @ItemId AND RBI.del_flg = 0
WHERE B.del_flg = 0
ORDER BY B.sort_order;",
new { ItemId = itemId });

            return dtoList;
        }
    }
}
