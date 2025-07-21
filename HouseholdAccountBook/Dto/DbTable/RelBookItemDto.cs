using HouseholdAccountBook.Dto.Abstract;
using HouseholdAccountBook.Dto.KHDbTable;

namespace HouseholdAccountBook.Dto.DbTable
{
    /// <summary>
    /// 帳簿-項目関連テーブルDTO
    /// </summary>
    public class RelBookItemDto : TableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RelBookItemDto() : base() { }

        public RelBookItemDto(CbrBookDto dto) : base(dto)
        {
            this.BookId = dto.BOOK_ID;
            this.ItemId = dto.ITEM_ID;
        }

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
