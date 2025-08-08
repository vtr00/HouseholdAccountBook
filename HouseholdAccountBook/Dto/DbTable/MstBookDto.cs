using HouseholdAccountBook.Dto.Abstract;
using HouseholdAccountBook.Dto.KHDbTable;
using System;
using System.Text;

namespace HouseholdAccountBook.Dto.DbTable
{
    /// <summary>
    /// 帳簿テーブルDTO
    /// </summary>
    public class MstBookDto : MstDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MstBookDto() : base() { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dto">記帳風月DTO</param>
        public MstBookDto(CbmBookDto dto) : base(dto)
        {
            this.BookId = dto.BOOK_ID;
            this.BookName = dto.BOOK_NAME;
            this.InitialValue = dto.BALANCE;
            this.SortOrder = dto.SORT_KEY;
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
        /// 初期値
        /// </summary>
        public int InitialValue { get; set; } = 0;
        /// <summary>
        /// 帳簿種別
        /// </summary>
        public int BookKind { get; set; } = 0;
        /// <summary>
        /// 支払日
        /// </summary>
        public int? PayDay { get; set; } = null;
        /// <summary>
        /// 支払い元帳簿ID
        /// </summary>
        public int? DebitBookId { get; set; } = null;

        /// <summary>
        /// MstBookのJSONコード
        /// </summary>
        public class JsonDto
        {
            /// <summary>
            /// 開始日
            /// </summary>
            public DateTime? StartDate { get; set; } = null;
            /// <summary>
            /// 終了日
            /// </summary>
            public DateTime? EndDate { get; set; } = null;

            /// <summary>
            /// CSVフォルダパス
            /// </summary>
            public string CsvFolderPath { get; set; } = null;
            /// <summary>
            /// テキストエンコーディング
            /// </summary>
            public int TextEncoding { get; set; } = Encoding.UTF8.CodePage;

            /// <summary>
            /// CSV内での日付のインデックス
            /// </summary>
            public int? CsvActDateIndex { get; set; } = null;
            /// <summary>
            /// CSV内での出費のインデックス
            /// </summary>
            public int? CsvOutgoIndex { get; set; } = null;
            /// <summary>
            /// CSV内での項目名のインデックス
            /// </summary>
            public int? CsvItemNameIndex { get; set; } = null;
        }
    }
}
