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
        /// <param name="defaultAssetId">デフォルトアセットID</param>
        /// <param name="startDate">開始日付</param>
        /// <param name="finishDate">終了日付(該当の日を含む)</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<SummaryInfoDto>> FindAllWithinPeriod(int defaultAssetId, DateOnly startDate, DateOnly finishDate)
        {
            using FuncLog funcLog = new(new { defaultAssetId, startDate, finishDate }, Log.LogLevel.Trace);

            IEnumerable<SummaryInfoDto> dtoList = await this.mDbHandler.QueryAsync<SummaryInfoDto>(@"
SELECT C.balance_kind, C.category_id, C.category_name, I.item_id, I.item_name, COALESCE(SUM((A.act_value / POWER(10, AA.scale)) * AA.base_rate / DA.base_rate), 0) AS main_total
FROM mst_item I
INNER JOIN rel_book_item RBI ON RBI.item_id = I.item_id AND RBI.del_flg = 0
INNER JOIN mst_category C ON C.category_id = I.category_id AND C.del_flg = 0
INNER JOIN mst_book B ON B.book_id = RBI.book_id AND B.del_flg = 0
LEFT JOIN hst_action A ON A.item_id = I.item_id AND A.book_id = B.book_id AND A.del_flg = 0 AND @StartDate <= A.act_time AND A.act_time < @FinishDate
LEFT JOIN mst_asset DA ON DA.asset_id = @DefaultAssetId AND DA.del_flg = 0 -- 表示するアセット(デフォルトアセット)
LEFT JOIN mst_asset AA ON AA.asset_id = COALESCE(A.asset_id, COALESCE(B.asset_id, @DefaultAssetId)) AND AA.del_flg = 0 -- 帳簿項目に紐づくアセット
WHERE I.move_flg = 0 AND I.del_flg = 0
GROUP BY C.balance_kind, C.category_id, C.category_name, I.item_id, I.item_name, C.sort_order, I.sort_order
ORDER BY C.balance_kind, C.sort_order, I.sort_order;",
new { DefaultAssetId = defaultAssetId, StartDate = startDate, FinishDate = finishDate.AddDays(1) });

            return dtoList;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/> に基づいて、<see cref="SummaryInfoDto"> リストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="defaultAssetId">デフォルトアセットID</param>
        /// <param name="startDate">開始日付</param>
        /// <param name="finishDate">終了日付(該当の日を含む)</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<SummaryInfoDto>> FindByBookIdWithinPeriod(int bookId, int defaultAssetId, DateOnly startDate, DateOnly finishDate)
        {
            using FuncLog funcLog = new(new { bookId, defaultAssetId, startDate, finishDate }, Log.LogLevel.Trace);

            IEnumerable<SummaryInfoDto> dtoList = await this.mDbHandler.QueryAsync<SummaryInfoDto>(@"
SELECT C.balance_kind, C.category_id, C.category_name, I.item_id, I.item_name, COALESCE(SUM((A.act_value / POWER(10, AA.scale)) * AA.base_rate / BA.base_rate), 0) AS main_total
FROM mst_item I
INNER JOIN mst_category C ON C.category_id = I.category_id AND C.del_flg = 0
INNER JOIN mst_book B ON B.book_id = @BookId AND B.del_flg = 0
LEFT JOIN hst_action A ON A.item_id = I.item_id AND A.book_id = B.book_id AND A.del_flg = 0 AND @StartDate <= A.act_time AND A.act_time < @FinishDate
LEFT JOIN mst_asset BA ON BA.asset_id = COALESCE(B.asset_id, @DefaultAssetId) AND BA.del_flg = 0 -- 表示するアセット(帳簿に紐づくアセット)
LEFT JOIN mst_asset AA ON AA.asset_id = COALESCE(A.asset_id, COALESCE(B.asset_id, @DefaultAssetId)) AND AA.del_flg = 0 -- 帳簿項目に紐づくアセット
WHERE (EXISTS (
    SELECT * FROM rel_book_item RBI
    WHERE RBI.item_id = I.item_id AND RBI.book_id = B.book_id AND RBI.del_flg = 0) OR I.move_flg = 1) AND I.del_flg = 0
GROUP BY C.balance_kind, C.category_id, C.category_name, I.item_id, I.item_name, C.sort_order, I.move_flg, I.sort_order
ORDER BY C.balance_kind, C.sort_order, I.move_flg DESC, I.sort_order;",
new { BookId = bookId, DefaultAssetId = defaultAssetId, StartDate = startDate, FinishDate = finishDate.AddDays(1) });

            return dtoList;
        }
    }
}
