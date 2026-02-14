using HouseholdAccountBook.ViewModels.Abstract;

namespace HouseholdAccountBook.ViewModels.Settings
{
    /// <summary>
    /// ウィンドウ設定VM
    /// </summary>
    public class WindowSettingViewModel : BindableBase
    {
        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 左位置
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// 上位置
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// 幅
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 高さ
        /// </summary>
        public double Height { get; set; }
    }
}
