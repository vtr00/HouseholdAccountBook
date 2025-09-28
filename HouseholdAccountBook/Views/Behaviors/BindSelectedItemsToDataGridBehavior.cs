using HouseholdAccountBook.ViewModels.Abstract;
using Microsoft.Xaml.Behaviors;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace HouseholdAccountBook.Views.Behaviors
{
    /// <summary>
    /// <see cref="DataGrid"/> 内で選択されたアイテムのコレクションを保持するビヘイビア
    /// </summary>
    /// <remarks>http://chorusde.hatenablog.jp/entry/2013/02/28/064747</remarks>
    /// <example>
    /// UIで選択時: DataGrid.SelectionChanged -> this.DataGrid_SelectionChanged -> this.SelectedItems.Add/Remove -> SelectedItems_CollectionChanged -> ISelectable.SelectFlag
    /// コードで選択時: this.SelectedItems.Add/Remove -> SelectedItems_CollectionChanged -> ISelectable.SelectFlag -> DataGrid.SelectionChanged -> this.DataGrid_SelectionChanged
    /// </example>
    public class BindSelectedItemsToDataGridBehavior : Behavior<DataGrid>
    {
        /// <summary>
        /// アタッチ時
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject != null) {
                // DataGridで選択項目変更時のイベントハンドラを登録する
                this.AssociatedObject.SelectionChanged += this.DataGrid_SelectionChanged;
            }
        }

        /// <summary>
        /// デタッチ時
        /// </summary>
        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null) {
                // DataGridで選択項目変更時のイベントハンドラを解除する
                this.AssociatedObject.SelectionChanged -= this.DataGrid_SelectionChanged;
            }

            base.OnDetaching();
        }

        #region 依存関係プロパティ
        /// <summary>
        /// 選択されたアイテムのコレクション
        /// </summary>
        #region SelectedItems
        public IList SelectedItems
        {
            get => (IList)this.GetValue(SelectedItemsProperty);
            set => this.SetValue(SelectedItemsProperty, value);
        }
        #endregion

        /// <summary>
        /// <see cref="SelectedItems"/> 依存関係プロパティを識別します。
        /// </summary>
        #region SelectedItemsProperty
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
                nameof(SelectedItems),
                typeof(IList),
                typeof(BindSelectedItemsToDataGridBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedItemsChanged));
        #endregion
        #endregion

        #region イベントハンドラ
        /// <summary>
        /// <see cref="SelectedItems"/> が変更されたときに <see cref="SelectedItems_CollectionChanged(object, NotifyCollectionChangedEventArgs)"/> を設定します。
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void SelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue) {
                if (e.OldValue is INotifyCollectionChanged oldValue) {
                    oldValue.CollectionChanged -= SelectedItems_CollectionChanged;
                }
                if (e.NewValue is INotifyCollectionChanged newValue) {
                    newValue.CollectionChanged += SelectedItems_CollectionChanged;
                }
            }
        }

        /// <summary>
        /// <see cref="DataGrid"> の選択されたアイテムの変更に応じて <see cref="SelectedItems"/> を更新します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //新たに選択されたアイテムをリストに追加する
            foreach (object addedItem in e.AddedItems) {
                if (!this.SelectedItems.Contains(addedItem)) {
                    _ = this.SelectedItems.Add(addedItem);
                }
            }
            //選択が解除されたアイテムをリストから削除する
            foreach (object removedItem in e.RemovedItems) {
                if (this.SelectedItems.Contains(removedItem)) {
                    this.SelectedItems.Remove(removedItem);
                }
            }
        }

        /// <summary>
        /// <see cref="SelectedItems"/> 内のアイテムが更新されたときに <see cref="ISelectable.SelectFlag"/> を更新します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null) {
                foreach (object oldItem in e.OldItems) {
                    if (oldItem is ISelectable oldV) {
                        oldV.SelectFlag = false;
                    }
                }
            }
            if (e.NewItems != null) {
                foreach (object newItem in e.NewItems) {
                    if (newItem is ISelectable newV) {
                        newV.SelectFlag = true;
                    }
                }
            }
        }
        #endregion
    }
}
