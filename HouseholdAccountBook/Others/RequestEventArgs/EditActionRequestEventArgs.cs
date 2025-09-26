using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Others.RequestEventArgs
{
    public class EditActionRequestEventArgs : DbRequestEventArgsBase
    {
        public int ActionId { get; set; }

        public EventHandler<EventArgs<List<int>>> Registered { get; set; }
    }
}
