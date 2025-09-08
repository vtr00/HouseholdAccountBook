using HouseholdAccountBook.Models.Dao.Abstract;
using HouseholdAccountBook.Models.DbHandler.Abstract;
using HouseholdAccountBook.Models.Dto.DbTable;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using static HouseholdAccountBook.Models.DbConstants;

namespace HouseholdAccountBook.Models.Dao.DbTable
{
    /// <summary>
    /// グループテーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class HstGroupDao(DbHandlerBase dbHandler) : PrimaryKeyDaoBase<HstGroupDto, int>(dbHandler)
    {
        public override async Task<IEnumerable<HstGroupDto>> FindAllAsync()
        {
            var dtoList = await this.dbHandler.QueryAsync<HstGroupDto>(@"
SELECT * 
FROM hst_group
WHERE del_flg = 0;");

            return dtoList;
        }

        public override async Task<HstGroupDto> FindByIdAsync(int pkey)
        {
            var dto = await this.dbHandler.QuerySingleOrDefaultAsync<HstGroupDto>(@"
SELECT * FROM hst_group
WHERE group_id = @GroupId AND del_flg = 0;",
new HstGroupDto { GroupId = pkey });

            return dto;
        }

        public override async Task SetIdSequenceAsync(int id)
        {
            if (this.dbHandler.DBLibKind == DBLibraryKind.SQLite) {
                return;
            }

            _ = await this.dbHandler.ExecuteAsync("SELECT setval('hst_group_group_id_seq', @GroupIdSeq);", new { GroupIdSeq = id });
        }

        public override async Task<int> InsertAsync(HstGroupDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
INSERT INTO hst_group
(group_id, group_kind, remark, json_code, del_flg, update_time, updater, insert_time, inserter)
VALUES (@GroupId, @GroupKind, @Remark, @JsonCode, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter);", dto);

            return count;
        }

        public override async Task<int> InsertReturningIdAsync(HstGroupDto dto)
        {
            int groupId = await this.dbHandler.QuerySingleAsync<int>(@"
INSERT INTO hst_group
(group_kind, remark, json_code, del_flg, update_time, updater, insert_time, inserter)
VALUES (@GroupKind, @Remark, @JsonCode, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter)
RETURNING group_id;", dto);

            return groupId;
        }

        public override Task<int> UpdateAsync(HstGroupDto dto)
        {
            throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");
        }

        public override Task<int> UpsertAsync(HstGroupDto dto)
        {
            throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");
        }

        public override async Task<int> DeleteAllAsync()
        {
            int count = await this.dbHandler.ExecuteAsync(@"DELETE FROM hst_group;");

            return count;
        }

        public override async Task<int> DeleteByIdAsync(int pkey)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE hst_group SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE group_id = @GroupId AND del_flg = 0;",
new HstGroupDto { GroupId = pkey });

            return count;
        }
    }
}
