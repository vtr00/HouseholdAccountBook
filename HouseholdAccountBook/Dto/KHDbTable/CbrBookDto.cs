using HouseholdAccountBook.Dto.Abstract;

namespace HouseholdAccountBook.Dto.KHDbTable
{
    public class CbrBookDto : KHDtoBase
    {
        public CbrBookDto() { }

        public int BOOK_ID { get; set; }
        public int ITEM_ID { get; set; }
    }
}
