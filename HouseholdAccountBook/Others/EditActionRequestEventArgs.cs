using HouseholdAccountBook.Models.DbHandler;
using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Others
{
    public class EditActionRequestEventArgs : EventArgs
    {
        public DbHandlerFactory DbHandlerFactory { get; set; }

        public int ActionId { get; set; }

        public int GroupId { get; set; }

        public EventHandler<EventArgs<List<int>>> Registered { get; set; }
    }
}
