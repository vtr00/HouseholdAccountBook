using HouseholdAccountBook.Interfaces;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace HouseholdAccountBook.ViewModels
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
        public int SortOrder
        {
            get => this._SortOrder;
            set => this.SetProperty(ref this._SortOrder, value);
        }
        private int _SortOrder = default;
        #endregion

        /// <summary>
        /// 名称
        /// </summary>
        #region Name
        public string Name
        {
            get => this._Name;
            set => this.SetProperty(ref this._Name, value);
        }
        private string _Name = default;
        #endregion

        /// <summary>
        /// 選択されているか
        /// </summary>
        #region SelectFlag
        public bool SelectFlag
        {
            get => this._SelectFlag;
            set => this.SetProperty(ref this._SelectFlag, value);
        }
        private bool _SelectFlag = default;
        #endregion

        /// <summary>
        /// 子要素VMリスト
        /// </summary>
        #region ChildrenVMList
        public ObservableCollection<HierarchicalViewModel> ChildrenVMList
        {
            get => this._ChildrenVMList;
            set => this.SetProperty(ref this._ChildrenVMList, value);
        }
        private ObservableCollection<HierarchicalViewModel> _ChildrenVMList = default;
        #endregion
        #endregion
    }
}
