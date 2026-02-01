using HouseholdAccountBook.Adapters.DbHandlers;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Args;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Utilities;
using HouseholdAccountBook.ViewModels.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        public event EventHandler<EventArgs<int?, bool>> IsMatchChanged {
            add => this.WVM.IsMatchChanged += value;
            remove => this.WVM.IsMatchChanged -= value;
        }
        /// <summary>
        /// 帳簿項目変更時イベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> ActionChanged {
            add => this.WVM.ActionChanged += value;
            remove => this.WVM.ActionChanged -= value;
        }
        /// <summary>
        /// 帳簿変更時イベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<int?>> BookChanged {
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
        /// <param name="initialBookId">初期選択する帳簿のID</param>
        public CsvComparisonWindow(Window owner, DbHandlerFactory dbHandlerFactory, int? initialBookId)
        {
            using FuncLog funcLog = new(new { initialBookId });

            this.Owner = owner;
            this.Name = UiConstants.WindowNameStr[nameof(CsvComparisonWindow)];
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();
            this.AddCommonEventHandlersToVM();
            this.AddEventHandlersToVM();

            this.WVM.Initialize(new WaitCursorManagerFactory(this), dbHandlerFactory);

            this.Loaded += async (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.Loaded));

                using (WaitCursorManager wcm = new WaitCursorManagerFactory(this).Create(methodName: nameof(this.Loaded))) {
                    await this.WVM.LoadAsync(initialBookId);
                }
                this.WVM.AddEventHandlers();
            };
        }

        private void AddEventHandlersToVM()
        {
            using FuncLog funcLog = new();

            this.WVM.ScrollToButtomRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.ScrollToButtomRequested));

                this.csvCompDataGrid.ScrollToButtom();
            };
            this.WVM.AddActionRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.AddActionRequested));

                ActionRegistrationWindow arw = new(this, e.DbHandlerFactory, e.InitialBookId, e.InitialRecord);
                arw.Registrated += e.Registered;
                arw.SetIsModal(true);
                _ = arw.ShowDialog();
            };
            this.WVM.AddActionListRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.AddActionListRequested));

                ActionListRegistrationWindow alrw = new(this, e.DbHandlerFactory, e.InitialBookId, e.InitialRecordList);
                alrw.Registrated += e.Registered;
                alrw.SetIsModal(true);
                _ = alrw.ShowDialog();
            };
            this.WVM.EditActionRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.EditActionRequested));

                ActionRegistrationWindow arw = new(this, e.DbHandlerFactory, e.TargetActionId);
                arw.Registrated += e.Registered;
                arw.SetIsModal(true);
                _ = arw.ShowDialog();
            };
            this.WVM.EditActionListRequested += (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.WVM.EditActionListRequested));

                ActionListRegistrationWindow alrw = new(this, e.DbHandlerFactory, e.TargetGroupId);
                alrw.Registrated += e.Registered;
                alrw.SetIsModal(true);
                _ = alrw.ShowDialog();
            };
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
            using FuncLog funcLog = new();

            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;
            if (oldValue != newValue) {
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
            using FuncLog funcLog = new();

            e.Cancel = true;

            this.Hide();
        }
        #endregion

        /// <summary>
        /// CheckBoxマウスホーバー時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_MouseEnter(object sender, MouseEventArgs e)
        {
            // Ctrlキーが押されていたらチェックを入れる
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                using FuncLog funcLog = new();

                if (sender is CheckBox checkBox) {
                    checkBox.IsChecked = !checkBox.IsChecked;

                    if (checkBox?.DataContext is CsvComparisonViewModel vm) {
                        this.WVM.SelectedCsvComparisonVM = vm;
                        this.WVM.ChangeIsMatchCommand?.Execute(null);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 選択対象の帳簿IDを設定する
        /// </summary>
        /// <param name="bookId">選択する帳簿ID</param>
        public void SetSelectedBookId(int? bookId)
        {
            using FuncLog funcLog = new(new { bookId });

            this.WVM.SelectedBookId = bookId;
        }
    }
}
