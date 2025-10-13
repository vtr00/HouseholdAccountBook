using HouseholdAccountBook.Adapters.Logger;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.Others
{
    /// <summary>
    /// <see cref="WaitCursorManager"/> のファクトリ
    /// </summary>
    public class WaitCursorManagerFactory(FrameworkElement fe)
    {
        private readonly FrameworkElement fe = fe;

        /// <summary>
        /// <see cref="WaitCursorManager"/> を生成する
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public WaitCursorManager Create([CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            return new WaitCursorManager(this.fe, methodName, lineNumber);
        }
    }

    /// <summary>
    /// <see cref="Cursors.Wait"/> を使用するためのオブジェクト
    /// </summary>
    public class WaitCursorManager : IDisposable
    {
        /// <summary>
        /// カウンタ排他用Mutex
        /// </summary>
        private static readonly Mutex _mutex = new(false);
        /// <summary>
        /// <see cref="FrameworkElement"/> 毎のカウンタ
        /// </summary>
        private static readonly Dictionary<FrameworkElement, int> _counter = [];

        /// <summary>
        /// 生成元インスタンス
        /// </summary>
        private readonly FrameworkElement _fe;
        /// <summary>
        /// 生成元関数名
        /// </summary>
        private readonly string _methodName;
        /// <summary>
        /// 生成元行数
        /// </summary>
        private readonly int _lineNumber;

        /// <summary>
        /// <see cref="WaitCursorManager"/> クラスの新しいインスタンスを初期化します
        /// </summary>
        /// <param name="fe">生成元インスタンス</param>
        /// <param name="methodName">生成元関数名</param>
        /// <param name="lineNumber">生成元行数</param>
        public WaitCursorManager(FrameworkElement fe, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            this._fe = fe;
            this._methodName = methodName;
            this._lineNumber = lineNumber;

            this.Increase();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Decrease();
        }

        /// <summary>
        /// <see cref="Cursors.Wait"/> のカウンタを増やす
        /// </summary>
        public void Increase()
        {
            _ = _mutex.WaitOne();
            if (!_counter.TryGetValue(this._fe, out int value)) {
                value = 0;
                _counter.Add(this._fe, value);
                this._fe.Cursor = Cursors.Wait;
            }

            _counter[this._fe] = ++value;
            Log.Debug(string.Format($"Increase count:{value} from:{this._methodName}({this._lineNumber})"));
            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// <see cref="Cursors.Wait"/> のカウンタを減らす
        /// </summary>
        /// <remarks>カウンタが0になったら<see cref="null"/>に戻す</remarks>
        public void Decrease()
        {
            _ = _mutex.WaitOne();
            if (_counter.TryGetValue(this._fe, out int value)) {
                _counter[this._fe] = --value;
                Log.Debug(string.Format($"Decrease count:{value} from:{this._methodName}({this._lineNumber})"));

                if (_counter[this._fe] <= 0) {
                    this._fe.Cursor = null;
                    _ = _counter.Remove(this._fe);
                }
            }
            else {
                Log.Debug($"Can't decrease WaitCounter from:{this._methodName}({this._lineNumber})");
            }
            _mutex.ReleaseMutex();
        }
    }
}
