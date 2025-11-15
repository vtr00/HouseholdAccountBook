using HouseholdAccountBook.Adapters.Dto.Abstract;

namespace HouseholdAccountBook.Adapters.Dto.Others
{
    /// <summary>
    /// 項目-帳簿関連情報DTO
    /// </summary>
    public class BookRelFromItemInfoDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BookRelFromItemInfoDto() : base() { }

        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int BookId { get; set; } = -1;
        /// <summary>
        /// 帳簿名
        /// </summary>
        public string BookName { get; set; } = string.Empty;
        /// <summary>
        /// 関連有無
        /// </summary>
        public bool IsRelated { get; set; }
    }
}
