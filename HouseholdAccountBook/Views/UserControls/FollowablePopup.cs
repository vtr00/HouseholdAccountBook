using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace HouseholdAccountBook.Views.UserControls
{
    /// <summary>
    /// ウィンドウの移動に追従する <see cref="Popup"/> 
    /// </summary>
    public class FollowablePopup : Popup
    {
        /// <summary>
        /// ディアクティベート時の <see cref="Popup.IsOpen"/>
        /// </summary>
        private bool mIsOpenOnDeactivated;

        /// <summary>
        /// <see cref="FollowablePopup"/> クラスの新しいインスタンスを初期化します
        /// </summary>
        static FollowablePopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FollowablePopup), new FrameworkPropertyMetadata(typeof(FollowablePopup)));

            // PopupのIsOpenプロパティ更新のイベントハンドラを設定する
            IsOpenProperty.OverrideMetadata(typeof(FollowablePopup), new FrameworkPropertyMetadata(IsOpenChanged));
        }

        /// <summary>
        /// <see cref="Popup.IsOpen"/> 変更時のイベント
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void IsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FollowablePopup followablePopup) { return; }

            var target = followablePopup.PlacementTarget;
            if (target == null) { return; }

            Window window = Window.GetWindow(target);
            // Popup の Placement の親要素にScrollViewer要素があれば取得する
            ScrollViewer scrollViewer = GetDependencyObjectFromVisualTree(target, typeof(ScrollViewer)) as ScrollViewer;

            // 更新前のIsOpenプロパティがtrueだったので、登録済みのイベントハンドラを解除する
            if (e.OldValue != null && (bool)e.OldValue == true) {
                if (window != null) {
                    // ウィンドウの移動/リサイズ時の処理を解除
                    window.LocationChanged -= followablePopup.OnFollowWindowRectChanged;
                    window.SizeChanged -= followablePopup.OnFollowWindowRectChanged;

                    if (window.IsActive) {
                        // ウィンドウがアクティブのときのみ、ウィンドウのアクティベート変更時の処理を解除
                        window.Activated -= followablePopup.OnFollowWindowActivated;
                        window.Deactivated -= followablePopup.OnFollowWindowDiactivated;
                    }
                }

                if (scrollViewer != null) {
                    // ListBoxなどのようなScrollViewerを持った要素内に設定された場合の動作
                    scrollViewer.ScrollChanged -= followablePopup.OnScrollChanged;
                }
            }

            // IsOpenプロパティをtrueに変更したので、各種イベントハンドラを登録する
            if (e.NewValue != null && (bool)e.NewValue == true) {
                if (window != null) {
                    // ウィンドウの移動/リサイズ時の処理を設定
                    window.LocationChanged += followablePopup.OnFollowWindowRectChanged;
                    window.SizeChanged += followablePopup.OnFollowWindowRectChanged;

                    // ウィンドウのアクティベート変更時の処理を再設定
                    window.Activated -= followablePopup.OnFollowWindowActivated;
                    window.Deactivated -= followablePopup.OnFollowWindowDiactivated;
                    window.Activated += followablePopup.OnFollowWindowActivated;
                    window.Deactivated += followablePopup.OnFollowWindowDiactivated;
                }

                if (scrollViewer != null) {
                    // ListBoxなどのようなScrollViewerを持った要素内に設定された場合の動作
                    scrollViewer.ScrollChanged += followablePopup.OnScrollChanged;
                }
            }
        }

        /// <summary>
        /// 追従対象の <see cref="Window"/> の位置またはサイズ変更時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFollowWindowRectChanged(object sender, EventArgs e)
        {
            double offset = this.HorizontalOffset;
            // HorizontalOffsetなどのプロパティを一度変更しないと、ポップアップの位置が更新されないため、同一プロパティに2回値をセットしている
            this.HorizontalOffset = offset + 1;
            this.HorizontalOffset = offset;
        }

        /// <summary>
        /// 追従対象の <see cref="Window"/> のアクティベート時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFollowWindowActivated(object sender, EventArgs e) =>
            // アクティベート時にディアクティベート時のPopupの表示状態に復帰する
            this.IsOpen = this.mIsOpenOnDeactivated;

        /// <summary>
        /// 追従対象の <see cref="Window"/> のディアクティベート時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFollowWindowDiactivated(object sender, EventArgs e)
        {
            this.mIsOpenOnDeactivated = this.IsOpen; // ディアクティベート時のPopup表示状態を記憶しておく
            this.IsOpen = false;
        }

        /// <summary>
        /// 追従対象の <see cref="ScrollViewer"/> のスクロール時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0 || e.HorizontalChange != 0) {
                this.IsOpen = false;
            }
        }

        private static DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
        {
            var target = startObject;
            while (target != null) {
                if (type.IsInstanceOfType(target)) {
                    break;
                }
                else {
                    target = VisualTreeHelper.GetParent(target);
                }
            }
            return target;
        }
    }
}
