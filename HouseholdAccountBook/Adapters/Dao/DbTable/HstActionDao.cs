using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Adapters.Logger;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.DbTable
{
    /// <summary>
    /// 帳簿項目テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class HstActionDao(DbHandlerBase dbHandler) : PrimaryKeyDaoBase<HstActionDto, int>(dbHandler)
    {
        public override Task<int> CreateTableAsync() => throw new NotImplementedException();

        public override async Task<IEnumerable<HstActionDto>> FindAllAsync()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            var dtoList = await this.mDbHandler.QueryAsync<HstActionDto>(@"
SELECT * 
FROM hst_action
WHERE del_flg = 0;");

            return dtoList;
        }

        /// <summary>
        /// <see cref="HstActionDto.ActionId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <returns>取得したレコード</returns>
        public override async Task<HstActionDto> FindByIdAsync(int actionId)
        {
            using FuncLog funcLog = new(new { actionId }, Log.LogLevel.Trace);

            var dto = await this.mDbHandler.QuerySingleOrDefaultAsync<HstActionDto>(@"
SELECT *
FROM hst_action
WHERE action_id = @ActionId AND del_flg = 0;",
new HstActionDto { ActionId = actionId });

            return dto;
        }

        /// <summary>
        /// <see cref="HstActionDto.GroupId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="groupId">グループID</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<HstActionDto>> FindByGroupIdAsync(int groupId)
        {
            using FuncLog funcLog = new(new { groupId }, Log.LogLevel.Trace);

            var dtoList = await this.mDbHandler.QueryAsync<HstActionDto>(@"
SELECT *
FROM hst_action
WHERE group_id = @GroupId AND del_flg = 0;",
new HstActionDto { GroupId = groupId });

            return dtoList;
        }

        /// <summary>
        /// <see cref="HstActionDto.ActionId"/> と同グループに属する同日以降のレコードを取得する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <returns>取得したレコードリスト</returns>
        /// <remarks>同日を含む</remarks>
        public async Task<IEnumerable<HstActionDto>> FindInGroupAfterDateByIdAsync(int actionId)
        {
            using FuncLog funcLog = new(new { actionId }, Log.LogLevel.Trace);

            var dtoList = await this.mDbHandler.QueryAsync<HstActionDto>(@"
SELECT *
FROM hst_action 
WHERE del_flg = 0 AND group_id = (SELECT group_id FROM hst_action WHERE action_id = @ActionId) AND (SELECT act_time FROM hst_action WHERE action_id = @ActionId) <= act_time
ORDER BY act_time ASC;",
new HstActionDto { ActionId = actionId });

            return dtoList;
        }

        /// <summary>
        /// <see cref="HstActionDto.BookId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<HstActionDto>> FindByBookIdAsync(int bookId)
        {
            using FuncLog funcLog = new(new { bookId }, Log.LogLevel.Trace);

            var dtoList = await this.mDbHandler.QueryAsync<HstActionDto>(@"
SELECT *
FROM hst_action
WHERE book_id = @BookId AND del_flg = 0;",
new HstActionDto { BookId = bookId });

            return dtoList;
        }

        /// <summary>
        /// <see cref="HstActionDto.ItemId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="itemId">項目ID</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<HstActionDto>> FindByItemIdAsync(int itemId)
        {
            using FuncLog funcLog = new(new { itemId }, Log.LogLevel.Trace);

            var dtoList = await this.mDbHandler.QueryAsync<HstActionDto>(@"
SELECT *
FROM hst_action
WHERE del_flg = 0 AND item_id = @ItemId;",
new HstActionDto { ItemId = itemId });

            return dtoList;
        }

        /// <summary>
        /// <see cref="HstActionDto.BookId"/> と <see cref="HstActionDto.ItemId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<HstActionDto>> FindByBookIdAndItemIdAsync(int bookId, int itemId)
        {
            using FuncLog funcLog = new(new { bookId, itemId }, Log.LogLevel.Trace);

            var dtoList = await this.mDbHandler.QueryAsync<HstActionDto>(@"
SELECT *
FROM hst_action
WHERE book_id = @BookId AND item_id = @ItemId AND del_flg = 0;",
new HstActionDto { BookId = bookId, ItemId = itemId });

            return dtoList;
        }

        public override async Task SetIdSequenceAsync(int idSeq)
        {
            using FuncLog funcLog = new(new { idSeq }, Log.LogLevel.Trace);

            _ = await this.mDbHandler.ExecuteAsync(@"SELECT setval('hst_action_action_id_seq', @ActionIdSeq);", new { ActionIdSeq = idSeq }, DBKindMask.PostgreSQL);
        }

        public override async Task<int> InsertAsync(HstActionDto dto)
        {
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
INSERT INTO hst_action
(action_id, book_id, item_id, act_time, act_value, shop_name, group_id, remark, is_match, json_code, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ActionId, @BookId, @ItemId, @ActTime, @ActValue, @ShopName, @GroupId, @Remark, @IsMatch, @JsonCode, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter);", dto);

            return count;
        }

        /// <summary>
        /// レコードを挿入し、<see cref="HstActionDto.ActionId"/> を返す
        /// </summary>
        /// <param name="dto">挿入するレコード</param>
        /// <returns>帳簿項目ID</returns>
        public override async Task<int> InsertReturningIdAsync(HstActionDto dto)
        {
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

            int actionId = await this.mDbHandler.QuerySingleAsync<int>(@"
INSERT INTO hst_action
(book_id, item_id, act_time, act_value, shop_name, group_id, remark, is_match, json_code, del_flg, update_time, updater, insert_time, inserter)
VALUES (@BookId, @ItemId, @ActTime, @ActValue, @ShopName, @GroupId, @Remark, @IsMatch, @JsonCode, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter)
RETURNING action_id;", dto);

            return actionId;
        }

        /// <summary>
        /// 移動レコードを挿入し、<see cref="HstActionDto.ActionId"/> を返す
        /// </summary>
        /// <param name="dto">挿入する移動レコード</param>
        /// <param name="balanceKind">移動の収支種別</param>
        /// <returns>帳簿項目ID</returns>
        public async Task<int> InsertMoveActionReturningIdAsync(HstActionDto dto, int balanceKind)
        {
            using FuncLog funcLog = new(new { dto, balanceKind }, Log.LogLevel.Trace);

            int actionId = await this.mDbHandler.QuerySingleAsync<int>(@"
INSERT INTO hst_action (book_id, item_id, act_time, act_value, group_id, is_match, json_code, del_flg, update_time, updater, insert_time, inserter)
VALUES (@BookId, (
  SELECT item_id FROM mst_item I
  INNER JOIN (SELECT * FROM mst_category WHERE balance_kind = @BalanceKind) C ON C.category_id = I.category_id
  WHERE move_flg = 1
), @ActTime, @ActValue, @GroupId, @IsMatch, @JsonCode, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter)
RETURNING action_id;",
new { dto.BookId, dto.ActTime, dto.ActValue, dto.GroupId, dto.IsMatch, dto.JsonCode, dto.DelFlg, dto.UpdateTime, dto.Updater, dto.InsertTime, dto.Inserter, BalanceKind = balanceKind });

            return actionId;
        }

        public override async Task<int> UpdateAsync(HstActionDto dto)
        {
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE hst_action
SET book_id = @BookId, item_id = @ItemId, act_time = @ActTime, act_value = @ActValue, shop_name = @ShopName, remark = @Remark, group_id = @GroupId, is_match = @IsMatch, 
    json_code = @JsonCode, update_time = @UpdateTime, updater = @Updater
WHERE action_id = @ActionId;", dto);

            return count;
        }

        /// <summary>
        /// 移動レコードを更新する
        /// </summary>
        /// <param name="dto">更新する移動レコード</param>
        /// <returns>更新件数</returns>
        public async Task<int> UpdateMoveActionAsync(HstActionDto dto)
        {
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE hst_action
SET book_id = @BookId, act_time = @ActTime, act_value = @ActValue, json_code = @JsonCode, update_time = @UpdateTime, updater = @Updater
WHERE action_id = @ActionId;", dto);

            return count;
        }

        /// <summary>
        /// <see cref="HstActionDto.ActionId"/> に基づいて、<see cref="HstActionDto.IsMatch"/> 以外のレコードを更新する
        /// </summary>
        /// <param name="dto">更新するレコード</param>
        /// <returns>更新件数</returns>
        public async Task<int> UpdateWithoutIsMatchAsync(HstActionDto dto)
        {
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE hst_action
SET book_id = @BookId, item_id = @ItemId, act_time = @ActTime, act_value = @ActValue, shop_name = @ShopName, remark = @Remark, group_id = @GroupId, 
    json_code = @JsonCode, update_time = @UpdateTime, updater = @Updater
WHERE action_id = @ActionId;", dto);

            return count;
        }

        /// <summary>
        /// <see cref="HstActionDto.ActionId"/> に基づいて、<see cref="HstActionDto.IsMatch"/> を更新する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <param name="isMatch">一致フラグ</param>
        /// <returns>削除件数</returns>
        public async Task<int> UpdateIsMatchByIdAsync(int actionId, int isMatch)
        {
            using FuncLog funcLog = new(new { actionId, isMatch }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE hst_action
SET is_match = @IsMatch, update_time = @UpdateTime, updater = @Updater
WHERE action_id = @ActionId and is_match <> @IsMatch;",
new HstActionDto { IsMatch = isMatch, ActionId = actionId });

            return count;
        }

        /// <summary>
        /// <see cref="HstActionDto.ActionId"/> に基づいて、<see cref="HstActionDto.ShopName"/> と <see cref="HstActionDto.Remark"/> を更新する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <param name="shopName">店舗名</param>
        /// <param name="remark">備考</param>
        /// <returns>更新件数</returns>
        public async Task<int> UpdateShopNameAndRemarkByIdAsync(int actionId, string shopName, string remark)
        {
            using FuncLog funcLog = new(new { actionId, shopName, remark }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE hst_action SET shop_name = @ShopName, remark = @Remark, json_code = @JsonCode, update_time = @UpdateTime, updater = @Updater
WHERE action_id = @ActionId AND del_flg = 0;",
new HstActionDto { ActionId = actionId, ShopName = shopName, Remark = remark });

            return count;
        }

        /// <summary>
        /// <see cref="HstActionDto.ActionId"/> に基づいて、<see cref="HstActionDto.GroupId"/> をクリアする
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <returns>クリア件数</returns>
        public async Task<int> ClearGroupIdByIdAsync(int actionId)
        {
            using FuncLog funcLog = new(new { actionId }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE hst_action
SET group_id = null, update_time = @UpdateTime, updater = @Updater
WHERE action_id = @ActionId AND del_flg = 0;",
new HstActionDto { ActionId = actionId });

            return count;
        }

        public override Task<int> UpsertAsync(HstActionDto dto) => throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod()?.Name}).");

        public override async Task<int> DeleteAllAsync()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"DELETE FROM hst_action;");

            return count;
        }

        /// <summary>
        /// <see cref="HstActionDto.ActionId"/> に基づいて、レコードを削除する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <returns>削除件数</returns>
        public override async Task<int> DeleteByIdAsync(int actionId)
        {
            using FuncLog funcLog = new(new { actionId }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE hst_action
SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE action_id = @ActionId;",
new HstActionDto { ActionId = actionId });

            return count;
        }

        /// <summary>
        /// <see cref="HstActionDto.GroupId"/> に基づいて、レコードを削除する
        /// </summary>
        /// <param name="groupId">グループID</param>
        /// <returns>削除件数</returns>
        public async Task<int> DeleteByGroupIdAsync(int groupId)
        {
            using FuncLog funcLog = new(new { groupId }, Log.LogLevel.Trace);

            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE hst_action
SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE group_id = @GroupId;",
new HstActionDto { GroupId = groupId });

            return count;
        }

        /// <summary>
        /// <see cref="HstActionDto.ActionId"/> と同グループに属する同日以降のレコードを削除する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <param name="withInTarget">指定したレコードを削除するか</param>
        /// <returns>削除件数</returns>
        public async Task<int> DeleteInGroupAfterDateByIdAsync(int actionId, bool withInTarget)
        {
            using FuncLog funcLog = new(new { actionId, withInTarget }, Log.LogLevel.Trace);

            int count = withInTarget
                ? await this.mDbHandler.ExecuteAsync(@"
UPDATE hst_action
SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE group_id = (SELECT group_id FROM hst_action WHERE action_id = @ActionId) AND (SELECT act_time FROM hst_action WHERE action_id = @ActionId) <= act_time;",
new HstActionDto { ActionId = actionId })
                : await this.mDbHandler.ExecuteAsync(@"
UPDATE hst_action
SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE group_id = (SELECT group_id FROM hst_action WHERE action_id = @ActionId) AND (SELECT act_time FROM hst_action WHERE action_id = @ActionId) < act_time;",
new HstActionDto { ActionId = actionId });

            return count;
        }
    }
}
