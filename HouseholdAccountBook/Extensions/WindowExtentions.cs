using System.Diagnostics;
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

                double right = window.Left + window.Width;
                double bottom = window.Top + window.Height;
                double OwnerRight = window.Owner.Left + window.Owner.Width;
                double OwnerBottom = window.Owner.Left + window.Owner.Height;

                Debug.WriteLine(string.Format("window: top {0}, right {1}, bottom {2}, left {3}, width {4}, height {5}", window.Top, right, bottom, window.Left, window.Width, window.Height));
                Debug.WriteLine(string.Format("owner : top {0}, right {1}, bottom {2}, left {3}, width {4}, height {5}", window.Owner.Top, OwnerRight, OwnerBottom, window.Owner.Left, window.Owner.Width, window.Owner.Height));
            }
        }
    }
}
