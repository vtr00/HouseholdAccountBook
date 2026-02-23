using HouseholdAccountBook.Models.Infrastructure.DbDto.Abstract;
using HouseholdAccountBook.Models.Infrastructure.DbDto.KHDbTable;
using HouseholdAccountBook.Models.Infrastructure.Logger;

namespace HouseholdAccountBook.Models.Infrastructure.DbDto.DbTable
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
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

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
