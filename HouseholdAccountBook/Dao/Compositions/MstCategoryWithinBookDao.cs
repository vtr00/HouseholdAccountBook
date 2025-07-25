﻿using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.DbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Compositions
{
    /// <summary>
    /// 帳簿内分類取得DAO
    /// </summary>
    public class MstCategoryWithinBookDao : ReadDaoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        public MstCategoryWithinBookDao(DbHandlerBase dbHandler) : base(dbHandler) { }

        /// <summary>
        /// <see cref="RelBookItemDto.BookId"/> と <see cref="MstCategoryDto.BalanceKind"/> に基づいて、<see cref="MstCategoryDto"/> リストを取得します
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="balanceKind">帳簿種別</param>
        /// <returns>DTO</returns>
        public async Task<IEnumerable<MstCategoryDto>> FindByBookIdAndBalanceKindAsync(int bookId, int balanceKind)
        {
            var dtoList = await this.dbHandler.QueryAsync<MstCategoryDto>(@"
SELECT C.category_id, C.category_name
FROM mst_category C
WHERE EXISTS (
  SELECT *
  FROM mst_item I
  INNER JOIN rel_book_item RBI ON RBI.item_id = I.item_id AND RBI.book_id = @BookId
  WHERE I.category_id = C.category_id AND I.del_flg = 0
) AND C.del_flg = 0 AND C.balance_kind = @BalanceKind
ORDER BY C.sort_order;",
new { BalanceKind = balanceKind, BookId = bookId });

            return dtoList;
        }
    }
}
