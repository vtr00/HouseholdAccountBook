using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.Others;
using HouseholdAccountBook.Dto.DbTable;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.Compositions
{
    /// <summary>
    /// 帳簿情報DAO
    /// </summary>
    public class BookInfoDao : ReadDaoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        public BookInfoDao(DbHandlerBase dbHandler) : base(dbHandler) { }

        /// <summary>
        /// <see cref="MstBookDto.BookId"/>に基づいて、<see cref="BookInfoDto"> を取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>帳簿情報DTO</returns>
        public async Task<BookInfoDto> FindByBookId(int bookId)
        {
            var dto = await this.dbHandler.QuerySingleAsync<BookInfoDto>(@"
SELECT B.book_name, B.book_kind, B.debit_book_id, B.pay_day, B.initial_value, B.json_code, B.sort_order, MIN(A.act_time) AS start_date, MAX(A.act_time) AS end_date
FROM mst_book B
LEFT OUTER JOIN hst_action A ON A.book_id = B.book_id AND A.del_flg = 0
WHERE B.book_id = @BookId AND B.del_flg = 0
GROUP BY B.book_id
ORDER BY B.sort_order;",
new MstBookDto { BookId = bookId });

            return dto;
        }
    }
}
