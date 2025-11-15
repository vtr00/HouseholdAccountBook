using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Adapters.Dto.Others;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Compositions
{
    /// <summary>
    /// 帳簿-項目関連情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class ItemRelFromBookInfoDao(DbHandlerBase dbHandler) : ReadDaoBase(dbHandler)
    {
        /// <summary>
        /// <see cref="MstBookDto.BookId"/> に基づいて、<see cref="ItemRelFromBookInfoDto"/> リストを取得します
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<ItemRelFromBookInfoDto>> FindByBookIdAsync(int bookId)
        {
            var dtoList = await this.mDbHandler.QueryAsync<ItemRelFromBookInfoDto>(@"
SELECT I.item_id AS item_id, C.balance_kind AS balance_kind, C.category_name AS category_name, I.item_name AS item_name, RBI.item_id IS NOT NULL AS is_related
FROM mst_item I
INNER JOIN (SELECT category_id, category_name, balance_kind, sort_order FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
LEFT JOIN (SELECT item_id FROM rel_book_item WHERE del_flg = 0 AND book_id = @BookId) RBI ON RBI.item_id = I.item_id
WHERE del_flg = 0 AND move_flg = 0
ORDER BY C.balance_kind, C.sort_order, I.sort_order;",
new { BookId = bookId });

            return dtoList;
        }
    }
}
