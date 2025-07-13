using System;

namespace HouseholdAccountBook.Dto
{
    /// <summary>
    /// 帳簿情報DTO
    /// </summary>
    public class BookInfoDto
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BookInfoDto() { }

        /// <summary>
        /// 帳簿名
        /// </summary>
        public string BookName { get; set; } = string.Empty;
        /// <summary>
        /// 帳簿種別
        /// </summary>
        public int BookKind { get; set; } = (int)ConstValue.ConstValue.BookKind.Uncategorized;
        /// <summary>
        /// 支払帳簿ID
        /// </summary>
        public int DebitBookId { get; set; } = -1;
        /// <summary>
        /// 支払日
        /// </summary>
        public int PayDay { get; set; } = 0;
        /// <summary>
        /// 初期値
        /// </summary>
        public int InitialValue { get; set; } = 0;
        /// <summary>
        /// JsonCode
        /// </summary>
        public string JsonCode { get; set; } = string.Empty;
        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; set; } = 0;
        /// <summary>
        /// 開始日
        /// </summary>
        public DateTime? StartDate { get; set; } = null;
        /// <summary>
        /// 終了日
        /// </summary>
        public DateTime? EndDate { get; set; } = null;
    }
}
