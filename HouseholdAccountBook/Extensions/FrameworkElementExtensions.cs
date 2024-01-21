using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.Extensions
{
    /// <summary>
    /// <see cref="FrameworkElement"/> の拡張メソッドを提供します
    /// </summary>
    public static class FrameworkElementExtensions
    {
        private static readonly Mutex _mutex = new Mutex(false);
        private static int _count = 0;

        /// <summary>
        /// <see cref="Cursors.Wait"/> のカウンタを増やす
        /// </summary>
        /// <param name="fe"></param>
        public static void WaitCursorCountIncrement(this FrameworkElement fe)
        {
            _mutex.WaitOne();
            _count++;
            fe.Cursor = Cursors.Wait;
            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// <see cref="Cursors.Wait"/> のカウンタを減らす
        /// </summary>
        /// <param name="fe"></param>
        /// <remarks>カウンタが0になったら<see cref="null"/>に戻す</remarks>
        public static void WaitCursorCountDecrement(this FrameworkElement fe)
        {
            _mutex.WaitOne();
            _count--;
            if (_count == 0) {
                fe.Cursor = null;
            }
            _mutex.ReleaseMutex();
        }
    }
}
