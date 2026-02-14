using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Adapters.Logger;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using static HouseholdAccountBook.Adapters.DbConstants;

namespace HouseholdAccountBook.Adapters.Dao.DbTable
{
    /// <summary>
    /// 項目テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class MstItemDao(DbHandlerBase dbHandler) : PrimaryKeyDaoBase<MstItemDto, int>(dbHandler)
    {
        public override Task<int> CreateTableAsync() => throw new NotImplementedException();

        public override async Task<IEnumerable<MstItemDto>> FindAllAsync()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            var dtoList = await this.mDbHandler.QueryAsync<MstItemDto>(@"
SELECT * 
FROM mst_item
WHERE del_flg = 0;");

            return dtoList;
        }

        /// <summary>
        /// <see cref="MstItemDto.ItemId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="itemId">分類ID</param>
        /// <returns>取得したレコード</returns>
        public override async Task<MstItemDto> FindByIdAsync(int itemId)
        {
            using FuncLog funcLog = new(new { itemId }, Log.LogLevel.Trace);

            var dto = await this.mDbHandler.QuerySingleOrDefaultAsync<MstItemDto>(@"
SELECT *
FROM mst_item
WHERE item_id = @ItemId AND del_flg = 0;",
new MstItemDto { ItemId = itemId });

            return dto;
        }

        /// <summary>
        /// <see cref="MstItemDto.CategoryId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="categoryId">分類ID</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<MstItemDto>> FindByCategoryIdAsync(int categoryId)
        {
            using FuncLog funcLog = new(new { categoryId }, Log.LogLevel.Trace);

            var dtoList = await this.mDbHandler.QueryAsync<MstItemDto>(@"
SELECT item_id, item_name, advance_flg, sort_order
FROM mst_item
WHERE category_id = @CategoryId AND del_flg = 0
ORDER BY sort_order;",
new MstItemDto { CategoryId = categoryId });

            return dtoList;
        }

        public override async Task SetIdSequenceAsync(int idSeq)
        {
            using FuncLog funcLog = new(new { idSeq }, Log.LogLevel.Trace);

            _ = await this.mDbHandler.ExecuteAsync(@"SELECT setval('mst_item_item_id_seq', @ItemIdSeq);", new { ItemIdSeq = idSeq }, DBKindMask.PostgreSQL);
        }

        public override async Task<int> InsertAsync(MstItemDto dto)
        {
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
INSERT INTO mst_item
(item_id, item_name, category_id, move_flg, advance_flg, json_code, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemId, @ItemName, @CategoryId, @MoveFlg, @AdvanceFlg, @JsonCode, @SortOrder, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter);", dto);

            return count;
        }

        public override async Task<int> InsertReturningIdAsync(MstItemDto dto)
        {
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

            int itemId = await this.mDbHandler.QuerySingleAsync<int>(@"
INSERT INTO mst_item
(item_name, category_id, move_flg, advance_flg, json_code, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemName, @CategoryId, @MoveFlg, @AdvanceFlg, @JsonCode, (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_item), @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter)
RETURNING item_id;", dto);

            return itemId;
        }

        public override Task<int> UpdateAsync(MstItemDto dto) => throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");

        /// <summary>
        /// <see cref="MstItemDto.ItemId"/> に基づいて、<see cref="MstItemDto.ItemName"/> を更新する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>更新件数</returns>
        public async Task<int> UpdateSetableAsync(MstItemDto dto)
        {
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE mst_item
SET item_name = @ItemName, update_time = @UpdateTime, updater = @Updater
WHERE item_id = @ItemId;", dto);

            return count;
        }

        /// <summary>
        /// レコードのソート順を最大に設定する
        /// </summary>
        /// <param name="categoryId">移動先の分類ID</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>更新件数</returns>
        public async Task<int> UpdateSortOrderToMaximumAsync(int categoryId, int itemId)
        {
            using FuncLog funcLog = new(new { categoryId, itemId }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE mst_item
SET category_id = @CategoryId, sort_order = (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_item), update_time = @UpdateTime, updater = @Updater
WHERE item_id = @ItemId;",
new MstItemDto { CategoryId = categoryId, ItemId = itemId });

            return count;
        }

        /// <summary>
        /// レコードのソート順を最小に設定する
        /// </summary>
        /// <param name="categoryId">移動先の分類ID</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>更新件数</returns>
        public async Task<int> UpdateSortOrderToMinimumAsync(int categoryId, int itemId)
        {
            using FuncLog funcLog = new(new { categoryId, itemId }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE mst_item
SET category_id = @CategoryId, sort_order = (SELECT COALESCE(MAX(sort_order) - 1, 1) FROM mst_item), update_time = @UpdateTime, updater = @Updater
WHERE item_id = @ItemId;",
new MstItemDto { CategoryId = categoryId, ItemId = itemId });

            return count;
        }

        /// <summary>
        /// レコードのソート順を入れ替える
        /// </summary>
        /// <param name="itemId1">項目ID1</param>
        /// <param name="itemId2">項目ID2</param>
        /// <returns>更新件数</returns>
        /// <remarks>PostgreSQLとSQLiteで挙動が変わる可能性があるため変更時は要動作確認</remarks>
        public async Task<int> SwapSortOrderAsync(int itemId1, int itemId2)
        {
            using FuncLog funcLog = new(new { itemId1, itemId2 }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@" 
WITH original AS (
  SELECT
    (SELECT sort_order FROM mst_item WHERE item_id = @ItemId1) AS sort_order1,
    (SELECT sort_order FROM mst_item WHERE item_id = @ItemId2) AS sort_order2
)
UPDATE mst_item
SET sort_order = CASE
  WHEN item_id = @ItemId1 THEN (SELECT sort_order2 FROM original)
  WHEN item_id = @ItemId2 THEN (SELECT sort_order1 FROM original)
  ELSE sort_order
END, update_time = @UpdateTime, updater = @Updater
WHERE item_id IN (@ItemId1, @ItemId2);",
new { ItemId1 = itemId1, ItemId2 = itemId2, UpdateTime = DateTime.Now, Updater });

            return count;
        }

        public override Task<int> UpsertAsync(MstItemDto dto) => throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");

        public override async Task<int> DeleteAllAsync()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"DELETE FROM mst_item;");

            return count;
        }

        /// <summary>
        /// <see cref="MstItemDto.ItemId"/> に基づいて、レコードを削除する
        /// </summary>
        /// <param name="itemId">項目ID</param>
        /// <returns>削除件数</returns>
        public override async Task<int> DeleteByIdAsync(int itemId)
        {
            using FuncLog funcLog = new(new { itemId }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE mst_item
SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE item_id = @ItemId;",
new MstItemDto { ItemId = itemId });

            return count;
        }
    }
}
