using HouseholdAccountBook.Adapters.Dto.Abstract;

namespace HouseholdAccountBook.Adapters.Dto.KHDbTable
{
    public class CbmItemDto : KHCbmDtoBase
    {
        public CbmItemDto() { }

        public override int GetId()
        {
            return this.ITEM_ID;
        }

        public int ITEM_ID { get; set; }
        public string ITEM_NAME { get; set; }
        public int CATEGORY_ID { get; set; }
        public int TARGET_VALUE { get; set; }
        public bool MOVE_FLG { get; set; }
    }
}
