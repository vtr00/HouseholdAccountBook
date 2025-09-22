using System.Windows;

namespace HouseholdAccountBook.ViewModels.Abstract
{
    /// <summary>
    /// WindowViewModelの基底クラス
    /// </summary>
    public abstract class WindowViewModelBase : WindowPartViewModelBase
    {
        /// <summary>
        /// ウィンドウのサイズ設定を取得/設定する
        /// </summary>
        public virtual Size WindowSizeSetting { get; set; }
        /// <summary>
        /// ウィンドウの位置設定を取得/設定する
        /// </summary>
        public abstract Point WindowPointSetting { get; set; }
        /// <summary>
        /// ウィンドウの状態設定を取得/設定する
        /// </summary>
        public virtual int WindowStateSetting { get; set; }
    }
}
