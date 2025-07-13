namespace HouseholdAccountBook.Dto
{
    /// <summary>
    /// 繰越残高情報DTO
    /// </summary>
    internal class EndingBalanceInfoDto
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EndingBalanceInfoDto() { }

        /// <summary>
        /// 繰越残高
        /// </summary>
        public int EndingBalance { get; set; } = 0;
    }
}
