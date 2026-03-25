using HouseholdAccountBook.Infrastructure.Logger;
using Notification.Wpf;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HouseholdAccountBook.Models.AppServices
{
    /// <summary>
    /// 通知ユーティリティ
    /// </summary>
    public static class NotificationService
    {
        private static readonly Brush mInformationForeground = Brushes.Blue;
        private static readonly Brush mInformationBackground = Brushes.LightBlue;

        private static readonly Brush mWarningForeground = Brushes.LightYellow;
        private static readonly Brush mWarningBackground = Brushes.DarkOrange;

        private static readonly Brush mErrorForeground = Brushes.LightPink;
        private static readonly Brush mErrorBackground = Brushes.DarkRed;

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
                Background = mErrorBackground,
                Foreground = mErrorForeground,
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
                Background = mWarningBackground,
                Foreground = mWarningForeground,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/resheet.ico"))
            };
            nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10));
        }

        /// <summary>
        /// 最新バージョン番号の取得に失敗したことを通知する
        /// </summary>
        public static void NotifyFailingToGetLatestVersionNumber()
        {
            // 最新バージョン取得失敗を通知する
            NotificationManager nm = new();
            NotificationContent nc = new() {
                Title = Properties.Resources.Title_MainWindow,
                Message = Properties.Resources.Message_FoultToGetLatestVersionNumber,
                Type = NotificationType.Warning,
                Background = mWarningBackground,
                Foreground = mWarningForeground,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/resheet.ico"))
            };
            nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10));
        }

        /// <summary>
        /// 現在バージョンが最新バージョンと同じことを通知する
        /// </summary>
        public static void NotifyCurrentIsLatestVersion()
        {
            // 最新バージョンを通知する
            NotificationManager nm = new();
            NotificationContent nc = new() {
                Title = Properties.Resources.Title_MainWindow,
                Message = Properties.Resources.Message_NotifyCurrentIsLatestVersion,
                Type = NotificationType.Information,
                Background = mInformationBackground,
                Foreground = mInformationForeground,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/resheet.ico"))
            };
            nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10));
        }

        /// <summary>
        /// 最新バージョン番号を通知する
        /// </summary>
        /// <param name="latest">最新バージョン番号</param>
        /// <param name="latestHtmlUrl">最新バージョンのURL</param>
        public static void NotifyLatestVersionNumber(Version latest, string latestHtmlUrl)
        {
            // 最新バージョンを通知する
            NotificationManager nm = new();
            NotificationContent nc = new() {
                Title = Properties.Resources.Title_MainWindow,
                Message = string.Format(Properties.Resources.Message_NotifyLatestVersionNumber, latest.ToString()),
                Type = NotificationType.Information,
                Background = mInformationBackground,
                Foreground = mInformationForeground,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/resheet.ico"))
            };
            nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10), onClick: () => {
                if (latestHtmlUrl != null) {
                    Log.Info($"Open latest html url: {latestHtmlUrl}");
                    try {
                        _ = Process.Start(new ProcessStartInfo() {
                            FileName = latestHtmlUrl,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception) { }
                }
            });
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
                Background = mInformationBackground,
                Foreground = mInformationForeground,
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
                Background = mErrorBackground,
                Foreground = mErrorForeground,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/resheet.ico"))
            };
            nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10));
        }
    }
}
