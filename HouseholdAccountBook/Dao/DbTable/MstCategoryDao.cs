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
    /// 分類テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class MstCategoryDao(DbHandlerBase dbHandler) : PrimaryKeyDaoBase<MstCategoryDto, int>(dbHandler)
    {
        public override async Task<IEnumerable<MstCategoryDto>> FindAllAsync()
        {
            var dtoList = await this.dbHandler.QueryAsync<MstCategoryDto>(@"
SELECT * 
FROM mst_category
WHERE del_flg = 0;");

            return dtoList;
        }

        public override async Task<MstCategoryDto> FindByIdAsync(int pkey)
        {
            var dto = await this.dbHandler.QuerySingleOrDefaultAsync<MstCategoryDto>(@"
SELECT *
FROM mst_category
WHERE category_id = @CategoryId AND del_flg = 0;",
new MstCategoryDto { CategoryId = pkey });

            return dto;
        }

        /// <summary>
        /// <see cref="MstCategoryDto.BalanceKind"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="balanceKind">収支種別</param>
        /// <returns>DTOリスト</returns>
        /// <remarks>移動を除く</remarks>
        public async Task<IEnumerable<MstCategoryDto>> FindByBalanceKindAsync(int balanceKind)
        {
            var dtoList = await this.dbHandler.QueryAsync<MstCategoryDto>(@"
SELECT *
FROM mst_category
WHERE balance_kind = @BalanceKind AND del_flg = 0 AND sort_order <> 0
ORDER BY sort_order;",
new MstCategoryDto { BalanceKind = balanceKind });

            return dtoList;
        }

        public override async Task SetIdSequenceAsync(int idSeq)
        {
            if (this.dbHandler.DBKind == DBKind.SQLite) {
                return;
            }

            _ = await this.dbHandler.ExecuteAsync(@"SELECT setval('mst_category_category_id_seq', @CategoryIdSeq);", new { CategoryIdSeq = idSeq });
        }

        public override async Task<int> InsertAsync(MstCategoryDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
INSERT INTO mst_category
(category_id, category_name, balance_kind, json_code, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@CategoryId, @CategoryName, @BalanceKind, @JsonCode, @SortOrder, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter);", dto);

            return count;
        }

        public override async Task<int> InsertReturningIdAsync(MstCategoryDto dto)
        {
            int categoryId = await this.dbHandler.QuerySingleAsync<int>(@"
INSERT INTO mst_category
(category_name, balance_kind, json_code, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@CategoryName, @BalanceKind, @JsonCode, (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_category), @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter)
RETURNING category_id;", dto);

            return categoryId;
        }

        public override Task<int> UpdateAsync(MstCategoryDto dto)
        {
            throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");
        }

        /// <summary>
        /// <see cref="MstCategoryDto.CategoryId"/> に基づいて、 <see cref="MstCategoryDto.CategoryName"/> を更新する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>更新行数</returns>
        public async Task<int> UpdateSetableAsync(MstCategoryDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE mst_category
SET category_name = @CategoryName, update_time = @UpdateTime, updater = @Updater
WHERE category_id = @CategoryId;", dto);

            return count;
        }

        /// <summary>
        /// レコードのソート順を入れ替える
        /// </summary>
        /// <param name="categoryId1">分類ID1</param>
        /// <param name="categoryId2">分類ID2</param>
        /// <returns>更新行数</returns>
        /// <remarks>PostgreSQLとSQLiteで挙動が変わる可能性があるため変更時は要動作確認</remarks>
        public async Task<int> SwapSortOrderAsync(int categoryId1, int categoryId2)
        {
            int count = await this.dbHandler.ExecuteAsync(@" 
WITH original AS (
  SELECT
    (SELECT sort_order FROM mst_category WHERE category_id = @CategoryId1) AS sort_order1,
    (SELECT sort_order FROM mst_category WHERE category_id = @CategoryId2) AS sort_order2
)
UPDATE mst_category
SET sort_order = CASE
  WHEN category_id = @CategoryId1 THEN (SELECT sort_order2 FROM original)
  WHEN category_id = @CategoryId2 THEN (SELECT sort_order1 FROM original)
  ELSE sort_order
END, update_time = @UpdateTime, updater = @Updater
WHERE category_id IN (@CategoryId1, @CategoryId2);",
new { CategoryId1 = categoryId1, CategoryId2 = categoryId2, UpdateTime = DateTime.Now, Updater });

            return count;
        }

        public override Task<int> UpsertAsync(MstCategoryDto dto)
        {
            throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");
        }

        public override async Task<int> DeleteAllAsync()
        {
            int count = await this.dbHandler.ExecuteAsync(@"DELETE FROM mst_category;");

            return count;
        }

        public override async Task<int> DeleteByIdAsync(int pkey)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE mst_category
SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE category_id = @CategoryId;",
new MstCategoryDto { CategoryId = pkey });

            return count;
        }
    }
}
