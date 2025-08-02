using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        /// <summary>
        /// カウンタ排他用Mutex
        /// </summary>
        private static readonly Mutex _mutex = new(false);
        /// <summary>
        /// <see cref="FrameworkElement"/> 毎のカウンタ
        /// </summary>
        private static readonly Dictionary<FrameworkElement, int> _counter = [];

        /// <summary>
        /// <see cref="WaitCursorUseObject"/> を生成します
        /// </summary>
        /// <param name="fe">生成元インスタンス</param>
        /// <param name="methodName">生成元関数名</param>
        /// <param name="lineNumber">生成元行数</param>
        /// <returns></returns>
        public static WaitCursorUseObject CreateWaitCorsorUseObject(this FrameworkElement fe, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            return new WaitCursorUseObject(fe, methodName, lineNumber);
        }

        /// <summary>
        /// <see cref="Cursors.Wait"/> を使用するためのオブジェクト
        /// </summary>
        public class WaitCursorUseObject : IDisposable
        {
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
            /// <see cref="WaitCursorUseObject"/> クラスの新しいインスタンスを初期化します
            /// </summary>
            /// <param name="fe">生成元インスタンス</param>
            /// <param name="methodName">生成元関数名</param>
            /// <param name="lineNumber">生成元行数</param>
            public WaitCursorUseObject(FrameworkElement fe, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
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
                _mutex.WaitOne();
                if (!_counter.ContainsKey(this._fe)) {
                    _counter.Add(this._fe, 0);
                    this._fe.Cursor = Cursors.Wait;
                }

                _counter[this._fe]++;
                Log.Debug(string.Format($"Increase WaitCounter count:{_counter[this._fe]} from:{this._methodName}:{this._lineNumber}"));
                _mutex.ReleaseMutex();
            }

            /// <summary>
            /// <see cref="Cursors.Wait"/> のカウンタを減らす
            /// </summary>
            /// <remarks>カウンタが0になったら<see cref="null"/>に戻す</remarks>
            public void Decrease()
            {
                _mutex.WaitOne();
                if (_counter.ContainsKey(this._fe)) {
                    _counter[this._fe]--;
                    Log.Debug(string.Format($"Decrease WaitCounter count:{_counter[this._fe]} from:{this._methodName}:{this._lineNumber}"));

                    if (_counter[this._fe] <= 0) {
                        this._fe.Cursor = null;
                        _counter.Remove(this._fe);
                    }
                }
                else {
                    Log.Debug($"Don't decrease WaitCounter from:{this._methodName}:{this._lineNumber}");
                }
                _mutex.ReleaseMutex();
            }
        }
    }
}
