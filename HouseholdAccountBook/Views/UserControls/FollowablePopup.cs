using HouseholdAccountBook.Infrastructure.Logger;
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
        /// <see cref="EnablePopup"/> 依存関係プロパティを識別します。
        /// </summary>
        public static readonly DependencyProperty EnablePopupProperty = DependencyProperty.Register(
                nameof(EnablePopup),
                typeof(bool),
                typeof(FollowablePopup),
                new PropertyMetadata(true)
            );
        /// <summary>
        /// <see cref="=FollowablePopup"> を使用するか
        /// </summary>
        public bool EnablePopup {
            get => (bool)this.GetValue(EnablePopupProperty);
            set => this.SetValue(EnablePopupProperty, value);
        }

        /// <summary>
        /// <see cref="FollowablePopup"/> クラスのオブジェクトを初期化します
        /// </summary>
        /// <remarks>PopupのIsOpenプロパティ更新のイベントハンドラを設定する</remarks>
        static FollowablePopup() => IsOpenProperty.OverrideMetadata(
                typeof(FollowablePopup),
                new FrameworkPropertyMetadata(false, (d, e) => ((FollowablePopup)d).IsOpenChanged(e), (d, e) => ((FollowablePopup)d).CoerceIsOpen(e))
            );
        /// <summary>
        /// <see cref="Popup.IsOpen"/> 変更時のイベント
        /// </summary>
        /// <param name="e"></param>
        private void IsOpenChanged(DependencyPropertyChangedEventArgs e)
        {
            UIElement target = this.PlacementTarget;
            if (target == null) { return; }

            using FuncLog funcLog = new(new { e.OldValue, e.NewValue });

            Window window = Window.GetWindow(target);
            // Popup の Placement の親要素にScrollViewer要素があれば取得する
            ScrollViewer scrollViewer = GetDependencyObjectFromVisualTree(target, typeof(ScrollViewer)) as ScrollViewer;

            // 更新前のIsOpenプロパティがtrueだったので、登録済みのイベントハンドラを解除する
            if (e.OldValue != null && (bool)e.OldValue) {
                if (window != null) {
                    // ウィンドウの移動/リサイズ時の処理を解除
                    window.LocationChanged -= this.Window_RectChanged;
                    window.SizeChanged -= this.Window_RectChanged;

                    // ウィンドウのアクティベート変更時の処理を解除
                    window.Activated -= this.Window_Activated;
                    window.Deactivated -= this.Window_Diactivated;
                }

                // ListBoxなどのようなScrollViewerを持った要素内に設定された場合の動作
                scrollViewer?.ScrollChanged -= this.ScrollViewer_ScrollChanged;
            }

            // IsOpenプロパティをtrueに変更したので、各種イベントハンドラを登録する
            if (e.NewValue != null && (bool)e.NewValue) {
                if (window != null) {
                    // ウィンドウの移動/リサイズ時の処理を設定
                    window.LocationChanged += this.Window_RectChanged;
                    window.SizeChanged += this.Window_RectChanged;

                    // ウィンドウのアクティベート変更時の処理を設定
                    window.Activated += this.Window_Activated;
                    window.Deactivated += this.Window_Diactivated;
                }

                // ListBoxなどのようなScrollViewerを持った要素内に設定された場合の動作
                scrollViewer?.ScrollChanged += this.ScrollViewer_ScrollChanged;
            }
        }
        /// <summary>
        /// <see cref="Popup.IsOpen"/> 変更のガード処理
        /// </summary>
        /// <param name="baseValue"></param>
        private object CoerceIsOpen(object baseValue) => !this.EnablePopup ? false : baseValue;

        /// <summary>
        /// 追従対象の <see cref="Window"/> の位置またはサイズ変更時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_RectChanged(object sender, EventArgs e)
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
        private void Window_Activated(object sender, EventArgs e)
        {
            using FuncLog funcLog = new();

            this.IsOpen = this.mIsOpenOnDeactivated; // アクティベート時にディアクティベート時のPopupの表示状態に復帰する
        }

        /// <summary>
        /// 追従対象の <see cref="Window"/> のディアクティベート時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Diactivated(object sender, EventArgs e)
        {
            using FuncLog funcLog = new();

            this.mIsOpenOnDeactivated = this.IsOpen; // ディアクティベート時のPopup表示状態を記憶しておく
            this.IsOpen = false; // 別のウィンドウがアクティブのときは非表示にする
        }

        /// <summary>
        /// 追従対象の <see cref="ScrollViewer"/> のスクロール時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0 || e.HorizontalChange != 0) {
                using FuncLog funcLog = new(new { e.VerticalChange, e.HorizontalChange });

                this.IsOpen = false;
            }
        }

        private static DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
        {
            DependencyObject target = startObject;
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
