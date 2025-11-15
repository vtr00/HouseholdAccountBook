using HouseholdAccountBook.Others;
using OxyPlot;
using OxyPlot.Series;
using System;

namespace HouseholdAccountBook.Views.UserControls
{
    /// <summary>
    /// <see cref="TrackerHitResult"/> の変化を通知する機能付き <see cref="BarSeries"/>
    /// </summary>
    public class CustomBarSeries : BarSeries
    {
        /// <summary>
        /// 前回の <see cref="GetNearestPoint(ScreenPoint, bool)"/> 呼び出し時の <see cref="TrackerHitResult"/>
        /// </summary>
        private TrackerHitResult mOldResult;

        /// <summary>
        /// <see cref="TrackerHitResult"/> の表示テキストが変化したことを示すイベント
        /// </summary>
        public event EventHandler<EventArgs<TrackerHitResult>> TrackerHitResultChanged;

        public override TrackerHitResult GetNearestPoint(ScreenPoint point, bool interpolate)
        {
            TrackerHitResult result = base.GetNearestPoint(point, interpolate);

            // いずれかがnullではないとき
            if (this.mOldResult != null || result != null) {
                // TrackerHitResult が変化していたとき
                if ((this.mOldResult == null && result != null) || (this.mOldResult != null && result == null)) {
                    // 通知する
                    TrackerHitResultChanged?.Invoke(this, new EventArgs<TrackerHitResult>(result));
                }
            }
            this.mOldResult = result;

            return result;
        }
    }
}
