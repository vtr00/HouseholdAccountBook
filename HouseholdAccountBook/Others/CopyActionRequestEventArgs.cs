using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Others
{
    public class CopyActionRequestEventArgs : DbRequestEventArgsBase
    {
        public int ActionId { get; set; }

        public EventHandler<EventArgs<List<int>>> Registered { get; set; }
    }
}
