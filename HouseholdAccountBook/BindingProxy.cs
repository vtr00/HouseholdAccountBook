using System.Windows;

namespace HouseholdAccountBook
{
    /// <summary>
    /// ViewModelのバインディングソースの代理として働くクラスです。
    /// </summary>
    /// <remarks>参考：http://iyemon018.hatenablog.com/entry/2016/01/30/235351 </remarks>
    public class BindingProxy : Freezable
    {
        /// <summary>
        /// Freezableオブジェクトのインスタンスを生成します。
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        /// <summary>
        /// Data の依存関係プロパティ定義
        /// </summary>
        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register(
                PropertyName<BindingProxy>.Get(x => x.DataContext), 
                typeof(object), typeof(BindingProxy), 
                new UIPropertyMetadata(null));

        /// <summary>
        /// 間をとりもつプロパティ
        /// データバインドした場合は、このプロパティがViewModelの代わりになる。
        /// </summary>
        public object DataContext
        {
            get { return (object)GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }
    }
}
