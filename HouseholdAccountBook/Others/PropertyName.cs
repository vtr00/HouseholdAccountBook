using System;
using System.Linq.Expressions;
using System.Windows;

namespace HouseholdAccountBook
{
    /// <summary>
    /// x => x.X みたいな <see cref="Expression"/> から "X" というプロパティ名を取り出すためのヘルパークラス。
    /// <see cref="DependencyProperty"/> の実装補助に使う。
    /// </summary>
    /// <typeparam name="TInstance">インスタンスの型</typeparam>
    public static class PropertyName<TInstance>
    {
        /// <summary>
        /// <see cref="Expression"/> からプロパティ名を取り出す。
        /// </summary>
        /// <typeparam name="TMember">プロパティの型。</typeparam>
        /// <param name="propertyExpression">取り出し元の式木。</param>
        /// <returns>取りだしたプロパティ名。</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// class SomeType
        /// {
        ///     double X { set; get; }
        ///     string _nameX = PropertyName<SomeType>.Get(x => x.X);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static string Get<TMember>(Expression<Func<TInstance, TMember>> propertyExpression)
        {
            if (propertyExpression.Body is not MemberExpression memberExp) {
                throw new ArgumentException();
            }

            return memberExp.Member.Name;
        }
    }
}
