using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Dto;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.Properties;
using HouseholdAccountBook.UserControls;
using HouseholdAccountBook.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static HouseholdAccountBook.ConstValue.ConstValue;

namespace HouseholdAccountBook.Windows
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        #region フィールド
        /// <summary>
        /// DAOビルダ
        /// </summary>
        private readonly DaoBuilder builder;

        /// <summary>
        /// 移動登録ウィンドウ
        /// </summary>
        private MoveRegistrationWindow mrw;
        /// <summary>
        /// 項目登録ウィンドウ
        /// </summary>
        private ActionRegistrationWindow arw;
        /// <summary>
        /// 項目リスト登録ウィンドウ
        /// </summary>
        private ActionListRegistrationWindow alrw;
        /// <summary>
        /// CSV比較ウィンドウ
        /// </summary>
        private CsvComparisonWindow ccw;

        /// <summary>
        /// ウィンドウの位置の上端(最終補正値)
        /// </summary>
        private double lastModTop = default;
        /// <summary>
        /// ウィンドウの位置の左端(最終補正値)
        /// </summary>
        private double lastModLeft = default;
        /// <summary>
        /// ウィンドウの高さ(最終補正値)
        /// </summary>
        private double lastModHeight = default;
        /// <summary>
        /// ウィンドウの幅(最終補正値)
        /// </summary>
        private double lastModWidth = default;

        /// <summary>
        /// ウィンドウログ
        /// </summary>
        private readonly WindowLog windowLog = null;
        #endregion

        /// <summary>
        /// 子ウィンドウを開いているか
        /// </summary>
        private bool ChildrenWindowOpened => this.mrw != null || this.arw != null || this.alrw != null || this.ccw != null;
        /// <summary>
        /// 登録ウィンドウを開いているか
        /// </summary>
        private bool RegistrationWindowOpened => this.mrw != null || this.arw != null || this.alrw != null;

        /// <summary>
        /// <see cref="MainWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        public MainWindow(DaoBuilder builder)
        {
            this.Name = "Main";
            this.builder = builder;

            this.windowLog = new WindowLog(this);
            this.windowLog.Log("Constructor", true);

            this.InitializeComponent();
        }

        #region イベントハンドラ
        #region コマンド
        #region ファイル
        /// <summary>
        /// ファイルメニュー操作可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 記帳風月のDBを取り込み可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportKichoHugetsuDbCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 記帳風月のDBを取り込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImportKichoHugetsuDbCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            string directory = string.Empty; // ディレクトリ
            string fileName = string.Empty; // ファイル名
            // 過去に読み込んだときのフォルダとファイルをデフォルトにする
            if (settings.App_KichoDBFilePath != string.Empty) {
                directory = Path.GetDirectoryName(settings.App_KichoDBFilePath);
                fileName = Path.GetFileName(settings.App_KichoDBFilePath);
            }

            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = "ファイル選択",
                Filter = "MDBファイル|*.mdb"
            };

            if (ofd.ShowDialog(this) == false) return;

            if (MessageBox.Show(MessageText.DeleteOldDataNotification, this.Title,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            settings.App_KichoDBFilePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            settings.Save();

            this.WaitCursorCountIncrement();

            bool isOpen = false;
            using (DaoOle daoOle = new DaoOle(ofd.FileName)) {
                isOpen = daoOle.IsOpen;

                if (isOpen) {
                    using (DaoBase dao = this.builder.Build()) {
                        await dao.ExecTransactionAsync(async () => {
                            // 既存のデータを削除する
                            await dao.ExecNonQueryAsync(@"DELETE FROM mst_book;");
                            await dao.ExecNonQueryAsync(@"DELETE FROM mst_category;");
                            await dao.ExecNonQueryAsync(@"DELETE FROM mst_item;");
                            await dao.ExecNonQueryAsync(@"DELETE FROM hst_action;");
                            await dao.ExecNonQueryAsync(@"DELETE FROM hst_group;");
                            await dao.ExecNonQueryAsync(@"DELETE FROM hst_shop;");
                            await dao.ExecNonQueryAsync(@"DELETE FROM hst_remark;");
                            await dao.ExecNonQueryAsync(@"DELETE FROM rel_book_item;");

                            DaoReader reader;
                            // 帳簿IDのシーケンスを更新する
                            reader = await daoOle.ExecQueryAsync(@"SELECT * FROM CBM_BOOK ORDER BY BOOK_ID;");
                            await dao.ExecNonQueryAsync("SELECT setval('mst_book_book_id_seq', @{0});", reader[reader.Count - 1].ToInt("BOOK_ID"));
                            // 帳簿テーブルのレコードを作成する
                            reader.ExecWholeRow((count, record) => {
                                int bookId = record.ToInt("BOOK_ID");
                                string bookName = record["BOOK_NAME"];
                                int balance = record.ToInt("BALANCE");
                                int sortKey = record.ToInt("SORT_KEY");
                                int delFlg = record.ToBoolean("DEL_FLG") ? 1 : 0;

                                dao.ExecNonQueryAsync(@"
INSERT INTO mst_book 
(book_id, book_name, initial_value, book_kind, pay_day, sort_order, del_flg, update_time, updater, insert_time, inserter) 
VALUES (@{0}, @{1}, @{2}, 0, NULL, @{3}, @{4}, 'now', @{5}, 'now', @{6});",
                                    bookId, bookName, balance, sortKey, delFlg, Updater, Inserter);
                                return true;
                            });

                            // 分類IDのシーケンスを更新する
                            reader = await daoOle.ExecQueryAsync(@"SELECT * FROM CBM_CATEGORY ORDER BY CATEGORY_ID;");
                            await dao.ExecNonQueryAsync("SELECT setval('mst_category_category_id_seq', @{0});", reader[reader.Count - 1].ToInt("CATEGORY_ID"));
                            // 分類テーブルのレコードを作成する
                            reader.ExecWholeRow((count, record) => {
                                int categoryId = record.ToInt("CATEGORY_ID");
                                string categoryName = record["CATEGORY_NAME"];
                                int rexpDiv = record.ToInt("REXP_DIV") - 1;
                                int sortKey = record.ToInt("SORT_KEY");
                                int delFlg = record.ToBoolean("DEL_FLG") ? 1 : 0;

                                dao.ExecNonQueryAsync(@"
INSERT INTO mst_category
(category_id, category_name, balance_kind, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, 'now', @{5}, 'now', @{6});",
                                    categoryId, categoryName, rexpDiv, sortKey, delFlg, Updater, Inserter);
                                return true;
                            });

                            // 項目IDのシーケンスを更新する
                            reader = await daoOle.ExecQueryAsync(@"SELECT * FROM CBM_ITEM ORDER BY ITEM_ID;");
                            await dao.ExecNonQueryAsync("SELECT setval('mst_item_item_id_seq', @{0});", reader[reader.Count - 1].ToInt("ITEM_ID"));
                            // 項目テーブルのレコードを作成する
                            reader.ExecWholeRow((count, record) => {
                                int itemId = record.ToInt("ITEM_ID");
                                string itemName = record["ITEM_NAME"];
                                int moveFlg = record.ToBoolean("MOVE_FLG") ? 1 : 0;
                                int categoryId = record.ToInt("CATEGORY_ID");
                                int sortKey = record.ToInt("SORT_KEY");
                                int delFlg = record.ToBoolean("DEL_FLG") ? 1 : 0;

                                dao.ExecNonQueryAsync(@"
INSERT INTO mst_item
(item_id, item_name, category_id, move_flg, advance_flg, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, @{6}, 'now', @{7}, 'now', @{8});",
                                    itemId, itemName, categoryId, moveFlg, 0, sortKey, delFlg, Updater, Inserter);
                                return true;
                            });

                            // 帳簿項目IDのシーケンスを更新する
                            reader = await daoOle.ExecQueryAsync(@"SELECT * FROM CBT_ACT ORDER BY ACT_ID;");
                            await dao.ExecNonQueryAsync("SELECT setval('hst_action_action_id_seq', @{0});", reader[reader.Count - 1].ToInt("ACT_ID"));
                            int maxGroupId = 0; // グループIDの最大値
                            // 帳簿テーブルのレコードを作成する
                            await reader.ExecWholeRowAsync(async (count, record) => {
                                int actId = record.ToInt("ACT_ID");
                                int bookId = record.ToInt("BOOK_ID");
                                int itemId = record.ToInt("ITEM_ID");
                                string actDt = record["ACT_DT"];
                                int income = record.ToInt("INCOME");
                                int expense = record.ToInt("EXPENSE");
                                int? groupId = record.ToInt("GROUP_ID") == 0 ? null : (int?)record.ToInt("GROUP_ID");
                                string noteName = record["NOTE_NAME"];
                                int delFlg = record.ToBoolean("DEL_FLG") ? 1 : 0;

                                await dao.ExecNonQueryAsync(@"
INSERT INTO hst_action
(action_id, book_id, item_id, act_time, act_value, shop_name, group_id, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, NULL, @{5}, @{6}, @{7}, 'now', @{8}, 'now', @{9});",
                                    actId, bookId, itemId, DateTime.Parse(actDt), (income != 0 ? income : -expense),
                                    groupId, noteName, delFlg, Updater, Inserter);

                                // groupId が存在するとき
                                if (groupId != null) {
                                    // グループIDの最大値を更新する
                                    if (maxGroupId < groupId) { maxGroupId = groupId.Value; }

                                    reader = await dao.ExecQueryAsync("SELECT * FROM hst_group WHERE group_id = @{0};", groupId);
                                    // groupId のレコードが登録されていないとき
                                    if (reader.Count == 0) {
                                        // グループの種類を調べる
                                        reader = await daoOle.ExecQueryAsync("SELECT * FROM CBT_ACT WHERE GROUP_ID = @{0};", groupId);
                                        GroupKind groupKind = GroupKind.Repeat;
                                        int? tmpBookId = null;
                                        reader.ExecWholeRow((count2, record2) => {
                                            if (tmpBookId == null) { // 1つ目のレコードの帳簿IDを記録する
                                                tmpBookId = record2.ToInt("BOOK_ID");
                                            }
                                            else { // 2つ目のレコードの帳簿IDが1つ目と一致するか
                                                if (tmpBookId != record2.ToInt("BOOK_ID")) {
                                                    // 帳簿が一致しない場合は移動
                                                    groupKind = GroupKind.Move;
                                                }
                                                else {
                                                    // 帳簿が一致する場合は繰り返し
                                                    groupKind = GroupKind.Repeat;
                                                }
                                                return false;
                                            }
                                            return true;
                                        });

                                        // グループテーブルのレコードを作成する
                                        await dao.ExecNonQueryAsync(@"
INSERT INTO hst_group
(group_id, group_kind, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, NULL, 0, 'now', @{2}, 'now', @{3});",
                                            groupId, (int)groupKind, Updater, Inserter);
                                    }
                                }
                                return true;
                            });
                            // グループIDのシーケンスを更新する
                            await dao.ExecNonQueryAsync("SELECT setval('hst_group_group_id_seq', @{0});", maxGroupId);

                            // 備考テーブルのレコードを作成する
                            reader = await daoOle.ExecQueryAsync(@"SELECT * FROM CBT_NOTE;");
                            reader.ExecWholeRow((count, record) => {
                                int itemId = record.ToInt("ITEM_ID");
                                string noteName = record["NOTE_NAME"];
                                int delFlg = record.ToBoolean("DEL_FLG") ? 1 : 0;

                                dao.ExecNonQueryAsync(@"
INSERT INTO hst_remark
(item_id, remark, remark_kind, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, 0, 'now', @{2}, 'now', @{3}, 'now', @{4});",
                                    itemId, noteName, delFlg, Updater, Inserter);
                                return true;
                            });

                            // 帳簿-項目テーブルのレコードを作成する
                            reader = await daoOle.ExecQueryAsync("SELECT * FROM CBR_BOOK;");
                            reader.ExecWholeRow((count, record) => {
                                int bookId = record.ToInt("BOOK_ID");
                                int itemId = record.ToInt("ITEM_ID");
                                int delFlg = record.ToBoolean("DEL_FLG") ? 1 : 0;

                                dao.ExecNonQueryAsync(@"
INSERT INTO rel_book_item
(book_id, item_id, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, 'now', @{3}, 'now', @{4});",
                                    bookId, itemId, delFlg, Updater, Inserter);
                                return true;
                            });
                        });
                    }
                }
            }

            if (isOpen) {
                await this.UpdateAsync(isUpdateBookList:true, isScroll: true, isUpdateActDateLastEdited: true);

                this.WaitCursorCountDecrement();

                MessageBox.Show(MessageText.FinishToImport, MessageTitle.Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                this.WaitCursorCountDecrement();

                MessageBox.Show(MessageText.FoultToImport, MessageTitle.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// カスタムファイル形式入力可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportCustomFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Properties.Settings.Default.App_Postgres_RestoreExePath != string.Empty && !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// カスタム形式ファイルを取り込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImportCustomFileCommand_Excuted(object sender, ExecutedRoutedEventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            string directory = string.Empty;
            string fileName = string.Empty;
            // 過去に読み込んだときのフォルダとファイルをデフォルトにする
            if (settings.App_CustomFormatFilePath != string.Empty) {
                directory = Path.GetDirectoryName(settings.App_CustomFormatFilePath);
                fileName = Path.GetFileName(settings.App_CustomFormatFilePath);
            }

            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = "ファイル選択",
                Filter = "カスタム形式ファイル|*.*",
                CheckPathExists = true
            };

            if (ofd.ShowDialog(this) == false) return;

            if (MessageBox.Show(MessageText.DeleteOldDataNotification, this.Title,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            settings.App_CustomFormatFilePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            settings.Save();

            this.WaitCursorCountIncrement();

            using (DaoBase dao = this.builder.Build()) {
                // 既存のデータを削除する
                await dao.ExecNonQueryAsync(@"DELETE FROM mst_book;");
                await dao.ExecNonQueryAsync(@"DELETE FROM mst_category;");
                await dao.ExecNonQueryAsync(@"DELETE FROM mst_item;");
                await dao.ExecNonQueryAsync(@"DELETE FROM hst_action;");
                await dao.ExecNonQueryAsync(@"DELETE FROM hst_group;");
                await dao.ExecNonQueryAsync(@"DELETE FROM hst_shop;");
                await dao.ExecNonQueryAsync(@"DELETE FROM hst_remark;");
                await dao.ExecNonQueryAsync(@"DELETE FROM rel_book_item;");
            }

            // 起動情報を設定する
            ProcessStartInfo info = new ProcessStartInfo() {
                FileName = settings.App_Postgres_RestoreExePath,
                Arguments = string.Format(
                    "--host {0} --port {1} --username \"{2}\" --role \"{3}\" --no-password --data-only --verbose --dbname \"{5}\" \"{4}\"",
                    settings.App_Postgres_Host,
                    settings.App_Postgres_Port,
                    settings.App_Postgres_UserName,
                    settings.App_Postgres_Role,
                    ofd.FileName,
#if DEBUG
                    settings.App_Postgres_DatabaseName_Debug
#else
                    settings.App_Postgres_DatabaseName
#endif
                    ),
                WindowStyle = ProcessWindowStyle.Hidden
            };
#if DEBUG
            Console.WriteLine(string.Format("リストア：\"{0}\" {1}", info.FileName, info.Arguments));
#endif

            // リストアする
            Process process = Process.Start(info);
            process.WaitForExit(10 * 1000);

            if (process.ExitCode == 0) {
                await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);

                this.WaitCursorCountDecrement();

                MessageBox.Show(MessageText.FinishToImport, MessageTitle.Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                this.WaitCursorCountDecrement();

                MessageBox.Show(MessageText.FoultToImport, MessageTitle.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// カスタム形式ファイル出力可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportCustomFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Properties.Settings.Default.App_Postgres_DumpExePath != string.Empty && !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// カスタム形式ファイルに出力する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportCustomFileCommand_Excuted(object sender, ExecutedRoutedEventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            string directory = string.Empty;
            string fileName = string.Empty;
            // 過去に読み込んだときのフォルダとファイルをデフォルトにする
            if (settings.App_CustomFormatFilePath != string.Empty) {
                directory = Path.GetDirectoryName(settings.App_CustomFormatFilePath);
                fileName = Path.GetFileName(settings.App_CustomFormatFilePath);
            }
            else {
                directory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }

            SaveFileDialog sfd = new SaveFileDialog() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = "ファイル選択",
                Filter = "カスタム形式ファイル|*.*"
            };

            if (sfd.ShowDialog(this) == false) return;

            settings.App_CustomFormatFilePath = Path.Combine(sfd.InitialDirectory, sfd.FileName);
            settings.Save();

            this.WaitCursorCountIncrement();

            // 起動情報を設定する
            ProcessStartInfo info = new ProcessStartInfo() {
                FileName = settings.App_Postgres_DumpExePath,
                Arguments = string.Format(
                    "--host {0} --port {1} --username \"{2}\" --role \"{3}\" --no-password --format custom --data-only --verbose --file \"{4}\" \"{5}\"",
                    settings.App_Postgres_Host,
                    settings.App_Postgres_Port,
                    settings.App_Postgres_UserName,
                    settings.App_Postgres_Role,
                    sfd.FileName,
#if DEBUG
                    settings.App_Postgres_DatabaseName_Debug
#else
                    settings.App_Postgres_DatabaseName
#endif
                    ),
                WindowStyle = ProcessWindowStyle.Hidden
            };
#if DEBUG
            Console.WriteLine(string.Format("ダンプ：\"{0}\" {1}", info.FileName, info.Arguments));
#endif

            // バックアップする
            Process process = Process.Start(info);
            process.WaitForExit(10 * 1000);
            this.WaitCursorCountDecrement();

            if (process.ExitCode == 0) {
                MessageBox.Show(MessageText.FinishToExport, MessageTitle.Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                MessageBox.Show(MessageText.FoultToExport, MessageTitle.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// 手動バックアップ可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void BackUpCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 手動バックアップを行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackUpCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WaitCursorCountIncrement();

            this.CreateBackUpFile();

            this.WaitCursorCountDecrement();

            MessageBox.Show(MessageText.FinishToBackUp, MessageTitle.Information);
        }

        /// <summary>
        /// ウィンドウを閉じれるか
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void ExitWindowCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 編集
        /// <summary>
        /// 編集メニュー操作可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 移動操作可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveToBookCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択 かつ 帳簿が2つ以上存在 かつ 選択されている帳簿が存在 かつ 子ウィンドウが開いていない
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab && this.WVM.BookVMList?.Count >= 2 && this.WVM.SelectedBookVM != null && !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 移動登録ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveToBookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.mrw = new MoveRegistrationWindow(this.builder, this.WVM.SelectedBookVM.Id,
                this.WVM.DisplayedTermKind == TermKind.Monthly ? this.WVM.DisplayedMonth : null, this.WVM.SelectedActionVM?.ActTime) { Owner = this };
            this.mrw.LoadWindowSetting();

            // 登録時イベントを登録する
            this.mrw.Registrated += async (sender2, e2) => {
                // 帳簿一覧タブを更新する
                await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
            };
            // クローズ時イベントを登録する
            this.mrw.Closed += (sender3, e3) => {
                this.mrw = null;
                this.Activate();
                this.actionDataGrid.Focus();
            };
            this.mrw.Show();
        }

        /// <summary>
        /// 項目追加可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択 かつ 選択されている帳簿が存在 かつ 子ウィンドウが開いていない
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab && this.WVM.SelectedBookVM != null && !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 項目追加ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.arw = new ActionRegistrationWindow(this.builder, this.WVM.SelectedBookVM.Id,
                this.WVM.DisplayedTermKind == TermKind.Monthly ? this.WVM.DisplayedMonth : null, this.WVM.SelectedActionVM?.ActTime) { Owner = this };
            this.arw.LoadWindowSetting();

            // 登録時イベントを登録する
            this.arw.Registrated += async (sender2, e2) => {
                // 帳簿一覧タブを更新する
                await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
            };
            // クローズ時イベントを登録する
            this.arw.Closed += (sender3, e3) => {
                this.arw = null;
                this.Activate();
                this.actionDataGrid.Focus();
            };
            this.arw.Show();
        }

        /// <summary>
        /// 項目リスト追加可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionListCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択 かつ 選択されている帳簿が存在 かつ 子ウィンドウを開いていない
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab && this.WVM.SelectedBookVM != null && !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 項目リスト追加ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionListCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.alrw = new ActionListRegistrationWindow(this.builder, this.WVM.SelectedBookVM.Id,
                this.WVM.DisplayedTermKind == TermKind.Monthly ? this.WVM.DisplayedMonth : null, this.WVM.SelectedActionVM?.ActTime) { Owner = this };
            this.alrw.LoadWindowSetting();

            // 登録時イベントを登録する
            this.alrw.Registrated += async (sender2, e2) => {
                // 帳簿一覧タブを更新する
                await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
            };
            // クローズ時イベントを登録する
            this.alrw.Closed += (sender3, e3) => {
                this.alrw = null;
                this.Activate();
                this.actionDataGrid.Focus();
            };
            this.alrw.Show();
        }

        /// <summary>
        /// 項目編集可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditActionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択 かつ 選択されている帳簿項目が1つだけ存在 かつ 選択している帳簿項目のIDが0より大きい かつ 子ウィンドウを開いていない
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab &&
                           this.WVM.SelectedActionVMList.Count == 1 && this.WVM.SelectedActionVMList[0].ActionId > 0 && !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 項目編集ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EditActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // グループ種別を特定する
            int? groupKind = null;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT A.group_id, G.group_kind
FROM hst_action A
LEFT JOIN (SELECT * FROM hst_group WHERE del_flg = 0) G ON G.group_id = A.group_id
WHERE A.action_id = @{0} AND A.del_flg = 0;", this.WVM.SelectedActionVM.ActionId);

                reader.ExecARow((record) => {
                    groupKind = record.ToNullableInt("group_kind");
                });
            }

            switch (groupKind) {
                case (int)GroupKind.Move:
                    // 移動の編集時の処理
                    this.mrw = new MoveRegistrationWindow(this.builder, this.WVM.SelectedBookVM.Id, this.WVM.SelectedActionVM.GroupId.Value) { Owner = this };
                    this.mrw.LoadWindowSetting();

                    // 登録時イベントを登録する
                    this.mrw.Registrated += async (sender2, e2) => {
                        // 帳簿一覧タブを更新する
                        await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                    };
                    // クローズ時イベントを登録する
                    this.mrw.Closed += (sender3, e3) => {
                        this.mrw = null;
                        this.Activate();
                        this.actionDataGrid.Focus();
                    };
                    this.mrw.Show();
                    break;
                case (int)GroupKind.ListReg:
                    // リスト登録された帳簿項目の編集時の処理
                    this.alrw = new ActionListRegistrationWindow(this.builder, this.WVM.SelectedActionVM.GroupId.Value) { Owner = this };
                    this.alrw.LoadWindowSetting();

                    // 登録時イベントを登録する
                    this.alrw.Registrated += async (sender2, e2) => {
                        // 帳簿一覧タブを更新する
                        await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                    };
                    // クローズ時イベントを登録する
                    this.alrw.Closed += (sender3, e3) => {
                        this.alrw = null;
                        this.Activate();
                        this.actionDataGrid.Focus();
                    };
                    this.alrw.Show();
                    break;
                case (int)GroupKind.Repeat:
                default:
                    // 移動・リスト登録以外の帳簿項目の編集時の処理
                    this.arw = new ActionRegistrationWindow(this.builder, this.WVM.SelectedActionVM.ActionId) { Owner = this };
                    this.arw.LoadWindowSetting();

                    // 登録時イベントを登録する
                    this.arw.Registrated += async (sender2, e2) => {
                        // 帳簿一覧タブを更新する
                        await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                    };

                    this.arw.Closed += (sender3, e3) => {
                        this.arw = null;
                        this.Activate();
                        this.actionDataGrid.Focus();
                    };
                    this.arw.Show();
                    break;
            }
        }

        /// <summary>
        /// 項目複製可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyActionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択 かつ 選択されている帳簿項目が1つだけ存在 かつ 選択している帳簿項目のIDが0より大きい かつ 子ウィンドウを開いていない
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab &&
                           this.WVM.SelectedActionVMList.Count == 1 && this.WVM.SelectedActionVMList[0].ActionId > 0 && !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 項目複製ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CopyActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // グループ種別を特定する
            int? groupKind = null;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT A.group_id, G.group_kind
FROM hst_action A
LEFT JOIN (SELECT * FROM hst_group WHERE del_flg = 0) G ON G.group_id = A.group_id
WHERE A.action_id = @{0} AND A.del_flg = 0;", this.WVM.SelectedActionVM.ActionId);

                reader.ExecARow((record) => {
                    groupKind = record.ToNullableInt("group_kind");
                });
            }

            if (groupKind == null || groupKind == (int)GroupKind.Repeat) {
                // 移動以外の帳簿項目の複製時の処理
                this.arw = new ActionRegistrationWindow(this.builder, this.WVM.SelectedActionVM.ActionId, RegistrationMode.Copy) { Owner = this };
                this.arw.LoadWindowSetting();

                // 登録時イベントを登録する
                this.arw.Registrated += async (sender2, e2) => {
                    // 帳簿一覧タブを更新する
                    await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                };
                this.arw.Closed += (sender3, e3) => {
                    this.arw = null;
                    this.Activate();
                    this.actionDataGrid.Focus();
                };
                this.arw.Show();
            }
            else {
                // 移動の複製時の処理
                this.mrw = new MoveRegistrationWindow(this.builder, this.WVM.SelectedBookVM.Id, this.WVM.SelectedActionVM.GroupId.Value, RegistrationMode.Copy) { Owner = this };
                this.mrw.LoadWindowSetting();

                // 登録時イベントを登録する
                this.mrw.Registrated += async (sender2, e2) => {
                    // 帳簿一覧タブを更新する
                    await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                };
                this.mrw.Closed += (sender3, e3) => {
                    this.mrw = null;
                    this.Activate();
                    this.actionDataGrid.Focus();
                };
                this.mrw.Show();
            }
        }

        /// <summary>
        /// 項目削除可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteActionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択 かつ 選択している帳簿項目が存在 かつ 選択している帳簿項目にIDが0より大きいものが存在 かつ 子ウィンドウが開いていない
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab &&
                           this.WVM.SelectedActionVMList.Where((vm) => { return vm.ActionId > 0; }).Count() != 0 && !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 項目削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(MessageText.DeleteNotification, this.Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                // 帳簿項目IDが0を超える項目についてループ
                foreach (ActionViewModel vm in this.WVM.SelectedActionVMList.Where((vm) => { return vm.ActionId > 0; })) {
                    int actionId = vm.ActionId;
                    int? groupId = vm.GroupId;

                    using (DaoBase dao = this.builder.Build()) {
                        await dao.ExecTransactionAsync(async () => {
                            // 対象となる帳簿項目を削除する
                            await dao.ExecNonQueryAsync(@"
UPDATE hst_action SET del_flg = 1, update_time = 'now', updater = @{1} 
WHERE action_id = @{0};", actionId, Updater);

                            if (groupId.HasValue) {
                                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT group_kind FROM hst_group
WHERE group_id = @{0} AND del_flg = 0;", groupId);

                                int groupKind = (int)GroupKind.Repeat;
                                reader.ExecARow((record) => { groupKind = record.ToInt("group_kind"); });

                                switch (groupKind) {
                                    case (int)GroupKind.Move: {
                                            // 移動の場合、削除項目と同じグループIDを持つ帳簿項目を削除する
                                            await dao.ExecNonQueryAsync(@"
UPDATE hst_action SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE group_id = @{1};", Updater, groupId);
                                        }
                                        break;
                                    case (int)GroupKind.Repeat: {
                                            // 繰返しの場合、削除項目の日時以降の同じグループIDを持つ帳簿項目を削除する
                                            await dao.ExecNonQueryAsync(@"
UPDATE hst_action SET del_flg = 1, update_time = 'now', updater = @{1}
WHERE group_id = @{2} AND act_time >= (SELECT act_time FROM hst_action WHERE action_id = @{0});", actionId, Updater, groupId);
                                        }
                                        break;
                                }

                                reader = await dao.ExecQueryAsync(@"
SELECT action_id FROM hst_action
WHERE group_id = @{0} AND del_flg = 0;", groupId);

                                // 同じグループIDを持つ帳簿項目が1つだけの場合にグループIDをクリアする
                                if (reader.Count == 1) {
                                    await reader.ExecARowAsync(async (record) => {
                                        await dao.ExecNonQueryAsync(@"
UPDATE hst_action SET group_id = null, update_time = 'now', updater = @{1}
WHERE action_id = @{0} AND del_flg = 0;", record.ToInt("action_id"), Updater);
                                    });
                                }

                                // 同じグループIDを持つ帳簿項目が存在しなくなる場合にグループを削除する
                                if (reader.Count <= 1) {
                                    await dao.ExecNonQueryAsync(@"
UPDATE hst_group SET del_flg = 1, update_time = 'now', updater = @{1}
WHERE group_id = @{0} AND del_flg = 0;", groupId, Updater);
                                }
                            }
                        });
                    }
                }

                // 帳簿一覧タブを更新する
                await this.UpdateBookTabDataAsync(isUpdateActDateLastEdited: true);
            }
        }

        /// <summary>
        /// CSV一致チェック可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeIsMatchCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択 かつ 選択している帳簿項目が存在 かつ 選択している帳簿項目にIDが0より大きいものが存在 かつ 登録ウィンドウが開いていない
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab &&
                           this.WVM.SelectedActionVMList.Where((vm) => { return vm.ActionId > 0; }).Count() != 0 && !this.RegistrationWindowOpened;
        }

        /// <summary>
        /// CSV一致チェック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeIsMatchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (DaoBase dao = this.builder.Build()) {
                // 帳簿項目IDが0を超える項目についてループ
                foreach (ActionViewModel vm in this.WVM.SelectedActionVMList.Where((vm) => { return vm.ActionId > 0; })) {
                    await dao.ExecNonQueryAsync(@"
UPDATE hst_action
SET is_match = @{0}, update_time = 'now', updater = @{1}
WHERE action_id = @{2} and is_match <> @{0};", vm.IsMatch ? 1 : 0, Updater, vm.ActionId);
                }
            }
        }
        #endregion

        #region 表示
        #region タブ切替
        /// <summary>
        /// 帳簿タブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateBookTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedTab != Tabs.BooksTab;
        }

        /// <summary>
        /// 帳簿タブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateBookTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WVM.SelectedTab = Tabs.BooksTab;
        }

        /// <summary>
        /// 日別グラフタブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateDailyGraphTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedTab != Tabs.DailyGraphTab;
        }

        /// <summary>
        /// 日別グラフタブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateDailyGraphTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WVM.SelectedTab = Tabs.DailyGraphTab;
        }

        /// <summary>
        /// 月別一覧タブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateMonthlyListTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedTab != Tabs.MonthlyListTab;
        }

        /// <summary>
        /// 月別一覧タブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateMonthlyListTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WVM.SelectedTab = Tabs.MonthlyListTab;
        }

        /// <summary>
        /// 月別グラフタブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateMonthlyGraphTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedTab != Tabs.MonthlyGraphTab;
        }

        /// <summary>
        /// 月別グラフタブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateMonthlyGraphTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WVM.SelectedTab = Tabs.MonthlyGraphTab;
        }

        /// <summary>
        /// 年別一覧タブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateYearlyListTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedTab != Tabs.YearlyListTab;
        }

        /// <summary>
        /// 年別一覧タブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateYearlyListTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WVM.SelectedTab = Tabs.YearlyListTab;
        }

        /// <summary>
        /// 年別グラフタブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateYearlyGraphTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedTab != Tabs.YearlyGraphTab;
        }

        /// <summary>
        /// 年別グラフタブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateYearlyGraphTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WVM.SelectedTab = Tabs.YearlyGraphTab;
        }
        #endregion

        #region 帳簿表示
        /// <summary>
        /// 先月を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToLastMonthCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿/日別一覧タブを選択している
            e.CanExecute = (this.WVM.SelectedTab == Tabs.BooksTab || this.WVM.SelectedTab == Tabs.DailyGraphTab) && this.WVM.DisplayedTermKind == TermKind.Monthly;
        }

        /// <summary>
        /// (表示している月の)先月を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GoToLastMonthCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WaitCursorCountIncrement();

            this.WVM.DisplayedMonth = this.WVM.DisplayedMonth.Value.AddMonths(-1);
            await this.UpdateAsync(isUpdateBookList: true, isScroll: true);

            this.WaitCursorCountDecrement();
        }

        /// <summary>
        /// 今月を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToThisMonthCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DateTime thisMonth = DateTime.Today.GetFirstDateOfMonth();
            // 帳簿/月間一覧タブを選択している かつ 今月が表示されていない
            e.CanExecute = (this.WVM.SelectedTab == Tabs.BooksTab || this.WVM.SelectedTab == Tabs.DailyGraphTab) &&
                           (this.WVM.DisplayedTermKind == TermKind.Selected || !(thisMonth <= this.WVM.DisplayedMonth && this.WVM.DisplayedMonth < thisMonth.AddMonths(1)));
        }

        /// <summary>
        /// 今月を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GoToThisMonthCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WaitCursorCountIncrement();

            this.WVM.DisplayedMonth = DateTime.Now.GetFirstDateOfMonth();
            await this.UpdateAsync(isUpdateBookList: true, isScroll: true);

            this.WaitCursorCountDecrement();
        }

        /// <summary>
        /// 翌月を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToNextMonthCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿/月間一覧タブを選択している
            e.CanExecute = (this.WVM.SelectedTab == Tabs.BooksTab || this.WVM.SelectedTab == Tabs.DailyGraphTab) && this.WVM.DisplayedTermKind == TermKind.Monthly;
        }

        /// <summary>
        /// (表示している月の)翌月を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GoToNextMonthCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WaitCursorCountIncrement();

            this.WVM.DisplayedMonth = this.WVM.DisplayedMonth.Value.AddMonths(1);
            await this.UpdateAsync(isUpdateBookList: true, isScroll: true);

            this.WaitCursorCountDecrement();
        }

        /// <summary>
        /// 日毎期間を選択するウィンドウを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OpenSelectingDailyTermWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TermWindow stw = null;
            switch (this.WVM.DisplayedTermKind) {
                case TermKind.Monthly:
                    stw = new TermWindow(builder, this.WVM.DisplayedMonth.Value) { Owner = this };
                    break;
                case TermKind.Selected:
                    stw = new TermWindow(builder, this.WVM.StartDate, this.WVM.EndDate) { Owner = this };
                    break;
            }
            stw.LoadWindowSetting();

            if (stw.ShowDialog() == true) {
                this.WaitCursorCountIncrement();

                this.WVM.StartDate = stw.WVM.StartDate;
                this.WVM.EndDate = stw.WVM.EndDate;

                await this.UpdateAsync(isUpdateBookList: true, isScroll: true);

                this.WaitCursorCountDecrement();
            }
        }
        #endregion

        #region 年間表示
        /// <summary>
        /// 前年を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToLastYearCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 年間一覧/年間グラフタブを選択している
            e.CanExecute = this.WVM.SelectedTab == Tabs.MonthlyListTab || this.WVM.SelectedTab == Tabs.MonthlyGraphTab;
        }

        /// <summary>
        /// (表示している年の)前年を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GoToLastYearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WaitCursorCountIncrement();

            this.WVM.DisplayedYear = this.WVM.DisplayedYear.AddYears(-1);
            await this.UpdateAsync(isUpdateBookList: true);

            this.WaitCursorCountDecrement();
        }

        /// <summary>
        /// 今年を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToThisYearCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DateTime thisYear = DateTime.Now.GetFirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);
            // 年間一覧/年間グラフタブを選択している かつ 今年が表示されていない
            e.CanExecute = (this.WVM.SelectedTab == Tabs.MonthlyListTab || this.WVM.SelectedTab == Tabs.MonthlyGraphTab) &&
                           !(thisYear <= this.WVM.DisplayedYear && this.WVM.DisplayedYear < thisYear.AddYears(1));
        }

        /// <summary>
        /// 今年を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GoToThisYearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WaitCursorCountIncrement();

            this.WVM.DisplayedYear = DateTime.Now.GetFirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);
            await this.UpdateAsync(isUpdateBookList: true);

            this.WaitCursorCountDecrement();
        }

        /// <summary>
        /// 来年を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToNextYearCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 年間一覧/年間グラフタブを選択している
            e.CanExecute = this.WVM.SelectedTab == Tabs.MonthlyListTab || this.WVM.SelectedTab == Tabs.MonthlyGraphTab;
        }

        /// <summary>
        /// (表示している年の)翌年を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GoToNextYearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WaitCursorCountIncrement();

            this.WVM.DisplayedYear = this.WVM.DisplayedYear.AddYears(1);
            await this.UpdateAsync(isUpdateBookList: true);

            this.WaitCursorCountDecrement();
        }
        #endregion

        /// <summary>
        /// 画面表示を更新する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WaitCursorCountIncrement();

            await this.UpdateAsync(isUpdateBookList: true);

            this.WaitCursorCountDecrement();
        }
        #endregion

        #region ツール
        /// <summary>
        /// ツールメニュー操作可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 設定操作可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void OpenSettingsWindowCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 設定ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OpenSettingsWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow(this.builder) { Owner = this };
            sw.LoadWindowSetting();

            if (sw.ShowDialog() == true) {
                this.WaitCursorCountIncrement();

                await this.UpdateAsync(isUpdateBookList: true);
                this.WVM.RaiseDisplayedYearChanged();

                this.WaitCursorCountDecrement();
            }
        }

        /// <summary>
        /// 帳簿ツールメニュー操作可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolInBookCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab;
        }
        
        /// <summary>
        /// CSV比較可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void OpenCsvComparisonWindowCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// CSV比較ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenCsvComparisonWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.ccw = new CsvComparisonWindow(this.builder, this.WVM.SelectedBookVM.Id) { Owner = this };
            this.ccw.LoadWindowSetting();

            // 帳簿項目の一致フラグ変更時のイベントを登録する
            this.ccw.IsMatchChanged += (sender2, e2) => {
                ActionViewModel vm = this.WVM.ActionVMList.FirstOrDefault((tmpVM) => { return tmpVM.ActionId == e2.Value1; });
                if (vm != null) {
                    // UI上の表記だけを更新する
                    vm.IsMatch = e2.Value2;
                }
            };
            // 帳簿項目変更時のイベントを登録する
            this.ccw.ActionChanged += async (sender3, e3) => {
                this.WaitCursorCountIncrement();

                // 帳簿一覧タブを更新する
                await this.UpdateBookTabDataAsync(isScroll: false, isUpdateActDateLastEdited: true);

                this.WaitCursorCountDecrement();
            };
            // 帳簿変更時のイベントを登録する
            this.ccw.BookChanged += (sender4, e4) => {
                this.WVM.SelectedBookVM = this.WVM.BookVMList.FirstOrDefault((vm) => { return vm.Id == e4.Value; });
            };
            // クローズ時イベントを登録する
            this.ccw.Closed += (sender4, e4) => {
                this.ccw = null;
                this.Activate();
            };
            this.ccw.Show();
        }
        #endregion

        #region ヘルプ
        /// <summary>
        /// バージョンウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenVersionWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            VersionWindow vw = new VersionWindow { Owner = this };
            vw.MoveOwnersCenter();

            vw.ShowDialog();
        }
        #endregion
        #endregion



        #region ウィンドウ

        private int wrapperCount = 0;
        /// <summary>
        /// ウィンドウのサイズと位置を変更する際のラッパ関数
        /// </summary>
        /// <param name="action"></param>
        private void ChangedLocationOrSizeWrapper(Action action)
        {
            if (wrapperCount == 0) {
                this.SizeChanged -= this.MainWindow_SizeChanged;
                this.LocationChanged -= this.MainWindow_LocationChanged;
            }
            wrapperCount++;

            action.Invoke();

            wrapperCount--;
            if (wrapperCount == 0) {
                this.SizeChanged += this.MainWindow_SizeChanged;
                this.LocationChanged += this.MainWindow_LocationChanged;
            }
        }

        /// <summary>
        /// ウィンドウ初期化完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            this.windowLog.Log("Initialized", true);

            this.lastModTop = this.Top;
            this.lastModLeft = this.Left;
            this.lastModWidth = this.Width;
            this.lastModHeight = this.Height;

            this.LoadWindowSetting();
        }

        /// <summary>
        /// ウィンドウ読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Info();

            this.windowLog.Log("WindowLoaded", true);

            Properties.Settings settings = Properties.Settings.Default;

            // 帳簿リスト更新
            await this.UpdateBookListAsync(settings.MainWindow_SelectedBookId);

            // タブ選択
            if (settings.MainWindow_SelectedTabIndex != -1) {
                this.WVM.SelectedTabIndex = settings.MainWindow_SelectedTabIndex;
            }
            // グラフ種別1選択
            if (settings.MainWindow_SelectedGraphKindIndex != -1) {
                this.WVM.SelectedGraphKind1Index = settings.MainWindow_SelectedGraphKindIndex;
            }
            // グラフ種別2選択
            if (settings.MainWindow_SelectedGraphKind2Index != -1) {
                this.WVM.SelectedGraphKind2Index = settings.MainWindow_SelectedGraphKind2Index;
            }

            Log.Info($"SelectedTabIndex: {this.WVM.SelectedTabIndex} SelectedGraphKind1Index: {this.WVM.SelectedGraphKind1Index} SelectedGraphKind2Index: {this.WVM.SelectedGraphKind2Index}");

            await this.UpdateAsync(isScroll: true, isUpdateActDateLastEdited: true);

            this.RegisterEventHandlerToWVM();
        }

        /// <summary>
        /// ウィンドウクローズ時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 他のウィンドウを開いているときは閉じない
            if (this.ChildrenWindowOpened) {
                e.Cancel = true;
                return;
            }

            Properties.Settings settings = Properties.Settings.Default;
            settings.Reload();

            this.Hide();

            if (settings.App_BackUpFlagAtClosing) {
#if !DEBUG
                this.CreateBackUpFile();
#endif
            }
        }

        /// <summary>
        /// ウィンドウクローズ後
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            this.SaveWindowSetting();

            this.windowLog.Log("WindowClosed", true);
        }

        /// <summary>
        /// ウィンドウ状態変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.App_BackUpFlagAtMinimizing) {
                if (this.WindowState == WindowState.Minimized) {
#if !DEBUG
                    this.CreateBackUpFile();
#endif
                }
            }

            this.windowLog.Log("WindowStateChanged", true);
            this.ModifyLocationOrSize();
        }

        /// <summary>
        /// ウィンドウ位置変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            this.windowLog.Log("WindowLocationChanged", true);
            this.ModifyLocationOrSize();
        }

        /// <summary>
        /// ウィンドウサイズ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.windowLog.Log("WindowSizeChanged", true);
            this.ModifyLocationOrSize();
        }
