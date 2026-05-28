using System;
using System.Runtime.CompilerServices;

namespace HouseholdAccountBook.Infrastructure.Logger
{
    /// <summary>
    /// 関数ログ
    /// </summary>
    public class FuncLog : IDisposable
    {
        #region フィールド
        /// <summary>
        /// ID(GUIDから生成)
        /// </summary>
        private readonly string mId;
        /// <summary>
        /// ログレベル
        /// </summary>
        private readonly Log.LogLevel mLevel;
        /// <summary>
        /// 呼び出し元ファイル名
        /// </summary>
        private readonly string mFileName;
        /// <summary>
        /// 呼び出し元関数名
        /// </summary>
        private readonly string mMethodName;
        /// <summary>
        /// 呼び出し元行数
        /// </summary>
        private readonly int mLineNumber;
        /// <summary>
        /// 開始日時
        /// </summary>
        private readonly DateTime mStartTime;
        #endregion

        #region プロパティ
        /// <summary>
        /// 戻り値
        /// </summary>
        public object Returns { get; set; }
        #endregion

        /// <summary>
        /// 関数ログ コンストラクタ
        /// </summary>
        /// <param name="args">引数オブジェクト</param>
        /// <param name="fileName">呼び出し元ファイル名</param>
        /// <param name="methodName">呼び出し元関数名</param>
        /// <param name="lineNumber">呼び出し元行数</param>
        public FuncLog(object args = null, Log.LogLevel level = Log.LogLevel.Info,
            [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            this.mStartTime = DateTime.Now;
            this.mId = Guid.NewGuid().ToString("N")[..7];
            this.mLevel = level;
            this.mFileName = fileName;
            this.mMethodName = methodName;
            this.mLineNumber = lineNumber;

            // コンストラクタで関数開始ログを出力
            Log.Vars($"func({this.mId}) start", args, this.mLevel, this.mFileName, this.mMethodName, this.mLineNumber);
        }

        public void Dispose()
        {
            DateTime finishTime = DateTime.Now;
            TimeSpan spendTime = finishTime - this.mStartTime;
            string timeLog = string.Empty;
            switch (spendTime) {
                case TimeSpan ts when ts > TimeSpan.FromMinutes(1):
                    timeLog = $"({spendTime:m\\:ss\\.fff})";
                    break;
                case TimeSpan ts when ts > TimeSpan.FromSeconds(1):
                    timeLog = $"({spendTime:s\\.fff})";
                    break;
                default:
                    break;
            }

            // 破棄時に関数終了ログを出力
            Log.Vars($"func({this.mId}) end{timeLog}", this.Returns, this.mLevel, this.mFileName, this.mMethodName, -(ushort)(Math.Log10(this.mLineNumber) + 1));

            GC.SuppressFinalize(this);
        }
    }
}
