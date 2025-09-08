using HouseholdAccountBook.Models.Dao.Abstract;
using HouseholdAccountBook.Models.DbHandler.Abstract;
using HouseholdAccountBook.Models.Dto.Abstract;
using HouseholdAccountBook.Models.Dto.DbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Dao.DbTable
{
    /// <summary>
    /// 備考テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class HstRemarkDao(DbHandlerBase dbHandler) : ReadWriteTableDaoBase<HstRemarkDto>(dbHandler)
    {
        public override async Task<IEnumerable<HstRemarkDto>> FindAllAsync()
        {
            var dtoList = await this.dbHandler.QueryAsync<HstRemarkDto>(@"
SELECT * 
FROM hst_remark
WHERE del_flg = 0;");

            return dtoList;
        }

        /// <summary>
        /// <see cref="HstRemarkDto.Remark"/> と <see cref="HstRemarkDto.ItemId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="dto"><see cref="HstRemarkDto"/></param>
        /// <param name="includeDeleted"><see cref="TableDtoBase.DelFlg">=1も含めるか</param>
        /// <returns>DTOリスト</returns>
        public async Task<HstRemarkDto> FindByRemarkAndItemIdAsync(string remark, int itemId, bool includeDeleted = false)
        {
            var dto = includeDeleted
                ? await this.dbHandler.QuerySingleOrDefaultAsync<HstRemarkDto>(@"
SELECT * FROM hst_remark
WHERE item_id = @ItemId AND remark = @Remark;",
new HstRemarkDto { Remark = remark, ItemId = itemId })
                : await this.dbHandler.QuerySingleOrDefaultAsync<HstRemarkDto>(@"
SELECT * FROM hst_remark
WHERE item_id = @ItemId AND remark = @Remark AND del_flg = 0;",
new HstRemarkDto { Remark = remark, ItemId = itemId });

            return dto;
        }

        public override async Task<int> InsertAsync(HstRemarkDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, json_code, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemId, @Remark, @RemarkKind, @UsedTime, @JsonCode, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter);", dto);

            return count;
        }

        /// <summary>
        /// <see cref="HstRemarkDto.ItemId"/> と <see cref="HstRemarkDto.Remark"/> が一致するレコードを更新する
        /// </summary>
        /// <param name="dto"><see cref="HstRemarkDto"/></param>
        /// <returns>更新行数</returns>
        public override async Task<int> UpdateAsync(HstRemarkDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE hst_remark
SET used_time = @UsedTime, json_code = @JsonCode, del_flg = @DelFlg, update_time = @UpdateTime, updater = @Updater
WHERE item_id = @ItemId AND remark = @Remark AND used_time < @UsedTime;", dto);

            return count;
        }

        /// <summary>
        /// <see cref="HstRemarkDto"/> を挿入または更新する
        /// </summary>
        /// <param name="dto"><see cref="HstRemarkDto"/></param>
        /// <returns>挿入/更新行数</returns>
        /// <remarks>PostgreSQLとSQLiteで挙動が変わる可能性があるため変更時は要動作確認</remarks>
        public override async Task<int> UpsertAsync(HstRemarkDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
INSERT INTO hst_remark (item_id, remark, remark_kind, used_time, json_code, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemId, @Remark, @RemarkKind, @UsedTime, @JsonCode, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter)
ON CONFLICT (item_id, remark) DO UPDATE
SET used_time = @UsedTime, json_code = @JsonCode, del_flg = @DelFlg, update_time = @UpdateTime, updater = @Updater
WHERE hst_remark.item_id = @ItemId AND hst_remark.remark = @Remark AND hst_remark.used_time < @UsedTime;", dto);

            return count;
        }

        public override async Task<int> DeleteAllAsync()
        {
            int count = await this.dbHandler.ExecuteAsync(@"DELETE FROM hst_remark;");

            return count;
        }

        /// <summary>
        /// <see cref="HstRemarkDto.ItemId"/> と <see cref="HstRemarkDto.Remark"/> が一致するレコードを削除する
        /// </summary>
        /// <param name="dto"><see cref="HstRemarkDto"/></param>
        /// <returns>削除行数</returns>
        public async Task<int> DeleteAsync(HstRemarkDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE hst_remark SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE remark = @Remark AND item_id = @ItemId;", dto);

            return count;
        }
    }
}