#endregion

        /// <summary>
        /// 月別一覧ダブルクリック時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonthlyListDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.WVM.SelectedTab != Tabs.MonthlyListTab) return;

            if (e.MouseDevice.DirectlyOver is FrameworkElement fe) {
                if (fe.Parent is DataGridCell cell) {
                    int col = cell.Column.DisplayIndex;
                    
                    if (1 <= col && col <= 12) {
                        // 選択された月の帳簿タブを開く
                        this.WVM.DisplayedMonth = this.WVM.DisplayedYear.AddMonths(col - 1);
                        this.WVM.SelectedTab = Tabs.BooksTab;
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// 年別一覧ダブルクリック時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YearlyListDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.WVM.SelectedTab != Tabs.YearlyListTab) return;

            if (e.MouseDevice.DirectlyOver is FrameworkElement fe) {
                if (fe.Parent is DataGridCell cell) {
                    int col = cell.Column.DisplayIndex;

                    if (1 <= col && col <= 10) {
                        // 選択された年の月別一覧タブを開く
                        Properties.Settings settings = Properties.Settings.Default;

                        this.WVM.DisplayedYear = DateTime.Now.GetFirstDateOfFiscalYear(settings.App_StartMonth).AddYears(col - 10);
                        this.WVM.SelectedTab = Tabs.MonthlyListTab;
                        e.Handled = true;
                    }
                }
            }
        }
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// 画面更新(タブ非依存)
        /// </summary>
        /// <param name="isUpdateBookList">帳簿リストを更新するか</param>
        /// <param name="isScroll">帳簿項目一覧をスクロールするか</param>
        /// <param name="isUpdateActDateLastEdited">最後に操作した帳簿項目を更新するか</param>
        private async Task UpdateAsync(bool isUpdateBookList = false, bool isScroll = false, bool isUpdateActDateLastEdited = false)
        {
            Log.Info($"isUpdateBookList: {isUpdateBookList} isScroll: {isScroll} isUpdateActDateLastEdited: {isUpdateActDateLastEdited}");

            if (isUpdateBookList) await this.UpdateBookListAsync();

            switch (this.WVM.SelectedTab) {
                case Tabs.BooksTab:
                    await this.UpdateBookTabDataAsync(isScroll: isScroll, isUpdateActDateLastEdited: isUpdateActDateLastEdited);
                    break;
                case Tabs.DailyGraphTab:
                    this.InitializeDailyGraphTabData();
                    await this.UpdateDailyGraphTabDataAsync();
                    break;
                case Tabs.MonthlyListTab:
                    await this.UpdateMonthlyListTabDataAsync();
                    break;
                case Tabs.MonthlyGraphTab:
                    this.InitializeMonthlyGraphTabData();
                    await this.UpdateMonthlyGraphTabDataAsync();
                    break;
                case Tabs.YearlyListTab:
                    await this.UpdateYearlyListTabDataAsync();
                    break;
                case Tabs.YearlyGraphTab:
                    this.InitializeYearlyGraphTabData();
                    await this.UpdateYearlyGraphTabDataAsync();
                    break;
            }
        }

        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        private async Task UpdateBookListAsync(int? bookId = null)
        {
            Log.Info($"bookId: {bookId}");

            int? tmpBookId = bookId ?? this.WVM.SelectedBookVM?.Id;
            Log.Info($"tmpBookId: {tmpBookId}");

            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>() {
                new BookViewModel() { Id = null, Name = "一覧" }
            };
            BookViewModel selectedBookVM = bookVMList[0];
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader = await dao.ExecQueryAsync(@"
SELECT *
FROM mst_book 
WHERE del_flg = 0 
ORDER BY sort_order;");

                reader.ExecWholeRow((count, record) => {
                    string jsonCode = record["json_code"];
                    MstBookJsonObject jsonObj = JsonConvert.DeserializeObject<MstBookJsonObject>(jsonCode);

                    if (DateTimeExtensions.IsWithIn(this.WVM.DisplayedStartDate, this.WVM.DisplayedEndDate, jsonObj?.StartDate, jsonObj?.EndDate)) {
                        BookViewModel vm = new BookViewModel() { Id = record.ToInt("book_id"), Name = record["book_name"] };
                        bookVMList.Add(vm);

                        if (vm.Id == tmpBookId) {
                            selectedBookVM = vm;
                            Log.Info($"select {selectedBookVM?.Id}");
                        }
                    }

                    return true;
                });
            }

            this.WVM.SelectedBookVM = selectedBookVM; // 先に選択しておく
            this.WVM.BookVMList = bookVMList;
        }

        /// <summary>
        /// 選択項目グラフを更新する
        /// </summary>
        private void UpdateSelectedGraph()
        {
            switch (this.WVM.SelectedTab) {
                case Tabs.DailyGraphTab:
                    this.UpdateSelectedDailyGraph();
                    break;
                case Tabs.MonthlyGraphTab:
                    this.UpdateSelectedMonthlyGraph();
                    break;
                case Tabs.YearlyGraphTab:
                    this.UpdateSelectedYearlyGraph();
                    break;
            }
        }

        #region VM取得用の関数
        /// <summary>
        /// 月内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="targetBookId">帳簿ID</param>
        /// <param name="includedTime">月内の時刻</param>
        /// <returns>帳簿項目VMリスト</returns>
        private async Task<ObservableCollection<ActionViewModel>> LoadActionViewModelListWithinMonthAsync(int? targetBookId, DateTime includedTime)
        {
            Log.Info($"targetBookId: {targetBookId}, includedTime: {includedTime}");

            DateTime startTime = includedTime.GetFirstDateOfMonth();
            DateTime endTime = startTime.GetLastDateOfMonth();
            return await this.LoadActionViewModelListAsync(targetBookId, startTime, endTime);
        }

        /// <summary>
        /// 期間内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="targetBookId">帳簿ID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>帳簿項目VMリスト</returns>
        private async Task<ObservableCollection<ActionViewModel>> LoadActionViewModelListAsync(int? targetBookId, DateTime startTime, DateTime endTime)
        {
            Log.Info($"targetBookId: {targetBookId} startTime:{startTime} endTime:{endTime}");

            ObservableCollection<ActionViewModel> actionVMList = new ObservableCollection<ActionViewModel>();
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader;
                if (targetBookId == null) {
                    // 全帳簿
                    reader = await dao.ExecQueryAsync(@"
-- 繰越残高
SELECT -1 AS action_id, @{1} AS act_time, -1 AS book_id, -1 AS category_id, -1 AS item_id, 
       '' AS book_name, '' AS category_name, '繰越残高' AS item_name, 0 AS act_value, (
           -- 残高
           SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT COALESCE(SUM(initial_value), 0) FROM mst_book WHERE del_flg = 0)
           FROM hst_action AA
           INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
           WHERE AA.del_flg = 0 AND AA.act_time < @{1}
       ) AS balance,
       NULL AS shop_name, NULL AS group_id, NULL AS remark, 0 AS is_match, 
       0 AS balance_kind, 0 AS c_order, 0 AS i_order, 0 AS b_order
UNION
-- 各帳簿項目
SELECT A.action_id AS action_id, A.act_time AS act_time, B.book_id AS book_id, C.category_id AS category_id, I.item_id AS item_id, 
       B.book_name AS book_name, C.category_name AS category_name, I.item_name AS item_name, A.act_value AS act_value, 0 AS balance, 
       A.shop_name AS shop_name, A.group_id AS group_id, A.remark AS remark, A.is_match AS is_match, 
       C.balance_kind AS balance_kind, C.sort_order AS c_order, I.sort_order AS i_order, B.sort_order AS b_order
FROM hst_action A
INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) B ON B.book_id = A.book_id
INNER JOIN (SELECT * FROM mst_item WHERE item_id IN (
  SELECT RBI.item_id 
  FROM rel_book_item RBI
  INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) B ON B.book_id = RBI.book_id
  WHERE RBI.del_flg = 0
) AND del_flg = 0) I ON I.item_id = A.item_id
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON I.category_id = C.category_id
WHERE A.del_flg = 0 AND @{1} <= A.act_time AND A.act_time <= @{2}
ORDER BY act_time, balance_kind, c_order, i_order, b_order, action_id;", null, startTime, endTime);
                }
                else {
                    // 各帳簿
                    reader = await dao.ExecQueryAsync(@"
-- 繰越残高
SELECT -1 AS action_id, @{1} AS act_time, -1 AS book_id, -1 AS category_id, -1 AS item_id, 
       '' AS book_name, '' AS category_name, '繰越残高' AS item_name, 0 AS act_value, (
           -- 残高
           SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT initial_value FROM mst_book WHERE book_id = @{0})
           FROM hst_action AA
           INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
           WHERE AA.book_id = @{0} AND AA.del_flg = 0 AND AA.act_time < @{1}
       ) AS balance, 
       NULL AS shop_name, NULL AS group_id, NULL AS remark, 0 AS is_match, 
       0 AS balance_kind, 0 AS c_order, 0 AS i_order
UNION
-- 各帳簿項目
SELECT A.action_id AS action_id, A.act_time AS act_time, B.book_id AS book_id, C.category_id AS category_id, I.item_id AS item_id, 
       B.book_name AS book_name, C.category_name AS category_name, I.item_name AS item_name, A.act_value AS act_value, 0 AS balance,
       A.shop_name AS shop_name, A.group_id AS group_id, A.remark AS remark, A.is_match AS is_match, 
       C.balance_kind AS balance_kind, C.sort_order AS c_order, I.sort_order AS i_order
FROM hst_action A
INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) B ON B.book_id = A.book_id
INNER JOIN (SELECT * FROM mst_item WHERE (move_flg = 1 OR item_id IN (
  SELECT item_id FROM rel_book_item WHERE book_id = @{0} AND del_flg = 0
)) AND del_flg = 0) I ON I.item_id = A.item_id
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON I.category_id = C.category_id
WHERE A.book_id = @{0} AND A.del_flg = 0 AND @{1} <= A.act_time AND A.act_time <= @{2}
ORDER BY act_time, balance_kind, c_order, i_order, action_id;", targetBookId, startTime, endTime);
                }
                int balance = 0;
                reader.ExecWholeRow((count, record) => {
                    int actionId = record.ToInt("action_id");
                    DateTime actTime = record.ToDateTime("act_time");
                    int bookId = record.ToInt("book_id");
                    int categoryId = record.ToInt("category_id");
                    int itemId = record.ToInt("item_id");
                    string bookName = record["book_name"];
                    string categoryName = record["category_name"];
                    string itemName = record["item_name"];
                    int actValue = record.ToInt("act_value");
                    balance = (count == 0 ? record.ToInt("balance") : balance + actValue);
                    string shopName = record["shop_name"];
                    int? groupId = record.ToNullableInt("group_id");
                    string remark = record["remark"];
                    bool isMatch = record.ToInt("is_match") == 1;

                    BalanceKind balanceKind = BalanceKind.Others;
                    int? income = null;
                    int? outgo = null;
                    if (actValue == 0) {
                        balanceKind = BalanceKind.Others;
                        income = null;
                        outgo = null;
                    }
                    else if (actValue < 0) {
                        balanceKind = BalanceKind.Outgo;
                        income = null;
                        outgo = -actValue;
                    }
                    else {
                        balanceKind = BalanceKind.Income;
                        income = actValue;
                        outgo = null;
                    }

                    if (actionId == -1) {
                        balanceKind = BalanceKind.Others;
                    }

                    ActionViewModel avm = new ActionViewModel() {
                        ActionId = actionId,
                        ActTime = actTime,
                        BookId = bookId,
                        CategoryId = categoryId,
                        ItemId = itemId,
                        BookName = bookName,
                        CategoryName = categoryName,
                        ItemName = itemName,
                        BalanceKind = balanceKind,
                        Income = income,
                        Outgo = outgo,
                        Balance = balance,
                        ShopName = shopName,
                        GroupId = groupId,
                        Remark = remark,
                        IsMatch = isMatch
                    };

                    actionVMList.Add(avm);
                    return true;
                });
            }

            return actionVMList;
        }

        /// <summary>
        /// 月内概要VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">月内の時間</param>
        /// <returns>概要VMリスト</returns>
        private async Task<ObservableCollection<SummaryViewModel>> LoadSummaryViewModelListWithinMonthAsync(int? bookId, DateTime includedTime)
        {
            Log.Info($"bookId: {bookId} includedTime: {includedTime}");

            DateTime startTime = includedTime.GetFirstDateOfMonth();
            DateTime endTime = startTime.GetLastDateOfMonth();
            return await this.LoadSummaryViewModelListAsync(bookId, startTime, endTime);
        }

        /// <summary>
        /// 期間内概要VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>概要VMリスト</returns>
        private async Task<ObservableCollection<SummaryViewModel>> LoadSummaryViewModelListAsync(int? bookId, DateTime startTime, DateTime endTime)
        {
            Log.Info($"bookId: {bookId} startTime: {startTime} endTime: {endTime}");

            ObservableCollection<SummaryViewModel> summaryVMList = new ObservableCollection<SummaryViewModel>();

            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader;

                if (bookId == null) {
                    reader = await dao.ExecQueryAsync(@"
-- 全帳簿が対象
SELECT C.balance_kind AS balance_kind, C.category_id AS category_id, C.category_name AS category_name, SQ.item_id AS item_id, I.item_name AS item_name, SQ.sum AS sum
FROM (
  SELECT I.item_id AS item_id, COALESCE(SUM(A.act_value), 0) AS sum
  FROM mst_item I
  LEFT JOIN (SELECT * FROM hst_action WHERE @{1} <= act_time AND act_time <= @{2} AND del_flg = 0) A ON A.item_id = I.item_id
  WHERE I.item_id IN (SELECT item_id FROM rel_book_item WHERE del_flg = 0) AND I.del_flg = 0
  GROUP BY I.item_id
) SQ -- Sub Query
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = SQ.item_id
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
ORDER BY C.balance_kind, C.sort_order, I.sort_order;", null, startTime, endTime);
                }
                else {
                    reader = await dao.ExecQueryAsync(@"
-- 特定の帳簿が対象
SELECT C.balance_kind AS balance_kind, C.category_id AS category_id, C.category_name AS category_name, SQ.item_id AS item_id, I.item_name AS item_name, SQ.sum AS sum
FROM (
  SELECT I.item_id AS item_id, COALESCE(SUM(A.act_value), 0) AS sum
  FROM mst_item I
  LEFT JOIN (SELECT * FROM hst_action WHERE book_id = @{0} AND @{1} <= act_time AND act_time <= @{2} AND del_flg = 0) A ON A.item_id = I.item_id
  WHERE (I.move_flg = 1 OR I.item_id IN (SELECT item_id FROM rel_book_item WHERE book_id = @{0} AND del_flg = 0)) AND I.del_flg = 0
  GROUP BY I.item_id
) SQ -- Sub Query
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = SQ.item_id
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
ORDER BY C.balance_kind, C.sort_order, I.sort_order;", bookId, startTime, endTime);
                }

                reader.ExecWholeRow((count, record) => {
                    int balanceKind = record.ToInt("balance_kind");
                    int categoryId = record.ToInt("category_id");
                    string categoryName = record["category_name"];
                    int itemId = record.ToInt("item_id");
                    string itemName = record["item_name"];
                    int summary = record.ToInt("sum");

                    summaryVMList.Add(new SummaryViewModel() {
                        BalanceKind = balanceKind,
                        BalanceName = BalanceKindStr[(BalanceKind)balanceKind],
                        CategoryId = categoryId,
                        CategoryName = categoryName,
                        ItemId = itemId,
                        ItemName = itemName,
                        Summary = summary
                    });
                    return true;
                });
            }

            // 差引損益
            int total = summaryVMList.Sum((obj) => obj.Summary);
            // 収入/支出
            List<SummaryViewModel> totalAsBalanceKindList = new List<SummaryViewModel>();
            // 分類小計
            List<SummaryViewModel> totalAsCategoryList = new List<SummaryViewModel>();

            // 収支別に計算する
            foreach (var g1 in summaryVMList.GroupBy((obj) => obj.BalanceKind)) {
                // 収入/支出の小計を計算する
                totalAsBalanceKindList.Add(new SummaryViewModel() {
                    BalanceKind = g1.Key,
                    BalanceName = BalanceKindStr[(BalanceKind)g1.Key],
                    Summary = g1.Sum((obj) => obj.Summary)
                });
                // 分類別の小計を計算する
                foreach (var g2 in g1.GroupBy((obj) => obj.CategoryId)) {
                    totalAsCategoryList.Add(new SummaryViewModel() {
                        BalanceKind = g1.Key,
                        BalanceName = BalanceKindStr[(BalanceKind)g1.Key],
                        CategoryId = g2.Key,
                        CategoryName = g2.First().CategoryName,
                        Summary = g2.Sum((obj) => obj.Summary)
                    });
                }
            }

            // 差引損益を追加する
            summaryVMList.Insert(0, new SummaryViewModel() {
                OtherName = "差引損益",
                Summary = total
            });
            // 収入/支出の小計を追加する
            foreach (SummaryViewModel svm in totalAsBalanceKindList) {
                summaryVMList.Insert(summaryVMList.IndexOf(summaryVMList.First(obj => obj.BalanceKind == svm.BalanceKind)), svm);
            }
            // 分類別の小計を追加する
            foreach (SummaryViewModel svm in totalAsCategoryList) {
                summaryVMList.Insert(summaryVMList.IndexOf(summaryVMList.First(obj => obj.CategoryId == svm.CategoryId)), svm);
            }

            return summaryVMList;
        }

        /// <summary>
        /// 月内日別系列VMリストを取得する(日別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">月内の時間</param>
        /// <returns>月内日別系列VMリスト</returns>
        private async Task<ObservableCollection<SeriesViewModel>> LoadDailySeriesViewModelListWithinMonthAsync(int? bookId, DateTime includedTime)
        {
            Log.Info($"bookId: {bookId} includedTime: {includedTime}");

            DateTime startTime = includedTime.GetFirstDateOfMonth();
            DateTime endTime = startTime.GetLastDateOfMonth();

            return await this.LoadDailySeriesViewModelListAsync(bookId, startTime, endTime);
        }

        /// <summary>
        /// 期間内日別系列VMリストを取得する(日別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>日別系列VMリスト</returns>
        private async Task<ObservableCollection<SeriesViewModel>> LoadDailySeriesViewModelListAsync(int? bookId, DateTime startTime, DateTime endTime)
        {
            Log.Info($"bookId: {bookId} startTime: {startTime} endTime: {endTime}");

            // 開始日までの収支を取得する
            int balance = 0;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader;
                if (bookId == null) {
                    // 全帳簿
                    reader = await dao.ExecQueryAsync(@"
SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT COALESCE(SUM(initial_value), 0) FROM mst_book WHERE del_flg = 0) AS sum
FROM hst_action AA
INNER JOIN(SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
WHERE AA.del_flg = 0 AND AA.act_time < @{1};", null, startTime);
                }
                else {
                    // 各帳簿
                    reader = await dao.ExecQueryAsync(@"
SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT initial_value FROM mst_book WHERE book_id = @{0}) AS sum
FROM hst_action AA
INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
WHERE AA.book_id = @{0} AND AA.del_flg = 0 AND AA.act_time < @{1};", bookId, startTime);
                }

                reader.ExecARow((record) => {
                    balance = record.ToInt("sum");
                });
            }

            // 系列データ
            ObservableCollection<SeriesViewModel> vmList = new ObservableCollection<SeriesViewModel> {
                new SeriesViewModel() {
                    OtherName = "残高",
                    Values = new List<int>()
                }
            };
            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の日の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.AddDays(1).AddMilliseconds(-1);
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Summary;
            vmList[0].Values.Add(balance); // 残高

            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Summary;
                SeriesViewModel vm = new SeriesViewModel(summaryVM) {
                    Values = new List<int>(),
                    Summary = value
                };
                // 平均値は過去のデータのみで計算する
                if (endTime < DateTime.Now) {
                    vm.Average = value;
                }
                else {
                    vm.Average = 0;
                }
                vm.Values.Add(value);
                vmList.Add(vm);
            }
            if (endTime < DateTime.Now) {
                ++averageCount;
            }

            // 最初以外の日の分を取得する
            int days = (endTime - startTime.AddDays(-1)).Days;
            for (int i = 1; i < days; ++i) {
                tmpStartTime = tmpStartTime.AddDays(1);
                tmpEndTime = tmpStartTime.AddDays(1).AddMilliseconds(-1);

                summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
                balance += summaryVMList[0].Summary;
                vmList[0].Values.Add(balance); // 残高
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    int value = summaryVMList[j].Summary;

                    vmList[j + 1].Values.Add(value);

                    if (tmpEndTime < DateTime.Now) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Summary += value;
                }
                if (tmpEndTime < DateTime.Now) {
                    ++averageCount;
                }
            }

            // 平均値を計算する
            foreach (SeriesViewModel vm in vmList) {
                if (vm.Average != null) {
                    if (averageCount != 0) {
                        vm.Average /= averageCount;
                    }
                    else {
                        vm.Average = 0;
                    }
                }
            }

            return vmList;
        }

        /// <summary>
        /// 年度内月別系列VMリストを取得する(月別一覧/月別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">年度内の時間</param>
        /// <returns>年度内月別系列VMリスト</returns>
        private async Task<ObservableCollection<SeriesViewModel>> LoadMonthlySeriesViewModelListWithinYearAsync(int? bookId, DateTime includedTime)
        {
            Log.Info($"bookId: {bookId} includedTime: {includedTime}");

            DateTime startTime = includedTime.GetFirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);
            DateTime endTime = startTime.GetLastDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);

            // 開始月までの収支を取得する
            int balance = 0;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader;
                if (bookId == null) {
                    // 全帳簿
                    reader = await dao.ExecQueryAsync(@"
SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT COALESCE(SUM(initial_value), 0) FROM mst_book WHERE del_flg = 0) AS sum
FROM hst_action AA
INNER JOIN(SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
WHERE AA.del_flg = 0 AND AA.act_time < @{1};", null, startTime);
                }
                else {
                    // 各帳簿
                    reader = await dao.ExecQueryAsync(@"
SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT initial_value FROM mst_book WHERE book_id = @{0}) AS sum
FROM hst_action AA
INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
WHERE AA.book_id = @{0} AND AA.del_flg = 0 AND AA.act_time < @{1};", bookId, startTime);
                }

                reader.ExecARow((record) => {
                    balance = record.ToInt("sum");
                });
            }

            ObservableCollection<SeriesViewModel> vmList = new ObservableCollection<SeriesViewModel> {
                new SeriesViewModel() {
                    OtherName = "残高",
                    Values = new List<int>()
                }
            };
            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の月の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.GetLastDateOfMonth();
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Summary;
            vmList[0].Values.Add(balance); // 残高
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Summary;
                SeriesViewModel vm = new SeriesViewModel(summaryVM) {
                    Values = new List<int>(),
                    Summary = value
                };
                if (tmpEndTime < DateTime.Now) {
                    vm.Average = value;
                }
                else {
                    vm.Average = 0;
                }
                vm.Values.Add(value);
                vmList.Add(vm);
            }
            if (tmpEndTime < DateTime.Now) {
                ++averageCount;
            }

            // 最初以外の月の分を取得する
            int monthes = (endTime.Year * 12 + endTime.Month) - (startTime.Year * 12 + startTime.Month - 1);
            for (int i = 1; i < monthes; ++i) {
                tmpStartTime = tmpStartTime.AddMonths(1);
                tmpEndTime = tmpStartTime.GetLastDateOfMonth();

                summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
                balance += summaryVMList[0].Summary;
                vmList[0].Values.Add(balance); // 残高
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    int value = summaryVMList[j].Summary;

                    vmList[j + 1].Values.Add(value);

                    if (tmpEndTime < DateTime.Now) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Summary += value;
                }
                if (tmpEndTime < DateTime.Now) {
                    ++averageCount;
                }
            }

            // 平均値を計算する
            foreach (SeriesViewModel vm in vmList) {
                if (vm.Average != null) {
                    if (averageCount != 0) {
                        vm.Average /= averageCount;
                    }
                    else {
                        vm.Average = 0;
                    }
                }
            }

            return vmList;
        }

        /// <summary>
        /// 10年内年別系列VMリストを取得する(年別一覧/年別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>年別系列VMリスト</returns>
        private async Task<ObservableCollection<SeriesViewModel>> LoadYearlySeriesViewModelListWithinDecadeAsync(int? bookId)
        {
            Log.Info($"bookId: {bookId}");

            Settings settings = Settings.Default;
            DateTime startTime = DateTime.Now.GetFirstDateOfFiscalYear(settings.App_StartMonth).AddYears(-9);
            DateTime endTime = startTime.AddYears(10);

            // 開始年までの収支を取得する
            int balance = 0;
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader;
                if (bookId == null) {
                    // 全帳簿
                    reader = await dao.ExecQueryAsync(@"
SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT COALESCE(SUM(initial_value), 0) FROM mst_book WHERE del_flg = 0) AS sum
FROM hst_action AA
INNER JOIN(SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
WHERE AA.del_flg = 0 AND AA.act_time < @{1};", null, startTime);
                }
                else {
                    // 各帳簿
                    reader = await dao.ExecQueryAsync(@"
SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT initial_value FROM mst_book WHERE book_id = @{0}) AS sum
FROM hst_action AA
INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
WHERE AA.book_id = @{0} AND AA.del_flg = 0 AND AA.act_time < @{1};", bookId, startTime);
                }

                reader.ExecARow((record) => {
                    balance = record.ToInt("sum");
                });
            }

            ObservableCollection<SeriesViewModel> vmList = new ObservableCollection<SeriesViewModel> {
                new SeriesViewModel() {
                    OtherName = "残高",
                    Values = new List<int>()
                }
            };
            int averageCount = 0; // 平均値計算に使用する年数(去年まで)

            // 最初の年の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.GetLastDateOfFiscalYear(settings.App_StartMonth);
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Summary;
            vmList[0].Values.Add(balance); // 残高
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Summary;
                SeriesViewModel vm = new SeriesViewModel(summaryVM) {
                    Values = new List<int>(),
                    Summary = value
                };
                if (tmpEndTime < DateTime.Now) {
                    vm.Average = value;
                }
                else {
                    vm.Average = 0;
                }
                vm.Values.Add(value);
                vmList.Add(vm);
            }
            if (tmpEndTime < DateTime.Now) {
                ++averageCount;
            }

            // 最初以外の年の分を取得する
            int years = 10;
            for (int i = 1; i < years; ++i) {
                tmpStartTime = tmpStartTime.AddYears(1);
                tmpEndTime = tmpStartTime.GetLastDateOfFiscalYear(settings.App_StartMonth);

                summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
                balance += summaryVMList[0].Summary;
                vmList[0].Values.Add(balance); // 残高
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    int value = summaryVMList[j].Summary;

                    vmList[j + 1].Values.Add(value);

                    if (tmpEndTime < DateTime.Now) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Summary += value;
                }
                if (tmpEndTime < DateTime.Now) {
                    ++averageCount;
                }
            }

            // 平均値を計算する
            foreach (SeriesViewModel vm in vmList) {
                if (vm.Average != null) {
                    if (averageCount != 0) {
                        vm.Average /= averageCount;
                    }
                    else {
                        vm.Average = 0;
                    }
                }
            }

            return vmList;
        }
        #endregion

        #region 帳簿タブ更新用の関数
        /// <summary>
        /// 帳簿タブに表示するデータを更新する
        /// </summary>
        /// <param name="actionIdList">選択対象の帳簿項目IDリスト</param>
        /// <param name="balanceKind">選択対象の収支種別</param>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <param name="itemId">選択対象の項目ID</param>
        /// <param name="isScroll">帳簿項目一覧をスクロールするか</param>
        /// <param name="isUpdateActDateLastEdited">最後に操作した帳簿項目を更新するか</param>
        private async Task UpdateBookTabDataAsync(List<int> actionIdList = null, int? balanceKind = null, int? categoryId = null, int? itemId = null, 
                                                  bool isScroll = false, bool isUpdateActDateLastEdited = false)
        {
            if (this.WVM.SelectedTab != Tabs.BooksTab) return;

            List<int> emptyIdList = new List<int>();
            Log.Info($"actionIdList: {string.Join(",", actionIdList ?? emptyIdList)} balanceKind: {balanceKind} categoryId: {categoryId} itemId: {itemId} isScroll: {isScroll} isUpdateActDateLastEdited: {isUpdateActDateLastEdited}");

            // 指定がなければ、更新前の帳簿項目の選択を維持する
            List<int> tmpActionIdList = actionIdList ?? new List<int>(this.WVM.SelectedActionVMList.Select((tmp) => tmp.ActionId));
            // 指定がなければ、更新前のサマリーの選択を維持する
            int? tmpBalanceKind = balanceKind ?? this.WVM.SelectedBalanceKind;
            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.WVM.SelectedItemId;
            Log.Info($"tmpActionIdList: {string.Join(",", tmpActionIdList ?? emptyIdList)} tmpBalanceKind: {tmpBalanceKind} tmpCategoryId: {tmpCategoryId} tmpItemId: {tmpItemId}");

            // 表示するデータを指定する
            switch (this.WVM.DisplayedTermKind) {
                case TermKind.Monthly:
                    var (tmp1, tmp2) = await (
                        this.LoadActionViewModelListWithinMonthAsync(this.WVM.SelectedBookVM?.Id, this.WVM.DisplayedMonth.Value),
                        this.LoadSummaryViewModelListWithinMonthAsync(this.WVM.SelectedBookVM?.Id, this.WVM.DisplayedMonth.Value)).WhenAll();
                    this.WVM.ActionVMList = tmp1;
                    this.WVM.SummaryVMList = tmp2;
                    break;
                case TermKind.Selected:
                    var (tmp3, tmp4) = await (
                        this.LoadActionViewModelListAsync(this.WVM.SelectedBookVM?.Id, this.WVM.StartDate, this.WVM.EndDate),
                        this.LoadSummaryViewModelListAsync(this.WVM.SelectedBookVM?.Id, this.WVM.StartDate, this.WVM.EndDate)).WhenAll();
                    this.WVM.ActionVMList = tmp3;
                    this.WVM.SummaryVMList = tmp4;
                    break;
            }

            // 帳簿項目を選択する(サマリーの選択はこの段階では無視して処理する)
            this.WVM.SelectedActionVMList = new ObservableCollection<ActionViewModel>(this.WVM.ActionVMList.Where((vm) => tmpActionIdList.Contains(vm.ActionId)));

            // サマリーを選択する
            this.WVM.SelectedSummaryVM = this.WVM.SummaryVMList.FirstOrDefault((vm) => (vm.BalanceKind == tmpBalanceKind && vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId));

            if (isScroll) {
                if (this.WVM.DisplayedTermKind == TermKind.Monthly &&
                    this.WVM.DisplayedMonth.Value.GetFirstDateOfMonth() < DateTime.Today && DateTime.Today < this.WVM.DisplayedMonth.Value.GetLastDateOfMonth()) {
                    // 今月の場合は、末尾が表示されるようにする
                    this.actionDataGrid.ScrollToButtom();
                }
                else {
                    // 今月でない場合は、先頭が表示されるようにする
                    this.actionDataGrid.ScrollToTop();
                }
            }

            // 最後に操作した帳簿項目の日付を更新する
            if (isUpdateActDateLastEdited) {
                if (actionIdList != null && actionIdList.Count != 0) {
                    using (DaoBase dao = this.builder.Build()) {
                        DaoReader reader = await dao.ExecQueryAsync(@"
SELECT act_time FROM hst_action WHERE action_id = @{0} AND del_flg = 0;", actionIdList[0]);

                        reader.ExecARow((record) => {
                            this.WVM.ActDateLastEdited = record.ToDateTime("act_time");
                        });
                    }
                }
                else {
                    this.WVM.ActDateLastEdited = null;
                }
            }
        }
        #endregion

        #region 日別グラフタブ更新用の関数
        /// <summary>
        /// 日別グラフタブに表示するデータを初期化する
        /// </summary>
        private void InitializeDailyGraphTabData()
        {
            if (this.WVM.SelectedTab != Tabs.DailyGraphTab) return;

            Log.Info();

            #region 全項目
            this.WVM.DailyGraphPlotModel.Axes.Clear();
            this.WVM.DailyGraphPlotModel.Series.Clear();

            // 横軸 - 日軸
            CategoryAxis horizontalAxis1 = new CategoryAxis() {
                Unit = "日",
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis1.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する日の文字列を作成する
            for (DateTime tmp = this.WVM.StartDate; tmp <= this.WVM.EndDate; tmp = tmp.AddDays(1)) {
                horizontalAxis1.Labels.Add(tmp.ToString("%d"));
            }
            this.WVM.DailyGraphPlotModel.Axes.Add(horizontalAxis1);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis1 = new LinearAxis() {
                Unit = "円",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0",
                Key = "Value",
                Title = GraphKind1Str[this.WVM.SelectedGraphKind1]
            };
            this.WVM.DailyGraphPlotModel.Axes.Add(verticalAxis1);

            this.WVM.DailyGraphPlotModel.InvalidatePlot(true);
            #endregion

            #region 選択項目
            this.WVM.SelectedDailyGraphPlotModel.Axes.Clear();
            this.WVM.SelectedDailyGraphPlotModel.Series.Clear();

            // 横軸 - 日軸
            CategoryAxis horizontalAxis2 = new CategoryAxis() {
                Unit = "日",
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis2.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する日の文字列を作成する
            for (DateTime tmp = this.WVM.StartDate; tmp <= this.WVM.EndDate; tmp = tmp.AddDays(1)) {
                horizontalAxis2.Labels.Add(tmp.ToString("%d"));
            }
            this.WVM.SelectedDailyGraphPlotModel.Axes.Add(horizontalAxis2);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis2 = new LinearAxis() {
                Unit = "円",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0",
                Key = "Value",
                Title = GraphKind1Str[this.WVM.SelectedGraphKind1]
            };
            this.WVM.SelectedDailyGraphPlotModel.Axes.Add(verticalAxis2);

            this.WVM.SelectedDailyGraphPlotModel.InvalidatePlot(true);
            #endregion
        }

        /// <summary>
        /// 日別グラフタブに表示するデータを更新する
        /// </summary>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <param name="itemId">選択対象の項目ID</param>
        private async Task UpdateDailyGraphTabDataAsync(int? categoryId = null, int? itemId = null)
        {
            if (this.WVM.SelectedTab != Tabs.DailyGraphTab) return;

            Log.Info($"categoryId: {categoryId} itemId: {itemId}");

            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.WVM.SelectedItemId ?? null;
            Log.Info($"tmpCategoryId: {tmpCategoryId} tmpItemId: {tmpItemId}");

            switch (this.WVM.SelectedGraphKind1) {
                case GraphKind1.IncomeAndOutgoGraph: {
                    List<int> sumPlus = new List<int>(); // 日ごとの合計収入(Y軸範囲の計算に使用)
                    List<int> sumMinus = new List<int>(); // 日ごとの合計支出(Y軸範囲の計算に使用)

                    // グラフ表示データを取得する
                    ObservableCollection<SeriesViewModel> tmpVMList = null;
                    switch (this.WVM.DisplayedTermKind) {
                        case TermKind.Monthly:
                            tmpVMList = await this.LoadDailySeriesViewModelListWithinMonthAsync(this.WVM.SelectedBookVM?.Id, this.WVM.DisplayedMonth.Value);
                            break;
                        case TermKind.Selected:
                            tmpVMList = await this.LoadDailySeriesViewModelListAsync(this.WVM.SelectedBookVM?.Id, this.WVM.StartDate, this.WVM.EndDate);
                            break;
                    }
                    // グラフ表示データを設定用に絞り込む
                    switch (this.WVM.SelectedGraphKind2) {
                        case GraphKind2.CategoryGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => (vm.CategoryId != -1 && vm.ItemId == -1) ));
                            break;
                        case GraphKind2.ItemGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => (vm.ItemId != -1) ));
                            break;
                    }
                    this.WVM.DailyGraphSeriesVMList = tmpVMList;

                    // グラフ表示データを設定する
                    this.WVM.DailyGraphPlotModel.Series.Clear();
                    foreach (SeriesViewModel tmpVM in this.WVM.DailyGraphSeriesVMList) {
                        CustomBarSeries wholeSeries = new CustomBarSeries() {
                            IsStacked = true,
                            Title = tmpVM.Name,
                            ItemsSource = tmpVM.Values.Select((value, index) => {
                                return new GraphDatumViewModel {
                                    Value = value,
                                    Number = index,
                                    ItemId = tmpVM.ItemId,
                                    CategoryId = tmpVM.CategoryId
                                };
                            }),
                            ValueField = "Value",
                            TrackerFormatString = "{0}\n{1}日: {2:#,0}", //日付: 金額
                            XAxisKey = "Value",
                            YAxisKey = "Category"
                        };
                        // 全項目日別グラフの項目を選択した時のイベントを登録する
                        wholeSeries.TrackerHitResultChanged += (sender, e) => {
                            if (e.Value == null) return;

                            GraphDatumViewModel datumVM = e.Value.Item as GraphDatumViewModel;
                            this.WVM.SelectedDailyGraphSeriesVM = this.WVM.DailyGraphSeriesVMList.FirstOrDefault((tmp) => (tmp.CategoryId == datumVM.CategoryId && tmp.ItemId == datumVM.ItemId));
                        };

                        this.WVM.DailyGraphPlotModel.Series.Add(wholeSeries);

                        // 全項目の日毎の合計を計算する
                        for (int i = 0; i < tmpVM.Values.Count; ++i) {
                            if (sumPlus.Count <= i) { sumPlus.Add(0); sumMinus.Add(0); }

                            if (tmpVM.Values[i] < 0) sumMinus[i] += tmpVM.Values[i];
                            else sumPlus[i] += tmpVM.Values[i];
                        }
                    }

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.WVM.DailyGraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            this.SetAxisRange(axis, sumMinus.Min(), sumPlus.Max(), 10, true);
                            break;
                        }
                    }
                    break;
                }
                case GraphKind1.BalanceGraph: {
                    // グラフ表示データを取得する
                    ObservableCollection<SeriesViewModel> seriesVMList = null;
                    switch (this.WVM.DisplayedTermKind) {
                        case TermKind.Monthly:
                            seriesVMList = await this.LoadDailySeriesViewModelListWithinMonthAsync(this.WVM.SelectedBookVM?.Id, this.WVM.DisplayedMonth.Value);
                            break;
                        case TermKind.Selected:
                            seriesVMList = await this.LoadDailySeriesViewModelListAsync(this.WVM.SelectedBookVM?.Id, this.WVM.StartDate, this.WVM.EndDate);
                            break;
                    }
                    this.WVM.DailyGraphSeriesVMList = seriesVMList;

                    // グラフ表示データを設定する
                    this.WVM.DailyGraphPlotModel.Series.Clear();
                    LineSeries series = new LineSeries() {
                        Title = "残高",
                        TrackerFormatString = "{2}日: {4:#,0}" //日付: 金額
                    };
                    series.Points.AddRange(new List<int>(this.WVM.DailyGraphSeriesVMList[0].Values).Select((value, index) => new DataPoint(index, value)));
                    this.WVM.DailyGraphPlotModel.Series.Add(series);

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.WVM.DailyGraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            this.SetAxisRange(axis, series.Points.Min((value) => value.Y), series.Points.Max((value) => value.Y), 10, true);
                            break;
                        }
                    }
                    break;
                }
            }
            this.WVM.DailyGraphPlotModel.InvalidatePlot(true);

            this.WVM.SelectedDailyGraphSeriesVM = this.WVM.DailyGraphSeriesVMList.FirstOrDefault((vm) => (vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId));
        }

        /// <summary>
        /// 選択項目日別グラフを更新する
        /// </summary>
        private void UpdateSelectedDailyGraph()
        {
            if (this.WVM.SelectedGraphKind1 != GraphKind1.IncomeAndOutgoGraph) return;

            Log.Info();

            SeriesViewModel vm = this.WVM.SelectedDailyGraphSeriesVM;

            // グラフ表示データを設定する
            this.WVM.SelectedDailyGraphPlotModel.Series.Clear();
            if (vm != null) {
                CustomBarSeries slectedSeries = new CustomBarSeries() {
                    IsStacked = true,
                    Title = vm.Name,
                    FillColor = (this.WVM.DailyGraphPlotModel.Series.FirstOrDefault(s => ((s as CustomBarSeries).Title == vm.Name)) as CustomBarSeries).ActualFillColor,
                    ItemsSource = vm.Values.Select((value, index) => {
                        return new GraphDatumViewModel {
                            Value = value,
                            Number = index,
                            ItemId = vm.ItemId,
                            CategoryId = vm.CategoryId
                        };
                    }),
                    ValueField = "Value",
                    TrackerFormatString = "{1}日: {2:#,0}", //日付: 金額
                    XAxisKey = "Value",
                    YAxisKey = "Category"
                };

                this.WVM.SelectedDailyGraphPlotModel.Series.Add(slectedSeries);

                foreach (Axis axis in this.WVM.SelectedDailyGraphPlotModel.Axes) {
                    if (axis.Position == AxisPosition.Left) {
                        this.SetAxisRange(axis, vm.Values.Min(), vm.Values.Max(), 4, true);
                        break;
                    }
                }
            }

            this.WVM.SelectedDailyGraphPlotModel.InvalidatePlot(true);
        }
        #endregion

        #region 月別一覧タブ更新用の関数
        /// <summary>
        /// 月別一覧タブに表示するデータを更新する
        /// </summary>
        /// <param name="balanceKind">選択対象の収支種別</param>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <param name="itemId">選択対象の項目ID</param>
        private async Task UpdateMonthlyListTabDataAsync(int? balanceKind = null, int? categoryId = null, int? itemId = null)
        {
            if (this.WVM.SelectedTab != Tabs.MonthlyListTab) return;

            Log.Info($"balanceKind: {balanceKind} categoryId: {categoryId} itemId: {itemId}");

            int? tmpBalanceKind = balanceKind ?? this.WVM.SelectedBalanceKind;
            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.WVM.SelectedItemId;
            Log.Info($"tmpBalanceKind: {tmpBalanceKind} tmpCategoryId: {tmpCategoryId} tmpItemId: {tmpItemId}");

            int startMonth = Properties.Settings.Default.App_StartMonth;
            DateTime tmpMonth = this.WVM.DisplayedYear.GetFirstDateOfFiscalYear(startMonth);

            // 表示する月の文字列を作成する
            ObservableCollection<DateTime> displayedMonths = new ObservableCollection<DateTime>();
            for (int i = 0; i < 12; ++i) {
                displayedMonths.Add(tmpMonth);
                tmpMonth = tmpMonth.AddMonths(1);
            }
            this.WVM.DisplayedMonths = displayedMonths;
            this.WVM.MonthlySeriesVMList = await this.LoadMonthlySeriesViewModelListWithinYearAsync(this.WVM.SelectedBookVM?.Id, this.WVM.DisplayedYear);

            this.WVM.SelectedMonthlySeriesVM = this.WVM.MonthlySeriesVMList.FirstOrDefault((vm) => (vm.BalanceKind == tmpBalanceKind && vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId));
        }
        #endregion

        #region 月別グラフタブ更新用の関数
        /// <summary>
        /// 月別グラフタブに表示するデータを初期化する
        /// </summary>
        private void InitializeMonthlyGraphTabData()
        {
            if (this.WVM.SelectedTab != Tabs.MonthlyGraphTab) return;

            Log.Info();

            Settings settings = Settings.Default;
            int startMonth = settings.App_StartMonth;
            #region 全項目
            this.WVM.MonthlyGraphPlotModel.Axes.Clear();
            this.WVM.MonthlyGraphPlotModel.Series.Clear();

            // 横軸 - 月軸
            CategoryAxis horizontalAxis1 = new CategoryAxis() {
                Unit = "月",
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis1.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する月の文字列を作成する
            for (int i = startMonth; i < startMonth + 12; ++i) {
                horizontalAxis1.Labels.Add(string.Format("{0}", (i - 1) % 12 + 1));
            }
            this.WVM.MonthlyGraphPlotModel.Axes.Add(horizontalAxis1);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis1 = new LinearAxis() {
                Unit = "円",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0",
                Key = "Value",
                Title = GraphKind1Str[this.WVM.SelectedGraphKind1]
            };
            this.WVM.MonthlyGraphPlotModel.Axes.Add(verticalAxis1);

            this.WVM.MonthlyGraphPlotModel.InvalidatePlot(true);
            #endregion

            #region 選択項目
            this.WVM.SelectedMonthlyGraphPlotModel.Axes.Clear();
            this.WVM.SelectedMonthlyGraphPlotModel.Series.Clear();

            // 横軸 - 月軸
            CategoryAxis horizontalAxis2 = new CategoryAxis() {
                Unit = "月",
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis2.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する月の文字列を作成する
            for (int i = startMonth; i < startMonth + 12; ++i) {
                horizontalAxis2.Labels.Add(string.Format("{0}", (i - 1) % 12 + 1));
            }
            this.WVM.SelectedMonthlyGraphPlotModel.Axes.Add(horizontalAxis2);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis2 = new LinearAxis() {
                Unit = "円",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0",
                Key = "Value",
                Title = GraphKind1Str[this.WVM.SelectedGraphKind1]
            };
            this.WVM.SelectedMonthlyGraphPlotModel.Axes.Add(verticalAxis2);

            this.WVM.SelectedMonthlyGraphPlotModel.InvalidatePlot(true);
            #endregion
        }

        /// <summary>
        /// 月別グラフタブに表示するデータを更新する
        /// </summary>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <param name="itemId">選択対象の項目ID</param>
        private async Task UpdateMonthlyGraphTabDataAsync(int? categoryId = null, int? itemId = null)
        {
            if (this.WVM.SelectedTab != Tabs.MonthlyGraphTab) return;

            Log.Info($"categoryId: {categoryId} itemId: {itemId}");

            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.WVM.SelectedItemId;
            Log.Info($"tmpCategoryId: {tmpCategoryId} tmpItemId: {tmpItemId}");

            Settings settings = Settings.Default;
            int startMonth = settings.App_StartMonth;

            switch (this.WVM.SelectedGraphKind1) {
                case GraphKind1.IncomeAndOutgoGraph: {
                    List<int> sumPlus = new List<int>(); // 月ごとの合計収入
                    List<int> sumMinus = new List<int>(); // 月ごとの合計支出

                    // グラフ表示データを取得する
                    ObservableCollection<SeriesViewModel> tmpVMList = await this.LoadMonthlySeriesViewModelListWithinYearAsync(this.WVM.SelectedBookVM?.Id, this.WVM.DisplayedYear);
                    // グラフ表示データを設定用に絞り込む
                    switch (this.WVM.SelectedGraphKind2) {
                        case GraphKind2.CategoryGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => (vm.CategoryId != -1 && vm.ItemId == -1)));
                            break;
                        case GraphKind2.ItemGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => (vm.ItemId != -1)));
                            break;
                    }
                    this.WVM.MonthlyGraphSeriesVMList = tmpVMList;

                    // グラフ表示データを設定する
                    this.WVM.MonthlyGraphPlotModel.Series.Clear();
                    foreach (SeriesViewModel tmpVM in this.WVM.MonthlyGraphSeriesVMList) {
                        string title = string.Empty;
                        switch (this.WVM.SelectedGraphKind2) {
                            case GraphKind2.CategoryGraph:
                                title = tmpVM.CategoryName;
                                break;
                            case GraphKind2.ItemGraph:
                                title = tmpVM.ItemName;
                                break;
                        }

                        CustomBarSeries wholeSeries = new CustomBarSeries() {
                            IsStacked = true,
                            Title = title,
                            ItemsSource = tmpVM.Values.Select((value, index) => new GraphDatumViewModel {
                                Value = value,
                                Number = index + startMonth,
                                ItemId = tmpVM.ItemId,
                                CategoryId = tmpVM.CategoryId
                            }),
                            ValueField = "Value",
                            TrackerFormatString = "{0}\n{1}月: {2:#,0}", //月: 金額
                            XAxisKey = "Value",
                            YAxisKey = "Category"
                        };
                        // 全項目年間グラフの項目を選択した時のイベントを登録する
                        wholeSeries.TrackerHitResultChanged += (sender, e) => {
                            if (e.Value == null) return;

                            GraphDatumViewModel datumVM = e.Value.Item as GraphDatumViewModel;
                            this.WVM.SelectedMonthlyGraphSeriesVM = this.WVM.MonthlyGraphSeriesVMList.FirstOrDefault((tmp) => (tmp.CategoryId == datumVM.CategoryId && tmp.ItemId == datumVM.ItemId));
                        };
                        this.WVM.MonthlyGraphPlotModel.Series.Add(wholeSeries);

                        // 全項目の月毎の合計を計算する
                        for (int i = 0; i < tmpVM.Values.Count; ++i) {
                            if (sumPlus.Count <= i) { sumPlus.Add(0); sumMinus.Add(0); }

                            if (tmpVM.Values[i] < 0) sumMinus[i] += tmpVM.Values[i];
                            else sumPlus[i] += tmpVM.Values[i];
                        }
                    }

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.WVM.MonthlyGraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            this.SetAxisRange(axis, sumMinus.Min(), sumPlus.Max(), 10, true);
                            break;
                        }
                    }
                    break;
                }
                case GraphKind1.BalanceGraph: {
                    // グラフ表示データを取得する
                    this.WVM.MonthlyGraphSeriesVMList = await this.LoadMonthlySeriesViewModelListWithinYearAsync(this.WVM.SelectedBookVM.Id, this.WVM.DisplayedYear);

                    // グラフ表示データを設定する
                    this.WVM.MonthlyGraphPlotModel.Series.Clear();
                    LineSeries series = new LineSeries() {
                        Title = "残高",
                        TrackerFormatString = "{2}月: {4:#,0}" //月: 金額
                    };
                    series.Points.AddRange(new List<int>(this.WVM.MonthlyGraphSeriesVMList[0].Values).Select((value, index) => new DataPoint(index, value)));
                    this.WVM.MonthlyGraphPlotModel.Series.Add(series);

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.WVM.MonthlyGraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            this.SetAxisRange(axis, series.Points.Min((value) => value.Y), series.Points.Max((value) => value.Y), 10, true);
                            break;
                        }
                    }
                    break;
                }
            }
            this.WVM.MonthlyGraphPlotModel.InvalidatePlot(true);

            this.WVM.SelectedMonthlyGraphSeriesVM = this.WVM.MonthlyGraphSeriesVMList.FirstOrDefault((vm) => (vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId));
        }
        
        /// <summary>
        /// 選択項目月別グラフを更新する
        /// </summary>
        private void UpdateSelectedMonthlyGraph()
        {
            if (this.WVM.SelectedGraphKind1 != GraphKind1.IncomeAndOutgoGraph) return;

            Log.Info();

            Settings settings = Settings.Default;
            int startMonth = settings.App_StartMonth;

            SeriesViewModel vm = this.WVM.SelectedMonthlyGraphSeriesVM;

            // グラフ表示データを設定する
            this.WVM.SelectedMonthlyGraphPlotModel.Series.Clear();
            if (vm != null) {
                CustomBarSeries selectedSeries = new CustomBarSeries() {
                    IsStacked = true,
                    Title = vm.Name,
                    FillColor = (this.WVM.MonthlyGraphPlotModel.Series.FirstOrDefault(s => ((s as CustomBarSeries).Title == vm.Name)) as CustomBarSeries).ActualFillColor,
                    ItemsSource = vm.Values.Select((value, index) => new GraphDatumViewModel {
                        Value = value,
                        Number = index + startMonth,
                        ItemId = vm.ItemId,
                        CategoryId = vm.CategoryId
                    }),
                    ValueField = "Value",
                    TrackerFormatString = "{1}月: {2:#,0}", //月: 金額
                    XAxisKey = "Value",
                    YAxisKey = "Category"
                };
                this.WVM.SelectedMonthlyGraphPlotModel.Series.Add(selectedSeries);

                // Y軸の範囲を設定する
                foreach (Axis axis in this.WVM.SelectedMonthlyGraphPlotModel.Axes) {
                    if (axis.Position == AxisPosition.Left) {
                        this.SetAxisRange(axis, vm.Values.Min(), vm.Values.Max(), 4, true);
                        break;
                    }
                }
            }

            this.WVM.SelectedMonthlyGraphPlotModel.InvalidatePlot(true);
        }
        #endregion

        #region 年別一覧タブ更新用の関数
        /// <summary>
        /// 年別一覧タブに表示するデータを更新する
        /// </summary>
        /// <param name="balanceKind">選択対象の収支種別</param>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <param name="itemId">選択対象の項目ID</param>
        private async Task UpdateYearlyListTabDataAsync(int? balanceKind = null, int? categoryId = null, int? itemId = null)
        {
            if (this.WVM.SelectedTab != Tabs.YearlyListTab) return;

            Log.Info($"balanceKind: {balanceKind} categoryId: {categoryId} itemId: {itemId}");

            int? tmpBalanceKind = balanceKind ?? this.WVM.SelectedBalanceKind;
            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.WVM.SelectedItemId;
            Log.Info($"tmpBalanceKind: {tmpBalanceKind} tmpCategoryId: {tmpCategoryId} tmpItemId: {tmpItemId}");

            Settings settings = Settings.Default;
            int startMonth = settings.App_StartMonth;
            DateTime tmpYear = DateTime.Now.GetFirstDateOfFiscalYear(startMonth).AddYears(-9);

            // 表示する月の文字列を作成する
            ObservableCollection<DateTime> displayedYears = new ObservableCollection<DateTime>();
            for (int i = 0; i < 10; ++i) {
                displayedYears.Add(tmpYear);
                tmpYear = tmpYear.AddYears(1);
            }

            this.WVM.DisplayedYears = displayedYears;
            this.WVM.YearlySeriesVMList = await this.LoadYearlySeriesViewModelListWithinDecadeAsync(this.WVM.SelectedBookVM?.Id);

            this.WVM.SelectedYearlySeriesVM = this.WVM.YearlySeriesVMList.FirstOrDefault((vm) => (vm.BalanceKind == tmpBalanceKind && vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId));
        }
        #endregion

        #region 年別グラフタブ更新用の関数
        /// <summary>
        /// 年別グラフタブに表示するデータを初期化する
        /// </summary>
        private void InitializeYearlyGraphTabData()
        {
            if (this.WVM.SelectedTab != Tabs.YearlyGraphTab) return;

            Log.Info();

            Settings settings = Settings.Default;
            int startYear = DateTime.Now.GetFirstDateOfFiscalYear(settings.App_StartMonth).Year - 9;

            #region 全項目
            this.WVM.YearlyGraphPlotModel.Axes.Clear();
            this.WVM.YearlyGraphPlotModel.Series.Clear();

            // 横軸 - 年軸
            CategoryAxis horizontalAxis1 = new CategoryAxis() {
                Unit = "年度",
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis1.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する年の文字列を作成する
            for (int i = startYear; i < startYear + 10; ++i) {
                horizontalAxis1.Labels.Add(string.Format("{0}", i));
            }
            this.WVM.YearlyGraphPlotModel.Axes.Add(horizontalAxis1);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis1 = new LinearAxis() {
                Unit = "円",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0",
                Key = "Value",
                Title = GraphKind1Str[this.WVM.SelectedGraphKind1]
            };
            this.WVM.YearlyGraphPlotModel.Axes.Add(verticalAxis1);

            this.WVM.YearlyGraphPlotModel.InvalidatePlot(true);
            #endregion

            #region 選択項目
            this.WVM.SelectedYearlyGraphPlotModel.Axes.Clear();
            this.WVM.SelectedYearlyGraphPlotModel.Series.Clear();

            // 横軸 - 年軸
            CategoryAxis horizontalAxis2 = new CategoryAxis() {
                Unit = "年度",
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis2.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する年の文字列を作成する
            for (int i = startYear; i < startYear + 10; ++i) {
                horizontalAxis2.Labels.Add(string.Format("{0}", i));
            }
            this.WVM.SelectedYearlyGraphPlotModel.Axes.Add(horizontalAxis2);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis2 = new LinearAxis() {
                Unit = "円",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0",
                Key = "Value",
                Title = GraphKind1Str[this.WVM.SelectedGraphKind1]
            };
            this.WVM.SelectedYearlyGraphPlotModel.Axes.Add(verticalAxis2);

            this.WVM.SelectedYearlyGraphPlotModel.InvalidatePlot(true);
            #endregion
        }

        /// <summary>
        /// 年別グラフタブに表示するデータを更新する
        /// </summary>
        /// <param name="categoryId">選択対象の分類ID</param>
        /// <param name="itemId">選択対象の項目ID</param>
        private async Task UpdateYearlyGraphTabDataAsync(int? categoryId = null, int? itemId = null)
        {
            if (this.WVM.SelectedTab != Tabs.YearlyGraphTab) return;

            Log.Info($"categoryId: {categoryId} itemId: {itemId}");

            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.WVM.SelectedItemId;
            Log.Info($"tmpCategoryId: {tmpCategoryId} tmpItemId: {tmpItemId}");

            Settings settings = Settings.Default;
            int startYear = DateTime.Now.GetFirstDateOfFiscalYear(settings.App_StartMonth).Year - 9;

            switch (this.WVM.SelectedGraphKind1) {
                case GraphKind1.IncomeAndOutgoGraph: {
                    List<int> sumPlus = new List<int>(); // 年ごとの合計収入
                    List<int> sumMinus = new List<int>(); // 年ごとの合計支出

                    // グラフ表示データを取得する
                    ObservableCollection<SeriesViewModel> tmpVMList = await this.LoadYearlySeriesViewModelListWithinDecadeAsync(this.WVM.SelectedBookVM?.Id);
                    // グラフ表示データを設定用に絞り込む
                    switch (this.WVM.SelectedGraphKind2) {
                        case GraphKind2.CategoryGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => (vm.CategoryId != -1 && vm.ItemId == -1)));
                            break;
                        case GraphKind2.ItemGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => (vm.ItemId != -1)));
                            break;
                    }
                    this.WVM.YearlyGraphSeriesVMList = tmpVMList;

                    // グラフ表示データを設定する
                    this.WVM.YearlyGraphPlotModel.Series.Clear();
                    foreach (SeriesViewModel tmpVM in this.WVM.YearlyGraphSeriesVMList) {
                        CustomBarSeries wholeSeries = new CustomBarSeries() {
                            IsStacked = true,
                            Title = tmpVM.Name,
                            ItemsSource = tmpVM.Values.Select((value, index) => new GraphDatumViewModel {
                                Value = value,
                                Number = index + startYear,
                                ItemId = tmpVM.ItemId,
                                CategoryId = tmpVM.CategoryId
                            }),
                            ValueField = "Value",
                            TrackerFormatString = "{0}\n{1}年度: {2:#,0}", //年度: 金額
                            XAxisKey = "Value",
                            YAxisKey = "Category"
                        };
                        // 全項目年間グラフの項目を選択した時のイベントを登録する
                        wholeSeries.TrackerHitResultChanged += (sender, e) => {
                            if (e.Value == null) return;

                            GraphDatumViewModel datumVM = e.Value.Item as GraphDatumViewModel;
                            this.WVM.SelectedYearlyGraphSeriesVM = this.WVM.YearlyGraphSeriesVMList.FirstOrDefault((tmp) => (tmp.CategoryId == datumVM.CategoryId && tmp.ItemId == datumVM.ItemId));
                        };
                        this.WVM.YearlyGraphPlotModel.Series.Add(wholeSeries);

                        // 全項目の月毎の合計を計算する
                        for (int i = 0; i < tmpVM.Values.Count; ++i) {
                            if (sumPlus.Count <= i) { sumPlus.Add(0); sumMinus.Add(0); }

                            if (tmpVM.Values[i] < 0) sumMinus[i] += tmpVM.Values[i];
                            else sumPlus[i] += tmpVM.Values[i];
                        }
                    }

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.WVM.YearlyGraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            this.SetAxisRange(axis, sumMinus.Min(), sumPlus.Max(), 10, true);
                            break;
                        }
                    }
                    break;
                }
                case GraphKind1.BalanceGraph: {
                    // グラフ表示データを取得する
                    this.WVM.YearlyGraphSeriesVMList = await this.LoadYearlySeriesViewModelListWithinDecadeAsync(this.WVM.SelectedBookVM.Id);

                    // グラフ表示データを設定する
                    this.WVM.YearlyGraphPlotModel.Series.Clear();
                    LineSeries series = new LineSeries() {
                        Title = "残高",
                        TrackerFormatString = "{2}年度: {4:#,0}" //年度: 金額
                    };
                    series.Points.AddRange(new List<int>(this.WVM.YearlyGraphSeriesVMList[0].Values).Select((value, index) => new DataPoint(index, value)));
                    this.WVM.YearlyGraphPlotModel.Series.Add(series);

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.WVM.YearlyGraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            this.SetAxisRange(axis, series.Points.Min((value) => value.Y), series.Points.Max((value) => value.Y), 10, true);
                            break;
                        }
                    }
                    break;
                }
            }
            this.WVM.YearlyGraphPlotModel.InvalidatePlot(true);

            this.WVM.SelectedYearlyGraphSeriesVM = this.WVM.YearlyGraphSeriesVMList.FirstOrDefault((vm) => (vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId));
        }
        
        /// <summary>
        /// 選択項目年別グラフを更新する
        /// </summary>
        private void UpdateSelectedYearlyGraph()
        {
            if (this.WVM.SelectedGraphKind1 != GraphKind1.IncomeAndOutgoGraph) return;

            Log.Info();

            Settings settings = Settings.Default;
            int startYear = DateTime.Now.GetFirstDateOfFiscalYear(settings.App_StartMonth).Year - 9;

            SeriesViewModel vm = this.WVM.SelectedYearlyGraphSeriesVM;

            // グラフ表示データを設定する
            this.WVM.SelectedYearlyGraphPlotModel.Series.Clear();
            if (vm != null) {
                CustomBarSeries selectedSeries = new CustomBarSeries() {
                    IsStacked = true,
                    Title = vm.Name,
                    FillColor = (this.WVM.YearlyGraphPlotModel.Series.FirstOrDefault(s => ((s as CustomBarSeries).Title == vm.Name)) as CustomBarSeries).ActualFillColor,
                    ItemsSource = vm.Values.Select((value, index) => new GraphDatumViewModel {
                        Value = value,
                        Number = index + startYear,
                        ItemId = vm.ItemId,
                        CategoryId = vm.CategoryId
                    }),
                    ValueField = "Value",
                    TrackerFormatString = "{1}年度: {2:#,0}", //年: 金額
                    XAxisKey = "Value",
                    YAxisKey = "Category"
                };
                this.WVM.SelectedYearlyGraphPlotModel.Series.Add(selectedSeries);

                // Y軸の範囲を設定する
                foreach (Axis axis in this.WVM.SelectedYearlyGraphPlotModel.Axes) {
                    if (axis.Position == AxisPosition.Left) {
                        this.SetAxisRange(axis, vm.Values.Min(), vm.Values.Max(), 4, true);
                        break;
                    }
                }
            }

            this.WVM.SelectedYearlyGraphPlotModel.InvalidatePlot(true);
        }
        #endregion

        /// <summary>
        /// 軸の範囲を設定する
        /// </summary>
        /// <param name="axis">軸</param>
        /// <param name="minValue">最小値</param>
        /// <param name="maxValue">最大値</param>
        /// <param name="divNum">目盛幅分割数(基準値)</param>
        /// <param name="isDisplayZero">0を表示するか</param>
        private void SetAxisRange(Axis axis, double minValue, double maxValue, int divNum, bool isDisplayZero)
        {
            double unit = 0.25; // 最大値/最小値の求める単位(1以下の値)
            Debug.Assert(0 < unit && unit <= 1);

            // 0を表示範囲に含める
            if (isDisplayZero && !(minValue < 0 && 0 < maxValue)) {
                if (Math.Abs(minValue - 1) < Math.Abs(maxValue + 1)) minValue = 0;
                else maxValue = 0;
            }

            // マージンを設ける
            double tmpMin = minValue * (minValue < 0 ? 1.05 : 0.95);
            double tmpMax = maxValue * (0 < maxValue ? 1.05 : 0.95);
            // 0はログが計算できないので近い値に置き換える
            tmpMin = (tmpMin == 0 || tmpMin == 1) ? -1 : tmpMin - 1;
            tmpMax = (tmpMax == -1 || tmpMax == 0) ? 1 : tmpMax + 1;

            double minDigit = Math.Floor(Math.Log10(Math.Abs(tmpMin))); // 最小値 の桁数
            double maxDigit = Math.Floor(Math.Log10(Math.Abs(tmpMax))); // 最大値 の桁数
            double diffDigit = Math.Max(minDigit, maxDigit);

            int minimum = (int)Math.Round(MathExtensions.Floor(tmpMin, Math.Pow(10, diffDigit) * unit)); // 軸の最小値
            int maximum = (int)Math.Round(MathExtensions.Ceiling(tmpMax, Math.Pow(10, diffDigit) * unit)); // 軸の最大値
            if (!(minValue == 0 && maxValue == 0)) {
                if (minValue == 0) minimum = 0;
                if (maxValue == 0) maximum = 0;
            }
            axis.Minimum = minimum;
            axis.Maximum = maximum;
            int majorStepBase = (int)(Math.Pow(10, diffDigit) * unit);
            axis.MajorStep = Math.Max((int)MathExtensions.Ceiling((double)(maximum - minimum) / divNum, majorStepBase), 1);
            axis.MinorStep = Math.Max(axis.MajorStep / 5, 1);
        }
        #endregion

        #region 設定反映用の関数
        /// <summary>
        /// ウィンドウ設定を読み込む
        /// </summary>
        public void LoadWindowSetting()
        {
            this.ChangedLocationOrSizeWrapper(() => {
                Properties.Settings settings = Properties.Settings.Default;
                settings.App_InitSizeFlag = false;
                settings.Save();

                if (0 <= settings.MainWindow_Left && 0 <= settings.MainWindow_Top) {
                    this.Left = settings.MainWindow_Left;
                    this.Top = settings.MainWindow_Top;
                }
                if (0 < settings.MainWindow_Width && 40 <= settings.MainWindow_Height) {
                    this.Width = settings.MainWindow_Width;
                    this.Height = settings.MainWindow_Height;
                }
                if (settings.MainWindow_WindowState != -1) {
                    this.WindowState = (WindowState)settings.MainWindow_WindowState;
                }

                this.windowLog.Log("SettingLoaded", true);
                this.ModifyLocationOrSize();
            });
        }

        /// <summary>
        /// ウィンドウ設定を保存する
        /// </summary>
        public void SaveWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;
            if (!settings.App_InitSizeFlag) {
                if (this.WindowState != WindowState.Minimized) {
                    settings.MainWindow_WindowState = (int)this.WindowState;
                }
                if (this.WindowState == WindowState.Normal) {
                    settings.MainWindow_Left = this.Left;
                    settings.MainWindow_Top = this.Top;
                    settings.MainWindow_Width = this.Width;
                    settings.MainWindow_Height = this.Height;
                }

                settings.Save();
            }
        }
        #endregion

        /// <summary>
        /// イベントハンドラをWVMに登録する
        /// </summary>
        private void RegisterEventHandlerToWVM()
        {
            Properties.Settings settings = Properties.Settings.Default;

            // タブ選択変更時
            this.WVM.SelectedTabChanged += async () => {
                Log.Info($"SelectedTabChanged SelectedTabIndex: {this.WVM.SelectedTabIndex}");

                this.WaitCursorCountIncrement();

                settings.MainWindow_SelectedTabIndex = this.WVM.SelectedTabIndex;
                settings.Save();

                await this.UpdateAsync(isUpdateBookList: true, isScroll: true);

                this.WaitCursorCountDecrement();
            };
            // 帳簿選択変更時
            this.WVM.SelectedBookChanged += async () => {
                Log.Info($"SelectedBookChanged SelectedBookId: {this.WVM.SelectedBookVM?.Id}");

                this.WaitCursorCountIncrement();

                settings.MainWindow_SelectedBookId = this.WVM.SelectedBookVM?.Id ?? -1;
                settings.Save();

                await this.UpdateAsync(isScroll: true);

                this.WaitCursorCountDecrement();
            };
            // グラフ種別選択変更時
            this.WVM.SelectedGraphKindChanged += async () => {
                Log.Info($"SelectedGraphKindChanged SelectedGraphKind1Index: {this.WVM.SelectedGraphKind1Index} SelectedGraphKind2Index: {this.WVM.SelectedGraphKind2Index}");

                this.WaitCursorCountIncrement();

                settings.MainWindow_SelectedGraphKindIndex = this.WVM.SelectedGraphKind1Index;
                settings.MainWindow_SelectedGraphKind2Index = this.WVM.SelectedGraphKind2Index;
                settings.Save();

                await this.UpdateAsync();

                this.WaitCursorCountDecrement();
            };
            // 系列選択変更時
            this.WVM.SelectedSeriesChanged += () => {
                Log.Info("SelectedSeriesChanged");

                this.WaitCursorCountIncrement();

                this.UpdateSelectedGraph();

                this.WaitCursorCountDecrement();
            };
        }

        /// <summary>
        /// ウィンドウ位置またはサイズを修正する
        /// </summary>
        private void ModifyLocationOrSize()
        {
            this.ChangedLocationOrSizeWrapper(() => {
                /// 位置調整
                if (30000 < Math.Max(Math.Abs(this.Left), Math.Abs(this.Top))) {
                    double tmpTop = this.Top;
                    double tmpLeft = this.Left;
                    if (30000 < Math.Max(Math.Abs(this.lastModLeft), Math.Abs(this.lastModTop))) {
                        this.Top = this.lastModTop;
                        this.Left = this.lastModLeft;
                    }
                    else {
                        this.MoveOwnersCenter();
                    }

                    if (tmpTop != this.Top || tmpLeft != this.Left) {
                        this.windowLog.Log("WindowLocationModified", true);
                    }
                    else {
                        this.windowLog.Log("FailedToModifyLocation", true);
                    }
                }
                /// サイズ調整
                if (this.Height < 40 || this.Width < 40) {
                    double tmpHeight = this.Height;
                    double tmpWidth = this.Width;
                    if (40 < this.lastModHeight && 40 < this.lastModWidth) {
                        this.Height = this.lastModHeight;
                        this.Width = this.lastModWidth;
                    }
                    else {
                        this.Height = 700;
                        this.Width = 1050;
                    }

                    if (tmpHeight != this.Height || tmpWidth != this.Width) {
                        this.windowLog.Log("WindowSizeModified", true);
                    }
                    else {
                        this.windowLog.Log("FailedToModifySize", true);
                    }
                }
            });

            this.lastModTop = this.Top;
            this.lastModLeft = this.Left;
            this.lastModWidth = this.Width;
            this.lastModHeight = this.Height;
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
            Properties.Settings settings = Properties.Settings.Default;

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

                if (tmpDumpExePath != string.Empty) {
                    if (tmpBackUpNum > 0) {
                        // 起動情報を設定する
                        ProcessStartInfo info = new ProcessStartInfo() {
                            FileName = tmpDumpExePath,
                            Arguments = string.Format(
                                "--host {0} --port {1} --username \"{2}\" --role \"{3}\" --no-password --format custom --data-only --verbose --file \"{4}\" \"{5}\"",
                                settings.App_Postgres_Host,
                                settings.App_Postgres_Port,
                                settings.App_Postgres_UserName,
                                settings.App_Postgres_Role,
                                string.Format(@"{0}/{1}", tmpBackUpFolderPath, BackupFileName),
                                settings.App_Postgres_DatabaseName),
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
