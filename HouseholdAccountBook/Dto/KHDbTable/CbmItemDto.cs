using HouseholdAccountBook.Dto.Abstract;

namespace HouseholdAccountBook.Dto.KHDbTable
{
    public class CbmItemDto : KHCbmDtoBase
    {
        public CbmItemDto() { }

        public int ITEM_ID { get; set; }
        public string ITEM_NAME { get; set; }
        public int CATEGORY_ID { get; set; }
        public int TARGET_VALUE { get; set; }
        public bool MOVE_FLG { get; set; }
    }
}
