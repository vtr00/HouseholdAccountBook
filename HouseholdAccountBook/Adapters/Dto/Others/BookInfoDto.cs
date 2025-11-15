using HouseholdAccountBook.Adapters.Dto.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using System;

namespace HouseholdAccountBook.Adapters.Dto.Others
{
    /// <summary>
    /// 帳簿情報DTO
    /// </summary>
    /// <remarks><see cref="MstBookDto"/></remarks>
    public class BookInfoDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BookInfoDto() : base() { }

        /// <summary>
        /// 帳簿名
        /// </summary>
        public string BookName { get; set; } = string.Empty;
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
        /// 支払帳簿ID
        /// </summary>
        public int? DebitBookId { get; set; }
        /// <summary>
        /// JsonCode
        /// </summary>
        public string JsonCode { get; set; } = string.Empty;
        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; set; }
        /// <summary>
        /// 開始日
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// 終了日
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
