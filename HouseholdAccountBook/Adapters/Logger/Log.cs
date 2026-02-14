using HouseholdAccountBook.Extensions;
using System.Runtime.CompilerServices;

namespace HouseholdAccountBook.Adapters.Logger
{
    /// <summary>
    /// ログ出力クラス
    /// </summary>
    public class Log
    {
        /// <summary>
        /// ログレベル
        /// </summary>
        public enum LogLevel
        {
            Trace = 0,
            Debug,
            Info,
            Warn,
            Error,
            Critical,
        }

        public delegate void OutputImplDelegate(LogLevel level, string message, string filePath, string methodName, int lineNumber);
        /// <summary>
        /// 出力用デリゲート
        /// </summary>
        public static OutputImplDelegate OutputImpl { get; set; }

        /// <summary>
        /// 関数開始ログを1行出力する
        /// </summary>
        /// <param name="args">引数を含む匿名クラス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void FuncStart(object args = null, LogLevel level = LogLevel.Info, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
            => Vars("func start", args, level, fileName, methodName, lineNumber);
        /// <summary>
        /// 関数終了ログを1行出力する
        /// </summary>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="digit">「-」の出力回数</param>
        public static void FuncEnd(LogLevel level = LogLevel.Info, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, ushort digit = 0)
            => OutputImpl?.Invoke(level, "func end", fileName, methodName, -digit);
        /// <summary>
        /// 変数ログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="vars">引数を含む匿名クラス</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void Vars(string message = "", object vars = null, LogLevel level = LogLevel.Info, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            string details = string.Empty;
            if (vars != null) {
                details += vars.ToString2();
            }

            string tmpMessage = string.Join((message != string.Empty && details != string.Empty) ? " - " : "", message, details);
            OutputImpl?.Invoke(level, tmpMessage, fileName, methodName, lineNumber);
        }

        /// <summary>
        /// 致命的ログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void Critical(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
            => OutputImpl?.Invoke(LogLevel.Critical, message, fileName, methodName, lineNumber);
        /// <summary>
        /// エラーログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void Error(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
            => OutputImpl?.Invoke(LogLevel.Error, message, fileName, methodName, lineNumber);
        /// <summary>
        /// 警告ログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void Warning(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
            => OutputImpl?.Invoke(LogLevel.Warn, message, fileName, methodName, lineNumber);
        /// <summary>
        /// 情報ログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="fileName">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void Info(string message = "", [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
            => OutputImpl?.Invoke(LogLevel.Info, message, fileName, methodName, lineNumber);
        /// <summary>
        /// デバッグログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="filePath">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void Debug(string message = "", [CallerFilePath] string filePath = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
            => OutputImpl?.Invoke(LogLevel.Debug, message, filePath, methodName, lineNumber);
        /// <summary>
        /// トレースログを1行出力する
        /// </summary>
        /// <param name="message">出力メッセージ</param>
        /// <param name="filePath">出力元ファイル名</param>
        /// <param name="methodName">出力元関数名</param>
        /// <param name="lineNumber">出力元行数</param>
        public static void Trace(string message = "", [CallerFilePath] string filePath = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
            => OutputImpl?.Invoke(LogLevel.Trace, message, filePath, methodName, lineNumber);
    }
}
