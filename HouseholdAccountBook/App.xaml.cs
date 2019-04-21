using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Extensions;
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
#if !DEBUG
        /// <summary>
        /// 多重起動抑止用Mutex
        /// </summary>
        private static Mutex mutex = null;
#endif
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

#if DEBUG
            Debug.Listeners.Add(new ConsoleTraceListener());
#endif

#if !DEBUG
            // 多重起動を抑止する
            App.mutex = new Mutex(false, this.GetType().Assembly.GetName().Name);
            if (!mutex.WaitOne(TimeSpan.Zero, false)) {
                MessageBox.Show("同時に複数起動することはできません。");
                this.Shutdown();
                return;
            }
#endif

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

            DaoBuilder builder = null;
            while (true)
            {
                // 接続設定を読み込む
                this.connectInfo = new DaoNpgsql.ConnectInfo()
                {
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
                builder = new DaoBuilder(this.connectInfo);

                // 接続を試行する
                bool isOpen = false;
                using (DaoBase dao = builder.Build()) {
                    isOpen = dao.IsOpen;
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

            // 休日リストを取得する
            DateTimeExtensions.DownloadHolidayListAsync();

            // 接続できる場合だけメインウィンドウを開く
            MainWindow mw = new MainWindow(builder);
            this.MainWindow = mw;
#if !DEBUG
            mw.Closing += (sender2, e2) => {
                if (this.connectInfo != null) {
                    settings.Reload();
                    mw.Hide();

                    if (settings.App_BackUpFlagAtClosing) {
                        OnBackUpWindow obuw = new OnBackUpWindow();
                        obuw.Top = settings.MainWindow_Top + settings.MainWindow_Height / 2 - obuw.Height / 2;
                        obuw.Left = settings.MainWindow_Left + settings.MainWindow_Width / 2 - obuw.Width / 2;
                        obuw.Topmost = true;
                        obuw.Show();
                        this.CreateBackUpFile();
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
            this.ReleaseMutex();
        }

        /// <summary>
        /// 多重起動防止用のMutexを開放する
        /// </summary>
        public void ReleaseMutex()
        {
#if !DEBUG
            if (mutex != null) {
                mutex.ReleaseMutex();
                mutex.Close();
                mutex = null;
            }
#endif
        }

        /// <summary>
        /// バックアップファイルを作成する
        /// </summary>
        /// <param name="dumpExePath">pg_dump.exeパス</param>
        /// <param name="backUpNum">バックアップ数</param>
        /// <param name="backUpFolderPath">バックアップ用フォルダパス</param>
        /// <returns>バックアップの成否</returns>
        public bool CreateBackUpFile(string dumpExePath = null, int? backUpNum = null, string backUpFolderPath = null)
        {
            Properties.Settings settings = HouseholdAccountBook.Properties.Settings.Default;

            string tmpDumpExePath = dumpExePath ?? settings.App_Postgres_DumpExePath;
            int tmpBackUpNum = backUpNum ?? settings.App_BackUpNum;
            string tmpBackUpFolderPath = backUpFolderPath ?? settings.App_BackUpFolderPath;

            if (tmpBackUpFolderPath != string.Empty) {
                if (!Directory.Exists(tmpBackUpFolderPath)) {
                    Directory.CreateDirectory(tmpBackUpFolderPath);
                }
                else {
                    // 古いバックアップを削除する
                    List<string> fileList = new List<string>(Directory.GetFiles(tmpBackUpFolderPath, "*.backup", SearchOption.TopDirectoryOnly));
                    if (fileList.Count >= tmpBackUpNum) {
                        fileList.Sort();

                        for (int i = 0; i <= fileList.Count - tmpBackUpNum; ++i) {
                            File.Delete(fileList[i]);
                        }
                    }
                }

                if (tmpDumpExePath != string.Empty){
                    if (tmpBackUpNum > 0) {
                        // 起動情報を設定する
                        ProcessStartInfo info = new ProcessStartInfo() {
                            FileName = tmpDumpExePath,
                            Arguments = string.Format(
                                "--host {0} --port {1} --username \"{2}\" --role \"{3}\" --no-password --format custom --data-only --verbose --file \"{4}\" \"{5}\"",
                                this.connectInfo.Host, this.connectInfo.Port, this.connectInfo.UserName, this.connectInfo.Role,
                                string.Format(@"{0}/{1}.backup", tmpBackUpFolderPath, DateTime.Now.ToString("yyyyMMddHHmmss")), this.connectInfo.DatabaseName),
                            WindowStyle = ProcessWindowStyle.Hidden
                        };

                        // バックアップする
                        Process process = Process.Start(info);
                        return process.WaitForExit(1 * 1000);
                    }
                    else {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
