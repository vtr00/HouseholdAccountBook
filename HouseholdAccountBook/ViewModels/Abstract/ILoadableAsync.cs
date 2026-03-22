using System.Threading;
using System.Threading.Tasks;

namespace HouseholdAccountBook.ViewModels.Abstract
{
    /// <summary>
    /// 非同期読込可能なViewModelのためのインターフェイス
    /// </summary>
    public interface ILoadableAsync
    {
        /// <summary>
        /// [非同期] 読み込む
        /// </summary>
        /// <param name="token">キャンセル用トークン</param>
        /// <returns></returns>
        Task LoadAsync(CancellationToken token = default);
    }
}
