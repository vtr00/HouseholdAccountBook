using HouseholdAccountBook.Adapters.Dto.Abstract;

namespace HouseholdAccountBook.Adapters.Dto.KHDbTable
{
    /// <summary>
    /// 備考DTO
    /// </summary>
    public class CbtNoteDto : KHDtoBase
    {
        public CbtNoteDto() { }

        /// <summary>
        /// 備考ID
        /// </summary>
        public int NOTE_ID { get; set; }
        /// <summary>
        /// 備考
        /// </summary>
        public string NOTE_NAME { get; set; }
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ITEM_ID { get; set; }
    }
}
