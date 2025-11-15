using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using System.Collections.ObjectModel;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// 階層構造設定VM
    /// </summary>
    public class HierarchicalSettingViewModel : BindableBase
    {
        /// <summary>
        /// 階層種別
        /// </summary>
        public enum HierarchicalKind
        {
            /// <summary>
            /// 収支
            /// </summary>
            Balance = 0,
            /// <summary>
            /// 分類
            /// </summary>
            Category,
            /// <summary>
            /// 項目
            /// </summary>
            Item
        }

        #region プロパティ
        /// <summary>
        /// 種類
        /// </summary>
        public HierarchicalKind Kind { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

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
        /// 関係性VMリスト
        /// </summary>
        #region RelationVMList
        public ObservableCollection<RelationViewModel> RelationVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された関係性VM
        /// </summary>
        #region SelectedRelationVM
        public RelationViewModel SelectedRelationVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 店舗VMリスト
        /// </summary>
        #region ShopVMList
        public ObservableCollection<ShopViewModel> ShopVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された店舗VM
        /// </summary>
        #region SelectedShopVM
        public ShopViewModel SelectedShopVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 備考VMリスト
        /// </summary>
        #region RemarkVMList
        public ObservableCollection<RemarkViewModel> RemarkVMList {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        /// <summary>
        /// 選択された備考VM
        /// </summary>
        #region SelectedRemarkVM
        public RemarkViewModel SelectedRemarkVM {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// リネーム可能か
        /// </summary>
        #region IsRenamable
        public bool IsRenamable => this.Kind != HierarchicalKind.Balance;
        #endregion

        /// <summary>
        /// 設定可能か
        /// </summary>
        #region IsSettable
        public bool IsSettable => this.Kind == HierarchicalKind.Item;
        #endregion
        #endregion

        /// <summary>
        /// 階層構造VMから階層種別を取得する
        /// </summary>
        /// <param name="vm">階層構造VM</param>
        /// <returns>階層種別</returns>
        public static HierarchicalKind? GetHierarchicalKind(HierarchicalViewModel vm) => (HierarchicalKind?)vm?.Depth;
    }
}
