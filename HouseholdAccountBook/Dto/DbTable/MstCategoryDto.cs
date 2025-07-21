using HouseholdAccountBook.Dto.Abstract;
using HouseholdAccountBook.Dto.KHDbTable;

namespace HouseholdAccountBook.Dto.DbTable
{
    /// <summary>
    /// 分類テーブルDTO
    /// </summary>
    public class MstCategoryDto : MstDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MstCategoryDto() : base() { }

        public MstCategoryDto(CbmCategoryDto dto) : base(dto)
        {
            this.CategoryId = dto.CATEGORY_ID;
            this.CategoryName = dto.CATEGORY_NAME;
            this.BalanceKind = dto.REXP_DIV - 1;
            this.SortOrder = dto.SORT_KEY;
        }

        /// <summary>
        /// 種別ID
        /// </summary>
        public int CategoryId { get; set; } = 0;
        /// <summary>
        /// 種別名
        /// </summary>
        public string CategoryName { get; set; } = "(no name)";
        /// <summary>
        /// 収支種別
        /// </summary>
        public int BalanceKind { get; set; } = 0;
    }
}
