using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.Args;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.Views.Extensions;
using HouseholdAccountBook.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HouseholdAccountBook.Views.Windows
{
    /// <summary>
    /// ActionListRegistrationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ActionListRegistrationWindow : Window
    {
        #region フィールド
        /// <summary>
        /// 金額列の最後に選択したセル
        /// </summary>
        private DataGridCell mLastDataGridCell;
        /// <summary>
        /// 最後に選択したセルのVM
        /// </summary>
        private DateValueViewModel mLastDateValueVM;
        #endregion

        #region イベント
        /// <summary>
        /// 帳簿変更時のイベント
        /// </summary>
        public event EventHandler<ChangedEventArgs<AccountIdObj>> SelectedAccountChanged {
            add => this.WVM.SelectedAccountChanged += value;
            remove => this.WVM.SelectedAccountChanged -= value;
        }

        /// <summary>
        /// 登録時のイベント
        /// </summary>
        public event EventHandler<EventArgs<IEnumerable<ActionIdObj>>> Registrated {
            add => this.WVM.Registrated += value;
            remove => this.WVM.Registrated -= value;
        }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 複数の帳簿項目の追加のために <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="initialAccountId">初期選択する帳簿のID</param>
        /// <param name="initialMonth">初期選択する年月</param>
        /// <param name="initialDate">初期選択する日付</param>
        public ActionListRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, AccountIdObj initialAccountId, DateOnly? initialMonth, DateOnly? initialDate = null)
            : this(owner, dbHandlerFactory, initialAccountId, initialMonth, initialDate, null, null, RegistrationKind.Add) { }

        /// <summary>
        /// 複数の帳簿項目の追加のために <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="initialAccountId">初期選択する帳簿のID</param>
        /// <param name="initialRecordList">初期表示するCSVレコードリスト</param>
        public ActionListRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, AccountIdObj initialAccountId, IEnumerable<ActionCsvModel> initialRecordList)
            : this(owner, dbHandlerFactory, initialAccountId, null, null, initialRecordList, null, RegistrationKind.Add) { }

        /// <summary>
        /// 複数の帳簿項目の編集のために <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="targetGroupId">編集する帳簿項目のグループID</param>
        public ActionListRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, GroupIdObj targetGroupId)
            : this(owner, dbHandlerFactory, null, null, null, null, targetGroupId, RegistrationKind.Edit) { }

        /// <summary>
        /// <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="initialAccountId">追加時、初期選択する帳簿ID</param>
        /// <param name="initialMonth">追加時、初期選択する年月</param>
        /// <param name="initialDate">追加時、初期選択する日付</param>
        /// <param name="initialRecordList">追加時、初期表示するCSVレコードリスト</param>
        /// <param name="targetGroupId">編集時、編集する帳簿項目のグループID</param>
        /// <param name="regKind">登録種別</param>
        private ActionListRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, AccountIdObj initialAccountId, DateOnly? initialMonth, DateOnly? initialDate,
            IEnumerable<ActionCsvModel> initialRecordList, GroupIdObj targetGroupId, RegistrationKind regKind)
        {
            using FuncLog funcLog = new(new { initialAccountId, initialMonth, initialDate, initialRecordList, targetGroupId, regKind });

            this.Owner = owner;
            this.Name = UiConstants.WindowNameStr[nameof(ActionListRegistrationWindow)];
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();
            this.AddCommonEventHandlersToVM();

            this.WVM.Initialize(dbHandlerFactory);
            this.WVM.RegKind = regKind;

            this.Loaded += async (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.Loaded));

                await this.WVM.LoadAsync(initialAccountId, initialMonth, initialDate, initialRecordList, targetGroupId);
                this.WVM.AddEventHandlers();
            };
        }
        #endregion

        #region イベントハンドラ
        #region コマンド
        /// <summary>
        /// 数値入力ボタン押下コマンド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonInputCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            NumericUpDown numericUpDown = this._popup.PlacementTarget as NumericUpDown;

            // 入力時処理
            numericUpDown.ButtonInputed(this.WVM.InputedValue, this.WVM.InputedKind);
            // Bindしているが、何故かコピーしないと反映されない
            this.mLastDateValueVM.InputedValue = numericUpDown.Value;
            // FollowablePopup の 表示状態をコピー
            this.WVM.IsOpenPopup = numericUpDown.IsOpen;

            if (this.WVM.InputedKind != NumericInputButton.InputKind.Close) {
                // 外れたフォーカスを元に戻す
                this.mLastDataGridCell.IsEditing = true; // セルを編集モードにする - 画面がちらつくがやむを得ない
                _ = numericUpDown.Focus();
                _ = this.mLastDataGridCell.Focus(); // Enterキーでの入力完了を有効にする
                //Keyboard.Focus(numericUpDown); // キーでの数値入力を有効にする - 意図した動作にならない
            }

            e.Handled = true;
        }
        #endregion

        /// <summary>
        /// 日付金額リスト追加時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            using FuncLog funcLog = new();

            if (this.WVM.InputedDateValueVMList.Count == 0) {
                // ダミーデータ
                e.NewItem = new DateValueViewModel() {
                    SelectedDate = DateTime.Now,
                    InputedValue = null
                };
            }
            else {
                // リストに入力済の末尾のデータの日付を追加時に採用する
                DateValueViewModel lastVM = this.WVM.InputedDateValueVMList.Last();
                e.NewItem = new DateValueViewModel() {
                    SelectedDate = this.WVM.IsDateAutoIncrement ? lastVM.SelectedDate.AddDays(1) : lastVM.SelectedDate,
                    InputedValue = null,
                    SelectedAccountAssetId = this.WVM.AccountSelectorVM.SelectedItem.AssetId
                };
            }
        }

        /// <summary>
        /// 日付金額リスト選択変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            if (dataGrid.SelectedIndex != -1) {
                _ = dataGrid.BeginEdit();
            }
        }

        /// <summary>
        /// 金額列セルフォーカス取得時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            using FuncLog funcLog = new();

            DataGridCell dataGridCell = sender as DataGridCell;

            // 新しいセルに移動していたら数値入力ボタンを非表示にする
            if (dataGridCell != this.mLastDataGridCell) {
                this.WVM.IsOpenPopup = false;
                this.mLastDataGridCell = dataGridCell;
            }
        }

        /// <summary>
        /// <see cref="NumericUpDown"/> フォーカス取得時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown_GotFocus(object sender, RoutedEventArgs e)
        {
            using FuncLog funcLog = new();

            if (!this.WVM.IsOpenPopup) {
                NumericUpDown numericUpDown = sender as NumericUpDown;
                this._popup.PlacementTarget = numericUpDown;
                this.WVM.IsOpenPopup = true;

                this.mLastDateValueVM = numericUpDown.DataContext as DateValueViewModel;
            }
            e.Handled = true;
        }
        #endregion
    }
}
