using HouseholdAccountBook.Dto.Abstract;

namespace HouseholdAccountBook.Dto.DbTable
{
    /// <summary>
    /// MstItemDto
    /// </summary>
    public class MstItemDto : MstDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MstItemDto() : base() { }

        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = 0;
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; } = string.Empty;
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
