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
        /// <param name="startDate">開始日付</param>
        /// <param name="finishDate">終了日付(該当の日を含む)</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<ActionInfoDto>> FindAllWithinTerm(DateOnly startDate, DateOnly finishDate)
        {
            using FuncLog funcLog = new(new { startDate, finishDate }, Log.LogLevel.Trace);

            var dtoList = await this.mDbHandler.QueryAsync<ActionInfoDto>(@"
SELECT A.action_id, A.act_time, B.book_id, C.category_id, I.item_id, B.book_name, C.category_name, I.item_name, A.act_value, 
       A.shop_name, A.group_id, A.remark, A.is_match
FROM hst_action A
INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) B ON B.book_id = A.book_id
INNER JOIN (SELECT * FROM mst_item WHERE item_id IN (
    SELECT RBI.item_id 
    FROM rel_book_item RBI
    INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) B ON B.book_id = RBI.book_id
    WHERE RBI.del_flg = 0
) AND del_flg = 0) I ON I.item_id = A.item_id
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON I.category_id = C.category_id
WHERE A.del_flg = 0 AND @StartDate <= A.act_time AND A.act_time < @FinishDate
ORDER BY act_time, balance_kind, C.sort_order, I.move_flg DESC, I.sort_order, B.sort_order, action_id;",
new { StartDate = startDate, FinishDate = finishDate.AddDays(1) });

            return dtoList;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/> に基づいて、<see cref="ActionInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startDate">開始日付</param>
        /// <param name="finishDate">終了日付(該当の日を含む)</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<ActionInfoDto>> FindByBookIdWithinTerm(int bookId, DateOnly startDate, DateOnly finishDate)
        {
            using FuncLog funcLog = new(new { bookId, startDate, finishDate }, Log.LogLevel.Trace);

            var dtoList = await this.mDbHandler.QueryAsync<ActionInfoDto>(@"
SELECT A.action_id, A.act_time, B.book_id, C.category_id, I.item_id, B.book_name, C.category_name, I.item_name, A.act_value,
       A.shop_name, A.group_id, A.remark, A.is_match
FROM hst_action A
INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) B ON B.book_id = A.book_id
INNER JOIN (SELECT * FROM mst_item WHERE (move_flg = 1 OR item_id IN (
    SELECT RBI.item_id
    FROM rel_book_item RBI
    WHERE RBI.book_id = @BookId AND RBI.del_flg = 0
)) AND del_flg = 0) I ON I.item_id = A.item_id
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON I.category_id = C.category_id
WHERE A.book_id = @BookId AND A.del_flg = 0 AND @StartDate <= A.act_time AND A.act_time < @FinishDate
ORDER BY act_time, balance_kind, C.sort_order, I.move_flg DESC, I.sort_order, action_id;",
new { BookId = bookId, StartDate = startDate, FinishDate = finishDate.AddDays(1) });

            return dtoList;
        }
    }
}
