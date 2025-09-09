using System;

namespace HouseholdAccountBook.Others
{
    public class CloseRequestEventArgs(bool? result, bool isDialog = true) : EventArgs
    {
        public bool? DialogResult { get; set; } = result;

        public bool IsDialog { get; set; } = isDialog;
    }
}
