namespace HouseholdAccountBook.Dto.KHDbTable
{
    public class CbmBookDto
    {
        public CbmBookDto() { }

        public int BOOK_ID { get; set; }
        public string BOOK_NAME { get; set; }
        public int BALANCE { get; set; }
        public bool INCLUDE_FLG { get; set; }
        public int SORT_KEY { get; set; }
        public bool DEL_FLG { get; set; }
    }
}
