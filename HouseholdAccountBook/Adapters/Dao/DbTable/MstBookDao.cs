using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Enums;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using static HouseholdAccountBook.Adapters.DbConstants;

namespace HouseholdAccountBook.Adapters.Dao.DbTable
{
    /// <summary>
    /// 帳簿テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class MstBookDao(DbHandlerBase dbHandler) : PrimaryKeyDaoBase<MstBookDto, int>(dbHandler)
    {
        public override async Task<IEnumerable<MstBookDto>> FindAllAsync()
        {
            var dtoList = await this.mDbHandler.QueryAsync<MstBookDto>(@"
SELECT * 
FROM mst_book
WHERE del_flg = 0
ORDER BY sort_order;");

            return dtoList;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>取得したレコード</returns>
        public override async Task<MstBookDto> FindByIdAsync(int bookId)
        {
            var dto = await this.mDbHandler.QuerySingleOrDefaultAsync<MstBookDto>(@"
SELECT *
FROM mst_book
WHERE book_id = @BookId AND del_flg = 0;",
new MstBookDto { BookId = bookId });

            return dto;
        }

        /// <summary>
        /// <see cref="MstBookDto.JsonCode"/> が空ではない全てのレコードを取得する
        /// </summary>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<MstBookDto>> FindIfJsonCodeExistsAsync()
        {
            var dtoList = await this.mDbHandler.QueryAsync<MstBookDto>(@"
SELECT * 
FROM mst_book 
WHERE del_flg = 0 AND json_code IS NOT NULL
ORDER BY sort_order;");

            return dtoList;
        }

        public override async Task SetIdSequenceAsync(int idSeq)
        {
            if (this.mDbHandler.DBKind == DBKind.SQLite) {
                return;
            }

            _ = await this.mDbHandler.ExecuteAsync(@"SELECT setval('mst_book_book_id_seq', @BookIdSeq);", new { BookIdSeq = idSeq });
        }

        public override async Task<int> InsertAsync(MstBookDto dto)
        {
            int count = await this.mDbHandler.ExecuteAsync(@"
INSERT INTO mst_book 
(book_id, book_name, book_kind, initial_value, pay_day, debit_book_id, json_code, sort_order, del_flg, update_time, updater, insert_time, inserter) 
VALUES (@BookId, @BookName, @BookKind, @InitialValue, @PayDay, @DebitBookId, @JsonCode, @SortOrder, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter);", dto);

            return count;
        }

        public override async Task<int> InsertReturningIdAsync(MstBookDto dto)
        {
            int bookId = await this.mDbHandler.QuerySingleAsync<int>(@"
INSERT INTO mst_book (book_name, book_kind, initial_value, pay_day, debit_book_id, json_code, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@BookName, @BookKind, @InitialValue, @PayDay, @DebitBookId, @JsonCode, (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_book), @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter)
RETURNING book_id;", dto);

            return bookId;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/> に対応するレコードの設定可能な項目を更新する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>更新件数</returns>
        public async Task<int> UpdateSetableAsync(MstBookDto dto)
        {
            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE mst_book
SET book_name = @BookName, book_kind = @BookKind, initial_value = @InitialValue, debit_book_id = @DebitBookId, pay_day = @PayDay, json_code = @JsonCode, update_time = @UpdateTime, updater = @Updater
WHERE book_id = @BookId;", dto);

            return count;
        }

        public override Task<int> UpdateAsync(MstBookDto dto) => throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");

        /// <summary>
        /// レコードのソート順を入れ替える
        /// </summary>
        /// <param name="bookId1">帳簿ID1</param>
        /// <param name="bookId2">帳簿ID2</param>
        /// <returns>更新件数</returns>
        /// <remarks>PostgreSQLとSQLiteで挙動が変わる可能性があるため変更時は要動作確認</remarks>
        public async Task<int> SwapSortOrderAsync(int bookId1, int bookId2)
        {
            int count = await this.mDbHandler.ExecuteAsync(@" 
WITH original AS (
  SELECT
    (SELECT sort_order FROM mst_book WHERE book_id = @BookId1) AS sort_order1,
    (SELECT sort_order FROM mst_book WHERE book_id = @BookId2) AS sort_order2
)
UPDATE mst_book
SET sort_order = CASE
  WHEN book_id = @BookId1 THEN (SELECT sort_order2 FROM original)
  WHEN book_id = @BookId2 THEN (SELECT sort_order1 FROM original)
  ELSE sort_order
END, update_time = @UpdateTime, updater = @Updater
WHERE book_id IN (@BookId1, @BookId2);",
new { BookId1 = bookId1, BookId2 = bookId2, UpdateTime = DateTime.Now, Updater });

            return count;
        }

        public override Task<int> UpsertAsync(MstBookDto dto) => throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");

        public override async Task<int> DeleteAllAsync()
        {
            int count = await this.mDbHandler.ExecuteAsync(@"DELETE FROM mst_book;");

            return count;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/> に基づいて、レコードを削除する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>削除件数</returns>
        public override async Task<int> DeleteByIdAsync(int bookId)
        {
            int count = await this.mDbHandler.ExecuteAsync(@"
UPDATE mst_book
SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE book_id = @BookId;",
new MstBookDto { BookId = bookId });

            return count;
        }
    }
}
