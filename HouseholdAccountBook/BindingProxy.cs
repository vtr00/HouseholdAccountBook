using System.Windows;

namespace HouseholdAccountBook
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
        public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register(
                PropertyName<BindingProxy>.Get(x => x.DataContext), 
                typeof(object), 
                typeof(BindingProxy), 
                new UIPropertyMetadata(null)
            );

        /// <summary>
        /// 間をとりもつプロパティ
        /// データバインドした場合は、このプロパティがViewModelの代わりになる。
        /// </summary>
        public object DataContext
        {
            get { return GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }
        #endregion

        /// <summary>
        /// <see cref="Freezable"/> クラスのインスタンスを生成します。
        /// </summary>
        /// <returns>インスタンス</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}
