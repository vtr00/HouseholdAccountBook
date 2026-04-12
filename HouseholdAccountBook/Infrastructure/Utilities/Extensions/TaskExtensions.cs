using HouseholdAccountBook.Infrastructure.Logger;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Infrastructure.Utilities.Extensions
{
    /// <summary>
    /// <see cref="Task"/> の拡張メソッドを提供します
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// <see cref="Task"/> を呼び出し元で待機せずに実行し、完了後に例外をUIスレッドで処理する
        /// </summary>
        /// <remarks>
        /// <see cref="OperationCanceledException"/> は無視し、それ以外の例外は再スローする
        /// </remarks>
        /// <param name="task">対象の <see cref="Task"/></param>
        public static async void FireAndForget(this Task task, [CallerFilePath] string fileName = null, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            ArgumentNullException.ThrowIfNull(task);

            try {
                await task;
            }
            catch (OperationCanceledException) {
                Log.Info($"{methodName} Canceled.", fileName, methodName);
            }
            catch (Exception) {
                Log.Error("Exception Occured.", fileName, methodName, lineNumber);
                throw;
            }
        }
    }
}
