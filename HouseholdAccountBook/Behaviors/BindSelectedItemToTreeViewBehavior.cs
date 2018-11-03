using HouseholdAccountBook.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace HouseholdAccountBook.Behaviors
{
    /// <summary>
    /// <see cref="TreeView"/> 内で選択されたアイテムを保持するビヘイビア
    /// </summary>
    class BindSelectedItemToTreeViewBehavior : Behavior<TreeView>
    {
        /// <summary>
        /// アタッチ時
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject != null) {
                // TreeViewで選択された項目が変更された場合に発生するイベントを登録する
                ((TreeView)this.AssociatedObject).SelectedItemChanged += this.TreeView_SelectedItemChanged;
            }
        }

        /// <summary>
        /// デタッチ時
        /// </summary>
        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null) {
                // TreeViewで選択された項目が変更された場合に発生するイベントを解除する
                ((TreeView)this.AssociatedObject).SelectedItemChanged -= this.TreeView_SelectedItemChanged;
            }

            base.OnDetaching();
        }

        #region 依存関係プロパティ
        #region SelectedItem
        /// <summary>
        /// 選択されたアイテム
        /// </summary>
        public object SelectedItem
        {
            get => this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        /// <summary>
        /// <see cref="SelectedItem"/> 依存関係プロパティを識別します。
        /// </summary>
        public static DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(BindSelectedItemToTreeViewBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedItemChanged));
        #endregion
        #endregion

        #region イベントハンドラ
        /// <summary>
        /// <see cref="TreeView.SelectedItem"/> 変更時に <see cref="SelectedItem"/> を変更します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.SelectedItem != (sender as TreeView)?.SelectedItem) {
                this.SelectedItem = (sender as TreeView)?.SelectedItem;
            }
        }

        /// <summary>
        /// <see cref="SelectedItem"/> が変更されたときに <see cref="IMultiSelectable.IsSelected"/> を更新します。
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue) {
                if (e.OldValue is IMultiSelectable oldV) {
                    oldV.IsSelected = false;
                }

                if (e.NewValue is IMultiSelectable newV) {
                    newV.IsSelected = true;
                }
            }
        }
        #endregion
    }
}
