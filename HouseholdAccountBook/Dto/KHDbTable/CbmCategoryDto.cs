namespace HouseholdAccountBook.Dto.KHDbTable
{
    public class CbmCategoryDto
    {
        public CbmCategoryDto() { }

        public int CATEGORY_ID { get; set; }
        public string CATEGORY_NAME { get; set; }
        public int REXP_DIV { get; set; }
        public int SORT_KEY { get; set; }
        public bool DEL_FLG { get; set; }

    }
}
