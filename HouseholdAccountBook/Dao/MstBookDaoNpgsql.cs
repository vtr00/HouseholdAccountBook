using CalcBinding.Inversion;
using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace HouseholdAccountBook.Dao
{
    internal class MstBookDaoNpgsql : MstBookDaoBase
    {
        /// <summary>
        /// MstBookDaoNpgsql クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="handler">DBハンドラ</param>
        public MstBookDaoNpgsql(DbHandlerNpgsql handler) : base(handler)
        {
        }

        public override async Task<IEnumerable<MstBookDto>> FindAllAsync()
        {
            DbReader reader = await this.handler.ExecQueryAsync(@"
SELECT * FROM mst_book
WHERE del_flg = 0");
            IEnumerable<MstBookDto> dtos = new List<MstBookDto>();
            reader.ExecWholeRow((cnt, record) => {
                dtos.Append(new MstBookDto(record));
                return true; // 継続
            });
            return dtos;
        }

        public override async Task<MstBookDto> FindByIdAsync(int bookId) {
            DbReader reader = await this.handler.ExecQueryAsync(@"
SELECT * FROM mst_book 
WHERE del_flg = 0 and book_id = @{0}", bookId);

            MstBookDto dto = null;
            reader.ExecARow((record) => {
                dto = new MstBookDto(record);
            });
            return dto;
        }

        public override async Task<int> InsertAsync(MstBookDto dto) {
            DbReader reader = await this.handler.ExecQueryAsync(@"
INSERT INTO mst_book (book_name, book_kind, pay_day, initial_value, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, (SELECT COALESCE(MAX(sort_order) + 1, 1) FROM mst_book), 0, 'now', @{4}, 'now', @{5})
RETURNING book_id;", dto.BookName, dto.BookKind, dto.PayDay, dto.InitialValue, dto.Updater, dto.Inserter);

            int bookId = 0;
            reader.ExecARow((record) => {
                bookId = record.ToInt("book_id");
            });
            return bookId;

        }

        public override Task<IEnumerable<int>> InsertAsync(IEnumerable<MstBookDto> dtos) => throw new NotImplementedException();
        public override Task<bool> UpdateAsync(MstBookDto dto) => throw new NotImplementedException();

        public override async Task<bool> DeleteAsync(MstBookDto dto)
        {
            await this.handler.ExecNonQueryAsync(@"
UPDATE mst_book
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE book_id = @{1};", dto.Updater, dto.BookId);

            return true;
        }
    }
}
