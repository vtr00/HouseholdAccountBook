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
    /// 帳簿項目比較情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class ActionCompInfoDao(DbHandlerBase dbHandler) : TableDaoBase(dbHandler)
    {
        /// <summary>
        /// <see cref="MstBookDto.BookId"/> と日付、値に基づいて、<see cref="ActionCompInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="defaultAssetId">デフォルトアセットID</param>
        /// <param name="date">CSVの日付</param>
        /// <param name="value">CSVの値</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<ActionCompInfoDto>> FindMatchesWithCsvAsync(int bookId, int defaultAssetId, DateTime date, decimal value)
        {
            using FuncLog funcLog = new(new { bookId, date, value }, Log.LogLevel.Trace);

            IEnumerable<ActionCompInfoDto> dtoList = await this.mDbHandler.QueryAsync<ActionCompInfoDto>(@"
SELECT A.action_id, A.act_time, I.item_id, I.item_name, A.asset_id,
       (A.act_value / POWER(10, AA.scale)) * AA.base_rate / BA.base_rate AS main_act_value, BA.asset_id AS act_asset_id,
       (A.act_value / POWER(10, AA.scale)) AS org_main_act_value, AA.asset_id AS org_asset_id,
       A.shop_name, A.remark, A.is_match, A.group_id
FROM hst_action A
INNER JOIN mst_item I ON I.item_id = A.item_id AND I.del_flg = 0
INNER JOIN mst_book B ON B.book_id = A.book_id AND B.del_flg = 0
INNER JOIN mst_asset BA ON BA.asset_id = COALESCE(B.asset_id, @DefaultAssetId) AND BA.del_flg = 0 -- 表示するアセット(帳簿に紐づくアセット)
INNER JOIN mst_asset AA ON AA.asset_id = COALESCE(A.asset_id, COALESCE(B.asset_id, @DefaultAssetId)) AND AA.del_flg = 0 -- 帳簿項目に紐づくアセット
WHERE @StartDate <= act_time AND act_time < @EndDate AND A.act_value = -@Value * POWER(10, AA.scale) AND B.book_id = @BookId AND A.del_flg = 0;",
new { DefaultAssetId = defaultAssetId, StartDate = date, EndDate = date.AddDays(1), Value = value, BookId = bookId });

            return dtoList;
        }
    }
}
