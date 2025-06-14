using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.Dto.Abstract;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.Dto
{
    /// <summary>
    /// 帳簿テーブルのDTOクラス
    /// </summary>
    public class MstBookDto : MstDtoBase
    {
        public MstBookDto(int bookId, string bookName, int bookKind, int initialValue, int? payDay, int? debitBookId, int sortOrder, string jsonCode)
            : base(sortOrder, jsonCode)
        {
            this.BookId = bookId;
            this.BookName = bookName;
            this.BookKind = bookKind;
            this.InitialValue = initialValue;
            this.PayDay = payDay;
            this.DebitBookId = debitBookId;
        }

        public MstBookDto(DbReader.Record record) : base(record)
        {
            this.BookId = record.ToInt("book_id");
            this.BookName = record["book_name"];
            this.BookKind = record.ToInt("book_kind");
            this.InitialValue = record.ToInt("initial_value");
            this.PayDay = record.ToInt("pay_day");
            this.DebitBookId = record.ToInt("debit_book_id");
        }

        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int BookId { get; set; } = 0;
        /// <summary>
        /// 帳簿名
        /// </summary>
        public string BookName { get; set; } = "(no name)";
        /// <summary>
        /// 帳簿種別
        /// </summary>
        public int BookKind { get; set; } = (int)ConstValue.ConstValue.BookKind.Uncategorized;
        /// <summary>
        /// 初期値
        /// </summary>
        public int InitialValue { get; set; } = 0;
        /// <summary>
        /// 支払日
        /// </summary>
        public int? PayDay { get; set; } = null;
        /// <summary>
        /// 支払帳簿ID
        /// </summary>
        public int? DebitBookId { get; set; } = null;
    }
}
