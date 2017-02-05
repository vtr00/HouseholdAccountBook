using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 接続情報
        /// </summary>
        private DaoNpgsql.ConnectInfo connectInfo = null;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public App()
        {
            // DB設定ダイアログ終了時に閉じないように設定する
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // 初回起動時
            if (HouseholdAccountBook.Properties.Settings.Default.App_InitFlag) {
                // データベース接続を設定する
                DbSettingWindow dsw = new DbSettingWindow("DB接続設定を入力してください。");
                bool? result = dsw.ShowDialog();

                if (result != true) {
                    connectInfo = null;
                    this.Shutdown();
                    return;
                }

                HouseholdAccountBook.Properties.Settings.Default.App_InitFlag = false;
                HouseholdAccountBook.Properties.Settings.Default.Save();
            }

            // 接続設定を読み込む
            connectInfo = new DaoNpgsql.ConnectInfo() {
                Host = HouseholdAccountBook.Properties.Settings.Default.App_Postgres_Host,
                Port = HouseholdAccountBook.Properties.Settings.Default.App_Postgres_Port,
                UserName = HouseholdAccountBook.Properties.Settings.Default.App_Postgres_UserName,
                Password = HouseholdAccountBook.Properties.Settings.Default.App_Postgres_Password,
#if DEBUG
                DatabaseName = HouseholdAccountBook.Properties.Settings.Default.App_Postgres_DatabaseName_Debug,
#else
                DatabaseName = HouseholdAccountBook.Properties.Settings.Default.App_Postgres_DatabaseName,
#endif
                Role = HouseholdAccountBook.Properties.Settings.Default.App_Postgres_Role
            };

            DaoBuilder builder = new DaoBuilder(connectInfo);
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
                        connectInfo = null;
                        this.Shutdown();
                        return;
                    }
                }
            }

            // 接続できる場合だけメインウィンドウを開く
            MainWindow mw = new MainWindow(builder);
            this.MainWindow = mw;
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            mw.Show();
        }

        /// <summary>
        /// アプリケーション終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if(connectInfo != null) { 
                CreateBackUpFile();
            }
        }

        /// <summary>
        /// バックアップファイルを作成する
        /// </summary>
        private void CreateBackUpFile()
        {
            int backUpNum = HouseholdAccountBook.Properties.Settings.Default.App_BackUpNum;
            string backUpFolderPath = HouseholdAccountBook.Properties.Settings.Default.App_BackUpFolderPath;

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

            // 起動情報を設定する
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = HouseholdAccountBook.Properties.Settings.Default.App_Postgres_DumpExePath;
            info.Arguments = string.Format(
                    "--host {0} --port {1} --username \"{2}\" --role \"{3}\" --no-password --format custom --data-only --verbose --file \"{4}\" \"{5}\"",
                    connectInfo.Host, connectInfo.Port, connectInfo.UserName, connectInfo.Role, 
                    string.Format(@"{0}/{1}.backup", backUpFolderPath, DateTime.Now.ToString("yyyyMMddHHmmss")), connectInfo.DatabaseName);
            info.WindowStyle = ProcessWindowStyle.Hidden;

            // バックアップする
            Process process = Process.Start(info);
            process.WaitForExit(10 * 1000);
        }
    }
}
