using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto.Others
{
    /// <summary>
    /// 繰越残高情報DTO
    /// </summary>
    public class EndingBalanceInfoDto : VirTableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndingBalanceInfoDto() : base() { }

        /// <summary>
        /// 繰越残高
        /// </summary>
        public int EndingBalance { get; set; }
    }
}
