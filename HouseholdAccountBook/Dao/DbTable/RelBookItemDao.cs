using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.DbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.DbTable
{
    /// <summary>
    /// 帳簿-項目関連テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class RelBookItemDao(DbHandlerBase dbHandler) : ReadWriteDaoBase<RelBookItemDto>(dbHandler)
    {
        /// <summary>
        /// 全レコードを取得する
        /// </summary>
        /// <returns>DTOリスト</returns>
        public override async Task<IEnumerable<RelBookItemDto>> FindAllAsync()
        {
            var dtoList = await this.dbHandler.QueryAsync<RelBookItemDto>(@"
SELECT * 
FROM rel_book_item
WHERE del_flg = 0;");

            return dtoList;
        }

        /// <summary>
        /// <see cref="RelBookItemDto.BookId"/> と <see cref="RelBookItemDto.ItemId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="dto"><see cref="HstShopDto"/></param>
        /// <param name="includeDeleted"><see cref="DtoBase.DelFlg">=1も含めるか</param>
        /// <returns>DTOリスト</returns>
        public async Task<RelBookItemDto> FindByBookIdAndItemIdAsync(int bookId, int itemId, bool includeDeleted = false)
        {
            var dto = includeDeleted
                ? await this.dbHandler.QuerySingleOrDefaultAsync<RelBookItemDto>(@"
SELECT * FROM rel_book_item
WHERE item_id = @ItemId AND book_id = @BookId;", new RelBookItemDto { BookId = bookId, ItemId = itemId })
                : await this.dbHandler.QuerySingleOrDefaultAsync<RelBookItemDto>(@"
SELECT * FROM rel_book_item
WHERE item_id = @ItemId AND book_id = @BookId AND del_flg = 0;",
new RelBookItemDto { BookId = bookId, ItemId = itemId });

            return dto;
        }

        /// <summary>
        /// <see cref="RelBookItemDto"/> を挿入する
        /// </summary>
        /// <param name="dto"><see cref="RelBookItemDto"/></param>
        /// <returns>挿入行数</returns>
        public override async Task<int> InsertAsync(RelBookItemDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
INSERT INTO rel_book_item
(item_id, book_id, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemId, @BookId, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter);", dto);

            return count;
        }

        /// <summary>
        /// <see cref="RelBookItemDto.ItemId"/> と <see cref="RelBookItemDto.ShopName"/> が一致するレコードを更新する
        /// </summary>
        /// <param name="dto"><see cref="RelBookItemDto"/></param>
        /// <returns>更新行数</returns>
        public override async Task<int> UpdateAsync(RelBookItemDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE rel_book_item
SET del_flg = @DelFlg, update_time = @UpdateTime, updater = @Updater
WHERE item_id = @ItemId AND book_id = @BookId;", dto);

            return count;
        }

        /// <summary>
        /// <see cref="RelBookItemDto"/> を挿入または更新する
        /// </summary>
        /// <param name="dto"><see cref="HstRemarkDto"/></param>
        /// <returns>挿入/更新行数</returns>
        public override async Task<int> UpsertAsync(RelBookItemDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
INSERT INTO rel_book_item (item_id, book_id, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemId, @BookId, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter)
ON CONFLICT (item_id, book_id) DO UPDATE
SET del_flg = @DelFlg, update_time = @UpdateTime, updater = @Updater
WHERE rel_book_item.item_id = @ItemId AND rel_book_item.book_id = @BookId;", dto);

            return count;
        }

        /// <summary>
        /// <see cref="RelBookItemDto"/> の全てのレコードを削除する
        /// </summary>
        /// <returns>削除行数</returns>
        public override async Task<int> DeleteAllAsync()
        {
            int count = await this.dbHandler.ExecuteAsync(@"DELETE FROM rel_book_item;");

            return count;
        }

        /// <summary>
        /// <see cref="RelBookItemDto.ItemId"/> と <see cref="RelBookItemDto.BookId"/> が一致するレコードを削除する
        /// </summary>
        /// <param name="dto"><see cref="RelBookItemDto"/></param>
        /// <returns>削除行数</returns>
        public async Task<int> DeleteAsync(RelBookItemDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE rel_book_item SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE item_id = @ItemId AND book_id = @BookId;", dto);

            return count;
        }
    }
}
