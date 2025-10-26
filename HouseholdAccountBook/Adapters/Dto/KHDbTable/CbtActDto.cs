using HouseholdAccountBook.Adapters.Dto.Abstract;
using System;

namespace HouseholdAccountBook.Adapters.Dto.KHDbTable
{
    /// <summary>
    /// 帳簿項目DTO
    /// </summary>
    public class CbtActDto : KHDtoBase, ISequentialIDDto
    {
        public CbtActDto() { }

        public int GetId()
        {
            return this.ACT_ID;
        }

        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public int ACT_ID { get; set; }
        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int BOOK_ID { get; set; }
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ITEM_ID { get; set; }
        /// <summary>
        /// 項目日時
        /// </summary>
        public DateTime ACT_DT { get; set; }
        /// <summary>
        /// 収入
        /// </summary>
        public int INCOME { get; set; }
        /// <summary>
        /// 支出
        /// </summary>
        public int EXPENSE { get; set; }
        /// <summary>
        /// 備考
        /// </summary>
        public string NOTE_NAME { get; set; }
        /// <summary>
        /// グループID
        /// </summary>
        public int GROUP_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FREQUENCY { get; set; }
    }
}
