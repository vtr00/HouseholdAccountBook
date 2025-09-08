using HouseholdAccountBook.Models.Dto.Abstract;

namespace HouseholdAccountBook.Models.Dto.KHDbTable
{
    public class CbrBookDto : KHDtoBase
    {
        public CbrBookDto() { }

        public int BOOK_ID { get; set; }
        public int ITEM_ID { get; set; }
    }
}
