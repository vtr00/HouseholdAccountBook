using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HouseholdAccountBook.Extensions
{
    /// <summary>
    /// <see cref="DependencyObject"/> 拡張
    /// </summary>
    public static class DependencyObjectExtensions
    {
        /// <summary>
        /// コンテンツの先頭まで水平方向にスクロールします。
        /// </summary>
        /// <param name="dObj"></param>
        public static void ScrollToLeftEnd(this DependencyObject dObj)
        {
            if (VisualTreeHelper.GetChildrenCount(dObj) > 0) {
                if (VisualTreeHelper.GetChild(dObj, 0) is Decorator decorator) {
                    if (decorator.Child is ScrollViewer scrollViewer) {
                        scrollViewer.ScrollToLeftEnd();
                    }
                }
            }
        }

        /// <summary>
        /// コンテンツの先頭まで垂直方向にスクロールします。
        /// </summary>
        /// <param name="dObj"></param>
        public static void ScrollToTop (this DependencyObject dObj)
        {
            if (VisualTreeHelper.GetChildrenCount(dObj) > 0) {
                if (VisualTreeHelper.GetChild(dObj, 0) is Decorator decorator) {
                    if (decorator.Child is ScrollViewer scrollViewer) {
                        scrollViewer.ScrollToTop();
                    }
                }
            }
        }

        /// <summary>
        /// コンテンツの末尾まで水平方向にスクロールします。
        /// </summary>
        /// <param name="dObj"></param>
        public static void ScrollToRightEnd(this DependencyObject dObj)
        {
            if (VisualTreeHelper.GetChildrenCount(dObj) > 0) {
                if (VisualTreeHelper.GetChild(dObj, 0) is Decorator decorator) {
                    if (decorator.Child is ScrollViewer scrollViewer) {
                        scrollViewer.ScrollToRightEnd();
                    }
                }
            }
        }

        /// <summary>
        /// コンテンツの末尾まで垂直方向にスクロールします。
        /// </summary>
        /// <param name="dObj"></param>
        public static void ScrollToButtom (this DependencyObject dObj)
        {
            if (VisualTreeHelper.GetChildrenCount(dObj) > 0) {
                if (VisualTreeHelper.GetChild(dObj, 0) is Decorator decorator) {
                    if (decorator.Child is ScrollViewer scrollViewer) {
                        scrollViewer.ScrollToBottom();
                    }
                }
            }
        }
    }
}
