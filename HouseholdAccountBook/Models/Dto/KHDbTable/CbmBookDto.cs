using HouseholdAccountBook.Models.Dto.Abstract;

namespace HouseholdAccountBook.Models.Dto.KHDbTable
{
    public class CbmBookDto : KHCbmDtoBase
    {
        public CbmBookDto() { }

        public override int GetId()
        {
            return this.BOOK_ID;
        }

        public int BOOK_ID { get; set; }
        public string BOOK_NAME { get; set; }
        public int BALANCE { get; set; }
        public bool INCLUDE_FLG { get; set; }
    }
}
