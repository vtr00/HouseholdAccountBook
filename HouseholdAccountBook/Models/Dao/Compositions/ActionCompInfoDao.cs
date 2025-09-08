using HouseholdAccountBook.Models.Dao.Abstract;
using HouseholdAccountBook.Models.DbHandler.Abstract;
using HouseholdAccountBook.Models.Dto.DbTable;
using HouseholdAccountBook.Models.Dto.Others;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.Dao.Compositions
{
    /// <summary>
    /// 帳簿項目比較情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class ActionCompInfoDao(DbHandlerBase dbHandler) : ReadDaoBase(dbHandler)
    {
        /// <summary>
        /// <see cref="MstBookDto.BookId"/> と日付、値に基づいて、<see cref="ActionCompInfoDto"/> リストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="date">CSVの日付</param>
        /// <param name="value">CSVの値</param>
        /// <returns>DTOリスト</returns>
        public async Task<IEnumerable<ActionCompInfoDto>> FindMatchesWithCsvAsync(int bookId, DateTime date, int value)
        {
            var dtoList = await this.dbHandler.QueryAsync<ActionCompInfoDto>(@"
SELECT A.action_id, I.item_name, A.act_value, A.shop_name, A.remark, A.is_match, A.group_id
FROM hst_action A
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = A.item_id
WHERE to_date(to_char(act_time, 'YYYY-MM-DD'), 'YYYY-MM-DD') = @Date AND A.act_value = -@Value AND book_id = @BookId AND A.del_flg = 0;",
new { Date = date, Value = value, BookId = bookId });

            return dtoList;
        }
    }
}
