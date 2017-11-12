using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace HouseholdAccountBook
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        #region フィールド
        /// <summary>
        /// 接続情報
        /// </summary>
        private DaoNpgsql.ConnectInfo connectInfo = null;
        /// <summary>
        /// 多重起動抑止用Mutex
        /// </summary>
        private static Mutex mutex = null;
        #endregion

        /// <summary>
        /// <see cref="App"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public App() { }

        /// <summary>
        /// アプリケーション開始時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Properties.Settings settings = HouseholdAccountBook.Properties.Settings.Default;
            
            // 多重起動を抑止する
            App.mutex = new Mutex(false, this.GetType().Assembly.GetName().Name);
            if (!mutex.WaitOne(TimeSpan.Zero, false)) {
                this.Shutdown();
                return;
            }

            this.Exit += this.Application_Exit;

            // DB設定ダイアログ終了時に閉じないように設定する
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // 前バージョンからのUpgradeを実行していないときはUpgradeを実施する
            Version assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (!Version.TryParse(settings.App_Version, out Version preVersion) || preVersion < assemblyVersion) {
                // Upgradeを実行する
                settings.Upgrade();
            }

            // 初回起動時
            if (settings.App_InitFlag) {
                // データベース接続を設定する
                DbSettingWindow dsw = new DbSettingWindow("DB接続設定を入力してください。");
                bool? result = dsw.ShowDialog();

                if (result != true) {
                    this.connectInfo = null;
                    this.Shutdown();
                    return;
                }

                settings.App_InitFlag = false;
                settings.Save();
            }

            // 接続設定を読み込む
            this.connectInfo = new DaoNpgsql.ConnectInfo() {
                Host = settings.App_Postgres_Host,
                Port = settings.App_Postgres_Port,
                UserName = settings.App_Postgres_UserName,
                Password = settings.App_Postgres_Password,
#if DEBUG
                DatabaseName = settings.App_Postgres_DatabaseName_Debug,
#else
                DatabaseName = settings.App_Postgres_DatabaseName,
#endif
                Role = settings.App_Postgres_Role
            };

            DaoBuilder builder = new DaoBuilder(this.connectInfo);
            while (true) {
                // 接続を試行する
                bool isOpen = false;
                using (DaoBase dao = builder.Build()) {
                    isOpen = dao.IsOpen();
                }

                if (isOpen) {
                    break;
                }
                else {
                    // データベース接続を設定する
                    DbSettingWindow dsw = new DbSettingWindow("接続に失敗しました。接続設定を見直してください。");
                    bool? result = dsw.ShowDialog();

                    if (result != true) {
                        this.connectInfo = null;
                        this.Shutdown();
                        return;
                    }
                }
            }

            // 接続できる場合だけメインウィンドウを開く
            MainWindow mw = new MainWindow(builder);
            this.MainWindow = mw;
#if !DEBUG
            mw.Closing += (sender2, e2) => {
                if (connectInfo != null) {
                    settings.Reload();
                    mw.Hide();

                    if (settings.App_Postgres_DumpExePath != string.Empty && settings.App_BackUpFolderPath != string.Empty && settings.App_BackUpNum > 0) {
                        OnBackUpWindow obuw = new OnBackUpWindow();
                        obuw.Top = settings.MainWindow_Top + settings.MainWindow_Height / 2 - obuw.Height / 2;
                        obuw.Left = settings.MainWindow_Left + settings.MainWindow_Width / 2 - obuw.Width / 2;
                        obuw.Topmost = true;
                        obuw.Show();
                        CreateBackUpFile(settings.App_Postgres_DumpExePath, settings.App_BackUpNum, settings.App_BackUpFolderPath);
                        obuw.Close();
                    }
                }
            };
#endif
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            mw.Show();
        }

        /// <summary>
        /// アプリケーション終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Exit(object sender, ExitEventArgs e) {
            if (mutex != null) {
                mutex.ReleaseMutex();
                mutex.Close();
            }
        }

        /// <summary>
        /// バックアップファイルを作成する
        /// </summary>
        /// <param name="dumpExePath">pg_dump.exeパス</param>
        /// <param name="backUpNum">バックアップ数</param>
        /// <param name="backUpFolderPath">バックアップ用フォルダパス</param>
        private void CreateBackUpFile(string dumpExePath, int backUpNum, string backUpFolderPath)
        {
            if (!Directory.Exists(backUpFolderPath)) {
                Directory.CreateDirectory(backUpFolderPath);
            }
            else {
                // 古いバックアップを削除する
                List<string> fileList = new List<string>(Directory.GetFiles(backUpFolderPath, "*.backup", SearchOption.TopDirectoryOnly));
                if (fileList.Count >= backUpNum) {
                    fileList.Sort();

                    for (int i = 0; i <= fileList.Count - backUpNum; ++i) {
                        File.Delete(fileList[i]);
                    }
                }
            }

            if(backUpNum > 0) {
                // 起動情報を設定する
                ProcessStartInfo info = new ProcessStartInfo() {
                    FileName = dumpExePath,
                    Arguments = string.Format(
                        "--host {0} --port {1} --username \"{2}\" --role \"{3}\" --no-password --format custom --data-only --verbose --file \"{4}\" \"{5}\"",
                        this.connectInfo.Host, this.connectInfo.Port, this.connectInfo.UserName, this.connectInfo.Role,
                        string.Format(@"{0}/{1}.backup", backUpFolderPath, DateTime.Now.ToString("yyyyMMddHHmmss")), this.connectInfo.DatabaseName),
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                // バックアップする
                Process process = Process.Start(info);
                process.WaitForExit(10 * 1000);
            }
        }
    }
}
