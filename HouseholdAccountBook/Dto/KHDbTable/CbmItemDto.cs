namespace HouseholdAccountBook.Dto.KHDbTable
{
    public class CbmItemDto
    {
        public int ITEM_ID { get; set; }
        public string ITEM_NAME { get; set; }
        public int CATEGORY_ID { get; set; }
        public int TARGET_VALUE { get; set; }
        public bool MOVE_FLG { get; set; }
        public int SORT_KEY { get; set; }
        public bool DEL_FLG { get; set; }
    }
}
