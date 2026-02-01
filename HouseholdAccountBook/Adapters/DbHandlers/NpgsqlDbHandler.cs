using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Logger;
using Npgsql;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.DbHandlers
{
    /// <summary>
    /// Npgsql DB Handler
    /// </summary>
    public partial class NpgsqlDbHandler : DbHandlerBase
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string mStringFormat = @"Server={0};Port={1};User Id={2};Password={3};Database={4}";
        /// <summary>
        /// 接続情報
        /// </summary>
        private readonly ConnectInfo mConnectInfo;

        /// <summary>
        /// 結果通知デリゲート
        /// </summary>
        public delegate void NotifyResult(int? exitCode);

        /// <summary>
        /// <see cref="NpgsqlDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public NpgsqlDbHandler(ConnectInfo info) : this(info.Host, info.Port, info.UserName, info.Password, info.DatabaseName) => this.mConnectInfo = info;

        /// <summary>
        /// <see cref="NpgsqlDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="port">ポート番号</param>
        /// <param name="userName">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <param name="databaseName">データベース名</param>
        private NpgsqlDbHandler(string uri, int port, string userName, string password, string databaseName)
            : base(new NpgsqlConnection(string.Format(mStringFormat, uri, port, userName, password, databaseName)))
        {
            this.DBLibKind = DBLibraryKind.PostgreSQL;
            this.DBKind = DBKind.PostgreSQL;
        }

        /// <summary>
        /// DB作成ロールを取得する
        /// </summary>
        /// <returns>DB作成時のロール</returns>
        public string GetDbCreationRoll() => this.mConnectInfo.Role;

        /// <summary>
        /// ダンプを実行する
        /// </summary>
        /// <param name="backupFilePath">バックアップファイルパス</param>
        /// <param name="dumpExePath">pg_dump.exeパス</param>
        /// <param name="passwordInput">パスワード入力方法</param>
        /// <param name="format">ダンプフォーマット</param>
        /// <param name="notifyResultAsync">実行結果を通知するデリゲート</param>
        /// <param name="waitForFinish">処理の完了を待機するか</param>
        /// <returns>成功/失敗/不明</returns>
        public async Task<int?> ExecuteDump(string backupFilePath, string dumpExePath, PostgresPasswordInput passwordInput, PostgresFormat format,
                                            NotifyResult notifyResultAsync = null, bool waitForFinish = true)
        {
            bool pgPassConf = passwordInput == PostgresPasswordInput.PgPassConf;

            // 起動情報を設定する
            ProcessStartInfo info = new() {
                FileName = dumpExePath,
                Arguments = string.Format(
                    "--host {0} --port {1} --username \"{2}\" --role \"{3}\" {4} --format {5} --data-only --verbose --column-inserts --file \"{6}\" \"{7}\"",
                    this.mConnectInfo.Host,
                    this.mConnectInfo.Port,
                    this.mConnectInfo.UserName,
                    this.mConnectInfo.Role,
                    pgPassConf ? "--no-password" : "--password",
                    format.ToString().ToLower(),
                    backupFilePath,
                    this.mConnectInfo.DatabaseName
                ),
                WindowStyle = pgPassConf ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };
#if DEBUG
            Log.Debug(string.Format($"Dump: \"{info.FileName}\" {info.Arguments}"));
#endif

            // バックアップする
            int? exitCode = await Task.Run(() => {
                int? localExitCode = -1;
                Log.Info("Start Backup");

                Process process = Process.Start(info);
                Log.Info("Process executing...");
                if (waitForFinish) {
                    if (process.WaitForExit(-1)) {
                        localExitCode = process.ExitCode;
                        if (localExitCode != 0) {
                            using (StreamReader r = process.StandardError) {
                                string errorMessage = r.ReadToEnd();
                                Log.Error(errorMessage);
                            }
                        }
                        Log.Info("Process exit");
                    }
                    else {
                        Log.Error("Process timeout");
                    }
                }
                else {
                    localExitCode = null;
                }
                return localExitCode;
            });

            notifyResultAsync?.Invoke(exitCode);

            return exitCode;
        }

        /// <summary>
        /// リストアを実行する
        /// </summary>
        /// <param name="backupFilePath">バックアップファイルパス</param>
        /// <param name="restoreExePath"></param>
        /// <returns>成功/失敗</returns>
        public async Task<int> ExecuteRestore(string backupFilePath, string restoreExePath, PostgresPasswordInput passwordInput)
        {
            bool pgPassConf = passwordInput == PostgresPasswordInput.PgPassConf;

            // 起動情報を設定する
            ProcessStartInfo info = new() {
                FileName = restoreExePath,
                Arguments = string.Format(
                    "--host {0} --port {1} --username \"{2}\" --role \"{3}\" {4} --data-only --verbose --dbname \"{5}\" \"{6}\"",
                    this.mConnectInfo.Host,
                    this.mConnectInfo.Port,
                    this.mConnectInfo.UserName,
                    this.mConnectInfo.Role,
                    pgPassConf ? "--no-password" : "--password",
                    this.mConnectInfo.DatabaseName,
                    backupFilePath
                ),
                WindowStyle = pgPassConf ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };
#if DEBUG
            Log.Debug(string.Format($"Restore: \"{info.FileName}\" {info.Arguments}"));
#endif

            // リストアする
            int exitCode = await Task.Run(() => {
                int localExitCode = -1;
                Log.Info("Start Restore");

                Process process = Process.Start(info);
                if (process.WaitForExit(-1)) {
                    localExitCode = process.ExitCode;
                    if (localExitCode != 0) {
                        using (StreamReader r = process.StandardError) {
                            string errorMessage = r.ReadToEnd();
                            Log.Error(errorMessage);
                        }
                    }
                }
                else {
                    Log.Error("Process timeout");
                }
                return localExitCode;
            });

            return exitCode;
        }
    }
}
