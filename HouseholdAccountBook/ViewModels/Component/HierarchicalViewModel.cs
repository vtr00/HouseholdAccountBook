using HouseholdAccountBook.ViewModels.Abstract;
using System.Collections.ObjectModel;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 階層構造VM
    /// </summary>
    public class HierarchicalViewModel<T> : BindableBase, ISelectable
    {
        #region プロパティ
        /// <summary>
        /// 階層の深さ
        /// </summary>
        public int Depth { get; init; }

        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// 親要素VM
        /// </summary>
        public T ParentVM { get; init; }

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; init; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// 子要素VMリスト
        /// </summary>
        public ObservableCollection<T> ChildrenVMList { get; init; }

        /// <summary>
        /// 選択されているか
        /// </summary>
        #region SelectFlag
        public bool SelectFlag {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion
    }
}
