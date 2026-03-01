using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// メニュー項目VM
    /// </summary>
    public class MenuItemViewModel
    {
        #region プロパティ
        /// <summary>
        /// 表示文字列
        /// </summary>
        public string Header { get; init; }
        /// <summary>
        /// 子メニュー項目
        /// </summary>
        public ObservableCollection<MenuItemViewModel> Children { get; init; }

        /// <summary>
        /// 選択時コマンド
        /// </summary>
        public ICommand Command { get; init; }
        #endregion
    }
}
