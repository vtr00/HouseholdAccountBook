using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Adapters.Dto.Others;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Compositions
{
    /// <summary>
    /// 帳簿情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class SummaryInfoDao(DbHandlerBase dbHandler) : ReadDaoBase(dbHandler)
    {
        /// <summary>
        /// 全帳簿の <see cref="SummaryInfoDto"> リストを取得する
        /// </summary>
        /// <param name="startTime">開始日付</param>
        /// <param name="finishTime">終了日付</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<SummaryInfoDto>> FindAllWithinTerm(DateTime startTime, DateTime finishTime)
        {
            var dtoList = await this.dbHandler.QueryAsync<SummaryInfoDto>(@"
SELECT C.balance_kind, C.category_id, C.category_name, SQ.item_id, I.item_name, SQ.total
FROM (
  SELECT I.item_id AS item_id, COALESCE(SUM(A.act_value), 0) AS total
  FROM mst_item I
  LEFT JOIN (SELECT * FROM hst_action WHERE @StartTime <= act_time AND act_time <= @FinishTime AND del_flg = 0) A ON A.item_id = I.item_id
  WHERE I.item_id IN (SELECT item_id FROM rel_book_item WHERE del_flg = 0) AND I.del_flg = 0
  GROUP BY I.item_id
) SQ -- Sub Query
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = SQ.item_id
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
ORDER BY C.balance_kind, C.sort_order, I.move_flg DESC, I.sort_order;",
new { StartTime = startTime, FinishTime = finishTime });

            return dtoList;
        }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/> に基づいて、<see cref="SummaryInfoDto"> リストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始日付</param>
        /// <param name="finishTime">終了日付</param>
        /// <returns>取得したレコードリスト</returns>
        public async Task<IEnumerable<SummaryInfoDto>> FindByBookIdWithinTerm(int bookId, DateTime startTime, DateTime finishTime)
        {
            var dtoList = await this.dbHandler.QueryAsync<SummaryInfoDto>(@"
SELECT C.balance_kind, C.category_id, C.category_name, SQ.item_id, I.item_name, SQ.total
FROM (
  SELECT I.item_id AS item_id, COALESCE(SUM(A.act_value), 0) AS total
  FROM mst_item I
  LEFT JOIN (SELECT * FROM hst_action WHERE book_id = @BookId AND @StartTime <= act_time AND act_time <= @FinishTime AND del_flg = 0) A ON A.item_id = I.item_id
  WHERE (I.move_flg = 1 OR I.item_id IN (SELECT item_id FROM rel_book_item WHERE book_id = @BookId AND del_flg = 0)) AND I.del_flg = 0
  GROUP BY I.item_id
) SQ -- Sub Query
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = SQ.item_id
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
ORDER BY C.balance_kind, C.sort_order, I.move_flg DESC, I.sort_order;",
new { BookId = bookId, StartTime = startTime, FinishTime = finishTime });

            return dtoList;
        }
    }
}
