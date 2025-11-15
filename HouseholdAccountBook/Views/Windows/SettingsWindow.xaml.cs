using HouseholdAccountBook.Adapters.DbHandler;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.ViewModels.Settings;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static HouseholdAccountBook.Extensions.FrameworkElementExtensions;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// SettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        #region フィールド
        /// <summary>
        /// 表示更新の必要があるか
        /// </summary>
        private bool mNeedToUpdate;
        #endregion

        /// <summary>
        /// <see cref="SettingsWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        public SettingsWindow(Window owner, DbHandlerFactory dbHandlerFactory)
        {
            using FuncLog funcLog = new();

            this.Owner = owner;
            this.Name = "Settings";
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();
            this.AddCommonEventHandlersToVM();
            this.WVM.NeedToUpdateChanged += (sender, e) => this.mNeedToUpdate = true;

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);

            this.Loaded += async (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.Loaded));

                await using (DbHandlerBase dbHandler = await dbHandlerFactory.CreateAsync()) {
                    this.WVM.OtherTabVM.SelectedDBKind = dbHandler.DBKind;
                }
                using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create(methodName: nameof(this.Loaded))) {
                    await this.WVM.LoadAsync();
                }
                this.WVM.AddEventHandlers();
            };
        }

        #region イベントハンドラ
        #region ウィンドウ
        /// <summary>
        /// ウィンドウ終了前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsWindow_Closing(object sender, CancelEventArgs e)
        {
            using FuncLog funcLog = new();

            this.DialogResult = this.mNeedToUpdate;
        }
        #endregion

        /// <summary>
        /// 選択中の設定タブを変更した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SettingsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource != sender) { return; }

            using FuncLog funcLog = new(); // ここで e.OldItems, e.NewItems をログに出すと、StackOverflow になる

            using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create()) {
                switch (this.WVM.SelectedTab) {
                    case SettingsTabs.ItemSettingsTab:
                        await this.WVM.ItemTabVM.LoadAsync(HierarchicalSettingViewModel.GetHierarchicalKind(this.WVM.ItemTabVM.SelectedHierarchicalVM), this.WVM.ItemTabVM.SelectedHierarchicalVM?.Id);
                        break;
                    case SettingsTabs.BookSettingsTab:
                        await this.WVM.BookTabVM.LoadAsync(this.WVM.BookTabVM.SelectedBookVM?.Id);
                        break;
                    case SettingsTabs.OtherSettingsTab:
                        this.WVM.OtherTabVM.Load();
                        break;
                }
            }
        }

        /// <summary>
        /// 項目設定で一覧の選択を変更した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.shopDataGrid.Items.Count > 0 && VisualTreeHelper.GetChildrenCount(this.shopDataGrid) > 0) {
                if (VisualTreeHelper.GetChild(this.shopDataGrid, 0) is Decorator border) {
                    if (border.Child is ScrollViewer scroll) {
                        scroll.ScrollToTop();
                    }
                }
            }
            if (this.remarkDataGrid.Items.Count > 0 && VisualTreeHelper.GetChildrenCount(this.remarkDataGrid) > 0) {
                if (VisualTreeHelper.GetChild(this.remarkDataGrid, 0) is Decorator border) {
                    if (border.Child is ScrollViewer scroll) {
                        scroll.ScrollToTop();
                    }
                }
            }
        }
        #endregion
    }
}
