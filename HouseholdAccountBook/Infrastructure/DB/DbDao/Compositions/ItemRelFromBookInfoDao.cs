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
    /// 帳簿-項目関連情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class ItemRelFromBookInfoDao(DbHandlerBase dbHandler) : TableDaoBase(dbHandler)
    {
        /// <summary>
        /// <see cref="MstBookDto.BookId"/> に基づいて、<see cref="ItemRelFromBookInfoDto"/> リストを取得します
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<ItemRelFromBookInfoDto>> FindByBookIdAsync(int bookId)
        {
            using FuncLog funcLog = new(new { bookId }, Log.LogLevel.Trace);

            IEnumerable<ItemRelFromBookInfoDto> dtoList = await this.mDbHandler.QueryAsync<ItemRelFromBookInfoDto>(@"
SELECT I.item_id AS item_id, C.balance_kind AS balance_kind, C.category_name AS category_name, I.item_name AS item_name, RBI.item_id IS NOT NULL AS is_related
FROM mst_item I
INNER JOIN mst_category C ON C.category_id = I.category_id AND C.del_flg = 0
LEFT JOIN rel_book_item RBI ON RBI.item_id = I.item_id AND RBI.book_id = @BookId AND RBI.del_flg = 0
WHERE I.del_flg = 0 AND I.move_flg = 0
ORDER BY C.balance_kind, C.sort_order, I.sort_order;",
new { BookId = bookId });

            return dtoList;
        }
    }
}
