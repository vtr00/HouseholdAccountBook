using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static HouseholdAccountBook.Extensions.FrameworkElementExtensions;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// CsvComparisonWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CsvComparisonWindow : Window
    {
        #region イベント
        /// <summary>
        /// 一致フラグ変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<int?, bool>> IsMatchChanged
        {
            add => this.WVM.IsMatchChanged += value;
            remove => this.WVM.IsMatchChanged -= value;
        }
        /// <summary>
        /// 帳簿項目変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> ActionChanged
        {
            add => this.WVM.ActionChanged += value;
            remove => this.WVM.ActionChanged -= value;
        }
        /// <summary>
        /// 帳簿変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<int?>> BookChanged
        {
            add => this.WVM.BookChanged += value;
            remove => this.WVM.BookChanged -= value;
        }

        /// <summary>
        /// ウィンドウ非表示時イベント
        /// </summary>
        public event EventHandler Hided;
        #endregion

        /// <summary>
        /// <see cref="CsvComparisonWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        public CsvComparisonWindow(Window owner, DbHandlerFactory dbHandlerFactory, int? selectedBookId)
        {
            this.Owner = owner;
            this.Name = "CsvComp";
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();

            this.AddCommonEventHandlers();
            // ロード時処理はコンストラクタで設定しておく
            this.Loaded += async (sender, e) => {
                using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create()) {
                    await this.WVM.LoadAsync(selectedBookId);
                }

                this.WVM.ScrollToButtomRequested += (sender, e) => {
                    this.csvCompDataGrid.ScrollToButtom();
                };
                this.WVM.AddActionRequested += (sender, e) => {
                    ActionRegistrationWindow arw = new(this, e.DbHandlerFactory, e.BookId, e.Record);
                    arw.Registrated += e.Registered;
                    _ = arw.ShowDialog();
                };
                this.WVM.AddActionListRequested += (sender, e) => {
                    ActionListRegistrationWindow alrw = new(this, e.DbHandlerFactory, e.BookId, e.Records);
                    alrw.Registrated += e.Registered;
                    _ = alrw.ShowDialog();
                };
                this.WVM.EditActionRequested += (sender, e) => {
                    ActionRegistrationWindow arw = new(this, e.DbHandlerFactory, e.ActionId);
                    arw.Registrated += e.Registered;
                    _ = arw.ShowDialog();
                };
                this.WVM.EditActionListRequested += (sender, e) => {
                    ActionListRegistrationWindow alrw = new(this, e.DbHandlerFactory, e.GroupId);
                    alrw.Registrated += e.Registered;
                    _ = alrw.ShowDialog();
                };

                var dcr = VisualTreeHelper.GetChild(this.csvCompDataGrid, 0) as Decorator;
                var sv = dcr.Child as ScrollViewer;
                sv.ScrollChanged += this.CsvCompDataGrid_ScrollChanged;
            };

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);
        }

        #region イベントハンドラ
        #region ウィンドウ
        /// <summary>
        /// ウィンドウ表示状態変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CsvComparisonWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            if (!oldValue && newValue) {
                if (newValue) {
                    await this.WVM.UpdateComparisonVMListAsync();
                }
                else {
                    this.Hided?.Invoke(sender, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// ウィンドウ終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsvComparisonWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            this.Hide();
        }
        #endregion

        /// <summary>
        /// DataGridスクロール時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsvCompDataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // TODO: 一致列の有効無効の表示状態を更新したい
            // 表示を更新する
            DataGrid dataGrid = sender as DataGrid;
            dataGrid?.UpdateLayout();
        }

        /// <summary>
        /// CheckBoxマウスホーバー時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_MouseEnter(object sender, MouseEventArgs e)
        {
            // Ctrlキーが押されていたらチェックを入れる
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                if (sender is CheckBox checkBox) {
                    checkBox.IsChecked = !checkBox.IsChecked;

                    if (checkBox?.DataContext is CsvComparisonViewModel vm) {
                        this.WVM.SelectedCsvComparisonVM = vm;
                        this.WVM.CheckIsMatchCommand?.Execute(null);
                    }
                }
            }
        }
        #endregion
    }
}
