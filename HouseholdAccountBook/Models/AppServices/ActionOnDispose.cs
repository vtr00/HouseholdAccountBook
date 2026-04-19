using System;

namespace HouseholdAccountBook.Models.AppServices
{
    /// <summary>
    /// Dispose時に指定した <see cref="Action"/> を実行するクラス
    /// </summary>
    /// <param name="action">Dispose時に実行する処理</param>
    public sealed class ActionOnDispose(Action action) : IDisposable
    {
        private Action mAction = action;
        private bool mDisposed;

        public void Dispose()
        {
            if (this.mDisposed) {
                return;
            }

            this.mDisposed = true;

            this.mAction?.Invoke();
            this.mAction = null;
        }
    }
}
