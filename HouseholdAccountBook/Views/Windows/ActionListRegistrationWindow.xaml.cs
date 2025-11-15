using HouseholdAccountBook.Adapters.DbHandler;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.Views.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static HouseholdAccountBook.Extensions.FrameworkElementExtensions;

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
        public event EventHandler<ChangedEventArgs<int?>> BookChanged {
            add => this.WVM.BookChanged += value;
            remove => this.WVM.BookChanged -= value;
        }

        /// <summary>
        /// 登録時のイベント
        /// </summary>
        public event EventHandler<EventArgs<List<int>>> Registrated {
            add => this.WVM.Registrated += value;
            remove => this.WVM.Registrated -= value;
        }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// 複数の帳簿項目の新規登録のために <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedMonth">選択された年月</param>
        /// <param name="selectedDate">選択された日付</param>
        public ActionListRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int? selectedBookId, DateTime? selectedMonth, DateTime? selectedDate = null)
            : this(owner, dbHandlerFactory, selectedBookId, selectedMonth, selectedDate, null, null, RegistrationKind.Add) { }

        /// <summary>
        /// 複数の帳簿項目の新規登録のために <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedRecordList">選択されたCSVレコードリスト</param>
        public ActionListRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int? selectedBookId, List<CsvViewModel> selectedRecordList)
            : this(owner, dbHandlerFactory, selectedBookId, null, null, selectedRecordList, null, RegistrationKind.Add) { }

        /// <summary>
        /// 複数の帳簿項目の編集(複製)のために <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedGroupId">選択されたグループID</param>
        /// <param name="regKind">登録種別</param>
        public ActionListRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int selectedGroupId, RegistrationKind regKind = RegistrationKind.Edit)
            : this(owner, dbHandlerFactory, null, null, null, null, selectedGroupId, regKind) { }

        /// <summary>
        /// <see cref="ActionListRegistrationWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="owner">親ウィンドウ</param>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <param name="selectedBookId">選択された帳簿ID</param>
        /// <param name="selectedMonth">選択された年月</param>
        /// <param name="selectedDate">選択された日付</param>
        /// <param name="selectedRecordList">選択されたCSVレコードリスト</param>
        /// <param name="selectedGroupId">選択されたグループID</param>
        /// <param name="regKind">登録種別</param>
        private ActionListRegistrationWindow(Window owner, DbHandlerFactory dbHandlerFactory, int? selectedBookId, DateTime? selectedMonth, DateTime? selectedDate,
            List<CsvViewModel> selectedRecordList, int? selectedGroupId, RegistrationKind regKind)
        {
            using FuncLog funcLog = new(new { selectedBookId, selectedMonth, selectedDate, selectedRecordList, selectedGroupId, regKind });

            this.Owner = owner;
            this.Name = "ActListReg";
            WindowLocationManager.Instance.Add(this);

            this.InitializeComponent();
            this.AddCommonEventHandlersToVM();

            this.WVM.Initialize(this.GetWaitCursorManagerFactory(), dbHandlerFactory);
            this.WVM.RegKind = regKind;

            this.Loaded += async (sender, e) => {
                using FuncLog funcLog = new(methodName: nameof(this.Loaded));

                using (WaitCursorManager wcm = this.GetWaitCursorManagerFactory().Create(methodName: nameof(this.Loaded))) {
                    await this.WVM.LoadAsync(selectedBookId, selectedMonth, selectedDate, selectedRecordList, selectedGroupId);
                }
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
        private void ButtonInputCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextBox textBox = this._popup.PlacementTarget as TextBox;
            if (textBox.DataContext is not DateValueViewModel vm) {
                vm = this.mLastDateValueVM; // textBoxのDataContextが取得できないため応急処置
            }

            switch (this.WVM.InputedKind) {
                case NumericInputButton.InputKind.Number:
                    int value = this.WVM.InputedValue.Value;
                    if (vm.ActValue == null) {
                        vm.ActValue = value;
                        textBox.Text = string.Format($"{vm.ActValue}");
                        textBox.SelectionStart = 1;
                    }
                    else {
                        // 選択された位置に値を挿入する
                        int selectionStart = textBox.SelectionStart;
                        int selectionEnd = selectionStart + textBox.SelectionLength;
                        string forwardText = textBox.Text[..selectionStart];
                        string backwardText = textBox.Text[selectionEnd..];

                        if (int.TryParse(string.Format($"{forwardText}{value}{backwardText}"), out int outValue)) {
                            vm.ActValue = outValue;
                            textBox.Text = string.Format($"{vm.ActValue}");
                            textBox.SelectionStart = selectionStart + 1;
                        }
                    }
                    break;
                case NumericInputButton.InputKind.BackSpace:
                    if (vm.ActValue == 0) {
                        vm.ActValue = null;
                    }
                    else {
                        int selectionStart = textBox.SelectionStart;
                        int selectionLength = textBox.SelectionLength;
                        int selectionEnd = selectionStart + textBox.SelectionLength;
                        string forwardText = textBox.Text[..selectionStart];
                        string backwardText = textBox.Text[selectionEnd..];

                        if (selectionLength != 0) {
                            if (int.TryParse(string.Format($"{forwardText}{backwardText}"), out int outValue)) {
                                vm.ActValue = outValue;
                                textBox.Text = string.Format($"{outValue}");
                                textBox.SelectionStart = selectionStart;
                            }
                        }
                        else if (selectionStart != 0) {
                            string newText = string.Format($"{forwardText[..(selectionStart - 1)]}{backwardText}");
                            if (string.Empty == newText || int.TryParse(newText, out _)) {
                                vm.ActValue = string.Empty == newText ? (int?)null : int.Parse(newText);
                                textBox.Text = string.Format($"{vm.ActValue}");
                                textBox.SelectionStart = selectionStart - 1;
                            }
                        }
                    }
                    break;
                case NumericInputButton.InputKind.Clear:
                    vm.ActValue = null;
                    break;
            }

            // 外れたフォーカスを元に戻す
            this.mLastDataGridCell.IsEditing = true; // セルを編集モードにする - 画面がちらつくがやむを得ない？
            _ = textBox.Focus();
            _ = this.mLastDataGridCell.Focus(); // Enterキーでの入力完了を有効にする
            //Keyboard.Focus(textBox); // キーでの数値入力を有効にする - 意図した動作にならない

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
            if (this.WVM.DateValueVMList.Count == 0) {
                e.NewItem = new DateValueViewModel() { ActDate = DateTime.Today, ActValue = null };
            }
            else {
                // リストに入力済の末尾のデータの日付を追加時に採用する
                DateValueViewModel lastVM = this.WVM.DateValueVMList.Last();
                e.NewItem = new DateValueViewModel() { ActDate = this.WVM.IsDateAutoIncrement ? lastVM.ActDate.AddDays(1) : lastVM.ActDate, ActValue = null };
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
            DataGridCell dataGridCell = sender as DataGridCell;

            // 新しいセルに移動していたら数値入力ボタンを非表示にする
            if (dataGridCell != this.mLastDataGridCell) {
                this.WVM.IsEditing = false;
            }
            this.mLastDataGridCell = dataGridCell;
        }

        /// <summary>
        /// テキストボックステキスト入力確定前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            bool yes_parse = false;
            if (sender != null) {
                // 既存のテキストボックス文字列に、新規に一文字追加された時、その文字列が数値として意味があるかどうかをチェック
                {
                    string tmp = textBox.Text + e.Text;
                    if (tmp == string.Empty) {
                        yes_parse = true;
                    }
                    else {
                        yes_parse = int.TryParse(tmp, out int xx);

                        // 範囲内かどうかチェック
                        if (yes_parse) {
                            if (xx < 0) {
                                yes_parse = false;
                            }
                        }
                    }
                }
            }
            // 更新したい場合は false, 更新したくない場合は true
            e.Handled = !yes_parse;
        }

        /// <summary>
        /// テキストボックスフォーカス取得時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!this.WVM.IsEditing) {
                TextBox textBox = sender as TextBox;
                this._popup.PlacementTarget = textBox;
                this.WVM.IsEditing = true;

                this.mLastDateValueVM = textBox.DataContext as DateValueViewModel;
            }
            e.Handled = true;
        }
        #endregion
    }
}
