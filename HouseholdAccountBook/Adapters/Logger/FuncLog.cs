using System;
using System.Runtime.CompilerServices;

namespace HouseholdAccountBook.Adapters.Logger
{
    /// <summary>
    /// 関数ログ
    /// </summary>
    public class FuncLog : IDisposable
    {
        /// <summary>
        /// 呼び出し元ファイル名
        /// </summary>
        private readonly string fileName;
        /// <summary>
        /// 呼び出し元関数名
        /// </summary>
        private readonly string methodName;
        /// <summary>
        /// 呼び出し元行数
        /// </summary>
        private readonly int lineNumber;

        /// <summary>
        /// 関数ログ コンストラクタ
        /// </summary>
        /// <param name="args">引数オブジェクト</param>
        /// <param name="fileName">呼び出し元ファイル名</param>
        /// <param name="methodName">呼び出し元関数名</param>
        /// <param name="lineNumber">呼び出し元行数</param>
        public FuncLog(object args = null, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            this.fileName = fileName;
            this.methodName = methodName;
            this.lineNumber = lineNumber;
            // コンストラクタで関数開始ログを出力
            Log.FuncStart(args, fileName, methodName, lineNumber);
        }

        public void Dispose()
        {
            // 破棄時に関数終了ログを出力
            Log.FuncEnd(this.fileName, this.methodName, (ushort)(Math.Log10(this.lineNumber) + 1));
            GC.SuppressFinalize(this);
        }
    }
}
