using HouseholdAccountBook.Models.ValueObjects;
using System.Collections.ObjectModel;

namespace HouseholdAccountBook.ViewModels.Abstract
{
    /// <summary>
    /// 階層構造VM
    /// </summary>
    public abstract class HierarchicalViewModelBase<T> : BindableBase, ISelectable
    {
        #region プロパティ
        /// <summary>
        /// 階層の深さ
        /// </summary>
        public int Depth { get; init; }

        /// <summary>
        /// ID
        /// </summary>
        public IdObj Id { get; init; } = -1;

        /// <summary>
        /// 親要素VM
        /// </summary>
        public T ParentVM { get; init; }

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; init; } = -1;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// 子要素VMリスト
        /// </summary>
        public ObservableCollection<T> ChildrenVMList { get; init; } = [];

        /// <summary>
        /// 選択されているか
        /// </summary>
        public bool SelectFlag {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
    }
}
