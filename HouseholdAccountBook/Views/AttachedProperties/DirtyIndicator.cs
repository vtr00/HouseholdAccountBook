using System.Windows;

namespace HouseholdAccountBook.Views.AttachedProperties
{
    /// <summary>
    /// 入力欄の未保存状態を扱う添付プロパティ
    /// </summary>
    public static class DirtyIndicator
    {
        public static readonly DependencyProperty IsDirtyProperty =
            DependencyProperty.RegisterAttached(
                "IsDirty",
                typeof(bool),
                typeof(DirtyIndicator),
                new FrameworkPropertyMetadata(false));

        public static void SetIsDirty(DependencyObject element, bool value) => element.SetValue(IsDirtyProperty, value);

        public static bool GetIsDirty(DependencyObject element) => (bool)element.GetValue(IsDirtyProperty);
    }
}
