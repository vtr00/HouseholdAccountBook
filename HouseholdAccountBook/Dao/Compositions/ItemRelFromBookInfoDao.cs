using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.DbTable;
using HouseholdAccountBook.Dto.Others;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Compositions
{
    /// <summary>
    /// 帳簿-項目関連情報DAO
    /// </summary>
    public class ItemRelFromBookInfoDao : ReadDaoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        public ItemRelFromBookInfoDao(DbHandlerBase dbHandler) : base(dbHandler) { }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/> に基づいて、<see cref="ItemRelFromBookInfoDto"/> リストを取得します
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>帳簿-項目関連情報のリスト</returns>
        public async Task<IEnumerable<ItemRelFromBookInfoDto>> FindByBookIdAsync(int bookId)
        {
            var dtoList = await this.dbHandler.QueryAsync<ItemRelFromBookInfoDto>(@"
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
