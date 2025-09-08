using HouseholdAccountBook.Models.Dto.Abstract;

namespace HouseholdAccountBook.Models.Dto.KHDbTable
{
    public class CbtNoteDto : KHDtoBase
    {
        public CbtNoteDto() { }

        public int NOTE_ID { get; set; }
        public string NOTE_NAME { get; set; }
        public int ITEM_ID { get; set; }
    }
}
