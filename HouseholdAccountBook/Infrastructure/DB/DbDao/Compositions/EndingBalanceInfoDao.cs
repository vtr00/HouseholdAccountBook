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
SELECT COALESCE(SUM((A.act_value / POWER(10, AA.scale)) * AA.base_rate / DA.base_rate), 0) + (
    SELECT COALESCE(SUM((BB.initial_value / POWER(10, MAA.scale)) * MAA.base_rate / MDA.base_rate), 0)
    FROM mst_book BB
    INNER JOIN mst_asset MDA ON MDA.asset_id = @DefaultAssetId AND MDA.del_flg = 0 -- 表示するアセット(デフォルトアセット)
    INNER JOIN mst_asset MAA ON MAA.asset_id = COALESCE(BB.asset_id, @DefaultAssetId) AND MAA.del_flg = 0 -- 帳簿に紐づくアセット
    WHERE BB.del_flg = 0) AS main_ending_balance, DA.asset_id
FROM mst_book B
INNER JOIN mst_asset DA ON DA.asset_id = @DefaultAssetId AND DA.del_flg = 0 -- 表示するアセット(デフォルトアセット)
LEFT JOIN hst_action A ON A.book_id = B.book_id AND A.del_flg = 0 AND A.act_time < @StartDate
LEFT JOIN mst_asset AA ON AA.asset_id = COALESCE(A.asset_id, COALESCE(B.asset_id, @DefaultAssetId)) AND AA.del_flg = 0 -- 帳簿項目に紐づくアセット
WHERE B.del_flg = 0
GROUP BY DA.asset_id;",
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
SELECT COALESCE(SUM((A.act_value / POWER(10, AA.scale)) * AA.base_rate / BA.base_rate), 0) + (
    SELECT BB.initial_value / POWER(10, MAA.scale)
    FROM mst_book BB
    INNER JOIN mst_asset MAA ON MAA.asset_id = COALESCE(BB.asset_id, @DefaultAssetId) AND MAA.del_flg = 0 -- 表示するアセット(帳簿に紐づくアセット)
    WHERE BB.book_id = @BookId AND BB.del_flg = 0) AS main_ending_balance, BA.asset_id
FROM mst_book B
INNER JOIN mst_asset BA ON BA.asset_id = COALESCE(B.asset_id, @DefaultAssetId) AND BA.del_flg = 0 -- 表示するアセット(帳簿に紐づくアセット)
LEFT JOIN hst_action A ON A.book_id = B.book_id AND A.del_flg = 0 AND A.act_time < @StartDate
LEFT JOIN mst_asset AA ON AA.asset_id = COALESCE(A.asset_id, COALESCE(B.asset_id, @DefaultAssetId)) AND AA.del_flg = 0 -- 帳簿項目に紐づくアセット
WHERE B.book_id = @BookId AND B.del_flg = 0
GROUP BY BA.asset_id;",
new { BookId = bookId, DefaultAssetId = defaultAssetId, StartDate = startDate });

            return dto;
        }
    }
}
