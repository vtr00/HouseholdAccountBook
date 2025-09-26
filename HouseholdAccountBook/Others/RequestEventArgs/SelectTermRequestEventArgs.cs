using HouseholdAccountBook.Enums;
using System;

namespace HouseholdAccountBook.Others.RequestEventArgs
{
    public class SelectTermRequestEventArgs : DbRequestEventArgsBase
    {
        public TermKind TermKind { get; set; }

        public DateTime? Month { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool Result { get; set; } = false;
    }
}
