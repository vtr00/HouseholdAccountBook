using System.Windows;

namespace HouseholdAccountBook.Extensions
{
    /// <summary>
    /// <see cref="Window"/> の拡張メソッドを提供します
    /// </summary>
    public static class WindowExtentions
    {
        /// <summary>
        /// <see cref="Window.Owner"/> の中央位置に移動する
        /// </summary>
        /// <param name="window"></param>
        public static void MoveOwnersCenter(this Window window)
        {
            window.WindowStartupLocation = window.Owner != null && window.Owner.WindowState == WindowState.Normal
                ? WindowStartupLocation.CenterOwner
                : WindowStartupLocation.CenterScreen;

            double right = window.Left + window.Width;
            double bottom = window.Top + window.Height;
            Log.Info(string.Format($"window - top:{window.Top} right:{right} bottom:{bottom} left:{window.Left} width:{window.Width} height:{window.Height}"));

            if (window.Owner != null) {
                double OwnerRight = window.Owner.Left + window.Owner.Width;
                double OwnerBottom = window.Owner.Left + window.Owner.Height;
                Log.Info(string.Format($"owner  - top:{window.Owner.Top} right:{OwnerRight} bottom:{OwnerBottom} left:{window.Owner.Left} width:{window.Owner.Width} height:{window.Owner.Height}"));
            }
        }
    }
}
