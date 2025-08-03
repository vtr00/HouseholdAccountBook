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
    /// 帳簿テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class MstBookDao(DbHandlerBase dbHandler) : PrimaryKeyDaoBase<MstBookDto, int>(dbHandler)
    {
        public override async Task<IEnumerable<MstBookDto>> FindAllAsync()
        {
            var dtoList = await this.dbHandler.QueryAsync<MstBookDto>(@"
SELECT * 
FROM mst_book
WHERE del_flg = 0
ORDER BY sort_order;");

            return dtoList;
        }

        public override async Task<MstBookDto> FindByIdAsync(int pkey)
        {
            var dto = await this.dbHandler.QuerySingleOrDefaultAsync<MstBookDto>(@"
SELECT *
FROM mst_book
WHERE book_id = @BookId AND del_flg = 0;",
new MstBookDto { BookId = pkey });

            return dto;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookKind"/> を除外して全てのレコードを取得する
        /// </summary>
        /// <returns>DTO</returns>
        public async Task<IEnumerable<MstBookDto>> FindIfJsonCodeExistsAsync()
        {
            var dtoList = await this.dbHandler.QueryAsync<MstBookDto>(@"
SELECT * 
FROM mst_book 
WHERE del_flg = 0 AND json_code IS NOT NULL
ORDER BY sort_order;");

            return dtoList;
        }

        public override async Task SetIdSequenceAsync(int idSeq)
        {
            _ = await this.dbHandler.ExecuteAsync(@"SELECT setval('mst_book_book_id_seq', @BookIdSeq);", new { BookIdSeq = idSeq });
        }

        public override async Task<int> InsertAsync(MstBookDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
INSERT INTO mst_book 
(book_id, book_name, book_kind, initial_value, pay_day, debit_book_id, sort_order, del_flg, update_time, updater, insert_time, inserter) 
VALUES (@BookId, @BookName, @BookKind, @InitialValue, @PayDay, @DebitBookId, @SortOrder, @DelFlg, 'now', @Updater, 'now', @Inserter);", dto);

            return count;
        }

        public override async Task<int> InsertReturningIdAsync(MstBookDto dto)
        {
            int bookId = await this.dbHandler.QuerySingleAsync<int>(@"
INSERT INTO mst_book (book_name, book_kind, initial_value, pay_day, debit_book_id, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@BookName, @BookKind, @InitialValue, @PayDay, @DebitBookId, (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_book), @DelFlg, 'now', @Updater, 'now', @Inserter)
RETURNING book_id;", dto);

            return bookId;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/> に対応するレコードの設定可能な項目を更新する
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>更新行数</returns>
        public async Task<int> UpdateSetableAsync(MstBookDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE mst_book
SET book_name = @BookName, book_kind = @BookKind, initial_value = @InitialValue, debit_book_id = @DebitBookId, pay_day = @PayDay, json_code = @JsonCode, update_time = 'now', updater = @Updater
WHERE book_id = @BookId;", dto);

            return count;
        }

        public override Task<int> UpdateAsync(MstBookDto dto)
        {
            throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");
        }

        /// <summary>
        /// レコードのソート順を入れ替える
        /// </summary>
        /// <param name="bookId1">帳簿ID1</param>
        /// <param name="bookId2">帳簿ID2</param>
        /// <returns></returns>
        public async Task<int> SwapSortOrderAsync(int bookId1, int bookId2)
        {
            int count = await this.dbHandler.ExecuteAsync(@" 
UPDATE mst_book
SET sort_order = CASE
  WHEN book_id = @BookId1 THEN (SELECT sort_order FROM mst_book WHERE book_id = @BookId2)
  WHEN book_id = @BookId2 THEN (SELECT sort_order FROM mst_book WHERE book_id = @BookId1)
  ELSE sort_order
END, update_time = 'now', updater = @Updater
WHERE book_id IN (@BookId1, @BookId2);",
new { BookId1 = bookId1, BookId2 = bookId2, Updater });

            return count;
        }

        public override Task<int> UpsertAsync(MstBookDto dto)
        {
            throw new NotSupportedException($"Unsupported operation({MethodBase.GetCurrentMethod().Name}).");
        }

        public override async Task<int> DeleteAllAsync()
        {
            int count = await this.dbHandler.ExecuteAsync(@"DELETE FROM mst_book;");

            return count;
        }

        public override async Task<int> DeleteByIdAsync(int pkey)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE mst_book
SET del_flg = 1, update_time = 'now', updater = @Updater
WHERE book_id = @BookId;",
new MstBookDto { BookId = pkey });

            return count;
        }
    }
}
