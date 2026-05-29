using HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

            IEnumerable<SummaryInfoDto> dtoList = await this.mDbHandler.QueryAsync<SummaryInfoDto>(@"
SELECT C.balance_kind, C.category_id, C.category_name, I.item_id, I.item_name, COALESCE(SUM(A.act_value), 0) AS total
FROM mst_item I
INNER JOIN rel_book_item RBI ON RBI.item_id = I.item_id AND RBI.del_flg = 0
INNER JOIN mst_category C ON C.category_id = I.category_id AND C.del_flg = 0
LEFT JOIN hst_action A ON A.item_id = I.item_id AND A.book_id = RBI.book_id AND A.del_flg = 0 AND @StartDate <= A.act_time AND A.act_time < @FinishDate
WHERE I.move_flg = 0 AND I.del_flg = 0
GROUP BY C.balance_kind, C.category_id, C.category_name, I.item_id, I.item_name, C.sort_order, I.sort_order
ORDER BY C.balance_kind, C.sort_order, I.sort_order;",
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

            IEnumerable<SummaryInfoDto> dtoList = await this.mDbHandler.QueryAsync<SummaryInfoDto>(@"
SELECT C.balance_kind, C.category_id, C.category_name, I.item_id, I.item_name, COALESCE(SUM(A.act_value), 0) AS total
FROM mst_item I
INNER JOIN mst_category C ON C.category_id = I.category_id AND C.del_flg = 0
LEFT JOIN hst_action A ON A.item_id = I.item_id AND A.book_id = @BookId AND A.del_flg = 0 AND @StartDate <= A.act_time AND A.act_time < @FinishDate
WHERE (EXISTS (
    SELECT * FROM rel_book_item RBI
    WHERE RBI.item_id = I.item_id AND RBI.book_id = @BookId AND RBI.del_flg = 0) OR I.move_flg = 1) AND I.del_flg = 0
GROUP BY C.balance_kind, C.category_id, C.category_name, I.item_id, I.item_name, C.sort_order, I.move_flg, I.sort_order
ORDER BY C.balance_kind, C.sort_order, I.move_flg DESC, I.sort_order;",
new { BookId = bookId, StartDate = startDate, FinishDate = finishDate.AddDays(1) });

            return dtoList;
        }
    }
}
