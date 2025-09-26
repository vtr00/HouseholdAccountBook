using System;
using System.Collections.Generic;

namespace HouseholdAccountBook.Others.RequestEventArgs
{
    public class EditActionListRequestEventArgs : DbRequestEventArgsBase
    {
        public int GroupId { get; set; }

        public EventHandler<EventArgs<List<int>>> Registered { get; set; }
    }
}
