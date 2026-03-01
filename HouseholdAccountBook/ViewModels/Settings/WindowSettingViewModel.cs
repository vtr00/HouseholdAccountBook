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
        /// 左位置
        /// </summary>
        public double Left { get; init; }

        /// <summary>
        /// 上位置
        /// </summary>
        public double Top { get; init; }

        /// <summary>
        /// 幅
        /// </summary>
        public double Width { get; init; }

        /// <summary>
        /// 高さ
        /// </summary>
        public double Height { get; init; }
    }
}
