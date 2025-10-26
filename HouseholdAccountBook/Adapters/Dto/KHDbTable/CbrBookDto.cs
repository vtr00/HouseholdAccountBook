using HouseholdAccountBook.Adapters.Dto.Abstract;

namespace HouseholdAccountBook.Adapters.Dto.KHDbTable
{
    /// <summary>
    /// 帳簿-項目関連DTO
    /// </summary>
    public class CbrBookDto : KHDtoBase
    {
        public CbrBookDto() { }

        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int BOOK_ID { get; set; }
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ITEM_ID { get; set; }
    }
}
