using System.Windows;

namespace HouseholdAccountBook.Extensions
{
    public static class WindowExtentions
    {
        public static void MoveOwnersCenter(this Window window)
        {
            if (window.Owner != null) {
                window.Left = window.Owner.Left + (window.Owner.Width - window.Width) / 2;
                window.Top = window.Owner.Top + (window.Owner.Height - window.Height) / 2; 
            }
        }
    }
}
