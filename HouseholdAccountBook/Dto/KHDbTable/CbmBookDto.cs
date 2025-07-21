using HouseholdAccountBook.Dto.Abstract;

namespace HouseholdAccountBook.Dto.KHDbTable
{
    public class CbmBookDto : KHCbmDtoBase
    {
        public CbmBookDto() { }

        public int BOOK_ID { get; set; }
        public string BOOK_NAME { get; set; }
        public int BALANCE { get; set; }
        public bool INCLUDE_FLG { get; set; }
    }
}
