using HouseholdAccountBook.Dto.Abstract;
using System;

namespace HouseholdAccountBook.Dto.KHDbTable
{
    public class CbtActDto : KHDtoBase
    {
        public CbtActDto() { }

        public int ACT_ID { get; set; }
        public int BOOK_ID { get; set; }
        public int ITEM_ID { get; set; }
        public DateTime ACT_DT { get; set; }
        public int INCOME { get; set; }
        public int EXPENSE { get; set; }
        public string NOTE_NAME { get; set; }
        public int GROUP_ID { get; set; }
        public int FREQUENCY { get; set; }
    }
}
