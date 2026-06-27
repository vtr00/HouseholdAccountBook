using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.ViewModels;
using HouseholdAccountBook.ViewModels.Abstract;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HouseholdAccountBook.Views.UserControls
{
    /// <summary>
    /// NumericUpDown.xaml の相互作用ロジック
    /// </summary>
    public partial class NumericUpDown : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// <see cref="DataGridCell"/> 内に配置するとき編集状態を保持するためのクラス
        /// </summary>
        public class EditSession : BindableBase
        {
            /// <summary>
            /// 編集中の文字列
            /// </summary>
            public string Text {
                get;
                set => _ = this.SetProperty(ref field, value);
            } = "";

            /// <summary>
            /// 選択開始位置
            /// </summary>
            public int SelectionStart { get; set; }

            /// <summary>
            /// 選択長
            /// </summary>
            public int SelectionLength { get; set; }

            /// <summary>
            /// 文字列の更新を通知する
            /// </summary>
            public void RaiseTextChanged() => this.RaisePropertyChanged(nameof(this.Text));
        }

        #region 依存関係プロパティ
        /// <summary>
        /// <see cref="Session"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty SessionProperty = DependencyProperty.Register(
                nameof(Session),
                typeof(EditSession),
                typeof(NumericUpDown),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    static (d, e) => ((NumericUpDown)d).SessionChanged(e)
                    )
            );
        /// <summary>
        /// 編集状態
        /// </summary>
        public EditSession Session {
            get => (EditSession)this.GetValue(SessionProperty);
            set => this.SetValue(SessionProperty, value);
        }
        /// <summary>
        /// <see cref="Session"/> 変更時処理
        /// </summary>
        /// <param name="e"></param>
        private void SessionChanged(DependencyPropertyChangedEventArgs e)
        {
            using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, Log.LogLevel.Trace);

            if (this.mIsLoadedCompleted) {
                EditSession session = e.NewValue as EditSession;
                session.RaiseTextChanged();
                this._textBox.SelectionStart = session.SelectionStart;
                this._textBox.SelectionLength = session.SelectionLength;
            }
        }

        /// <summary>
        /// <see cref="Value"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
                nameof(Value),
                typeof(decimal?),
                typeof(NumericUpDown),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    static (d, e) => ((NumericUpDown)d).ValueChanged(e),
                    static (d, e) => ((NumericUpDown)d).CoerceValueImpl(e)
                    )
            );
        /// <summary>
        /// 値(主単位)
        /// </summary>
        public decimal? Value {
            get => (decimal?)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
        /// <summary>
        /// <see cref="Value"/> 変更時処理
        /// </summary>
        /// <param name="e"></param>
        private void ValueChanged(DependencyPropertyChangedEventArgs e)
        {
            using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, Log.LogLevel.Trace);

            if (this.mIsLoadedCompleted) {
                bool valid = false;
                decimal value = 0;
                if (e.NewValue is null) { // 値がNullならNullValueを表示
                    valid = true;
                    value = this.NullValue;
                }
                else if (e.NewValue is decimal newValue) {
                    if (e.OldValue is null) { // 更新前の値がNullなら更新後の値を設定
                        valid = true;
                        value = this.Value.Value;
                    }
                    else if (e.OldValue is decimal oldValue && oldValue != newValue) { // 値が変更されていたら設定
                        valid = true;
                        value = this.Value.Value;
                    }
                }

                if (valid) {
                    this.Session.Text = !this.IsFloating ? value.ToString($"F{this.Scale}") : value.ToString();
                }
            }
        }
        /// <summary>
        /// <see cref="Value"/> ガード処理
        /// </summary>
        /// <param name="baseValue"></param>
        /// <returns></returns>
        private decimal? CoerceValueImpl(object baseValue)
        {
            if (baseValue is not null) {
                decimal value = (decimal)baseValue;
                return Math.Clamp(value, this.MinValue, this.MaxValue);
            }
            return null;
        }

        /// <summary>
        /// <see cref="Scale"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
                nameof(Scale),
                typeof(int),
                typeof(NumericUpDown),
                new FrameworkPropertyMetadata(0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    static (d, e) => ((NumericUpDown)d).ScaleChanged(e),
                    static (d, e) => ((NumericUpDown)d).CoerceScale(e))
            );
        /// <summary>
        /// 小数点以下桁数
        /// </summary>
        public int Scale {
            get => (int)this.GetValue(ScaleProperty);
            set => this.SetValue(ScaleProperty, value);
        }
        /// <summary>
        /// <see cref="Scale"/> 変更時処理
        /// </summary>
        /// <param name="e"></param>
        private void ScaleChanged(DependencyPropertyChangedEventArgs e)
        {
            using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, Log.LogLevel.Trace);

            if (this.mIsLoadedCompleted) {
                bool valid = false;
                int scale = 0;
                if (this.Value is not null) {
                    if (e.NewValue is int newScale) {
                        if (e.OldValue is null) {
                            valid = true;
                            scale = newScale;
                        }
                        else if (e.OldValue is int oldScale && oldScale != newScale) {
                            valid = true;
                            scale = newScale;
                        }
                    }
                }

                if (valid) {
                    Log.Trace($"{nameof(this.Scale)}:{this.Scale}");
                    if (this.Value is null) {
                        this.Session.Text = !this.IsFloating ? this.NullValue.ToString($"F{scale}") : this.NullValue.ToString();
                    }
                    else {
                        this.Session.Text = !this.IsFloating ? this.Value.Value.ToString($"F{scale}") : this.Value.Value.ToString();
                    }
                }
            }
        }
        /// <summary>
        /// <see cref="Scale"/> ガード処理
        /// </summary>
        /// <param name="baseValue"></param>
        /// <returns></returns>
        private int CoerceScale(object baseValue)
        {
            int scale = (int)baseValue;
            return Math.Clamp(scale, 0, 28);
        }

        /// <summary>
        /// <see cref="IsFloating"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty IsFloatingProperty = DependencyProperty.Register(
                nameof(IsFloating),
                typeof(bool),
                typeof(NumericUpDown),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((NumericUpDown)d).IsFloatingChanged(e))
            );
        /// <summary>
        /// 浮動小数点か
        /// </summary>
        public bool IsFloating {
            get => (bool)this.GetValue(IsFloatingProperty);
            set => this.SetValue(IsFloatingProperty, value);
        }
        /// <summary>
        /// <see cref="IsFloating"/> 変更時処理
        /// </summary>
        /// <param name="e"></param>
        private void IsFloatingChanged(DependencyPropertyChangedEventArgs e)
        {
            using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, Log.LogLevel.Trace);

            if (this.mIsLoadedCompleted) {
                if (e.NewValue is bool newIsFloating) {
                    if (this.Value is null) {
                        this.Session.Text = !newIsFloating ? this.NullValue.ToString($"F{this.Scale}") : this.NullValue.ToString();
                    }
                    else {
                        this.Session.Text = !newIsFloating ? this.Value.Value.ToString($"F{this.Scale}") : this.Value.Value.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// <see cref="Stride"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty StrideProperty = DependencyProperty.Register(
                nameof(Stride),
                typeof(decimal),
                typeof(NumericUpDown),
                new PropertyMetadata(1m)
            );
        /// <summary>
        /// 刻み幅
        /// </summary>
        public decimal Stride {
            get => (decimal)this.GetValue(StrideProperty);
            set => this.SetValue(StrideProperty, value);
        }

        /// <summary>
        /// <see cref="MaxValue"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
                nameof(MaxValue),
                typeof(decimal),
                typeof(NumericUpDown),
                new PropertyMetadata(decimal.MaxValue, static (d, e) => ((NumericUpDown)d).MaxValueChanged(e))
            );
        /// <summary>
        /// 最大値
        /// </summary>
        public decimal MaxValue {
            get => (decimal)this.GetValue(MaxValueProperty);
            set => this.SetValue(MaxValueProperty, value);
        }
        /// <summary>
        /// <see cref="MaxValue"/> 変更時処理
        /// </summary>
        /// <param name="e"></param>
        private void MaxValueChanged(DependencyPropertyChangedEventArgs e)
        {
            using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, Log.LogLevel.Trace);

            if (this.mIsLoadedCompleted) {
                // 値を範囲内に制限する
                if (e.NewValue is decimal newMaxValue) {
                    if (this.Value is not null) {
                        this.Value = Math.Clamp(this.Value.Value, this.MinValue, newMaxValue);
                    }
                }
            }
        }

        /// <summary>
        /// <see cref="MinValue"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
                nameof(MinValue),
                typeof(decimal),
                typeof(NumericUpDown),
                new PropertyMetadata(0m, static (d, e) => ((NumericUpDown)d).MinValueChanged(e))
            );
        /// <summary>
        /// 最小値
        /// </summary>
        public decimal MinValue {
            get => (decimal)this.GetValue(MinValueProperty);
            set => this.SetValue(MinValueProperty, value);
        }
        /// <summary>
        /// <see cref="MinValue"/> 変更時処理
        /// </summary>
        /// <param name="e"></param>
        private void MinValueChanged(DependencyPropertyChangedEventArgs e)
        {
            using FuncLog funcLog = new(new { e.OldValue, e.NewValue }, Log.LogLevel.Trace);

            if (this.mIsLoadedCompleted) {
                // 値を範囲内に制限する
                if (e.NewValue is decimal newMinValue) {
                    if (this.Value is not null) {
                        this.Value = Math.Clamp(this.Value.Value, newMinValue, this.MaxValue);
                    }
                }
            }
        }

        /// <summary>
        /// <see cref="NullValue"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty NullValueProperty = DependencyProperty.Register(
                nameof(NullValue),
                typeof(decimal),
                typeof(NumericUpDown),
                new PropertyMetadata(0m)
            );
        /// <summary>
        /// 未設定時の内部値
        /// </summary>
        public decimal NullValue {
            get => (decimal)this.GetValue(NullValueProperty);
            set => this.SetValue(NullValueProperty, value);
        }

        /// <summary>
        /// <see cref="IsMouseWheelEnabled"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty IsMouseWheelEnabledProperty = DependencyProperty.Register(
                nameof(IsMouseWheelEnabled),
                typeof(bool),
                typeof(NumericUpDown),
                new PropertyMetadata(true)
            );
        /// <summary>
        /// マウスホイールが有効か
        /// </summary>
        public bool IsMouseWheelEnabled {
            get => (bool)this.GetValue(IsMouseWheelEnabledProperty);
            set => this.SetValue(IsMouseWheelEnabledProperty, value);
        }

        /// <summary>
        /// <see cref="IsPopupEnabled"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty IsPopupEnabledProperty = DependencyProperty.Register(
                nameof(IsPopupEnabled),
                typeof(bool),
                typeof(NumericUpDown),
                new PropertyMetadata(true)
            );
        /// <summary>
        /// 内部 <see cref="FollowablePopup"/> を使用するか
        /// </summary>
        public bool IsPopupEnabled {
            get => (bool)this.GetValue(IsPopupEnabledProperty);
            set => this.SetValue(IsPopupEnabledProperty, value);
        }
        #endregion

        #region イベント
        /// <summary>
        /// プロパティ変更時イベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region フィールド
        /// <summary>
        /// IsOpenの瞬時値
        /// </summary>
        private bool mLocalIsOpen;
        /// <summary>
        /// 読込完了済か
        /// </summary>
        private bool mIsLoadedCompleted;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 数値入力ボタンの入力値
        /// </summary>
        public int? InputedNumber {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// 数値入力ボタンの入力種別
        /// </summary>
        public NumericInputButton.InputKind InputedKind {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> の表示状態
        /// </summary>
        public bool IsOpen {
            get;
            set => this.SetProperty(ref field, value);
        }

        /// <summary>
        /// <see cref="NumericUpDown"/> のフォーカスの状態
        /// </summary>
        public bool NumericUpDownFocused {
            get;
            set {
                Log.Trace($"{nameof(this.NumericUpDownFocused)}:{value}");
                field = value;
                this.UpdateIsOpen();
            }
        }

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> のフォーカスの状態
        /// </summary>
        public bool PopupFocused {
            get;
            set {
                Log.Trace($"{nameof(this.PopupFocused)}:{value}");
                field = value;
                this.UpdateIsOpen();
            }
        }
        #endregion

        #region コマンド
        /// <summary>
        /// 増加コマンド
        /// </summary>
        public ICommand IncreaseCommand => field ??= new RelayCommand(this.IncreaseNumber, () => !this.Value.HasValue || this.Value < this.MaxValue);
        /// <summary>
        /// 減少コマンド
        /// </summary>
        public ICommand DecreaseCommand => field ??= new RelayCommand(this.DecreaseNumber, () => !this.Value.HasValue || this.Value > this.MinValue);
        /// <summary>
        /// 入力コマンド
        /// </summary>
        public ICommand InputCommand => field ??= new RelayCommand(this.ButtonInputCommand_Executed);
        /// <summary>
        /// 入力コマンド処理
        /// </summary>
        private void ButtonInputCommand_Executed() => this.ButtonInputed(this.InputedNumber, this.InputedKind);
        #endregion

        /// <summary>
        /// <see cref="NumericUpDown"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public NumericUpDown()
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);

            this.InitializeComponent();
        }

        #region イベントハンドラ
        /// <summary>
        /// コントロールの読込が完了したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown_Loaded(object sender, RoutedEventArgs e)
        {
            using FuncLog funcLog = new(new { this.Session, this.Value, this.Scale, this.MaxValue, this.MinValue }, Log.LogLevel.Trace);

            // Session が未初期化のとき、Value の指定があればそれを初期表示、なければ NullValue を初期表示
            if (this.Session is null) {
                this.Session = new();
                if (this.Value is null) {
                    this.Session.Text = !this.IsFloating ? this.NullValue.ToString($"F{this.Scale}") : this.NullValue.ToString();
                    if (this.NullValue == 0m) { // 初期表示値が0なら、0の次の位置を選択する
                        this.Session.SelectionStart = 1;
                    }
                }
                else {
                    this.Session.Text = !this.IsFloating ? this.Value.Value.ToString($"F{this.Scale}") : this.Value.Value.ToString();
                    if (this.Value.Value == 0m) { // 初期表示値が0なら、0の次の位置を選択する
                        this.Session.SelectionStart = 1;
                    }
                }
            }

            // TextBox の選択位置を更新する
            this._textBox.SelectionStart = this.Session.SelectionStart;
            this._textBox.SelectionLength = this.Session.SelectionLength;

            this._textBox.SelectionChanged += this.TextBox_SelectionChanged;
            this._textBox.TextChanged += this.TextBox_TextChanged;

            this.mIsLoadedCompleted = true;
        }
        /// <summary>
        /// コントロールの破棄前のとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown_Unloaded(object sender, RoutedEventArgs e)
        {
            using FuncLog funcLog = new(new { this.Session, this.Value, this.Scale, this.MaxValue, this.MinValue }, Log.LogLevel.Trace);

            // TextBox の選択位置を保存する
            this.Session.SelectionStart = this._textBox.SelectionStart;
            this.Session.SelectionLength = this._textBox.SelectionLength;
        }

        /// <summary>
        /// コントロールからフォーカスが外れたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown_LostFocus(object sender, RoutedEventArgs e)
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);
            this.NumericUpDownFocused = false;
        }

        /// <summary>
        /// コントロールがフォーカスを得たとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown_GotFocus(object sender, RoutedEventArgs e)
        {
            using FuncLog funcLog = new(new { }, Log.LogLevel.Trace);
            this.NumericUpDownFocused = true;
        }

        /// <summary>
        /// テキストボックスにテキストが入力される前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            using FuncLog funcLog = new(new { this.Session }, Log.LogLevel.Trace);

            // 既存のテキストボックス文字列に、新規に一文字追加された時、その文字列が数値として意味があるかどうかをチェック
            int selectionStart = this.Session.SelectionStart;
            int selectionEnd = selectionStart + this.Session.SelectionLength;
            string forwardText = this.Session.Text[..selectionStart];
            string backwardText = this.Session.Text[selectionEnd..];

            string nextText = forwardText + e.Text + backwardText;

            // 更新したい場合は false, 更新したくない場合は true
            e.Handled = !this.TryParse(nextText, out decimal _);
        }

        /// <summary>
        /// テキストボックスのテキストが削除される前に妥当かどうか検証する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            using FuncLog funcLog = new(new { this.Session }, Log.LogLevel.Trace);
            bool yes_parse = true;

            switch (e.Key) {
                case Key.Back: {
                    string nextText = string.Empty;
                    if (this.Session.SelectionLength > 0) {
                        // 選択範囲を削除
                        nextText = this.Session.Text.Remove(this.Session.SelectionStart, this.Session.SelectionLength);
                    }
                    else if (this.Session.SelectionStart > 0) {
                        // カーソル位置の1文字前を削除
                        nextText = this.Session.Text.Remove(this.Session.SelectionStart - 1, 1);
                    }
                    yes_parse = this.TryParse(nextText, out decimal _);
                    break;
                }
                case Key.Delete: {
                    string nextText = string.Empty;
                    if (this.Session.SelectionLength > 0) {
                        // 選択範囲を削除
                        nextText = this.Session.Text.Remove(this.Session.SelectionStart, this.Session.SelectionLength);
                    }
                    else if (this.Session.SelectionStart < this.Session.Text.Length) {
                        // カーソル位置の1文字を削除
                        nextText = this.Session.Text.Remove(this.Session.SelectionStart, 1);
                    }
                    yes_parse = this.TryParse(nextText, out decimal _);
                    break;
                }
            }
            e.Handled = !yes_parse;
        }

        /// <summary>
        /// テキストボックスで選択を変更したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox) {
                using FuncLog funcLog = new(new { textBox.SelectionStart, textBox.SelectionLength }, Log.LogLevel.Trace);

                this.Session.SelectionStart = textBox.SelectionStart;
                this.Session.SelectionLength = textBox.SelectionLength;
            }
        }

        /// <summary>
        /// テキストボックスのテキストが確定したとき変換可能なら値を更新する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            using FuncLog funcLog = new(new { this.Session.Text });

            this.CommitText(this.Session.Text, this._textBox.SelectionStart);
        }

        /// <summary>
        /// テキストボックスでホイールが回転したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!this.IsMouseWheelEnabled) { return; }

            using FuncLog funcLog = new();

            // 方向に合わせて数字を増減する
            if (e.Delta > 0) {
                this.IncreaseNumber();
            }
            else if (e.Delta < 0) {
                this.DecreaseNumber();
            }
        }

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> からフォーカスが外れたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Popup_LostFocus(object sender, RoutedEventArgs e)
        {
            using FuncLog funcLog = new();
            this.PopupFocused = false;
        }

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> がフォーカスを得たとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Popup_GotFocus(object sender, RoutedEventArgs e)
        {
            using FuncLog funcLog = new();
            this.PopupFocused = true;
        }
        #endregion

        /// <summary>
        /// ボタンによる入力を処理する
        /// </summary>
        /// <param name="inputedValue">入力された値</param>
        /// <param name="inputedKind">入力の種類</param>
        /// <remarks>DataGrid内に埋め込みの場合は、IsLoaded=falseで呼ばれる</remarks>
        public void ButtonInputed(int? inputedValue, NumericInputButton.InputKind inputedKind)
        {
            using FuncLog funcLog = new(new { inputedValue, inputedKind }, Log.LogLevel.Trace);

            if (this.mIsLoadedCompleted) {
                this.Session.SelectionStart = this._textBox.SelectionStart;
                this.Session.SelectionLength = this._textBox.SelectionLength;
            }

            decimal? oldValue = this.Value; // 変更前の数字
            int oldSelectionStart = this.Session.SelectionStart; // 選択開始位置
            int newSelectionStart = oldSelectionStart;
            int oldSelectionLength = this.Session.SelectionLength; // 選択長
            int oldSelectionEnd = oldSelectionStart + this.Session.SelectionLength;
            string newText = this.Session.Text; // 変更後のテキスト

            string forwardText = this.Session.Text[..oldSelectionStart]; // 選択している箇所より前
            string backwardText = this.Session.Text[oldSelectionEnd..]; // 選択している箇所より後

            // 入力に合わせてテキストを編集する(Closeを除く)
            switch (inputedKind) {
                case NumericInputButton.InputKind.Number: {
                    Log.Trace($"Forward:{forwardText} Backward:{backwardText}");

                    newText = string.Format($"{forwardText}{inputedValue}{backwardText}"); // 選択している箇所を入力値に置き換え
                    break;
                }
                case NumericInputButton.InputKind.BackSpace: {
                    Log.Trace($"Forward:{forwardText} Backward:{backwardText}");

                    if (oldSelectionLength != 0) { // 選択しているテキストが存在するなら削除する
                        newText = string.Format($"{forwardText}{backwardText}");
                        newSelectionStart = oldSelectionStart;
                    }
                    else if (oldSelectionStart != 0) { // 選択しているテキストが存在しないなら直前の1文字を削除する
                        newText = string.Format($"{forwardText[..(oldSelectionStart - 1)]}{backwardText}");
                        newSelectionStart = oldSelectionStart - 1;
                    }
                    break;
                }
                case NumericInputButton.InputKind.Clear:
                    newText = string.Empty;
                    newSelectionStart = 0;
                    break;
                case NumericInputButton.InputKind.Close:
                    this.NumericUpDownFocused = false;
                    this.IsOpen = false;
                    break;
            }

            if (inputedKind != NumericInputButton.InputKind.Close) {
                this.CommitText(newText, newSelectionStart);

                _ = this._textBox.Focus();
            }
        }

        /// <summary>
        /// 入力テキストを確定する
        /// </summary>
        /// <param name="tmpText">入力テキスト</param>
        /// <param name="tmpSelectionStart">選択位置</param>
        private void CommitText(string tmpText, int tmpSelectionStart)
        {
            using FuncLog funcLog = new(new { tmpText, tmpSelectionStart }, Log.LogLevel.Trace);

            string text = tmpText;
            int selectionStart = tmpSelectionStart;

            if (text == string.Empty) {
                text = !this.IsFloating ? this.NullValue.ToString($"N{this.Scale}") : this.NullValue.ToString();
            }

            if (this.TryParse(text, out decimal value)) {
                // 選択箇所が小数点より前なら選択位置を入力文字の次にする
                int oldPointIndex = tmpText.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                if (oldPointIndex == -1) { oldPointIndex = text.Length; }
                if (selectionStart < oldPointIndex) {
                    selectionStart = selectionStart + 1;
                }
                // 先頭が0で1以上の数なら先頭の0を削除
                if (1 <= text.Length && text[0] == '0' && 1 <= value) {
                    text = text[1..];
                    selectionStart = selectionStart - 1;
                }
                // 2文字目が0で-1以下の数なら2文字目の0を削除
                if (2 <= text.Length && text[0] == '-' && text[1] == '0' && value <= -1) {
                    text = '-' + text[2..];
                    selectionStart = selectionStart - 1;
                }
                if (!this.IsFloating) {
                    // 小数点以下桁数を超える範囲は削除
                    int newPointIndex = text.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    if (newPointIndex == -1) { newPointIndex = text.Length; }
                    if (this.Scale < text.Length - newPointIndex - 1) {
                        text = text[..(newPointIndex + this.Scale + 1)];
                    }
                }

                this.Value = !this.IsFloating ? decimal.Round(value, this.Scale) : value;
                this.Session.Text = text;
                this.Session.SelectionStart = value != 0m ? selectionStart : 1;
                this.Session.SelectionLength = 0;

                if (this.mIsLoadedCompleted) {
                    this._textBox.SelectionStart = this.Session.SelectionStart;
                    this._textBox.SelectionLength = this.Session.SelectionLength;
                }

                funcLog.Returns = new { this.Session, this.Value };
            }
        }

        /// <summary>
        /// 数値として変換可能か
        /// </summary>
        /// <param name="nextText">新規テキスト</param>
        /// <returns>変換可能か</returns>
        private bool TryParse(string nextText, out decimal value)
        {
            using FuncLog funcLog = new(new { nextText }, Log.LogLevel.Trace);

            bool yes_parse;

            int pointIndex = nextText.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            if (pointIndex != -1) {
                // 小数点以下の桁数が0の場合は、小数点入力不可
                if (!this.IsFloating && this.Scale == 0) {
                    value = 0;
                    return false;
                }

                // 小数点を2つ入力は不可
                if (nextText.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, pointIndex + 1) != -1) {
                    value = 0;
                    return false;
                }
            }

            if (nextText == string.Empty) {
                yes_parse = true;
                value = this.NullValue;
            }
            else {
                yes_parse = decimal.TryParse(nextText, out decimal nextValue);
                value = nextValue;

                // 範囲内かどうかチェック
                if (yes_parse) {
                    if (nextValue < this.MinValue || this.MaxValue < nextValue) {
                        yes_parse = false;
                    }
                }
            }
            return yes_parse;
        }

        /// <summary>
        /// 数値をインクリメントする
        /// </summary>
        private void IncreaseNumber()
        {
            if (this.Value is not null) {
                decimal tmpValue = Math.Min(this.Value.Value + this.Stride, this.MaxValue);
                this.Value = !this.IsFloating ? decimal.Round(tmpValue, this.Scale) : tmpValue;
            }
            else {
                this.Value = this.NullValue;
            }
        }

        /// <summary>
        /// 数値をデクリメントする
        /// </summary>
        private void DecreaseNumber()
        {
            if (this.Value is not null) {
                decimal tmpValue = Math.Max(this.Value.Value - this.Stride, this.MinValue);
                this.Value = !this.IsFloating ? decimal.Round(tmpValue, this.Scale) : tmpValue;
            }
            else {
                this.Value = this.NullValue;
            }
        }

        /// <summary>
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> の表示状態を更新する
        /// </summary>
        private async void UpdateIsOpen()
        {
            this.mLocalIsOpen = this.NumericUpDownFocused || this.PopupFocused;

            // true の場合は即座に更新し、false の場合は遅延して反映させる
            if (this.mLocalIsOpen) {
                this.IsOpen = true;
            }
            else {
                await Task.Delay(50);
                this.IsOpen = this.mLocalIsOpen || false;
            }
            Log.Trace($"{nameof(this.IsOpen)}:{this.IsOpen}");
        }

        /// <summary>
        /// フィールドの値を変更し、必要に応じて変更通知を発行する
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">フィールドへの参照</param>
        /// <param name="value">新しい値</param>
        /// <param name="propertyName">変更されたプロパティの名前。省略時は呼び出し元の名前を自動取得</param>
        /// <returns>値が変更された場合はtrue、変更がなければfalse</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) { return false; }

            field = value;
            this.PropertyChanged?.Invoke(this, new(propertyName));

            return true;
        }
    }
}
