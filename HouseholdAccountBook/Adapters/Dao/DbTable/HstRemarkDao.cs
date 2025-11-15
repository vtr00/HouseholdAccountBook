using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Dto.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.DbTable
{
    /// <summary>
    /// 備考テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class HstRemarkDao(DbHandlerBase dbHandler) : ReadWriteTableDaoBase<HstRemarkDto>(dbHandler)
    {
        public override async Task<IEnumerable<HstRemarkDto>> FindAllAsync()
        {
            var dtoList = await this.mDbHandler.QueryAsync<HstRemarkDto>(@"
SELECT * 
FROM hst_remark
WHERE del_flg = 0;");

            return dtoList;
        }

        /// <summary>
        /// <see cref="HstRemarkDto.Remark"/> と <see cref="HstRemarkDto.ItemId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="remark">備考</param>
        /// <param name="itemId">項目ID</param>
        /// <param name="includeDeleted"><see cref="TableDtoBase.DelFlg">=1も含めるか</param>
        /// <returns>取得したレコード</returns>
        public async Task<HstRemarkDto> FindByRemarkAndItemIdAsync(string remark, int itemId, bool includeDeleted = false)
        {
            var dto = includeDeleted
                ? await this.mDbHandler.QuerySingleOrDefaultAsync<HstRemarkDto>(@"
SELECT * FROM hst_remark
WHERE item_id = @ItemId AND remark = @Remark;",
new HstRemarkDto { Remark = remark, ItemId = itemId })
                : await this.mDbHandler.QuerySingleOrDefaultAsync<HstRemarkDto>(@"
SELECT * FROM hst_remark
WHERE item_id = @ItemId AND remark = @Remark AND del_flg = 0;",
new HstRemarkDto { Remark = remark, ItemId = itemId });

            return dto;
        }

        public override async Task<int> InsertAsync(HstRemarkDto dto)
        {
            int count = await this.mDbHandler.ExecuteAsync(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, json_code, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemId, @Remark, @RemarkKind, @UsedTime, @JsonCode, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter);", dto);

            return count;
        }

        /// <summary>
        /// <see cref="HstRemarkDto.ItemId"/> と <see cref="HstRemarkDto.Remark"/> が一致するレコードを更新する
        /// </summary>
        /// <param name="dto">更新するレコード</param>
        /// <returns>更新件数</returns>
        public override async Task<int> UpdateAsync(HstRemarkDto dto)
        {
            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE hst_remark
SET used_time = @UsedTime, json_code = @JsonCode, del_flg = @DelFlg, update_time = @UpdateTime, updater = @Updater
WHERE item_id = @ItemId AND remark = @Remark AND used_time < @UsedTime;", dto);

            return count;
        }

        /// <summary>
        /// <see cref="HstRemarkDto"/> を挿入または更新する
        /// </summary>
        /// <param name="dto">挿入/更新するレコード</param>
        /// <returns>挿入/更新件数</returns>
        /// <remarks>PostgreSQLとSQLiteで挙動が変わる可能性があるため変更時は要動作確認</remarks>
        public override async Task<int> UpsertAsync(HstRemarkDto dto)
        {
            int count = await this.mDbHandler.ExecuteAsync(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, json_code, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemId, @Remark, @RemarkKind, @UsedTime, @JsonCode, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter)
ON CONFLICT (item_id, remark) DO UPDATE
SET used_time = @UsedTime, json_code = @JsonCode, del_flg = @DelFlg, update_time = @UpdateTime, updater = @Updater
WHERE hst_remark.item_id = @ItemId AND hst_remark.remark = @Remark AND hst_remark.used_time < @UsedTime;", dto);

            return count;
        }

        public override async Task<int> DeleteAllAsync()
        {
            int count = await this.mDbHandler.ExecuteAsync(@"DELETE FROM hst_remark;");

            return count;
        }

        /// <summary>
        /// <see cref="HstRemarkDto.ItemId"/> と <see cref="HstRemarkDto.Remark"/> が一致するレコードを削除する
        /// </summary>
        /// <param name="dto">削除するレコード</param>
        /// <returns>削除件数</returns>
        public async Task<int> DeleteAsync(HstRemarkDto dto)
        {
            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE hst_remark SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE remark = @Remark AND item_id = @ItemId;", dto);

            return count;
        }
    }
}
