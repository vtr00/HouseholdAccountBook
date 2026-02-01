using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Enums;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.DbTable
{
    /// <summary>
    /// グループテーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class HstGroupDao(DbHandlerBase dbHandler) : PrimaryKeyDaoBase<HstGroupDto, int>(dbHandler)
    {
        public override Task<int> CreateTableAsync() => throw new NotImplementedException();

        public override async Task<IEnumerable<HstGroupDto>> FindAllAsync()
        {
            var dtoList = await this.mDbHandler.QueryAsync<HstGroupDto>(@"
SELECT * 
FROM hst_group
WHERE del_flg = 0;");

            return dtoList;
        }

        /// <summary>
        /// <see cref="HstActionDto.GroupId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="groupId">グループID</param>
        /// <returns>取得したレコード</returns>
        public override async Task<HstGroupDto> FindByIdAsync(int groupId)
        {
            var dto = await this.mDbHandler.QuerySingleOrDefaultAsync<HstGroupDto>(@"
SELECT * FROM hst_group
WHERE group_id = @GroupId AND del_flg = 0;",
new HstGroupDto { GroupId = groupId });

            return dto;
        }

        public override async Task SetIdSequenceAsync(int idSeq)
        {
            if (this.mDbHandler.DBKind == DBKind.SQLite) {
                return;
            }

            _ = await this.mDbHandler.ExecuteAsync("SELECT setval('hst_group_group_id_seq', @GroupIdSeq);", new { GroupIdSeq = idSeq });
        }

        public override async Task<int> InsertAsync(HstGroupDto dto)
        {
            int count = await this.mDbHandler.ExecuteAsync(@"
INSERT INTO hst_group
(group_id, group_kind, remark, json_code, del_flg, update_time, updater, insert_time, inserter)
VALUES (@GroupId, @GroupKind, @Remark, @JsonCode, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter);", dto);

            return count;
        }

        public override async Task<int> InsertReturningIdAsync(HstGroupDto dto)
        {
            int groupId = await this.mDbHandler.QuerySingleAsync<int>(@"
INSERT INTO hst_group
(group_kind, remark, json_code, del_flg, update_time, updater, insert_time, inserter)
VALUES (@GroupKind, @Remark, @JsonCode, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter)
RETURNING group_id;", dto);

            return groupId;
        }

        public override Task<int> UpdateAsync(HstGroupDto dto) => throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");

        public override Task<int> UpsertAsync(HstGroupDto dto) => throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");

        public override async Task<int> DeleteAllAsync()
        {
            int count = await this.mDbHandler.ExecuteAsync(@"DELETE FROM hst_group;");

            return count;
        }

        /// <summary>
        /// <see cref="HstActionDto.GroupId"/> に基づいて、レコードを削除する
        /// </summary>
        /// <param name="groupId">グループID</param>
        /// <returns>削除件数</returns>
        public override async Task<int> DeleteByIdAsync(int groupId)
        {
            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE hst_group SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE group_id = @GroupId AND del_flg = 0;",
new HstGroupDto { GroupId = groupId });

            return count;
        }
    }
}
