using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Adapters.Dto.Others;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Compositions
{
    /// <summary>
    /// 分類-項目情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class CategoryItemInfoDao(DbHandlerBase dbHandler) : ReadDaoBase(dbHandler)
    {
        /// <summary>
        /// <see cref="MstBookDto.BookId"/> と <see cref="MstCategoryDto.BalanceKind"/> に基づいて、<see cref="CategoryItemInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="balanceKind">帳簿種別</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<CategoryItemInfoDto>> FindByBookIdAndBalanceKindAsync(int bookId, int balanceKind)
        {
            var dtoList = await this.dbHandler.QueryAsync<CategoryItemInfoDto>(@"
SELECT I.item_id, I.item_name, C.category_name
FROM mst_item I
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
WHERE I.del_flg = 0 AND C.balance_kind = @BalanceKind AND
      EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @BookId AND RBI.item_id = I.item_id AND del_flg = 0)
ORDER BY C.sort_order, I.sort_order;",
new { BalanceKind = balanceKind, BookId = bookId });

            return dtoList;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/> と <see cref="MstCategoryDto.CategoryId"/> に基づいて、<see cref="CategoryItemInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="categoryId">分類ID</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<CategoryItemInfoDto>> FindByBookIdAndCategoryIdAsync(int bookId, int categoryId)
        {
            var dtoList = await this.dbHandler.QueryAsync<CategoryItemInfoDto>(@"
SELECT I.item_id, I.item_name, C.category_name
FROM mst_item I
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
WHERE I.del_flg = 0 AND C.category_id = @CategoryId AND
      EXISTS (SELECT * FROM rel_book_item RBI WHERE book_id = @BookId AND RBI.item_id = I.item_id AND del_flg = 0)
ORDER BY I.sort_order;",
new { CategoryId = categoryId, BookId = bookId });

            return dtoList;
        }
    }
}
