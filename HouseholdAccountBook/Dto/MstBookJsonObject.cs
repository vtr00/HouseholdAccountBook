using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dto
{
    public class MstBookJsonObject
    {
        public int? CsvActDateIndex { get; set; } = null;
        public int? CsvOutgoIndex { get; set; } = null;
        public int? CsvItemNameIndex { get; set; } = null;
    }
}
