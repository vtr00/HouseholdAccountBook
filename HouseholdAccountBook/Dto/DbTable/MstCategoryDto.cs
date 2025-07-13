using HouseholdAccountBook.Dto.Abstract;

namespace HouseholdAccountBook.Dto.DbTable
{
    /// <summary>
    /// MstCategoryDto
    /// </summary>
    public class MstCategoryDto : MstDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MstCategoryDto() : base() { }

        /// <summary>
        /// 種別ID
        /// </summary>
        public int CategoryId { get; set; } = 0;
        /// <summary>
        /// 種別名
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>
        /// 収支種別
        /// </summary>
        public int BalanceKind { get; set; } = 0;
    }
}
