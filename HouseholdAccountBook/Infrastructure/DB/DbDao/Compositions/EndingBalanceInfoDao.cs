using HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using System;
using System.Threading.Tasks;

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
        /// <param name="defaultAssetId">デフォルトアセットID</param>
        /// <param name="startDate">開始日付</param>
        /// <returns>取得したレコード</returns>
        public async Task<EndingBalanceInfoDto> Find(int defaultAssetId, DateOnly startDate)
        {
            using FuncLog funcLog = new(new { defaultAssetId, startDate }, Log.LogLevel.Trace);

            EndingBalanceInfoDto dto = await this.mDbHandler.QuerySingleAsync<EndingBalanceInfoDto>(@"
SELECT COALESCE(SUM(A.act_value / POWER(10, MA.scale)), 0) + (
    SELECT COALESCE(SUM(BB.initial_value / POWER(10, MAA.scale)), 0)
    FROM mst_book BB
	INNER JOIN mst_asset MAA ON MAA.asset_id = @DefaultAssetId AND MAA.del_flg = 0
	WHERE BB.del_flg = 0) AS ending_main_balance
FROM hst_action A
INNER JOIN mst_book B ON B.book_id = A.book_id AND B.del_flg = 0
INNER JOIN mst_asset MA ON MA.asset_id = @DefaultAssetId AND MA.del_flg = 0
WHERE A.del_flg = 0 AND A.act_time < @StartDate;",
new { DefaultAssetId = defaultAssetId, StartDate = startDate });

            return dto;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/>に基づいて、開始日付までの <see cref="EndingBalanceInfoDto"/> を取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="defaultAssetId">デフォルトアセットID</param>
        /// <param name="startDate">開始日付</param>
        /// <returns>取得したレコード</returns>
        public async Task<EndingBalanceInfoDto> FindByBookId(int bookId, int defaultAssetId, DateOnly startDate)
        {
            using FuncLog funcLog = new(new { bookId, defaultAssetId, startDate }, Log.LogLevel.Trace);

            EndingBalanceInfoDto dto = await this.mDbHandler.QuerySingleAsync<EndingBalanceInfoDto>(@"
SELECT COALESCE(SUM(A.act_value / POWER(10, MA.scale)), 0) + (
    SELECT BB.initial_value / POWER(10, MAA.scale)
	FROM mst_book BB
	INNER JOIN mst_asset MAA ON MAA.asset_id = @DefaultAssetId AND MAA.del_flg = 0
	WHERE BB.book_id = @BookId AND BB.del_flg = 0) AS ending_main_balance
FROM hst_action A
INNER JOIN mst_book B ON B.book_id = A.book_id AND B.del_flg = 0
INNER JOIN mst_asset MA ON MA.asset_id = @DefaultAssetId AND MA.del_flg = 0
WHERE A.book_id = @BookId AND A.del_flg = 0 AND A.act_time < @StartDate;",
new { BookId = bookId, DefaultAssetId = defaultAssetId, StartDate = startDate });

            return dto;
        }
    }
}
