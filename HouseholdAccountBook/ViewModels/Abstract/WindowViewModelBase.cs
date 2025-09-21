using HouseholdAccountBook.Models.DbHandler;
using HouseholdAccountBook.Others;
using System;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.Abstract
{
    /// <summary>
    /// WindowViewModelの基底クラス
    /// </summary>
    public abstract class WindowViewModelBase : WindowPartViewModelBase
    {
        /// <summary>
        /// ウィンドウの領域設定を指定する
        /// </summary>
        public abstract Rect WindowRectSetting { set; }
        /// <summary>
        /// ウィンドウのサイズ設定を取得する
        /// </summary>
        public abstract Size? WindowSizeSetting { get; }
        /// <summary>
        /// ウィンドウの位置設定を取得する
        /// </summary>
        public abstract Point? WindowPointSetting { get; }

        /// <summary>
        /// ウィンドウのサイズ設定を取得する
        /// </summary>
        /// <param name="width">ウィンドウの幅設定</param>
        /// <param name="height">ウィンドウの高さ設定</param>
        /// <returns>ウィンドウのサイズ設定</returns>
        /// <remarks>設定が初期値であればNULLを返す</remarks>
        protected static Size? WindowSizeSettingImpl(double width, double height)
        {
            if (width != -1 && height != -1) {
                Size size = new() {
                    Width = width,
                    Height = height
                };
                return size;
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// ウィンドウの位置設定を取得する
        /// </summary>
        /// <param name="left">ウィンドウの左位置設定</param>
        /// <param name="top">ウィンドウの上位置設定</param>
        /// <param name="isPositionSaved">ウィンドウ位置の保存有無設定</param>
        /// <returns>ウィンドウの位置設定</returns>
        /// <remarks>位置を保存しないまたは位置が不適切な場合はNULLを返す</remarks>
        protected static Point? WindowPointSettingImpl(double left, double top, bool isPositionSaved = true)
        {
            if (isPositionSaved && -10 <= left && 0 <= top) {
                Point point = new() {
                    X = left,
                    Y = top,
                };
                return point;
            }
            else {
                return null;
            }
        }
    }
}
