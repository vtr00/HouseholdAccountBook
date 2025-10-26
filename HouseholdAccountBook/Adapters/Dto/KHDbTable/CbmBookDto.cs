using HouseholdAccountBook.Adapters.Dto.Abstract;

namespace HouseholdAccountBook.Adapters.Dto.KHDbTable
{
    /// <summary>
    /// 帳簿DTO
    /// </summary>
    public class CbmBookDto : KHCbmDtoBase
    {
        public CbmBookDto() { }

        public override int GetId()
        {
            return this.BOOK_ID;
        }

        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int BOOK_ID { get; set; }
        /// <summary>
        /// 帳簿名
        /// </summary>
        public string BOOK_NAME { get; set; }
        /// <summary>
        /// 初期値
        /// </summary>
        public int BALANCE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool INCLUDE_FLG { get; set; }
    }
}
