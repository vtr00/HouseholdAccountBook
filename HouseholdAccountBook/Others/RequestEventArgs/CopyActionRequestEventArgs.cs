using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Others.RequestEventArgs
{
    public class CopyActionRequestEventArgs : DbRequestEventArgsBase
    {
        public int ActionId { get; set; }

        public EventHandler<EventArgs<List<int>>> Registered { get; set; }
    }
}
