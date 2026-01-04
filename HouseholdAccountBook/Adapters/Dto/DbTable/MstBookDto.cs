using HouseholdAccountBook.Adapters.Dto.Abstract;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;
using System;
using System.Text;
using System.Text.Json;

namespace HouseholdAccountBook.Adapters.Dto.DbTable
{
    /// <summary>
    /// 帳簿DTO
    /// </summary>
    public class MstBookDto : MstDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MstBookDto() : base() { }

        /// <summary>
        /// コンストラクタ(記帳風月の帳簿DTOからのコピー)
        /// </summary>
        /// <param name="dto">記帳風月の帳簿DTO</param>
        public MstBookDto(CbmBookDto dto) : base(dto)
        {
            this.BookId = dto.BOOK_ID;
            this.BookName = dto.BOOK_NAME;
            this.InitialValue = dto.BALANCE;
            this.SortOrder = dto.SORT_KEY;
        }

        public override int GetId() => this.BookId;

        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int BookId { get; set; }
        /// <summary>
        /// 帳簿名
        /// </summary>
        public string BookName { get; set; } = "(no name)";
        /// <summary>
        /// 初期値
        /// </summary>
        public int InitialValue { get; set; }
        /// <summary>
        /// 帳簿種別
        /// </summary>
        public int BookKind { get; set; }
        /// <summary>
        /// 支払日
        /// </summary>
        public int? PayDay { get; set; }
        /// <summary>
        /// 支払い元帳簿ID
        /// </summary>
        public int? DebitBookId { get; set; }

        /// <summary>
        /// MstBookのJSONコード
        /// </summary>
        public class JsonDto
        {
            /// <summary>
            /// 備考
            /// </summary>
            public string Remark { get; set; }

            /// <summary>
            /// 開始日
            /// </summary>
            public DateTime? StartDate { get; set; }
            /// <summary>
            /// 終了日
            /// </summary>
            public DateTime? EndDate { get; set; }

            /// <summary>
            /// CSVフォルダパス
            /// </summary>
            public string CsvFolderPath { get; set; }
            /// <summary>
            /// テキストエンコーディング
            /// </summary>
            public int TextEncoding { get; set; } = Encoding.UTF8.CodePage;

            /// <summary>
            /// CSV内での日付のインデックス
            /// </summary>
            public int? CsvActDateIndex { get; set; }
            /// <summary>
            /// CSV内での出費のインデックス
            /// </summary>
            public int? CsvOutgoIndex { get; set; }
            /// <summary>
            /// CSV内での項目名のインデックス
            /// </summary>
            public int? CsvItemNameIndex { get; set; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public JsonDto() { }
            /// <summary>
            /// コンストラクタ(JSONコードから生成)
            /// </summary>
            /// <param name="jsonCode"><see cref="MstBookDto"/>のJsonCode</param>
            public JsonDto(string jsonCode)
            {
                if (string.IsNullOrEmpty(jsonCode)) { return; }
                try {
                    JsonDto dto = JsonSerializer.Deserialize<JsonDto>(jsonCode);
                    if (dto != null) {
                        this.Copy(dto);
                    }
                }
                catch (Exception) {
                    // 例外が発生した場合は何もしない
                }
            }

            /// <summary>
            /// フィールドをコピーする
            /// </summary>
            /// <param name="org">コピー元</param>
            protected void Copy(JsonDto org)
            {
                this.Remark = org.Remark;
                this.StartDate = org.StartDate;
                this.EndDate = org.EndDate;
                this.CsvFolderPath = org.CsvFolderPath;
                this.TextEncoding = org.TextEncoding;
                this.CsvActDateIndex = org.CsvActDateIndex;
                this.CsvOutgoIndex = org.CsvOutgoIndex;
                this.CsvItemNameIndex = org.CsvItemNameIndex;
            }

            /// <summary>
            /// Jsonコードに変換する
            /// </summary>
            /// <returns>Jsonコード</returns>
            public string ToJson() => JsonSerializer.Serialize(this);
        }
    }
}
