using HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs;
using System.Threading.Tasks;

namespace HouseholdAccountBook.ViewModels.Abstract
{
    /// <summary>
    /// 進捗ダイアログ要求可能なインターフェイス
    /// </summary>
    public interface IProgressDialogRequestable
    {
        /// <summary>
        /// 進捗ダイアログ要求
        /// </summary>
        /// <param name="e">要求時引数</param>
        /// <returns>完了タスク</returns>
        Task ProgressDialogRequest(ProgressDialogRequestEventArgs e);
    }
}
