using System;
using System.Threading.Tasks;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;

namespace HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions
{
    /// <summary>
    /// 繰越残高情報を取得するDAOクラス
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class EndingBalanceInfoDao(DbHandlerBase dbHandler) : TableDaoBase(dbHandler)
    {
        /// <summary>
        /// 全帳簿の開始日付までの <see cref="EndingBalanceInfoDto"/> を取得する
        /// </summary>
        /// <param name="startDate">開始日付</param>
        /// <returns>取得したレコード</returns>
        public async Task<EndingBalanceInfoDto> Find(DateOnly startDate)
        {
            using FuncLog funcLog = new(new { startDate }, Log.LogLevel.Trace);

            var dto = await this.mDbHandler.QuerySingleAsync<EndingBalanceInfoDto>(@"
SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT COALESCE(SUM(initial_value), 0) FROM mst_book WHERE del_flg = 0) AS ending_balance
FROM hst_action AA
INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
WHERE AA.del_flg = 0 AND AA.act_time < @StartDate;",
new { StartDate = startDate });

            return dto;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/>に基づいて、開始日付までの <see cref="EndingBalanceInfoDto"/> を取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startDate">開始日付</param>
        /// <returns>取得したレコード</returns>
        public async Task<EndingBalanceInfoDto> FindByBookId(int bookId, DateOnly startDate)
        {
            using FuncLog funcLog = new(new { bookId, startDate }, Log.LogLevel.Trace);

            var dto = await this.mDbHandler.QuerySingleAsync<EndingBalanceInfoDto>(@"
SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT initial_value FROM mst_book WHERE book_id = @BookId) AS ending_balance
FROM hst_action AA
INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
WHERE AA.book_id = @BookId AND AA.del_flg = 0 AND AA.act_time < @StartDate;",
new { BookId = bookId, StartDate = startDate });

            return dto;
        }
    }
}
