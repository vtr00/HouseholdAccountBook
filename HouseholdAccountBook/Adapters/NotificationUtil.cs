using HouseholdAccountBook.Adapters.Logger;
using Notification.Wpf;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HouseholdAccountBook.Adapters
{
    public static class NotificationUtil
    {
        private static readonly Brush InformationForeground = Brushes.Blue;
        private static readonly Brush InformationBackground = Brushes.LightBlue;

        private static readonly Brush WarningForeground = Brushes.LightYellow;
        private static readonly Brush WarningBackground = Brushes.DarkOrange;

        private static readonly Brush ErrorForeground = Brushes.LightPink;
        private static readonly Brush ErrorBackground = Brushes.DarkRed;

        /// <summary>
        /// 捕捉されない例外を通知する
        /// </summary>
        /// <param name="absoluteLogFilePath"></param>
        public static void NotifyUnhandledException(string absoluteLogFilePath)
        {
            // ハンドルされない例外の発生を通知する
            NotificationManager nm = new();
            NotificationContent nc = new() {
                Title = Properties.Resources.Title_MainWindow,
                Message = Properties.Resources.Message_UnhandledExceptionOccurred,
                Type = NotificationType.Error,
                Background = ErrorBackground,
                Foreground = ErrorForeground,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/resheet.ico"))
            };
            nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10), onClick: () => {
                Log.Info($"Create Unhandled Exception Info Absolute File: {absoluteLogFilePath}");
                try {
                    _ = Process.Start(new ProcessStartInfo() {
                        FileName = absoluteLogFilePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception) { }
                if (Application.Current.MainWindow == null || !Application.Current.MainWindow.IsInitialized) {
                    Application.Current.Shutdown(1); // メインウィンドウがない場合は強制終了
                }
            });
        }

        /// <summary>
        /// 祝日リストの取得に失敗したことを通知する
        /// </summary>
        public static void NotifyFailingToGetHolidayList()
        {
            // 祝日取得失敗を通知する
            NotificationManager nm = new();
            NotificationContent nc = new() {
                Title = Properties.Resources.Title_MainWindow,
                Message = Properties.Resources.Message_FoultToGetHolidayList,
                Type = NotificationType.Warning,
                Background = WarningBackground,
                Foreground = WarningForeground,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/resheet.ico"))
            };
            nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10));
        }

        /// <summary>
        /// バックアップ完了を通知する
        /// </summary>
        public static void NotifyFinishingToBackup()
        {
            NotificationManager nm = new();
            NotificationContent nc = new() {
                Title = Properties.Resources.Title_MainWindow,
                Message = Properties.Resources.Message_FinishToBackup,
                Type = NotificationType.Information,
                Background = InformationBackground,
                Foreground = InformationForeground,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/resheet.ico"))
            };
            nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10));
        }

        /// <summary>
        /// バックアップ失敗を通知する
        /// </summary>
        public static void NotifyFailingToBackup()
        {
            NotificationManager nm = new();
            NotificationContent nc = new() {
                Title = Properties.Resources.Title_MainWindow,
                Message = Properties.Resources.Message_FoultToBackup,
                Type = NotificationType.Error,
                Background = ErrorBackground,
                Foreground = ErrorForeground,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/resheet.ico"))
            };
            nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10));
        }
    }
}
