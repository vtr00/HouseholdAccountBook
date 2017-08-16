using System;
using System.ComponentModel;
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
        public static readonly DependencyProperty InputedValueProperty = DependencyProperty.Register(
                PropertyName<NumericInputButton>.Get(x => x.InputedValue),
                typeof(int?),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );
        /// <summary>
        /// 入力値
        /// </summary>
        public int? InputedValue
        {
            get { return (int?)GetValue(InputedValueProperty); }
            set { SetValue(InputedValueProperty, value); }
        }

        /// <summary>
        /// <see cref="InputedKind"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty InputedKindProperty = DependencyProperty.Register(
                PropertyName<NumericInputButton>.Get(x => x.InputedKind),
                typeof(InputKind),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(InputKind.Unputed, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );
        /// <summary>
        /// 入力種別
        /// </summary>
        public InputKind InputedKind
        {
            get { return (InputKind)GetValue(InputedKindProperty); }
            set { SetValue(InputedKindProperty, value); }
        }

        /// <summary>
        /// <see cref="Command"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
                PropertyName<NumericInputButton>.Get(x => x.Command),
                typeof(ICommand),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(CommandChanged))
            );
        /// <summary>
        /// コマンド
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// <see cref="CommandParameter"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
                PropertyName<NumericInputButton>.Get(x => x.CommandParameter),
                typeof(object),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None)
            );
        /// <summary>
        /// コマンドパラメータ
        /// </summary>
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// <see cref="CommandTarget"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
                PropertyName<NumericInputButton>.Get(x => x.CommandTarget),
                typeof(IInputElement),
                typeof(NumericInputButton),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None)
            );
        /// <summary>
        /// コマンドターゲット
        /// </summary>
        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }
        #endregion

        /// <summary>
        /// <see cref="NumericInputButton"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public NumericInputButton()
        {
            InitializeComponent();
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
            CallCommandToExecute();

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
            CallCommandToExecute();

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
            CallCommandToExecute();

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
        private void OnCanExecuteChanged (object sender, EventArgs e)
        {
            if(this.Command != null) {
                if (this.Command is RoutedCommand command) {
                    if (command.CanExecute(CommandParameter, CommandTarget)) {
                        this.IsEnabled = true;
                    }
                    else {
                        this.IsEnabled = false;
                    }
                }
                else {
                    if (Command.CanExecute(CommandParameter)) {
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
        private void HookUpCommand (ICommand oldCommand, ICommand newCommand)
        {
            if(oldCommand != null) {
                oldCommand.CanExecuteChanged -= OnCanExecuteChanged;
            }
            if(newCommand != null) {
                newCommand.CanExecuteChanged += OnCanExecuteChanged;
            }
        }

        /// <summary>
        /// 入力があったときにコマンドを実行する
        /// </summary>
        private void CallCommandToExecute()
        {
            if(this.Command != null) {
                if(this.Command is RoutedCommand command) {
                    command.Execute(CommandParameter, CommandTarget);
                }
                else {
                    this.Command.Execute(CommandParameter);
                }
            }
        }
    }
}
