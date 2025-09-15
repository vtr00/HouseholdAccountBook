using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Others
{
    public class AddActionRequestEventArgs : EventArgs
    {
        public DbHandlerFactory DbHandlerFactory { get; set; }

        public int BookId { get; set; }

        public CsvViewModel Record { get; set; }

        public List<CsvViewModel> Records { get; set; }

        public EventHandler<EventArgs<List<int>>> Registered { get; set; }
    }
}
