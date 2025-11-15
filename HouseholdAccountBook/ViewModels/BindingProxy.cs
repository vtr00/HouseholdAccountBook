using System.Windows;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// ViewModelのバインディングソースの代理として働くクラスです。
    /// </summary>
    /// <remarks>参考：http://iyemon018.hatenablog.com/entry/2016/01/30/235351 </remarks>
    public class BindingProxy : Freezable
    {
        #region 依存関係プロパティ
        /// <summary>
        /// <see cref="DataContext"/> 依存関係プロパティを識別します。
        /// </summary>
        #region DataContextProperty
        public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register(
                nameof(DataContext),
                typeof(object),
                typeof(BindingProxy),
                new UIPropertyMetadata(null)
            );
        #endregion

        /// <summary>
        /// 間をとりもつプロパティ
        /// データバインドした場合は、このプロパティがViewModelの代わりになる。
        /// </summary>
        #region DataContext
        public object DataContext {
            get => this.GetValue(DataContextProperty);
            set => this.SetValue(DataContextProperty, value);
        }
        #endregion
        #endregion

        /// <summary>
        /// <see cref="Freezable"/> クラスのインスタンスを生成します。
        /// </summary>
        /// <returns>インスタンス</returns>
        protected override Freezable CreateInstanceCore() => new BindingProxy();
    }
}
