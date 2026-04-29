using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;

namespace HouseholdAccountBook.Views.Behaviors
{
    /// <summary>
    /// <see cref="ListBox"/> で選択されたアイテムにスクロールするためのビヘイビア
    /// </summary>
    public class ScrollToSelectedItemListBoxBehavior : Behavior<ListBox>
    {
        /// <summary>
        /// アタッチ時
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            // ListBoxで選択された項目が変更された場合に発生するイベントを登録する
            this.AssociatedObject?.SelectionChanged += this.ListBox_SelectionChanged;
        }

        /// <summary>
        /// デタッチ時
        /// </summary>
        protected override void OnDetaching()
        {
            // ListBoxで選択された項目が変更された場合に発生するイベントを解除する
            this.AssociatedObject?.SelectionChanged -= this.ListBox_SelectionChanged;

            base.OnDetaching();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.AssociatedObject?.SelectedItem != null) {
                this.AssociatedObject.ScrollIntoView(this.AssociatedObject.SelectedItem);
            }
        }
    }
}
