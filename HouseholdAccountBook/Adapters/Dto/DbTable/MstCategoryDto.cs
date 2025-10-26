using HouseholdAccountBook.Adapters.Dto.Abstract;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;

namespace HouseholdAccountBook.Adapters.Dto.DbTable
{
    /// <summary>
    /// 分類DTO
    /// </summary>
    public class MstCategoryDto : MstDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MstCategoryDto() : base() { }
        /// <summary>
        /// コンストラクタ(記帳風月の分類DTOからのコピー)
        /// </summary>
        /// <param name="dto">記帳風月の分類DTO</param>
        public MstCategoryDto(CbmCategoryDto dto) : base(dto)
        {
            this.CategoryId = dto.CATEGORY_ID;
            this.CategoryName = dto.CATEGORY_NAME;
            this.BalanceKind = dto.REXP_DIV - 1;
            this.SortOrder = dto.SORT_KEY;
        }

        public override int GetId()
        {
            return this.CategoryId;
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
        public int BalanceKind { get; set; } = (int)Enums.BalanceKind.Income;
    }
}
