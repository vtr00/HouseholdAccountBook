using HouseholdAccountBook.Adapters.Logger;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.Utilities
{

    /// <summary>
    /// <see cref="Cursors.Wait"/> を使用するためのオブジェクト
    /// </summary>
    public class WaitCursorManager : IDisposable
    {
        /// <summary>
        /// カウンタ排他用Mutex
        /// </summary>
        private static readonly Mutex mMutex = new(false);
        /// <summary>
        /// <see cref="FrameworkElement"/> 毎のカウンタ
        /// </summary>
        private static readonly Dictionary<FrameworkElement, int> mCounter = [];

        /// <summary>
        /// 生成元インスタンス
        /// </summary>
        private readonly FrameworkElement mFe;
        /// <summary>
        /// 呼び出し元情報
        /// </summary>
        private readonly string mCallerStr;

        /// <summary>
        /// <see cref="WaitCursorManager"/> クラスの新しいインスタンスを初期化します
        /// </summary>
        /// <param name="fe">生成元インスタンス</param>
        /// <param name="filePath">生成元ファイル名</param>
        /// <param name="methodName">生成元関数名</param>
        /// <param name="lineNumber">生成元行数</param>
        public WaitCursorManager(FrameworkElement fe, [CallerFilePath] string filePath = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            this.mFe = fe;

            int index = filePath.LastIndexOf('\\');
            string fileName = filePath.Substring(index + 1, filePath.IndexOf('.', index + 1) - index - 1);
            string callerStr = $"{fileName}";
            if (methodName != ".ctor") {
                callerStr += $"::{methodName}";
            }
            callerStr += $":{lineNumber}";

            this.mCallerStr = callerStr;

            this.Increase();
        }

        public void Dispose()
        {
            this.Decrease();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// <see cref="Cursors.Wait"/> のカウンタを増やす
        /// </summary>
        public void Increase()
        {
            _ = mMutex.WaitOne();
            if (!mCounter.TryGetValue(this.mFe, out int value)) {
                value = 0;
                mCounter.Add(this.mFe, value);
                this.mFe.Cursor = Cursors.Wait;
            }

            mCounter[this.mFe] = ++value;

            Log.Trace($"Increase count:{value} from:[{this.mCallerStr}]");
            mMutex.ReleaseMutex();
        }

        /// <summary>
        /// <see cref="Cursors.Wait"/> のカウンタを減らす
        /// </summary>
        /// <remarks>カウンタが0になったら<see cref="null"/>に戻す</remarks>
        public void Decrease()
        {
            _ = mMutex.WaitOne();
            if (mCounter.TryGetValue(this.mFe, out int value)) {
                mCounter[this.mFe] = --value;
                Log.Trace(string.Format($"Decrease count:{value} from:[{this.mCallerStr}]"));

                if (mCounter[this.mFe] <= 0) {
                    this.mFe.Cursor = null;
                    _ = mCounter.Remove(this.mFe);
                }
            }
            else {
                Log.Warning($"Can't decrease WaitCounter from:[{this.mCallerStr}]");
            }
            mMutex.ReleaseMutex();
        }
    }
}
