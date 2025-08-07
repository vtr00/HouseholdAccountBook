using HouseholdAccountBook.DbHandler.Abstract;
using Npgsql;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static HouseholdAccountBook.Others.DbConstants;

namespace HouseholdAccountBook.DbHandler
{
    public partial class NpgsqlDbHandler : DbHandlerBase
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        private const string stringFormat = @"Server={0};Port={1};User Id={2};Password={3};Database={4}";
        /// <summary>
        /// 接続情報
        /// </summary>
        private readonly ConnectInfo connectInfo;

        /// <summary>
        /// 結果通知デリゲート
        /// </summary>
        public delegate void NotifyResult(int? exitCode);

        /// <summary>
        /// <see cref="NpgsqlDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="info">接続情報</param>
        public NpgsqlDbHandler(ConnectInfo info) : this(info.Host, info.Port, info.UserName, info.Password, info.DatabaseName)
        {
            this.connectInfo = info;
        }

        /// <summary>
        /// <see cref="NpgsqlDbHandler"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="port">ポート番号</param>
        /// <param name="userName">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <param name="databaseName">データベース名</param>
        private NpgsqlDbHandler(string uri, int port, string userName, string password, string databaseName)
            : base(new NpgsqlConnection(string.Format(stringFormat, uri, port, userName, password, databaseName)))
        {
            this.DBLibKind = DBLibraryKind.PostgreSQL;
            this.DBKind = DBKind.PostgreSQL;

            _ = this.Open();
        }

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
                    this.connectInfo.Host,
                    this.connectInfo.Port,
                    this.connectInfo.UserName,
                    this.connectInfo.Role,
                    pgPassConf ? "--no-password" : "--password",
                    format.ToString().ToLower(),
                    backupFilePath,
                    this.connectInfo.DatabaseName
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
                    if (process.WaitForExit(pgPassConf ? 10 * 1000 : -1)) {
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
                    this.connectInfo.Host,
                    this.connectInfo.Port,
                    this.connectInfo.UserName,
                    this.connectInfo.Role,
                    pgPassConf ? "--no-password" : "--password",
                    this.connectInfo.DatabaseName,
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
                if (process.WaitForExit(pgPassConf ? 10 * 1000 : -1)) {
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
