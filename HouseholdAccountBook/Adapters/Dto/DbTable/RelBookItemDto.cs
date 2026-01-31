using HouseholdAccountBook.Adapters.Dto.Abstract;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;

namespace HouseholdAccountBook.Adapters.Dto.DbTable
{
    /// <summary>
    /// 帳簿-項目関連DTO
    /// </summary>
    public class RelBookItemDto : CommonTableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RelBookItemDto() : base() { }
        /// <summary>
        /// コンストラクタ(記帳風月の帳簿-項目関連DTOからのコピー)
        /// </summary>
        /// <param name="dto">記帳風月の帳簿-項目関連DTO</param>
        public RelBookItemDto(CbrBookDto dto) : base(dto)
        {
            this.BookId = dto.BOOK_ID;
            this.ItemId = dto.ITEM_ID;
        }

        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int BookId { get; set; }
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; }
    }
}
