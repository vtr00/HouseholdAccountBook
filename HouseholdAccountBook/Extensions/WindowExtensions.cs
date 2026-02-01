using HouseholdAccountBook.Adapters.Logger;
using System.Windows;

namespace HouseholdAccountBook.Extensions
{
    /// <summary>
    /// <see cref="Window"/> の拡張メソッドを提供します
    /// </summary>
    public static class WindowExtensions
    {
        public static readonly DependencyProperty IsModalProperty = DependencyProperty.RegisterAttached(
            "IsModal",
            typeof(bool),
            typeof(WindowExtensions),
            new PropertyMetadata(false));

        public static void SetIsModal(this Window window, bool value) => window.SetValue(IsModalProperty, value);

        public static bool GetIsModal(this Window window) => (bool)window.GetValue(IsModalProperty);

        /// <summary>
        /// <see cref="Window.Owner"/> の中央位置に移動する
        /// </summary>
        /// <param name="window"></param>
        public static void MoveOwnersCenter(this Window window)
        {
            using FuncLog funcLog = new(new { WindowName = window.Name });

            window.WindowStartupLocation = window.Owner != null && window.Owner.WindowState == WindowState.Normal
                ? WindowStartupLocation.CenterOwner
                : WindowStartupLocation.CenterScreen;

            double right = window.Left + window.Width;
            double bottom = window.Top + window.Height;
            Log.Debug($"child - top:{window.Top} right:{right} bottom:{bottom} left:{window.Left} width:{window.Width} height:{window.Height}");

            if (window.Owner != null) {
                double ownerRight = window.Owner.Left + window.Owner.Width;
                double ownerBottom = window.Owner.Left + window.Owner.Height;
                Log.Debug($"owner - top:{window.Owner.Top} right:{ownerRight} bottom:{ownerBottom} left:{window.Owner.Left} width:{window.Owner.Width} height:{window.Owner.Height}");
            }
        }
    }
}
