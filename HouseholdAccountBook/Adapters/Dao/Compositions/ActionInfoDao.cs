using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Adapters.Dto.Others;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Compositions
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
        /// <param name="startTime">開始日付</param>
        /// <param name="finishTime">終了日付</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<ActionInfoDto>> FindAllWithinTerm(DateTime startTime, DateTime finishTime)
        {
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
WHERE A.del_flg = 0 AND @StartTime <= A.act_time AND A.act_time <= @FinishTime
ORDER BY act_time, balance_kind, C.sort_order, I.move_flg DESC, I.sort_order, B.sort_order, action_id;",
new { StartTime = startTime, FinishTime = finishTime });

            return dtoList;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/> に基づいて、<see cref="ActionInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始日付</param>
        /// <param name="finishTime">終了日付</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<ActionInfoDto>> FindByBookIdWithinTerm(int bookId, DateTime startTime, DateTime finishTime)
        {
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
WHERE A.book_id = @BookId AND A.del_flg = 0 AND @StartTime <= A.act_time AND A.act_time <= @FinishTime
ORDER BY act_time, balance_kind, C.sort_order, I.move_flg DESC, I.sort_order, action_id;",
new { BookId = bookId, StartTime = startTime, FinishTime = finishTime });

            return dtoList;
        }
    }
}
