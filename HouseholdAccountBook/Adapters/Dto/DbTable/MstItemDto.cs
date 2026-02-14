using HouseholdAccountBook.Adapters.Dto.Abstract;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;
using HouseholdAccountBook.Adapters.Logger;

namespace HouseholdAccountBook.Adapters.Dto.DbTable
{
    /// <summary>
    /// 項目DTO
    /// </summary>
    public class MstItemDto : MstDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MstItemDto() : base() { }
        /// <summary>
        /// コンストラクタ(記帳風月の項目DTOからのコピー)
        /// </summary>
        /// <param name="dto">記帳風月の項目DTO</param>
        public MstItemDto(CbmItemDto dto) : base(dto)
        {
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

            this.ItemId = dto.ITEM_ID;
            this.ItemName = dto.ITEM_NAME;
            this.CategoryId = dto.CATEGORY_ID;
            this.MoveFlg = dto.MOVE_FLG ? 1 : 0;
            this.AdvanceFlg = 0;
        }

        public override int GetId() => this.ItemId;

        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; } = "(no name)";
        /// <summary>
        /// 分類ID
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// 立替フラグ(未使用)
        /// </summary>
        public int AdvanceFlg { get; set; }
        /// <summary>
        /// 移動フラグ
        /// </summary>
        public int MoveFlg { get; set; }
    }
}
