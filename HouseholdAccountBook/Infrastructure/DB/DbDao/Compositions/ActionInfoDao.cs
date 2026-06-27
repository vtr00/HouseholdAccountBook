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
    /// 帳簿項目情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class ActionInfoDao(DbHandlerBase dbHandler) : TableDaoBase(dbHandler)
    {
        /// <summary>
        /// 全帳簿の <see cref="ActionInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="defaultAssetId">デフォルトアセットID</param>
        /// <param name="startDate">開始日付</param>
        /// <param name="finishDate">終了日付(該当の日を含む)</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<ActionInfoDto>> FindAllWithinPeriod(int defaultAssetId, DateOnly startDate, DateOnly finishDate)
        {
            using FuncLog funcLog = new(new { defaultAssetId, startDate, finishDate }, Log.LogLevel.Trace);

            IEnumerable<ActionInfoDto> dtoList = await this.mDbHandler.QueryAsync<ActionInfoDto>(@"
SELECT A.action_id, A.act_time, B.book_id, C.category_id, I.item_id, B.book_name, C.category_name, 
       I.item_name, A.act_value / POWER(10, MA.scale) AS act_main_value, A.shop_name, A.group_id, A.remark, A.is_match
FROM hst_action A
INNER JOIN mst_book B ON B.book_id = A.book_id AND B.del_flg = 0 
INNER JOIN mst_item I ON I.item_id = A.item_id AND I.move_flg = 0 AND I.del_flg = 0 
INNER JOIN rel_book_item RBI ON RBI.item_id = I.item_id AND RBI.book_id = A.book_id AND RBI.del_flg = 0
INNER JOIN mst_category C ON I.category_id = C.category_id AND C.del_flg = 0
INNER JOIN mst_asset MA ON MA.asset_id = @DefaultAssetId AND MA.del_flg = 0
WHERE A.del_flg = 0 AND @StartDate <= A.act_time AND A.act_time < @FinishDate
ORDER BY act_time, balance_kind, C.sort_order, I.sort_order, B.sort_order, action_id;
",
new { DefaultAssetId = defaultAssetId, StartDate = startDate, FinishDate = finishDate.AddDays(1) });

            return dtoList;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/> に基づいて、<see cref="ActionInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="defaultAssetId">デフォルトアセットID</param>
        /// <param name="startDate">開始日付</param>
        /// <param name="finishDate">終了日付(該当の日を含む)</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<ActionInfoDto>> FindByBookIdWithinPeriod(int bookId, int defaultAssetId, DateOnly startDate, DateOnly finishDate)
        {
            using FuncLog funcLog = new(new { bookId, defaultAssetId, startDate, finishDate }, Log.LogLevel.Trace);

            IEnumerable<ActionInfoDto> dtoList = await this.mDbHandler.QueryAsync<ActionInfoDto>(@"
SELECT A.action_id, A.act_time, B.book_id, C.category_id, I.item_id, B.book_name, C.category_name,
       I.item_name, A.act_value / POWER(10, MA.scale) AS act_main_value, A.shop_name, A.group_id, A.remark, A.is_match
FROM hst_action A
INNER JOIN mst_book B ON B.book_id = A.book_id AND B.del_flg = 0
INNER JOIN mst_item I ON I.item_id = A.item_id AND (EXISTS (
    SELECT * FROM rel_book_item RBI 
    WHERE RBI.item_id = I.item_id AND RBI.book_id = B.book_id AND RBI.del_flg = 0) OR I.move_flg = 1) AND I.del_flg = 0
INNER JOIN mst_category C ON I.category_id = C.category_id AND C.del_flg = 0
INNER JOIN mst_asset MA ON MA.asset_id = @DefaultAssetId AND MA.del_flg = 0
WHERE A.del_flg = 0 AND B.book_id = @BookId AND @StartDate <= A.act_time AND A.act_time < @FinishDate
ORDER BY act_time, balance_kind, C.sort_order, I.move_flg DESC, I.sort_order, B.sort_order, action_id;",
new { BookId = bookId, DefaultAssetId = defaultAssetId, StartDate = startDate, FinishDate = finishDate.AddDays(1) });

            return dtoList;
        }
    }
}
