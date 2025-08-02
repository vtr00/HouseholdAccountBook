using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.DbTable;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using static HouseholdAccountBook.Others.DbConstants;

namespace HouseholdAccountBook.Dao.DbTable
{
    /// <summary>
    /// 項目テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class MstItemDao(DbHandlerBase dbHandler) : PrimaryKeyDaoBase<MstItemDto, int>(dbHandler)
    {
        public override async Task<IEnumerable<MstItemDto>> FindAllAsync()
        {
            var dtoList = await this.dbHandler.QueryAsync<MstItemDto>(@"
SELECT * 
FROM mst_item
WHERE del_flg = 0;");

            return dtoList;
        }

        public override async Task<MstItemDto> FindByIdAsync(int pkey)
        {
            var dto = await this.dbHandler.QuerySingleOrDefaultAsync<MstItemDto>(@"
SELECT *
FROM mst_item
WHERE item_id = @ItemId AND del_flg = 0;",
new MstItemDto { ItemId = pkey });

            return dto;
        }

        /// <summary>
        /// <see cref="MstItemDto.CategoryId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <returns>DTOリスト</returns>
        public async Task<IEnumerable<MstItemDto>> FindByCategoryIdAsync(int categoryId)
        {
            var dtoList = await this.dbHandler.QueryAsync<MstItemDto>(@"
SELECT item_id, item_name, advance_flg, sort_order
FROM mst_item
WHERE category_id = @CategoryId AND del_flg = 0
ORDER BY sort_order;",
new MstItemDto { CategoryId = categoryId });

            return dtoList;
        }

        public override async Task SetIdSequenceAsync(int idSeq)
        {
            await this.dbHandler.ExecuteAsync(@"SELECT setval('mst_item_item_id_seq', @ItemIdSeq);", new { ItemIdSeq = idSeq });
        }

        public override async Task<int> InsertAsync(MstItemDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
INSERT INTO mst_item
(item_id, item_name, category_id, move_flg, advance_flg, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemId, @ItemName, @CategoryId, @MoveFlg, @AdvanceFlg, @SortOrder, @DelFlg, 'now', @Updater, 'now', @Inserter);", dto);

            return count;
        }

        public override async Task<int> InsertReturningIdAsync(MstItemDto dto)
        {
            int itemId = await this.dbHandler.QuerySingleAsync<int>(@"
INSERT INTO mst_item
(item_name, category_id, move_flg, advance_flg, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES ('(no name)', @CategoryId, @MoveFlg, @AdvanceFlg, (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_item), @DelFlg, 'now', @Updater, 'now', @Inserter)
RETURNING item_id;", dto);

            return itemId;
        }

        public override Task<int> UpdateAsync(MstItemDto dto)
        {
            throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");
        }

        /// <summary>
        /// <see cref="MstItemDto.ItemId"/> に基づいて、<see cref="MstItemDto.ItemName"/> を更新する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>更新行数</returns>
        public async Task<int> UpdateSetableAsync(MstItemDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE mst_item
SET item_name = @ItemName, update_time = 'now', updater = @Updater
WHERE item_id = @ItemId;", dto);

            return count;
        }

        /// <summary>
        /// レコードのソート順を最大に設定する
        /// </summary>
        /// <param name="categoryId">移動先の分類ID</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>更新行数</returns>
        public async Task<int> UpdateSortOrderToMaximumAsync(int categoryId, int itemId)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE mst_item
SET category_id = @CategoryId, sort_order = (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_item), update_time = 'now', updater = @Updater
WHERE item_id = @ItemId;",
new MstItemDto { CategoryId = categoryId, ItemId = itemId });

            return count;
        }

        /// <summary>
        /// レコードのソート順を最小に設定する
        /// </summary>
        /// <param name="categoryId">移動先の分類ID</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>更新行数</returns>
        public async Task<int> UpdateSortOrderToMinimumAsync(int categoryId, int itemId)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE mst_item
SET category_id = @CategoryId, sort_order = (SELECT COALESCE(MAX(sort_order) - 1, 1) FROM mst_item), update_time = 'now', updater = @Updater
WHERE item_id = @ItemId;",
new MstItemDto { CategoryId = categoryId, ItemId = itemId });

            return count;
        }

        /// <summary>
        /// レコードのソート順を入れ替える
        /// </summary>
        /// <param name="itemId1">項目ID1</param>
        /// <param name="itemId2">項目ID2</param>
        /// <returns></returns>
        public async Task<int> SwapSortOrderAsync(int itemId1, int itemId2)
        {
            int count = await this.dbHandler.ExecuteAsync(@" 
UPDATE mst_item
SET sort_order = CASE
  WHEN item_id = @ItemId1 THEN (SELECT sort_order FROM mst_item WHERE item_id = @ItemId2)
  WHEN item_id = @ItemId2 THEN (SELECT sort_order FROM mst_item WHERE item_id = @ItemId1)
  ELSE sort_order
END, update_time = 'now', updater = @Updater
WHERE item_id IN (@ItemId1, @ItemId2);",
new { ItemId1 = itemId1, ItemId2 = itemId2, Updater });

            return count;
        }

        public override Task<int> UpsertAsync(MstItemDto dto)
        {
            throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");
        }

        public override async Task<int> DeleteAllAsync()
        {
            int count = await this.dbHandler.ExecuteAsync(@"DELETE FROM mst_item;");

            return count;
        }

        public override async Task<int> DeleteByIdAsync(int pkey)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE mst_item
SET del_flg = 1, update_time = 'now', updater = @Updater
WHERE item_id = @ItemId;",
new MstItemDto { ItemId = pkey });

            return count;
        }
    }
}
