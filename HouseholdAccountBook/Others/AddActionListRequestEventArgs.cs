using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Others
{
    public class AddActionListRequestEventArgs : DbRequestEventArgsBase
    {
        public int? BookId { get; set; }

        public DateTime? Month { get; set; }

        public DateTime? Date { get; set; }

        public List<CsvViewModel> Records { get; set; }

        public EventHandler<EventArgs<List<int>>> Registered { get; set; }
    }
}
