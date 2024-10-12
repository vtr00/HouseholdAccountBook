using System.Collections.Generic;
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
        private static readonly Dictionary<FrameworkElement, int> _countMap = new Dictionary<FrameworkElement, int>();

        /// <summary>
        /// <see cref="Cursors.Wait"/> のカウンタを増やす
        /// </summary>
        /// <param name="fe"></param>
        public static void WaitCursorCountIncrement(this FrameworkElement fe)
        {
            _mutex.WaitOne();
            if (!_countMap.ContainsKey(fe)) {
                _countMap.Add(fe, 0);
                fe.Cursor = Cursors.Wait;
            }
            _countMap[fe]++;

            Log.Debug(string.Format("WaitCounter increment: {0}", _countMap[fe]));
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
            if (_countMap.ContainsKey(fe)) {
                Log.Debug(string.Format("WaitCounter decrement: {0}", _countMap[fe]-1));

                _countMap[fe]--;
                if (_countMap[fe] <= 0) {
                    fe.Cursor = null;
                    _countMap.Remove(fe);
                }
            }
            _mutex.ReleaseMutex();
        }
    }
}
