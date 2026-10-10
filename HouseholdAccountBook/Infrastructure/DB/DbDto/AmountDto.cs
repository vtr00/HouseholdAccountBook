using System.Diagnostics;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto
{
    /// <summary>
    /// 金額DTO
    /// </summary>
    [DebuggerDisplay("{MainValue}")]
    public class AmountDto
    {
        /// <summary>
        /// 金額(主単位)
        /// </summary>
        public decimal MainValue { get; set; }

        /// <summary>
        /// アセットID
        /// </summary>
        public int AssetId { get; set; }
    }
}
