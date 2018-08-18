using System.Windows.Interactivity;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Collections.Specialized;
using HouseholdAccountBook.Interfaces;

namespace HouseholdAccountBook.Behaviors
{
    /// <summary>
    /// <see cref="DataGrid"/> 内で選択されたアイテムのコレクションを保持するビヘイビア
    /// </summary>
    /// <remarks>http://chorusde.hatenablog.jp/entry/2013/02/28/064747</remarks>
    public class BindSelectedItemsToDataGridBehavior : Behavior<DataGrid>
    {
        /// <summary>
        /// アタッチ時
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject != null) {
                // DataGridで選択された項目が変更された場合に発生するイベントを登録する
                ((DataGrid)this.AssociatedObject).SelectionChanged += this.DataGrid_SelectionChanged;
            }
        }

        /// <summary>
        /// デタッチ時
        /// </summary>
        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null) {
                // DataGridで選択された項目が変更された場合に発生するイベントを解除する
                ((DataGrid)this.AssociatedObject).SelectionChanged -= this.DataGrid_SelectionChanged;
            }

            base.OnDetaching();
        }

        #region 依存関係プロパティ
        #region SelectedItems
        /// <summary>
        /// 選択されたアイテムリスト
        /// </summary>
        public IList SelectedItems
        {
            get { return (IList)this.GetValue(SelectedItemsProperty); }
            set { this.SetValue(SelectedItemsProperty, value); }
        }

        /// <summary>
        /// <see cref="SelectedItems"/> 依存関係プロパティを識別します。
        /// </summary>
        public static DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
                PropertyName<BindSelectedItemsToDataGridBehavior>.Get(x => x.SelectedItems), 
                typeof(IList), 
                typeof(BindSelectedItemsToDataGridBehavior), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, BindSelectedItemsToDataGridBehavior.SelectedItemsChanged));
        #endregion
        #endregion

        #region イベントハンドラ
        /// <summary>
        /// <see cref="SelectedItems"/> を選択されたアイテムの変更に応じて更新します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //新たに選択されたアイテムをリストに追加する
            foreach (object addedItem in e.AddedItems) {
                if (!this.SelectedItems.Contains(addedItem)) {
                    this.SelectedItems.Add(addedItem);
                }
            }

            //選択解除されたアイテムをリストから削除する
            foreach (object removedItem in e.RemovedItems) {
                while (this.SelectedItems.Contains(removedItem)) {
                    this.SelectedItems.Remove(removedItem);
                }
            }
        }

        /// <summary>
        /// <see cref="SelectedItems"/> が変更されたときに <see cref="CollectionChanged(object, NotifyCollectionChangedEventArgs)"/> を設定します。
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void SelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.OldValue != e.NewValue) {
                if(e.OldValue is INotifyCollectionChanged oldValue) {
                    oldValue.CollectionChanged -= BindSelectedItemsToDataGridBehavior.CollectionChanged;
                }
                if(e.NewValue is INotifyCollectionChanged newValue) {
                    newValue.CollectionChanged += BindSelectedItemsToDataGridBehavior.CollectionChanged;
                }
            }
        }

        /// <summary>
        /// <see cref="SelectedItems"/> 内のアイテムが更新されたときに <see cref="IMultiSelectable.IsSelected"/> を更新します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != e.NewItems) {
                if (e.OldItems != null) {
                    foreach (object oldItem in e.OldItems) {
                        if (oldItem is IMultiSelectable oldV) {
                            oldV.IsSelected = false;
                        }
                    }
                }
                if (e.NewItems != null) {
                    foreach (object newItem in e.NewItems) {
                        if (newItem is IMultiSelectable newV) {
                            newV.IsSelected = true;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
