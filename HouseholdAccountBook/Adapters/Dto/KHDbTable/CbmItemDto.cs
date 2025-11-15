using HouseholdAccountBook.Adapters.Dto.Abstract;

namespace HouseholdAccountBook.Adapters.Dto.KHDbTable
{
    /// <summary>
    /// 項目DTO
    /// </summary>
    public class CbmItemDto : KHCbmDtoBase
    {
        public CbmItemDto() { }

        public override int GetId() => this.ITEM_ID;

        /// <summary>
        /// 項目ID
        /// </summary>
        public int ITEM_ID { get; set; }
        /// <summary>
        /// 項目名
        /// </summary>
        public string ITEM_NAME { get; set; }
        /// <summary>
        /// 分類ID
        /// </summary>
        public int CATEGORY_ID { get; set; }
        /// <summary>
        /// 目標値
        /// </summary>
        public int TARGET_VALUE { get; set; }
        /// <summary>
        /// 移動フラグ
        /// </summary>
        public bool MOVE_FLG { get; set; }
    }
}
