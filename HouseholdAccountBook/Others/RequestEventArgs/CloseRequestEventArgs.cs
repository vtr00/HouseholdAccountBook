using System;

namespace HouseholdAccountBook.Others.RequestEventArgs
{
    public class CloseRequestEventArgs(bool? result) : EventArgs
    {
        public bool? DialogResult { get; set; } = result;
    }
}
