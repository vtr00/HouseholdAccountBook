using HouseholdAccountBook.Infrastructure.DB.DbDao.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions
{
    /// <summary>
    /// 帳簿情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class BookInfoDao(DbHandlerBase dbHandler) : TableDaoBase(dbHandler)
    {
        /// <summary>
        /// <see cref="MstBookDto.BookId"/>に基づいて、<see cref="BookInfoDto"> を取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="defaultAssetId">デフォルトアセットID</param>
        /// <returns>取得したレコード</returns>
        public async Task<BookInfoDto> FindByBookIdAsync(int bookId, int defaultAssetId)
        {
            using FuncLog funcLog = new(new { bookId, defaultAssetId }, Log.LogLevel.Trace);

            BookInfoDto dto = await this.mDbHandler.QuerySingleAsync<BookInfoDto>(@"
SELECT B.book_name, B.asset_id, B.book_kind, B.debit_book_id, B.pay_day, B.initial_value / POWER(10, BA.scale) AS initial_main_value, B.json_code, B.sort_order, 
       MIN(A.act_time) AS start_date, MAX(A.act_time) AS end_date
FROM mst_book B
INNER JOIN mst_asset BA ON BA.asset_id = COALESCE(B.asset_id, @DefaultAssetId) AND BA.del_flg = 0
LEFT OUTER JOIN hst_action A ON A.book_id = B.book_id AND A.del_flg = 0
WHERE B.book_id = @BookId AND B.del_flg = 0
GROUP BY B.book_id, BA.asset_id
ORDER BY B.sort_order;",
new { BookId = bookId, DefaultAssetId = defaultAssetId });

            return dto;
        }
    }
}
