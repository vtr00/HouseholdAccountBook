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
        public Size? WindowSizeSetting {
            get {
                var (width, height) = this.WindowSizeSettingRaw;
                return width <= 0 || height <= 0 ? null : new Size(width, height);
            }
            set {
                if (value is not null) {
                    this.WindowSizeSettingRaw = (value.Value.Width, value.Value.Height);
                }
            }
        }
        /// <summary>
        /// ウィンドウの位置設定を取得/設定する
        /// </summary>
        public abstract Point WindowPointSetting { get; set; }
        /// <summary>
        /// ウィンドウの状態設定を取得/設定する
        /// </summary>
        public virtual int WindowStateSetting { get; set; }

        /// <summary>
        /// ウィンドウのサイズ設定の生データを取得/設定する
        /// </summary>
        protected virtual (double, double) WindowSizeSettingRaw { get; set; }
    }
}
