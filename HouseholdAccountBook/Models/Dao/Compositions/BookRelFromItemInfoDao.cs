﻿using HouseholdAccountBook.Models.Dao.Abstract;
using HouseholdAccountBook.Models.DbHandler.Abstract;
using HouseholdAccountBook.Models.Dto.DbTable;
using HouseholdAccountBook.Models.Dto.Others;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Dao.Compositions
{
    /// <summary>
    /// 項目-帳簿関連情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class BookRelFromItemInfoDao(DbHandlerBase dbHandler) : ReadDaoBase(dbHandler)
    {
        /// <summary>
        /// <see cref="MstItemDto.ItemId"/> に基づいて、<see cref="BookRelFromItemInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="itemId">項目ID</param>
        /// <returns>項目-帳簿関連情報のリスト</returns>
        public async Task<IEnumerable<BookRelFromItemInfoDto>> FindByItemIdAsync(int itemId)
        {
            var dtoList = await this.dbHandler.QueryAsync<BookRelFromItemInfoDto>(@"
SELECT B.book_id AS book_id, B.book_name, RBI.book_id IS NOT NULL AS is_related
FROM mst_book B
LEFT JOIN (SELECT book_id FROM rel_book_item WHERE del_flg = 0 AND item_id = @ItemId) RBI ON RBI.book_id = B.book_id
WHERE del_flg = 0
ORDER BY B.sort_order;",
new { ItemId = itemId });

            return dtoList;
        }
    }
}
