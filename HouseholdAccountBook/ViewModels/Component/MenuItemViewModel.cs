using HouseholdAccountBook.ViewModels.Abstract;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// メニュー項目VM
    /// </summary>
    public class MenuItemViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 表示文字列
        /// </summary>
        public string Header { get; set; }
        /// <summary>
        /// 子メニュー項目
        /// </summary>
        public ObservableCollection<MenuItemViewModel> Children { get; set; }

        /// <summary>
        /// 選択時コマンド
        /// </summary>
        public ICommand Command { get; set; }
        #endregion
    }
}
