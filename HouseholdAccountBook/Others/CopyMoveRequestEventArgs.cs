using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Others
{
    public class CopyMoveRequestEventArgs : DbRequestEventArgsBase
    {
        public int GroupId { get; set; }

        public EventHandler<EventArgs<List<int>>> Registered { get; set; }
    }
}
