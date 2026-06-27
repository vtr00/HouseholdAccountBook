using HouseholdAccountBook.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HouseholdAccountBook.Views.UserControls
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
        public static readonly DependencyProperty InputedValueProperty = DependencyProperty.Register(
                nameof(InputedValue),
                typeof(int?),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );
        /// <summary>
        /// 入力値
        /// </summary>
        public int? InputedValue {
            get => (int?)this.GetValue(InputedValueProperty);
            set => this.SetValue(InputedValueProperty, value);
        }

        /// <summary>
        /// <see cref="InputedKind"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty InputedKindProperty = DependencyProperty.Register(
                nameof(InputedKind),
                typeof(InputKind),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(InputKind.Unputed, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );
        /// <summary>
        /// 入力種別
        /// </summary>
        public InputKind InputedKind {
            get => (InputKind)this.GetValue(InputedKindProperty);
            set => this.SetValue(InputedKindProperty, value);
        }

        /// <summary>
        /// <see cref="Command"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, static (d, e) => ((NumericInputButton)d).CommandChanged(e))
            );
        /// <summary>
        /// コマンド
        /// </summary>
        public ICommand Command {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }
        /// <summary>
        /// コマンドプロパティ変更時イベント
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private void CommandChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is not null and ICommand oldCommand) {
                oldCommand.CanExecuteChanged -= this.CanExecuteChanged;
            }
            if (e.NewValue is not null and ICommand newCommand) {
                newCommand.CanExecuteChanged += this.CanExecuteChanged;
            }
        }
        /// <summary>
        /// 実行の可不可が変更になったときにコントロールを無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanExecuteChanged(object sender, EventArgs e)
        {
            if (this.Command != null) {
                this.IsEnabled = this.Command is RoutedCommand command
                    ? command?.CanExecute(this.CommandParameter, this.CommandTarget) ?? true
                    : this.Command?.CanExecute(this.CommandParameter) ?? true;
            }
        }

        /// <summary>
        /// <see cref="CommandParameter"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
                nameof(CommandParameter),
                typeof(object),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None)
            );
        /// <summary>
        /// コマンドパラメータ
        /// </summary>
        public object CommandParameter {
            get => this.GetValue(CommandParameterProperty);
            set => this.SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// <see cref="CommandTarget"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
                nameof(CommandTarget),
                typeof(IInputElement),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None)
            );
        /// <summary>
        /// コマンドターゲット
        /// </summary>
        public IInputElement CommandTarget {
            get => (IInputElement)this.GetValue(CommandTargetProperty);
            set => this.SetValue(CommandTargetProperty, value);
        }
        #endregion

        #region コマンド
        /// <summary>
        /// 数字ボタンコマンド
        /// </summary>
        public ICommand NumberInputCommand => field ??= new RelayCommand<string>(this.NumberInputCommand_Execute);
        /// <summary>
        /// 数字ボタン押下時
        /// </summary>
        /// <param name="value">押されたボタン</param>
        private void NumberInputCommand_Execute(string value)
        {
            if (int.TryParse(value, out int ret)) {
                this.InputedValue = ret;
            }
            else {
                this.InputedValue = null;
            }
            this.InputedKind = InputKind.Number;
            this.ExecuteCommand();
        }

        /// <summary>
        /// BackSpaceコマンド
        /// </summary>
        public ICommand BackSpaceInputCommand => field ??= new RelayCommand(this.BackSpaceInputCommand_Execute);
        /// <summary>
        /// BackSpaceボタン押下時
        /// </summary>
        private void BackSpaceInputCommand_Execute()
        {
            this.InputedKind = InputKind.BackSpace;
            this.ExecuteCommand();
        }

        /// <summary>
        /// Clearコマンド
        /// </summary>
        public ICommand ClearCommand => field ??= new RelayCommand(this.ClearCommand_Execute);
        /// <summary>
        /// Clearボタン押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearCommand_Execute()
        {
            this.InputedKind = InputKind.Clear;
            this.ExecuteCommand();
        }

        /// <summary>
        /// Closeコマンド
        /// </summary>
        public ICommand CloseCommand => field ??= new RelayCommand(this.CloseCommand_Execute);
        /// <summary>
        /// Closeボタン押下時
        /// </summary>
        private void CloseCommand_Execute()
        {
            this.InputedKind = InputKind.Close;
            this.ExecuteCommand();
        }
        #endregion

        /// <summary>
        /// <see cref="NumericInputButton"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public NumericInputButton() => this.InitializeComponent();

        /// <summary>
        /// コマンドを実行する
        /// </summary>
        private void ExecuteCommand()
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
