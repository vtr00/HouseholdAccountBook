using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Others.RequestEventArgs
{
    public class AddActionRequestEventArgs : DbRequestEventArgsBase
    {
        public int? BookId { get; set; }

        public DateTime? Month { get; set; }

        public DateTime? Date { get; set; }

        public CsvViewModel Record { get; set; }

        public EventHandler<EventArgs<List<int>>> Registered { get; set; }
    }
}
