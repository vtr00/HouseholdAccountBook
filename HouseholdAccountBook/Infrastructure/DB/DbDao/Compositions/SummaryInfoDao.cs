using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;

namespace HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions
{
    /// <summary>
    /// 帳簿情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class SummaryInfoDao(DbHandlerBase dbHandler) : TableDaoBase(dbHandler)
    {
        /// <summary>
        /// 全帳簿の <see cref="SummaryInfoDto"> リストを取得する
        /// </summary>
        /// <param name="startDate">開始日付</param>
        /// <param name="finishDate">終了日付(該当の日を含む)</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<SummaryInfoDto>> FindAllWithinPeriod(DateOnly startDate, DateOnly finishDate)
        {
            using FuncLog funcLog = new(new { startDate, finishDate }, Log.LogLevel.Trace);

            var dtoList = await this.mDbHandler.QueryAsync<SummaryInfoDto>(@"
SELECT C.balance_kind, C.category_id, C.category_name, SQ.item_id, I.item_name, SQ.total
FROM (
  SELECT I.item_id AS item_id, COALESCE(SUM(A.act_value), 0) AS total
  FROM mst_item I
  LEFT JOIN (SELECT * FROM hst_action WHERE @StartDate <= act_time AND act_time < @FinishDate AND del_flg = 0) A ON A.item_id = I.item_id
  WHERE I.item_id IN (SELECT item_id FROM rel_book_item WHERE del_flg = 0) AND I.del_flg = 0
  GROUP BY I.item_id
) SQ -- Sub Query
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = SQ.item_id
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
ORDER BY C.balance_kind, C.sort_order, I.move_flg DESC, I.sort_order;",
new { StartDate = startDate, FinishDate = finishDate.AddDays(1) });

            return dtoList;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/> に基づいて、<see cref="SummaryInfoDto"> リストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startDate">開始日付</param>
        /// <param name="finishDate">終了日付(該当の日を含む)</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<SummaryInfoDto>> FindByBookIdWithinPeriod(int bookId, DateOnly startDate, DateOnly finishDate)
        {
            using FuncLog funcLog = new(new { bookId, startDate, finishDate }, Log.LogLevel.Trace);

            var dtoList = await this.mDbHandler.QueryAsync<SummaryInfoDto>(@"
SELECT C.balance_kind, C.category_id, C.category_name, SQ.item_id, I.item_name, SQ.total
FROM (
  SELECT I.item_id AS item_id, COALESCE(SUM(A.act_value), 0) AS total
  FROM mst_item I
  LEFT JOIN (SELECT * FROM hst_action WHERE book_id = @BookId AND @StartDate <= act_time AND act_time < @FinishDate AND del_flg = 0) A ON A.item_id = I.item_id
  WHERE (I.move_flg = 1 OR I.item_id IN (SELECT item_id FROM rel_book_item WHERE book_id = @BookId AND del_flg = 0)) AND I.del_flg = 0
  GROUP BY I.item_id
) SQ -- Sub Query
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = SQ.item_id
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
ORDER BY C.balance_kind, C.sort_order, I.move_flg DESC, I.sort_order;",
new { BookId = bookId, StartDate = startDate, FinishDate = finishDate.AddDays(1) });

            return dtoList;
        }
    }
}
