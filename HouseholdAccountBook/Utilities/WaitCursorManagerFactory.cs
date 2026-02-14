using System.Runtime.CompilerServices;
using System.Windows;

namespace HouseholdAccountBook.Utilities
{
    /// <summary>
    /// <see cref="WaitCursorManager"/> のファクトリ
    /// </summary>
    /// <param name="fe">生成元インスタンス</param>
    public class WaitCursorManagerFactory(FrameworkElement fe)
    {
        /// <summary>
        /// 生成元インスタンス
        /// </summary>
        private readonly FrameworkElement mFe = fe;

        /// <summary>
        /// <see cref="WaitCursorManager"/> を生成する
        /// </summary>
        /// <param name="fileName">生成元ファイル名</param>
        /// <param name="methodName">生成元関数名</param>
        /// <param name="lineNumber">生成元行数</param>
        /// <returns></returns>
        public WaitCursorManager Create([CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
            => new(this.mFe, fileName, methodName, lineNumber);
    }
}
