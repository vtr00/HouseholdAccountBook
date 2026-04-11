using System.Windows;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// ウィンドウ設定VM
    /// </summary>
    public class WindowSettingViewModel
    {
        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        public string Title { get; init; }

        /// <summary>
        /// 位置
        /// </summary>
        public Point Point { get; init; }

        /// <summary>
        /// 左位置
        /// </summary>
        public double Left => this.Point.X;

        /// <summary>
        /// 上位置
        /// </summary>
        public double Top => this.Point.Y;

        /// <summary>
        /// サイズ
        /// </summary>
        public (double width, double height) Size { get; init; }

        /// <summary>
        /// 幅
        /// </summary>
        public double Width => this.Size.width;

        /// <summary>
        /// 高さ
        /// </summary>
        public double Height => this.Size.height;
    }
}
