﻿using HouseholdAccountBook.Others;
using System.Windows;

namespace HouseholdAccountBook.Extensions
{
    /// <summary>
    /// <see cref="FrameworkElement"/> の拡張メソッドを提供します
    /// </summary>
    public static class FrameworkElementExtensions
    {
        /// <summary>
        /// <see cref="WaitCursorManagerFactory"/> を取得します
        /// </summary>
        /// <param name="fe"></param>
        /// <returns></returns>
        public static WaitCursorManagerFactory GetWaitCursorManagerFactory(this FrameworkElement fe)
        {
            return new WaitCursorManagerFactory(fe);
        }
    }
}
