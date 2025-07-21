using HouseholdAccountBook.Dto.Abstract;
using HouseholdAccountBook.Dto.KHDbTable;

namespace HouseholdAccountBook.Dto.DbTable
{
    /// <summary>
    /// 項目テーブルDTO
    /// </summary>
    public class MstItemDto : MstDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MstItemDto() : base() { }

        public MstItemDto(CbmItemDto dto) : base(dto) {
            this.ItemId = dto.ITEM_ID;
            this.ItemName = dto.ITEM_NAME;
            this.CategoryId = dto.CATEGORY_ID;
            this.MoveFlg = dto.MOVE_FLG ? 1 : 0;
            this.AdvanceFlg = 0;
        }

        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = 0;
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; } = "(no name)";
        /// <summary>
        /// 分類ID
        /// </summary>
        public int CategoryId { get; set; } = 0;
        /// <summary>
        /// 立替フラグ
        /// </summary>
        public int AdvanceFlg { get; set; } = 0;
        /// <summary>
        /// 移動フラグ
        /// </summary>
        public int MoveFlg { get; set; } = 0;
    }
}
