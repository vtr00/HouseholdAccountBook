using HouseholdAccountBook.ViewModels.Abstract;
using System.Collections.ObjectModel;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// 階層構造VM
    /// </summary>
    public class HierarchicalViewModel : BindableBase, ISelectable
    {
        #region プロパティ
        /// <summary>
        /// 階層の深さ
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 親要素VM
        /// </summary>
        public HierarchicalViewModel ParentVM { get; set; }

        /// <summary>
        /// ソート順
        /// </summary>
        #region SortOrder
        public int SortOrder {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 名称
        /// </summary>
        #region Name
        public string Name {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 選択されているか
        /// </summary>
        #region SelectFlag
        public bool SelectFlag {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 子要素VMリスト
        /// </summary>
        #region ChildrenVMList
        public ObservableCollection<HierarchicalViewModel> ChildrenVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion
    }
}
