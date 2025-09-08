using HouseholdAccountBook.Models.Dao.Abstract;
using HouseholdAccountBook.Models.DbHandler.Abstract;
using HouseholdAccountBook.Models.Dto.DbTable;
using HouseholdAccountBook.Models.Dto.Others;
using System;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Dao.Compositions
{
    /// <summary>
    /// 繰越残高情報を取得するDAOクラス
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class EndingBalanceInfoDao(DbHandlerBase dbHandler) : ReadDaoBase(dbHandler)
    {
        /// <summary>
        /// 全帳簿の開始日付までの <see cref="EndingBalanceInfoDto"/> を取得する
        /// </summary>
        /// <param name="startTime">開始日付</param>
        /// <returns>繰越残高情報</returns>
        public async Task<EndingBalanceInfoDto> Find(DateTime startTime)
        {
            var dto = await this.dbHandler.QuerySingleAsync<EndingBalanceInfoDto>(@"
SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT COALESCE(SUM(initial_value), 0) FROM mst_book WHERE del_flg = 0) AS ending_balance
FROM hst_action AA
INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
WHERE AA.del_flg = 0 AND AA.act_time < @StartTime;",
new { StartTime = startTime });

            return dto;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/>に基づいて、開始日付までの <see cref="EndingBalanceInfoDto"/> を取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始日付</param>
        /// <returns>繰越残高情報</returns>
        public async Task<EndingBalanceInfoDto> FindByBookId(int bookId, DateTime startTime)
        {
            var dto = await this.dbHandler.QuerySingleAsync<EndingBalanceInfoDto>(@"
SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT initial_value FROM mst_book WHERE book_id = @BookId) AS ending_balance
FROM hst_action AA
INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
WHERE AA.book_id = @BookId AND AA.del_flg = 0 AND AA.act_time < @StartTime;",
new { BookId = bookId, StartTime = startTime });

            return dto;
        }
    }
}
