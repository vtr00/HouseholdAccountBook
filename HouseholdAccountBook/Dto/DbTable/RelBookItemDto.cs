using HouseholdAccountBook.Dto.Abstract;

namespace HouseholdAccountBook.Dto.DbTable
{
    /// <summary>
    /// RelBookItemDto
    /// </summary>
    public class RelBookItemDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RelBookItemDto() : base() { }

        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int BookId { get; set; } = 0;
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = 0;
    }
}
