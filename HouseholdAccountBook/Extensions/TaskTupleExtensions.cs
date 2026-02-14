using System.Threading.Tasks;

namespace HouseholdAccountBook.Extensions
{
    /// <summary>
    /// 複数の <see cref="Task"/> の同時実行を提供します 
    /// </summary>
    public static class TaskTupleExtensions
    {
        /// <summary>
        /// 複数の <see cref="Task"/> を同時に非同期で実行する
        /// </summary>
        /// <typeparam name="T1"><see cref="Task"/> 実行結果の型1</typeparam>
        /// <typeparam name="T2"><see cref="Task"/> 実行結果の型2</typeparam>
        /// <param name="tasks"><see cref="Task"/> の <see cref="System.Tuple"/></param>
        /// <param name="continueOnCapturedContext"></param>
        /// <returns><see cref="Task"/> の実行結果</returns>
        public static async Task<(T1, T2)> WhenAll<T1, T2>(this (Task<T1>, Task<T2>) tasks, bool continueOnCapturedContext = false)
        {
            await Task.WhenAll(tasks.Item1, tasks.Item2).ConfigureAwait(continueOnCapturedContext);
            return (tasks.Item1.Result, tasks.Item2.Result);
        }

        /// <summary>
        /// 複数の <see cref="Task"/> を同時に非同期で実行する
        /// </summary>
        /// <typeparam name="T1"><see cref="Task"/> 実行結果の型1</typeparam>
        /// <typeparam name="T2"><see cref="Task"/> 実行結果の型2</typeparam>
        /// <typeparam name="T3"><see cref="Task"/> 実行結果の型3</typeparam>
        /// <param name="tasks"><see cref="Task"/> の <see cref="System.Tuple"/></param>
        /// <param name="continueOnCapturedContext"></param>
        /// <returns><see cref="Task"/> の実行結果</returns>
        public static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(this (Task<T1>, Task<T2>, Task<T3>) tasks, bool continueOnCapturedContext = false)
        {
            await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3).ConfigureAwait(continueOnCapturedContext);
            return (tasks.Item1.Result, tasks.Item2.Result, tasks.Item3.Result);
        }

        /// <summary>
        /// 複数の <see cref="Task"/> を同時に非同期で実行する
        /// </summary>
        /// <typeparam name="T1"><see cref="Task"/> 実行結果の型1</typeparam>
        /// <typeparam name="T2"><see cref="Task"/> 実行結果の型2</typeparam>
        /// <typeparam name="T3"><see cref="Task"/> 実行結果の型3</typeparam>
        /// <typeparam name="T4"><see cref="Task"/> 実行結果の型4</typeparam>
        /// <param name="tasks"><see cref="Task"/> の <see cref="System.Tuple"/></param>
        /// <param name="continueOnCapturedContext"></param>
        /// <returns><see cref="Task"/> の実行結果</returns>
        public static async Task<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>) tasks, bool continueOnCapturedContext = false)
        {
            await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4).ConfigureAwait(continueOnCapturedContext);
            return (tasks.Item1.Result, tasks.Item2.Result, tasks.Item3.Result, tasks.Item4.Result);
        }
    }
}
