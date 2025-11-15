using HouseholdAccountBook.Adapters.Dto.Abstract;

namespace HouseholdAccountBook.Adapters.Dto.KHDbTable
{
    /// <summary>
    /// 分類DTO
    /// </summary>
    public class CbmCategoryDto : KHCbmDtoBase
    {
        public CbmCategoryDto() { }

        public override int GetId() => this.CATEGORY_ID;

        /// <summary>
        /// 分類ID
        /// </summary>
        public int CATEGORY_ID { get; set; }
        /// <summary>
        /// 分類名
        /// </summary>
        public string CATEGORY_NAME { get; set; }
        /// <summary>
        /// 分類種別
        /// </summary>
        public int REXP_DIV { get; set; }
    }
}
