using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HouseholdAccountBook.UserControls
{
    /// <summary>
    /// NumericInputButton.xaml の相互作用ロジック
    /// </summary>
    public partial class NumericInputButton : UserControl, ICommandSource
    {
        #region 依存関係プロパティ
        /// <summary>
        /// <see cref="InputedValue"/> 依存関係プロパティを識別します。
        /// </summary>
        #region InputedValueProperty
        public static readonly DependencyProperty InputedValueProperty = DependencyProperty.Register(
                nameof(InputedValue),
                typeof(int?),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );
        #endregion
        /// <summary>
        /// 入力値
        /// </summary>
        #region InputedValue
        public int? InputedValue
        {
            get => (int?)this.GetValue(InputedValueProperty);
            set => this.SetValue(InputedValueProperty, value);
        }
        #endregion

        /// <summary>
        /// <see cref="InputedKind"/> 依存関係プロパティを識別します。
        /// </summary>
        #region InputedKindProperty
        public static readonly DependencyProperty InputedKindProperty = DependencyProperty.Register(
                nameof(InputedKind),
                typeof(InputKind),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(InputKind.Unputed, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );
        #endregion
        /// <summary>
        /// 入力種別
        /// </summary>
        #region InputedKind
        public InputKind InputedKind
        {
            get => (InputKind)this.GetValue(InputedKindProperty);
            set => this.SetValue(InputedKindProperty, value);
        }
        #endregion

        /// <summary>
        /// <see cref="Command"/> 依存関係プロパティを識別します。
        /// </summary>
        #region CommandProperty
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(CommandChanged))
            );
        #endregion
        /// <summary>
        /// コマンド
        /// </summary>
        #region Command
        public ICommand Command
        {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }
        #endregion

        /// <summary>
        /// <see cref="CommandParameter"/> 依存関係プロパティを識別します。
        /// </summary>
        #region CommandParameterProperty
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
                nameof(CommandParameter),
                typeof(object),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None)
            );
        #endregion
        /// <summary>
        /// コマンドパラメータ
        /// </summary>
        #region CommandParameter
        public object CommandParameter
        {
            get => this.GetValue(CommandParameterProperty);
            set => this.SetValue(CommandParameterProperty, value);
        }
        #endregion

        /// <summary>
        /// <see cref="CommandTarget"/> 依存関係プロパティを識別します。
        /// </summary>
        #region CommandTargetProperty
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
                nameof(CommandTarget),
                typeof(IInputElement),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None)
            );
        #endregion
        /// <summary>
        /// コマンドターゲット
        /// </summary>
        #region CommandTarget
        public IInputElement CommandTarget
        {
            get => (IInputElement)this.GetValue(CommandTargetProperty);
            set => this.SetValue(CommandTargetProperty, value);
        }
        #endregion
        #endregion

        /// <summary>
        /// <see cref="NumericInputButton"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public NumericInputButton()
        {
            this.InitializeComponent();
        }

        #region イベントハンドラ
        #region コマンド
        /// <summary>
        /// 数字ボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberInputCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ContentControl control = e.OriginalSource as ContentControl;
            int value = Int32.Parse(control.Content.ToString()); // 入力値

            this.InputedValue = value;
            this.InputedKind = InputKind.Number;
            this.CallCommandToExecute();

            e.Handled = true;
        }

        /// <summary>
        /// BackSpaceボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackSpaceInputCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.InputedKind = InputKind.BackSpace;
            this.CallCommandToExecute();

            e.Handled = true;
        }

        /// <summary>
        /// Clearボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.InputedKind = InputKind.Clear;
            this.CallCommandToExecute();

            e.Handled = true;
        }
        #endregion

        /// <summary>
        /// コマンドプロパティ変更時イベント
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void CommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericInputButton nib = (NumericInputButton)d;
            nib.HookUpCommand((ICommand)e.OldValue, (ICommand)e.NewValue);
        }

        /// <summary>
        /// 実行の可不可が変更になったときにコントロールを無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            if (this.Command != null) {
                if (this.Command is RoutedCommand command) {
                    if (command.CanExecute(this.CommandParameter, this.CommandTarget)) {
                        this.IsEnabled = true;
                    }
                    else {
                        this.IsEnabled = false;
                    }
                }
                else {
                    if (this.Command.CanExecute(this.CommandParameter)) {
                        this.IsEnabled = true;
                    }
                    else {
                        this.IsEnabled = false;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// コマンドプロパティ変更時処理
        /// </summary>
        /// <param name="oldCommand">変更前のコマンド</param>
        /// <param name="newCommand">変更後のコマンド</param>
        private void HookUpCommand(ICommand oldCommand, ICommand newCommand)
        {
            if (oldCommand != null) {
                oldCommand.CanExecuteChanged -= this.OnCanExecuteChanged;
            }
            if (newCommand != null) {
                newCommand.CanExecuteChanged += this.OnCanExecuteChanged;
            }
        }

        /// <summary>
        /// 入力があったときにコマンドを実行する
        /// </summary>
        private void CallCommandToExecute()
        {
            if (this.Command != null) {
                if (this.Command is RoutedCommand command) {
                    command.Execute(this.CommandParameter, this.CommandTarget);
                }
                else {
                    this.Command.Execute(this.CommandParameter);
                }
            }
        }
    }
}
