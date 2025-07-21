using HouseholdAccountBook.Dto.Abstract;

namespace HouseholdAccountBook.Dto.Others
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
