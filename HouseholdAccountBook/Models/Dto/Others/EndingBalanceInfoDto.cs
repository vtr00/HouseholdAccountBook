using HouseholdAccountBook.Models.Dto.Abstract;

namespace HouseholdAccountBook.Models.Dto.Others
{
    /// <summary>
    /// 繰越残高情報DTO
    /// </summary>
    public class EndingBalanceInfoDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndingBalanceInfoDto() : base() { }

        /// <summary>
        /// 繰越残高
        /// </summary>
        public int EndingBalance { get; set; } = 0;
    }
}
