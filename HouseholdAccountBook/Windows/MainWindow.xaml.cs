using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Extentions;
using HouseholdAccountBook.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static HouseholdAccountBook.ConstValue.ConstValue;
using static HouseholdAccountBook.ViewModels.SettingsViewModel;

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
        private DaoBuilder builder;
        /// <summary>
        /// 前回選択していたタブ
        /// </summary>
        private Tab oldSelectedTab = Tab.BookTab;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        public MainWindow(DaoBuilder builder)
        {
            this.builder = builder;

            InitializeComponent();
            
            this.MainWindowVM.DisplayedMonth = DateTime.Now;

            UpdateBookList(Properties.Settings.Default.MainWindow_SelectedBookId);
            UpdateBookData();
            UpdateYearsListData();
            UpdateGraphData();
            UpdateSettingData();

            LoadSetting();
        }

        #region コマンド
        #region フォームの操作
        /// <summary>
        /// フォームを閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region ファイルの操作
        /// <summary>
        /// 記帳風月のDBを取り込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportKichoHugetsuDbCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string directory = string.Empty;
            string fileName = string.Empty;
            if (Properties.Settings.Default.App_KichoDBFilePath != string.Empty) {
                directory = Path.GetDirectoryName(Properties.Settings.Default.App_KichoDBFilePath);
                fileName = Path.GetFileName(Properties.Settings.Default.App_KichoDBFilePath);
            }

            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = "ファイル選択",
                Filter = "MDBファイル|*.mdb"
            };

            if (ofd.ShowDialog() == false) return;

            if (MessageBox.Show("既存のデータを削除します。よろしいですか？", this.Title,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            Properties.Settings.Default.App_KichoDBFilePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            Properties.Settings.Default.Save();

            Cursor cCursor = Cursor;
            Cursor = Cursors.Wait;

            bool isOpen = false;
            using (DaoOle daoOle = new DaoOle(ofd.FileName)) {
                isOpen = daoOle.IsOpen();

                if (isOpen) {
                    using (DaoBase dao = builder.Build()) {
                        dao.ExecTransaction(() => {
                            // 既存のデータの削除
                            dao.ExecNonQuery(@"DELETE FROM mst_book;");
                            dao.ExecNonQuery(@"DELETE FROM mst_category;");
                            dao.ExecNonQuery(@"DELETE FROM mst_item;");
                            dao.ExecNonQuery(@"DELETE FROM hst_action;");
                            dao.ExecNonQuery(@"DELETE FROM hst_group;");
                            dao.ExecNonQuery(@"DELETE FROM hst_remark;");
                            dao.ExecNonQuery(@"DELETE FROM rel_book_item;");

                            DaoReader reader;
                            reader = daoOle.ExecQuery(@"SELECT * FROM CBM_BOOK ORDER BY BOOK_ID;");
                            dao.ExecNonQuery("SELECT setval('mst_book_book_id_seq', @{0});", reader[reader.Count - 1].ToInt("BOOK_ID"));
                            reader.ExecWholeRow((count, record) => {
                                int bookId = record.ToInt("BOOK_ID");
                                string bookName = record["BOOK_NAME"];
                                int balance = record.ToInt("BALANCE");
                                int sortKey = record.ToInt("SORT_KEY");
                                int delFlg = record.ToBoolean("DEL_FLG") ? 1 : 0;

                                dao.ExecNonQuery(@"
INSERT INTO mst_book 
(book_id, book_name, initial_value, book_kind, pay_day, sort_order, del_flg, update_time, updater, insert_time, inserter) 
VALUES (@{0}, @{1}, @{2}, 0, NULL, @{3}, @{4}, 'now', @{5}, 'now', @{6});",
                                    bookId, bookName, balance, sortKey, delFlg, Updater, Inserter);
                            });

                            reader = daoOle.ExecQuery(@"SELECT * FROM CBM_CATEGORY ORDER BY CATEGORY_ID;");
                            dao.ExecNonQuery("SELECT setval('mst_category_category_id_seq', @{0});", reader[reader.Count - 1].ToInt("CATEGORY_ID"));
                            reader.ExecWholeRow((count, record) => {
                                int categoryId = record.ToInt("CATEGORY_ID");
                                string categoryName = record["CATEGORY_NAME"];
                                int rexpDiv = record.ToInt("REXP_DIV") - 1;
                                int sortKey = record.ToInt("SORT_KEY");
                                int delFlg = record.ToBoolean("DEL_FLG") ? 1 : 0;

                                dao.ExecNonQuery(@"
INSERT INTO mst_category
(category_id, category_name, balance_kind, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, 'now', @{5}, 'now', @{6});",
                                    categoryId, categoryName, rexpDiv, sortKey, delFlg, Updater, Inserter);
                            });

                            reader = daoOle.ExecQuery(@"SELECT * FROM CBM_ITEM ORDER BY ITEM_ID;");
                            dao.ExecNonQuery("SELECT setval('mst_item_item_id_seq', @{0});", reader[reader.Count - 1].ToInt("ITEM_ID"));
                            reader.ExecWholeRow((count, record) => {
                                int itemId = record.ToInt("ITEM_ID");
                                string itemName = record["ITEM_NAME"];
                                int moveFlg = record.ToBoolean("MOVE_FLG") ? 1 : 0;
                                int categoryId = record.ToInt("CATEGORY_ID");
                                int sortKey = record.ToInt("SORT_KEY");
                                int delFlg = record.ToBoolean("DEL_FLG") ? 1 : 0;

                                dao.ExecNonQuery(@"
INSERT INTO mst_item
(item_id, item_name, category_id, move_flg, advance_flg, sort_order, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, @{5}, @{6}, 'now', @{7}, 'now', @{8});",
                                    itemId, itemName, categoryId, moveFlg, 0, sortKey, delFlg, Updater, Inserter);
                            });

                            reader = daoOle.ExecQuery(@"SELECT * FROM CBT_ACT ORDER BY ACT_ID;");
                            dao.ExecNonQuery("SELECT setval('hst_action_action_id_seq', @{0});", reader[reader.Count - 1].ToInt("ACT_ID"));
                            int maxGroupId = 0;
                            reader.ExecWholeRow((count, record) => {
                                int actId = record.ToInt("ACT_ID");
                                int bookId = record.ToInt("BOOK_ID");
                                int itemId = record.ToInt("ITEM_ID");
                                string actDt = record["ACT_DT"];
                                int income = record.ToInt("INCOME");
                                int expense = record.ToInt("EXPENSE");
                                int? groupId = record.ToInt("GROUP_ID") == 0 ? null : (int?)record.ToInt("GROUP_ID");
                                string noteName = record["NOTE_NAME"];
                                int delFlg = record.ToBoolean("DEL_FLG") ? 1 : 0;

                                dao.ExecNonQuery(@"
INSERT INTO hst_action
(action_id, book_id, item_id, act_time, act_value, shop_name, group_id, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, @{3}, @{4}, NULL, @{5}, @{6}, @{7}, 'now', @{8}, 'now', @{9});",
                                    actId, bookId, itemId, DateTime.Parse(actDt), (income != 0 ? income : -expense),
                                    groupId, noteName, delFlg, Updater, Inserter);

                                // groupId が存在するとき
                                if (groupId != null) {
                                    if (maxGroupId < groupId) { maxGroupId = groupId.Value; }

                                    reader = dao.ExecQuery("SELECT * FROM hst_group WHERE group_id = @{0};", groupId);
                                    // groupId のレコードが登録されていないとき
                                    if (reader.Count == 0) {
                                        // グループの種類を調べる
                                        reader = daoOle.ExecQuery("SELECT * FROM CBT_ACT WHERE GROUP_ID = @{0};", groupId);
                                        GroupKind groupKind = GroupKind.Repeat;
                                        int? tmpBookId = null;
                                        reader.ExecWholeRow((count2, record2) => {
                                            if (tmpBookId == null) {
                                                tmpBookId = record2.ToInt("BOOK_ID");
                                            }
                                            else {
                                                if (tmpBookId != record2.ToInt("BOOK_ID")) {
                                                    // 帳簿が一致しない場合は移動
                                                    groupKind = GroupKind.Move;
                                                }
                                                else {
                                                    // 帳簿が一致する場合は繰り返し
                                                    groupKind = GroupKind.Repeat;
                                                }
                                            }
                                        });

                                        dao.ExecNonQuery(@"
INSERT INTO hst_group
(group_id, group_kind, remark, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, NULL, 0, 'now', @{2}, 'now', @{3});",
                                            groupId, (int)groupKind, Updater, Inserter);
                                    }
                                }
                            });
                            dao.ExecNonQuery("SELECT setval('hst_group_group_id_seq', @{0});", maxGroupId);

                            reader = daoOle.ExecQuery(@"SELECT * FROM CBT_NOTE;");
                            reader.ExecWholeRow((count, record) => {
                                int itemId = record.ToInt("ITEM_ID");
                                string noteName = record["NOTE_NAME"];
                                int delFlg = record.ToBoolean("DEL_FLG") ? 1 : 0;

                                dao.ExecNonQuery(@"
INSERT INTO hst_remark
(item_id, remark, remark_kind, used_time, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, 0, 'now', @{2}, 'now', @{3}, 'now', @{4});",
                                    itemId, noteName, delFlg, Updater, Inserter);
                            });

                            reader = daoOle.ExecQuery("SELECT * FROM CBR_BOOK;");
                            reader.ExecWholeRow((count, record) => {
                                int bookId = record.ToInt("BOOK_ID");
                                int itemId = record.ToInt("ITEM_ID");
                                int delFlg = record.ToBoolean("DEL_FLG") ? 1 : 0;

                                dao.ExecNonQuery(@"
INSERT INTO rel_book_item
(book_id, item_id, del_flg, update_time, updater, insert_time, inserter)
VALUES (@{0}, @{1}, @{2}, 'now', @{3}, 'now', @{4});",
                                    bookId, itemId, delFlg, Updater, Inserter);
                            });
                        });
                    }
                }
            }

            if (isOpen) {
                UpdateBookList();
                UpdateBookData();
                UpdateYearsListData();

                Cursor = cCursor;

                MessageBox.Show(Message.FinishToImport, MessageTitle.Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                Cursor = cCursor;
                MessageBox.Show(Message.FoultToImport, MessageTitle.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// カスタムファイル形式入力可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportCustomFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Properties.Settings.Default.App_Postgres_RestoreExePath != string.Empty;
        }

        /// <summary>
        /// カスタム形式ファイルを取り込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportCustomFileCommand_Excuted(object sender, ExecutedRoutedEventArgs e)
        {
            string directory = string.Empty;
            string fileName = string.Empty;
            if (Properties.Settings.Default.App_CustomFormatFilePath != string.Empty) {
                directory = Path.GetDirectoryName(Properties.Settings.Default.App_CustomFormatFilePath);
                fileName = Path.GetFileName(Properties.Settings.Default.App_CustomFormatFilePath);
            }

            OpenFileDialog ofd = new OpenFileDialog() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = "ファイル選択",
                Filter = "カスタム形式ファイル|*.*",
                CheckPathExists = true
            };

            if (ofd.ShowDialog() == false) return;

            if (MessageBox.Show("既存のデータを削除します。よろしいですか？", this.Title,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            Properties.Settings.Default.App_CustomFormatFilePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            Properties.Settings.Default.Save();

            Cursor cCursor = Cursor;
            Cursor = Cursors.Wait;

            using (DaoBase dao = builder.Build()) {
                // 既存のデータの削除
                dao.ExecNonQuery(@"DELETE FROM mst_book;");
                dao.ExecNonQuery(@"DELETE FROM mst_category;");
                dao.ExecNonQuery(@"DELETE FROM mst_item;");
                dao.ExecNonQuery(@"DELETE FROM hst_action;");
                dao.ExecNonQuery(@"DELETE FROM hst_group;");
                dao.ExecNonQuery(@"DELETE FROM hst_remark;");
                dao.ExecNonQuery(@"DELETE FROM rel_book_item;");
            }

            // 起動情報を設定する
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Properties.Settings.Default.App_Postgres_RestoreExePath;
            info.Arguments = string.Format(
                    "--host {0} --port {1} --username \"{2}\" --role \"{3}\" --no-password --data-only --verbose --dbname \"{5}\" \"{4}\"",
                    Properties.Settings.Default.App_Postgres_Host,
                    Properties.Settings.Default.App_Postgres_Port,
                    Properties.Settings.Default.App_Postgres_UserName,
                    Properties.Settings.Default.App_Postgres_Role,
                    ofd.FileName,
#if DEBUG
                    Properties.Settings.Default.App_Postgres_DatabaseName_Debug
#else
                    Properties.Settings.Default.App_Postgres_DatabaseName
#endif
                    );
            info.WindowStyle = ProcessWindowStyle.Hidden;

            // リストアする
            Process process = Process.Start(info);
            process.WaitForExit(10 * 1000);

            if (process.ExitCode == 0) {
                UpdateBookList();
                UpdateBookData();
                UpdateYearsListData();

                Cursor = cCursor;

                MessageBox.Show(Message.FinishToImport, MessageTitle.Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                Cursor = cCursor;

                MessageBox.Show(Message.FoultToImport, MessageTitle.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// カスタム形式ファイル出力可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportCustomFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Properties.Settings.Default.App_Postgres_DumpExePath != string.Empty;
        }

        /// <summary>
        /// カスタム形式ファイルに吐き出す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportCustomFileCommand_Excuted(object sender, ExecutedRoutedEventArgs e)
        {
            string directory = string.Empty;
            string fileName = string.Empty;
            if (Properties.Settings.Default.App_CustomFormatFilePath != string.Empty) {
                directory = Path.GetDirectoryName(Properties.Settings.Default.App_CustomFormatFilePath);
                fileName = Path.GetFileName(Properties.Settings.Default.App_CustomFormatFilePath);
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

            if (sfd.ShowDialog() == false) return;

            Properties.Settings.Default.App_CustomFormatFilePath = Path.Combine(sfd.InitialDirectory, sfd.FileName);
            Properties.Settings.Default.Save();

            Cursor cCursor = Cursor;
            Cursor = Cursors.Wait;

            // 起動情報を設定する
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Properties.Settings.Default.App_Postgres_DumpExePath;
            info.Arguments = string.Format(
                    "--host {0} --port {1} --username \"{2}\" --role \"{3}\" --no-password --format custom --data-only --verbose --file \"{4}\" \"{5}\"",
                    Properties.Settings.Default.App_Postgres_Host,
                    Properties.Settings.Default.App_Postgres_Port,
                    Properties.Settings.Default.App_Postgres_UserName,
                    Properties.Settings.Default.App_Postgres_Role, 
                    sfd.FileName,
#if DEBUG
                    Properties.Settings.Default.App_Postgres_DatabaseName_Debug
#else
                    Properties.Settings.Default.App_Postgres_DatabaseName
#endif
                    );
            info.WindowStyle = ProcessWindowStyle.Hidden;

            // バックアップする
            Process process = Process.Start(info);
            process.WaitForExit(10 * 1000);
            Cursor = cCursor;

            if (process.ExitCode == 0) {
                MessageBox.Show(Message.FinishToExport, MessageTitle.Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }else {
                MessageBox.Show(Message.FoultToExport, MessageTitle.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }
        #endregion

        #region 帳簿項目の操作
        /// <summary>
        /// 移動操作可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveToBookCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択していて、帳簿が2つ以上存在していて、選択されている帳簿が存在する
            e.CanExecute = this.MainWindowVM.SelectedTab == Tab.BookTab && this.MainWindowVM.BookVMList?.Count >= 2 && this.MainWindowVM.SelectedBookVM != null;
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveToBookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveRegistrationWindow mrw = new MoveRegistrationWindow(builder,
                this.MainWindowVM.SelectedBookVM.Id, this.MainWindowVM.SelectedActionVM?.ActTime);
            mrw.Registrated += (sender2, e2) => {
                UpdateBookData(e2.Id);
                actionDataGrid.Focus();
            };
            mrw.Owner = this;
            mrw.ShowDialog();
        }

        /// <summary>
        /// 項目追加可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionToBookCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択していて、選択されている帳簿が存在する
            e.CanExecute = this.MainWindowVM.SelectedTab == Tab.BookTab && this.MainWindowVM.SelectedBookVM != null;
        }

        /// <summary>
        /// 項目追加処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionToBookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ActionRegistrationWindow arw = new ActionRegistrationWindow(builder,
                this.MainWindowVM.SelectedBookVM.Id, this.MainWindowVM.SelectedActionVM?.ActTime);
            arw.Registrated += (sender2, e2)=> {
                UpdateBookData(e2.Id);
                actionDataGrid.Focus();
            };
            arw.Owner = this;
            arw.ShowDialog();
        }

        /// <summary>
        /// 項目まとめて追加可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionListToBookCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択していて、選択されている帳簿が存在する
            e.CanExecute = this.MainWindowVM.SelectedTab == Tab.BookTab && this.MainWindowVM.SelectedBookVM != null;
        }

        /// <summary>
        /// 項目まとめて追加処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionListToBookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ActionListRegistrationWindow alrw = new ActionListRegistrationWindow(builder,
                this.MainWindowVM.SelectedBookVM.Id, this.MainWindowVM.SelectedActionVM?.ActTime);
            alrw.Registrated += (sender2, e2) => {
                UpdateBookData(e2.Id);
                actionDataGrid.Focus();
            };
            alrw.Owner = this;
            alrw.ShowDialog();
        }

        /// <summary>
        /// 項目編集可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditActionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択していて、選択されている帳簿項目が存在していて、選択している帳簿項目のIDが0より大きい
            e.CanExecute = this.MainWindowVM.SelectedTab == Tab.BookTab && this.MainWindowVM.SelectedActionVM != null && this.MainWindowVM.SelectedActionVM.ActionId > 0;
        }

        /// <summary>
        /// 項目編集処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int? groupKind = null;
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT A.group_id, G.group_kind
FROM hst_action A
LEFT JOIN (SELECT * FROM hst_group WHERE del_flg = 0) G ON G.group_id = A.group_id
WHERE A.action_id = @{0} AND A.del_flg = 0;", this.MainWindowVM.SelectedActionVM.ActionId);

                reader.ExecARow((record) => {
                    groupKind = record.ToNullableInt("group_kind");
                });
            }

            if (groupKind == null || groupKind == (int)GroupKind.Repeat) {
                ActionRegistrationWindow arw = new ActionRegistrationWindow(builder, this.MainWindowVM.SelectedActionVM.ActionId);
                arw.Registrated += (sender2, e2) => {
                    UpdateBookData(e2.Id);
                    actionDataGrid.Focus();
                };
                arw.ShowDialog();
            }
            else {
                // 移動時の処理
                MoveRegistrationWindow mrw = new MoveRegistrationWindow(builder, this.MainWindowVM.SelectedBookVM.Id, this.MainWindowVM.SelectedActionVM.GroupId.Value);
                mrw.Registrated += (sender2, e2) => {
                    UpdateBookData(e2.Id);
                    actionDataGrid.Focus();
                };
                mrw.ShowDialog();
            }
        }

        /// <summary>
        /// 項目削除可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteActionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択していて、選択している帳簿項目が存在していて、選択している帳簿項目のIDが0より大きい
            e.CanExecute = this.MainWindowVM.SelectedTab == Tab.BookTab && this.MainWindowVM.SelectedActionVM != null && this.MainWindowVM.SelectedActionVM.ActionId > 0;
        }

        /// <summary>
        /// 項目削除処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int actionId = this.MainWindowVM.SelectedActionVM.ActionId;
            int? groupId = this.MainWindowVM.SelectedActionVM.GroupId;

            if (MessageBox.Show(Message.DeleteNotification, this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                using (DaoBase dao = builder.Build()) {
                    dao.ExecTransaction(() => {
                        dao.ExecNonQuery(@"
UPDATE hst_action SET del_flg = 1, update_time = 'now', updater = @{1} 
WHERE action_id = @{0};", actionId, Updater);

                        // 削除項目の日時以降の同じグループIDを持つ帳簿項目を削除する(日付の等号は「移動」削除用)
                        if (groupId != null) {
                            dao.ExecNonQuery(@"
UPDATE hst_action SET del_flg = 1, update_time = 'now', updater = @{1}
WHERE group_id = @{2} AND act_time >= (SELECT act_time FROM hst_action WHERE action_id = @{0});", actionId, Updater, groupId);

                            DaoReader reader = dao.ExecQuery(@"
SELECT action_id FROM hst_action
WHERE group_id = @{0} AND del_flg = 0;", groupId);

                            if(reader.Count == 1) {
                                // 同じグループIDを持つ帳簿項目が1つだけの場合にグループIDをクリアする
                                reader.ExecARow((record) => {
                                    dao.ExecNonQuery(@"
UPDATE hst_action SET group_id = null, update_time = 'now', updater = @{1}
WHERE group_id = @{0} AND del_flg = 0;", groupId, Updater);
                                });
                            }

                            if(reader.Count <= 1) {
                                // グループを削除する
                                dao.ExecNonQuery(@"
UPDATE hst_group
SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE del_flg = 0 AND group_id = @{1};", Updater, groupId);
                            }
                        }
                    });
                    
                }

                UpdateBookData();
            }
        }
        #endregion

        #region タブ表示の操作
        /// <summary>
        /// 帳簿タブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateBookCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.MainWindowVM.SelectedTab != Tab.BookTab;
        }

        /// <summary>
        /// 帳簿タブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateBookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWindowVM.SelectedTab = Tab.BookTab;
        }

        /// <summary>
        /// 一覧タブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateListCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.MainWindowVM.SelectedTab != Tab.ListTab;
        }

        /// <summary>
        /// 一覧タブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateListCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWindowVM.SelectedTab = Tab.ListTab;
        }

        /// <summary>
        /// グラフタブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateGraphCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.MainWindowVM.SelectedTab != Tab.GraphTab && false;
        }

        /// <summary>
        /// グラフタブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateGraphCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWindowVM.SelectedTab = Tab.GraphTab;
        }

        /// <summary>
        /// 設定タブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateSettingCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.MainWindowVM.SelectedTab != Tab.SettingTab;
        }

        /// <summary>
        /// 設定タブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndicateSettingCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWindowVM.SelectedTab = Tab.SettingTab;
        }
        #endregion

        #region 帳簿表示の操作
        /// <summary>
        /// 先月を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToLastMonthCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択している
            e.CanExecute = this.MainWindowVM.SelectedTab == Tab.BookTab;
        }

        /// <summary>
        /// (表示している月の)先月を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToLastMonthCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Cursor cCursor = Cursor;
            Cursor = Cursors.Wait;

            this.MainWindowVM.DisplayedMonth = this.MainWindowVM.DisplayedMonth.AddMonths(-1);
            UpdateBookData();

            Cursor = cCursor;
        }

        /// <summary>
        /// 今月を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToThisMonthCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DateTime thisMonth = DateTime.Today.GetFirstDateOfMonth();
            // 帳簿タブを選択している かつ 今月が表示されていない
            e.CanExecute = this.MainWindowVM.SelectedTab == Tab.BookTab && !(thisMonth <= this.MainWindowVM.DisplayedMonth && this.MainWindowVM.DisplayedYear < thisMonth.AddMonths(1));
        }

        /// <summary>
        /// 今月を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToThisMonthCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Cursor cCursor = Cursor;
            Cursor = Cursors.Wait;

            this.MainWindowVM.DisplayedMonth = DateTime.Now.GetFirstDateOfMonth();
            UpdateBookData();

            Cursor = cCursor;
        }

        /// <summary>
        /// 来月を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToNextMonthCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択している
            e.CanExecute = this.MainWindowVM.SelectedTab == Tab.BookTab;
        }

        /// <summary>
        /// (表示している月の)翌月を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToNextMonthCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Cursor cCursor = Cursor;
            Cursor = Cursors.Wait;

            this.MainWindowVM.DisplayedMonth = this.MainWindowVM.DisplayedMonth.AddMonths(1);
            UpdateBookData();

            Cursor = cCursor;
        }
        #endregion

        #region 年間一覧表示の操作
        /// <summary>
        /// 前年を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToLastYearCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択している
            e.CanExecute = this.MainWindowVM.SelectedTab == Tab.ListTab;
        }

        /// <summary>
        /// (表示している年の)前年を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToLastYearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Cursor cCursor = Cursor;
            Cursor = Cursors.Wait;

            this.MainWindowVM.DisplayedYear = this.MainWindowVM.DisplayedYear.AddYears(-1);
            UpdateYearsListData();

            Cursor = cCursor;
        }

        /// <summary>
        /// 今年を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToThisYearCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DateTime thisYear = DateTime.Now.GetFirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);
            // 帳簿タブを選択している かつ 今年が表示されていない
            e.CanExecute = this.MainWindowVM.SelectedTab == Tab.ListTab && !(thisYear <= this.MainWindowVM.DisplayedYear && this.MainWindowVM.DisplayedYear < thisYear.AddYears(1));
        }

        /// <summary>
        /// 今年を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToThisYearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Cursor cCursor = Cursor;
            Cursor = Cursors.Wait;

            this.MainWindowVM.DisplayedYear = DateTime.Now.GetFirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);
            UpdateYearsListData();

            Cursor = cCursor;
        }

        /// <summary>
        /// 来年を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToNextYearCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択している
            e.CanExecute = this.MainWindowVM.SelectedTab == Tab.ListTab;
        }

        /// <summary>
        /// (表示している年の)翌年を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToNextYearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Cursor cCursor = Cursor;
            Cursor = Cursors.Wait;

            this.MainWindowVM.DisplayedYear = this.MainWindowVM.DisplayedYear.AddYears(1);
            UpdateYearsListData();

            Cursor = cCursor;
        }
        #endregion

        #region 設定の操作
        /// <summary>
        /// カテゴリを追加可能か判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddCategoryCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.SettingsVM.SelectedItemVM != null && this.SettingsVM.SelectedItemVM.Kind != HierarchicalKind.Item;
        }

        /// <summary>
        /// カテゴリを追加する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddCategoryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        /// <summary>
        /// 項目を追加可能か判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.SettingsVM.SelectedItemVM != null && this.SettingsVM.SelectedItemVM.Kind != HierarchicalKind.Balance;
        }

        /// <summary>
        /// 項目を追加する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        /// <summary>
        /// 分類/項目を削除可能か判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.SettingsVM.SelectedItemVM != null && this.SettingsVM.SelectedItemVM.Kind != HierarchicalKind.Balance;
        }

        /// <summary>
        /// 分類/項目を削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        /// <summary>
        /// 分類/項目の表示順を上げれるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RaiseItemSortOrderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.SettingsVM.SelectedItemVM != null && this.SettingsVM.SelectedItemVM.ParentVM != null;
            if (e.CanExecute) {
                // 同じ階層で、よりソート順序が上の分類/項目がある場合trueになる
                var parentVM = this.SettingsVM.SelectedItemVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.SettingsVM.SelectedItemVM);
                e.CanExecute = 0 < index;

                // 選択された対象が項目で分類内の最も上位にいる場合
                if (!e.CanExecute && parentVM.ParentVM != null) {
                    // 項目の属する分類について、同じ階層内によりソート順序が上の分類がある場合trueになる
                    var grandparentVM = parentVM.ParentVM;
                    int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                    e.CanExecute = 0 < index2;
                }
            }
        }

        /// <summary>
        /// 分類/項目の表示順を上げる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RaiseItemSortOrderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        /// <summary>
        /// 分類/項目の表示順を下げれるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropItemSortOrderCommand_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.SettingsVM.SelectedItemVM != null && this.SettingsVM.SelectedItemVM.ParentVM != null;
            if (e.CanExecute) {
                // 同じ階層で、よりソート順序が下の分類/項目がある場合trueになる
                var parentVM = this.SettingsVM.SelectedItemVM.ParentVM;
                int index = parentVM.ChildrenVMList.IndexOf(this.SettingsVM.SelectedItemVM);
                e.CanExecute = parentVM.ChildrenVMList.Count - 1 > index;

                // 選択された対象が項目で分類内の最も上位にいる場合
                if (!e.CanExecute && parentVM.ParentVM != null) {
                    // 項目の属する分類について、同じ階層内によりソート順序が下の分類がある場合trueになる
                    var grandparentVM = parentVM.ParentVM;
                    int index2 = grandparentVM.ChildrenVMList.IndexOf(parentVM);
                    e.CanExecute = grandparentVM.ChildrenVMList.Count - 1 > index2;
                }
            }
        }

        /// <summary>
        /// 分類/項目の表示順を下げる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropItemSortOrderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        /// <summary>
        /// 帳簿を追加する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddBookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        /// <summary>
        /// 帳簿を削除可能か判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteBookCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.SettingsVM.SelectedBookVM != null;
        }

        /// <summary>
        /// 帳簿を削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteBookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        /// <summary>
        /// 帳簿の表示順を上げれるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RaiseBookSortOrderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.SettingsVM.BookVMList != null;
            if (e.CanExecute) {
                int index = this.SettingsVM.BookVMList.IndexOf(this.SettingsVM.SelectedBookVM);
                e.CanExecute = index > 0;
            }
        }

        /// <summary>
        /// 帳簿の表示順を上げる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RaiseBookSortOrderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        /// <summary>
        /// 帳簿の表示順を下げれるか判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropBookSortOrderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.SettingsVM.BookVMList != null;
            if (e.CanExecute) {
                int index = this.SettingsVM.BookVMList.IndexOf(this.SettingsVM.SelectedBookVM);
                e.CanExecute = index != -1 && index < this.SettingsVM.BookVMList.Count - 1;
            }
        }

        /// <summary>
        /// 帳簿の表示順を下げる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropBookSortOrderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        #endregion
        #endregion

        #region イベントハンドラ
        /// <summary>
        /// 読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 処理なし
        }

        /// <summary>
        /// フォーム終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveSetting();
        }

        /// <summary>
        /// 選択中の帳簿を変更した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BookComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Cursor cCursor = Cursor;
            Cursor = Cursors.Wait;
            
            UpdateBookData();
            UpdateYearsListData();
            UpdateGraphData();

            Cursor = cCursor;
        }

        /// <summary>
        /// 選択中のタブを変更した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (oldSelectedTab != this.MainWindowVM.SelectedTab) {
                Cursor cCursor = Cursor;
                Cursor = Cursors.Wait;

                switch (this.MainWindowVM.SelectedTab) {
                    case Tab.BookTab:
                        UpdateBookData();
                        break;
                    case Tab.ListTab:
                        UpdateYearsListData();
                        break;
                    case Tab.GraphTab:
                        UpdateGraphData();
                        break;
                    case Tab.SettingTab:
                        UpdateSettingData();
                        break;
                }
                Cursor = cCursor;
            }
            oldSelectedTab = this.MainWindowVM.SelectedTab;
        }

        #region 帳簿項目操作
        /// <summary>
        /// 帳簿項目をダブルクリックした時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RoutedCommand command = this.Resources["EditActionCommand"] as RoutedCommand;

            if (command != null && command.CanExecute(null, sender as IInputElement)) {
                command.Execute(null, sender as IInputElement);
            }
            e.Handled = true;
        }

        /// <summary>
        /// 帳簿項目選択中にキー入力した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActionDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) {
                case Key.Enter: {
                        RoutedCommand command = this.Resources["EditActionCommand"] as RoutedCommand;

                        if (command != null && command.CanExecute(null, sender as IInputElement)) {
                            command.Execute(null, sender as IInputElement);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.Delete: {
                        RoutedCommand command = this.Resources["DeleteActionCommand"] as RoutedCommand;

                        if (command != null && command.CanExecute(null, sender as IInputElement)) {
                            command.Execute(null, sender as IInputElement);
                        }
                        e.Handled = true;
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 設定操作
        /// <summary>
        /// 項目設定で一覧の選択を変更した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView treeView = sender as TreeView;
            SettingsVM.SelectedItemVM = treeView.SelectedItem as HierarchicalItemViewModel;

            if (this.shopNameListBox.Items.Count > 0 && VisualTreeHelper.GetChildrenCount(this.shopNameListBox) > 0) {
                if (VisualTreeHelper.GetChild(this.shopNameListBox, 0) is Decorator border) {
                    if (border.Child is ScrollViewer scroll) {
                        scroll.ScrollToTop();
                    }
                }
            }
            if (this.remarkListBox.Items.Count > 0 && VisualTreeHelper.GetChildrenCount(this.remarkListBox) > 0) {
                if (VisualTreeHelper.GetChild(this.remarkListBox, 0) is Decorator border) {
                    if (border.Child is ScrollViewer scroll) {
                        scroll.ScrollToTop();
                    }
                }
            }
        }

        /// <summary>
        /// 項目設定の店舗名リストでキー入力した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShopNameListBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) {
                case Key.Delete: {
                        if (this.SettingsVM.SelectedItemVM?.SelectedShopName != null) {
                            if (MessageBox.Show(Message.DeleteNotification, MessageTitle.Information, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                                Debug.Assert(this.SettingsVM.SelectedItemVM.Kind == HierarchicalKind.Item);
                                using (DaoBase dao = builder.Build()) {
                                    dao.ExecQuery(@"
UPDATE hst_shop SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE shop_name = @{1} AND item_id = @{2};", Updater, this.SettingsVM.SelectedItemVM.SelectedShopName, this.SettingsVM.SelectedItemVM.Id);
                                }

                                using (DaoBase dao = builder.Build()) {
                                    DaoReader reader = dao.ExecQuery(@"
SELECT shop_name
FROM hst_shop
WHERE del_flg = 0 AND item_id = @{0}
ORDER BY used_time;", this.SettingsVM.SelectedItemVM.Id);

                                    this.SettingsVM.SelectedItemVM.ShopNameList.Clear();
                                    reader.ExecWholeRow((count2, record2) => {
                                        string shopName = record2["shop_name"];

                                        this.SettingsVM.SelectedItemVM.ShopNameList.Add(shopName);
                                    });
                                }
                            }
                            e.Handled = true;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 項目設定の備考リストでキー入力した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemarkListBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) {
                case Key.Delete: {
                        if (this.SettingsVM.SelectedItemVM?.SelectedRemark != null) {
                            if (MessageBox.Show(Message.DeleteNotification, MessageTitle.Information, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                                Debug.Assert(this.SettingsVM.SelectedItemVM.Kind == HierarchicalKind.Item);
                                using (DaoBase dao = builder.Build()) {
                                    dao.ExecQuery(@"
UPDATE hst_remark SET del_flg = 1, update_time = 'now', updater = @{0}
WHERE remark = @{1} AND item_id = @{2};", Updater, this.SettingsVM.SelectedItemVM.SelectedRemark, this.SettingsVM.SelectedItemVM.Id);
                                }
                            }

                            using (DaoBase dao = builder.Build()) {
                                DaoReader reader = dao.ExecQuery(@"
SELECT remark
FROM hst_remark
WHERE del_flg = 0 AND item_id = @{0}
ORDER BY used_time;", this.SettingsVM.SelectedItemVM.Id);

                                this.SettingsVM.SelectedItemVM.RemarkList.Clear();
                                reader.ExecWholeRow((count2, record2) => {
                                    string remark = record2["remark"];

                                    this.SettingsVM.SelectedItemVM.RemarkList.Add(remark);
                                });
                            }
                            e.Handled = true;
                        }
                    }
                    break;
            }
        }
        #endregion
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        private void UpdateBookList(int? bookId = null)
        {
            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>() {
                new BookViewModel() { Id = null, Name = "一覧" }
            };
            BookViewModel selectedBookVM = bookVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT * 
FROM mst_book 
WHERE del_flg = 0 
ORDER BY sort_order;");
                reader.ExecWholeRow((count, record) => {
                    BookViewModel vm = new BookViewModel() { Id = record.ToInt("book_id"), Name = record["book_name"] };
                    bookVMList.Add(vm);

                    if(vm.Id == bookId) {
                        selectedBookVM = vm;
                    }
                });
            }
            this.MainWindowVM.BookVMList = bookVMList;
            this.MainWindowVM.SelectedBookVM = selectedBookVM;
        }

        /// <summary>
        /// 帳簿タブに表示するデータを更新する
        /// </summary>
        /// <param name="actionId">選択対象の帳簿項目ID</param>
        private void UpdateBookData(int? actionId = null)
        {
            if (this.MainWindowVM.SelectedTab == Tab.BookTab) {
                this.MainWindowVM.ActionVMList = LoadActionViewModelList(
                    this.MainWindowVM.SelectedBookVM?.Id, this.MainWindowVM.DisplayedMonth.GetFirstDateOfMonth(),
                    this.MainWindowVM.DisplayedMonth.GetFirstDateOfMonth().AddMonths(1).AddMilliseconds(-1));

                this.MainWindowVM.SummaryVMList = LoadSummaryViewModelList(
                    this.MainWindowVM.SelectedBookVM?.Id, this.MainWindowVM.DisplayedMonth.GetFirstDateOfMonth(),
                    this.MainWindowVM.DisplayedMonth.GetFirstDateOfMonth().AddMonths(1).AddMilliseconds(-1));

                IEnumerable<ActionViewModel> query = this.MainWindowVM.ActionVMList.Where((avm) => { return avm.ActionId == actionId; });
                this.MainWindowVM.SelectedActionVM = query.Count() == 0 ? null : query.First();

                if (this.actionDataGrid.Items.Count > 0 && VisualTreeHelper.GetChildrenCount(this.actionDataGrid) > 0) {
                    if (VisualTreeHelper.GetChild(this.actionDataGrid, 0) is Decorator border) {
                        if (border.Child is ScrollViewer scroll) {
                            if (this.MainWindowVM.DisplayedMonth.GetFirstDateOfMonth() < DateTime.Today && DateTime.Today < this.MainWindowVM.DisplayedMonth.GetFirstDateOfMonth().AddMonths(1).AddMilliseconds(-1)) {
                                // 今月の場合は、末尾が表示されるようにする
                                scroll.ScrollToBottom();
                            }
                            else {
                                // 今月でない場合は、先頭が表示されるようにする
                                scroll.ScrollToTop();
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 帳簿項目VMリストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始日</param>
        /// <param name="endTime">終了日</param>
        /// <param name="actionId">選択対象の帳簿項目ID</param>
        /// <returns>帳簿項目VMリスト</returns>
        private ObservableCollection<ActionViewModel> LoadActionViewModelList(int? bookId, DateTime startTime, DateTime endTime)
        {
            ObservableCollection<ActionViewModel> actionVMList = new ObservableCollection<ActionViewModel>();
            using (DaoBase dao = builder.Build()) {
                DaoReader reader;
                if (bookId == null) {
                    // 全帳簿
                    reader = dao.ExecQuery(@"
-- 繰越残高
SELECT -1 AS action_id, @{1} AS act_time, '繰越残高' AS item_name, 0 AS act_value, (
  SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT COALESCE(SUM(initial_value), 0) FROM mst_book WHERE del_flg = 0)
  FROM hst_action AA
  INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
  WHERE AA.del_flg = 0 AND AA.act_time < @{1}
) AS balance, NULL AS shop_name, NULL AS group_id, NULL AS remark
UNION
-- 各帳簿項目
SELECT A.action_id AS action_id, A.act_time AS act_time, I.item_name AS item_name, A.act_value AS act_value, (
  SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT COALESCE(SUM(initial_value), 0) FROM mst_book WHERE del_flg = 0)
  FROM hst_action AA 
  INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
  WHERE AA.del_flg = 0 AND (AA.act_time < A.act_time OR (AA.act_time = A.act_time AND AA.action_id <= A.action_id) )
) AS balance, A.shop_name AS shop_name, A.group_id AS group_id, A.remark AS remark
FROM hst_action A
INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) B ON B.book_id = A.book_id
INNER JOIN (SELECT * FROM mst_item WHERE item_id IN (
  SELECT RBI.item_id 
  FROM rel_book_item RBI
  INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) B ON B.book_id = RBI.book_id
  WHERE RBI.del_flg = 0
) AND del_flg = 0) I ON I.item_id = A.item_id
WHERE A.del_flg = 0 AND @{1} <= A.act_time AND A.act_time <= @{2}
ORDER BY act_time, action_id;", null, startTime, endTime);
                }
                else {
                    // 各帳簿
                    reader = dao.ExecQuery(@"
-- 繰越残高
SELECT -1 AS action_id, @{1} AS act_time, '繰越残高' AS item_name, 0 AS act_value, (
  SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT initial_value FROM mst_book WHERE book_id = @{0})
  FROM hst_action AA
  INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
  WHERE AA.book_id = @{0} AND AA.del_flg = 0 AND AA.act_time < @{1}
) AS balance, NULL AS shop_name, NULL AS group_id, NULL AS remark
UNION
-- 各帳簿項目
SELECT A.action_id AS action_id, A.act_time AS act_time, I.item_name AS item_name, A.act_value AS act_value, (
  SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT initial_value FROM mst_book WHERE book_id = @{0})
  FROM hst_action AA 
  WHERE AA.book_id = @{0} AND AA.del_flg = 0 AND (AA.act_time < A.act_time OR (AA.act_time = A.act_time AND AA.action_id <= A.action_id) )
) AS balance, A.shop_name AS shop_name, A.group_id AS group_id, A.remark AS remark
FROM hst_action A
INNER JOIN (SELECT * FROM mst_item WHERE (move_flg = 1 OR item_id IN (
  SELECT item_id FROM rel_book_item WHERE book_id = @{0} AND del_flg = 0
)) AND del_flg = 0) I ON I.item_id = A.item_id
WHERE A.book_id = @{0} AND A.del_flg = 0 AND @{1} <= A.act_time AND A.act_time <= @{2}
ORDER BY act_time, action_id;", bookId, startTime, endTime);
                }
                reader.ExecWholeRow((count, record) => {
                    ActionViewModel avm = new ActionViewModel() {
                        ActionId = record.ToInt("action_id"),
                        ActTime = DateTime.Parse(record["act_time"]),
                        ItemName = record["item_name"],
                        Balance = record.ToInt("balance"),
                        ShopName = record["shop_name"],
                        GroupId = record.ToNullableInt("group_id"),
                        Remark = record["remark"]
                    };
                    int actValue = record.ToInt("act_value");
                    if (actValue == 0) {
                        avm.Income = null;
                        avm.Outgo = null;
                    }
                    else if (actValue < 0) {
                        avm.Income = null;
                        avm.Outgo = -actValue;
                    }
                    else {
                        avm.Income = actValue;
                        avm.Outgo = null;
                    }

                    actionVMList.Add(avm);
                });
            }

            return actionVMList;
        }

        /// <summary>
        /// 合計項目VMリストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始日</param>
        /// <param name="endTime">終了日</param>
        /// <returns>合計項目VMリスト</returns>
        private ObservableCollection<SummaryViewModel> LoadSummaryViewModelList(int? bookId, DateTime startTime, DateTime endTime)
        {
            ObservableCollection<SummaryViewModel> summaryVMList = new ObservableCollection<SummaryViewModel>();

            using (DaoBase dao = builder.Build()) {
                DaoReader reader;

                if (bookId == null) {
                    reader = dao.ExecQuery(@"
-- 全帳簿が対象
SELECT C.balance_kind AS balance_kind, C.category_id AS category_id, C.category_name AS category_name, SQ.item_id AS item_id, I.item_name AS item_name, SQ.sum AS sum
FROM (
  SELECT I.item_id AS item_id, COALESCE(SUM(A.act_value), 0) AS sum
  FROM mst_item I
  LEFT JOIN (SELECT * FROM hst_action WHERE @{0} <= act_time AND act_time <= @{1} AND del_flg = 0) A ON A.item_id = I.item_id
  WHERE I.item_id IN (SELECT item_id FROM rel_book_item WHERE del_flg = 0) AND I.del_flg = 0
  GROUP BY I.item_id
) SQ -- Sub Query
INNER JOIN (SELECT * FROM mst_item WHERE del_flg = 0) I ON I.item_id = SQ.item_id
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
ORDER BY C.balance_kind, C.sort_order, I.sort_order;", startTime, endTime);
                }
                else {
                    reader = dao.ExecQuery(@"
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
                        CategoryId = categoryId,
                        CategoryName = categoryName,
                        ItemId = itemId,
                        ItemName = String.Format("  {0}", itemName),
                        Summary = summary
                    });
                });
            }

            // 差引損益
            int total = summaryVMList.Sum(obj => obj.Summary);
            // 収入/支出
            List<SummaryViewModel> totalAsBalanceKind = new List<SummaryViewModel>();
            // カテゴリ小計
            List<SummaryViewModel> totalAsCategory = new List<SummaryViewModel>();

            foreach (IGrouping<int, SummaryViewModel> g1 in summaryVMList.GroupBy(obj => obj.BalanceKind)) {
                // 収入/支出の小計を計算する
                totalAsBalanceKind.Add(new SummaryViewModel() {
                    BalanceKind = g1.Key,
                    CategoryId = -1,
                    CategoryName = String.Empty,
                    ItemId = -1,
                    ItemName = BalanceStr[(BalanceKind)g1.Key],
                    Summary = g1.Sum(obj => obj.Summary)
                });
                // カテゴリ別の小計を計算する
                foreach (IGrouping<int, SummaryViewModel> g2 in g1.GroupBy(obj => obj.CategoryId)) {
                    totalAsCategory.Add(new SummaryViewModel() {
                        BalanceKind = g1.Key,
                        CategoryId = g2.Key,
                        CategoryName = String.Empty,
                        ItemId = -1,
                        ItemName = g2.First().CategoryName,
                        Summary = g2.Sum(obj => obj.Summary)
                    });
                }
            }

            // 差引損益を追加する
            summaryVMList.Insert(0, new SummaryViewModel() {
                BalanceKind = -1,
                CategoryId = -1,
                CategoryName = String.Empty,
                ItemId = -1,
                ItemName = "差引損益",
                Summary = total
            });
            // 収入/支出の小計を追加する
            foreach (SummaryViewModel svm in totalAsBalanceKind) {
                summaryVMList.Insert(summaryVMList.IndexOf(summaryVMList.First(obj => obj.BalanceKind == svm.BalanceKind)), svm);
            }
            // カテゴリ別の小計を追加する
            foreach (SummaryViewModel svm in totalAsCategory) {
                summaryVMList.Insert(summaryVMList.IndexOf(summaryVMList.First(obj => obj.CategoryId == svm.CategoryId)), svm);
            }

            return summaryVMList;
        }

        /// <summary>
        /// 年間一覧タブに表示するデータを更新する
        /// </summary>
        private void UpdateYearsListData()
        {
            if (this.MainWindowVM.SelectedTab == Tab.ListTab) {
                int startMonth = Properties.Settings.Default.App_StartMonth;

                // 表示する月の文字列を作成する
                ObservableCollection<string> displayedMonths = new ObservableCollection<string>();
                for (int i = startMonth; i < startMonth + 12; ++i) {
                    displayedMonths.Add(string.Format("{0}月", (i - 1) % 12 + 1));
                }
                this.MainWindowVM.DisplayedMonths = displayedMonths;
                this.MainWindowVM.SummaryWithinYearVMList = LoadSummaryWithinYearViewModelList(this.MainWindowVM.SelectedBookVM.Id, this.MainWindowVM.DisplayedYear);
            }
        }

        /// <summary>
        /// 年度内合計項目VMリストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="year">表示年</param>
        /// <returns>年度内合計項目VMリスト</returns>
        private ObservableCollection<SummaryWithinYearViewModel> LoadSummaryWithinYearViewModelList(int? bookId, DateTime year)
        {
            DateTime startTime = new DateTime(year.Year, Properties.Settings.Default.App_StartMonth, 1);
            DateTime endTime = startTime.AddMonths(1).AddMilliseconds(-1);

            // 開始月までの収支を取得する
            int balance = 0;
            using (DaoBase dao = builder.Build()) {
                DaoReader reader;
                if (bookId == null) {
                    // 全帳簿
                    reader = dao.ExecQuery(@"
SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT COALESCE(SUM(initial_value), 0) FROM mst_book WHERE del_flg = 0) AS sum
FROM hst_action AA
INNER JOIN(SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
WHERE AA.del_flg = 0 AND AA.act_time < @{1};", null, startTime);
                }
                else {
                    // 各帳簿
                    reader = dao.ExecQuery(@"
SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT initial_value FROM mst_book WHERE book_id = @{0}) AS sum
FROM hst_action AA
INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
WHERE AA.book_id = @{0} AND AA.del_flg = 0 AND AA.act_time < @{1};", bookId, startTime);
                }

                reader.ExecARow((record) => {
                    balance = record.ToInt("sum");
                });
            }

            ObservableCollection<SummaryWithinYearViewModel> vmList = new ObservableCollection<SummaryWithinYearViewModel>();
            vmList.Add(new SummaryWithinYearViewModel() {
                BalanceKind = -1,
                CategoryId = -1,
                CategoryName = String.Empty,
                ItemId = -1,
                ItemName = "残高",
                Values = new List<int>()
            });

            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の月の分を取得する
            ObservableCollection<SummaryViewModel> summaryVMList = this.LoadSummaryViewModelList(bookId, startTime, endTime);
            balance = balance + summaryVMList[0].Summary;
            vmList[0].Values.Add(balance); // 残高
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Summary;
                SummaryWithinYearViewModel vm = new SummaryWithinYearViewModel() {
                    BalanceKind = summaryVM.BalanceKind,
                    CategoryId = summaryVM.CategoryId,
                    CategoryName = summaryVM.CategoryName,
                    ItemId = summaryVM.ItemId,
                    ItemName = summaryVM.ItemName,
                    Values = new List<int>(),
                    Summary = value
                };
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

            // 最初以外の月の分を取得する
            for (int i = 1; i < 12; ++i) {
                startTime = startTime.AddMonths(1);
                endTime = startTime.AddMonths(1).AddMilliseconds(-1);

                summaryVMList = this.LoadSummaryViewModelList(bookId, startTime, endTime);
                balance = balance + summaryVMList[0].Summary;
                vmList[0].Values.Add(balance); // 残高
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    int value = summaryVMList[j].Summary;

                    vmList[j + 1].Values.Add(value);

                    if (endTime < DateTime.Now) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Summary += value;
                }
                if (endTime < DateTime.Now) {
                    ++averageCount;
                }
            }

            // 平均値を計算する
            foreach (SummaryWithinYearViewModel vm in vmList) {
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
        /// グラフタブに表示するデータを更新する
        /// </summary>
        private void UpdateGraphData()
        {
            if(this.MainWindowVM.SelectedTab == Tab.GraphTab) {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 設定タブに表示するデータを更新する
        /// </summary>
        private void UpdateSettingData() {
            if(this.MainWindowVM.SelectedTab == Tab.SettingTab) {
                this.SettingsVM.HierachicalItemVMList = LoadItemViewModelList();
                this.SettingsVM.BookVMList = LoadBookSettingViewModelList();
            }
        }

        /// <summary>
        /// 階層構造項目VMリストを取得する
        /// </summary>
        /// <returns>階層構造項目VMリスト</returns>
        private ObservableCollection<HierarchicalItemViewModel> LoadItemViewModelList()
        {
            ObservableCollection<HierarchicalItemViewModel> vmList = new ObservableCollection<HierarchicalItemViewModel>();
            HierarchicalItemViewModel incomeVM = new HierarchicalItemViewModel() {
                Kind = HierarchicalKind.Balance,
                Id = (int)BalanceKind.Income,
                Name = "収入項目",
                ParentVM = null,
                RelationVMList = null,
                ChildrenVMList = new ObservableCollection<HierarchicalItemViewModel>()
            };
            vmList.Add(incomeVM);

            HierarchicalItemViewModel outgoVM = new HierarchicalItemViewModel() {
                Kind = HierarchicalKind.Balance,
                Id = (int)BalanceKind.Outgo,
                Name = "支出項目",
                ParentVM = null,
                RelationVMList = null,
                ChildrenVMList = new ObservableCollection<HierarchicalItemViewModel>()
            };
            vmList.Add(outgoVM);

            foreach (HierarchicalItemViewModel vm in vmList) {
                using (DaoBase dao = builder.Build()) {
                    DaoReader reader = dao.ExecQuery(@"
SELECT category_id, category_name 
FROM mst_category
WHERE balance_kind = @{0} AND del_flg = 0 AND sort_order <> 0
ORDER BY sort_order;", vm.Id);

                    reader.ExecWholeRow((count, record) => {
                        int categoryId = record.ToInt("category_id");
                        string categoryName = record["category_name"];

                        vm.ChildrenVMList.Add(new HierarchicalItemViewModel() {
                            Kind = HierarchicalKind.Category,
                            Id = categoryId,
                            Name = categoryName,
                            ParentVM = vm,
                            RelationVMList = null,
                            ChildrenVMList = new ObservableCollection<HierarchicalItemViewModel>()
                        });
                    });

                    foreach (HierarchicalItemViewModel childVM in vm.ChildrenVMList) {
                        reader = dao.ExecQuery(@"
SELECT item_id, item_name
FROM mst_item
WHERE category_id = @{0} AND del_flg = 0
ORDER BY sort_order;", childVM.Id);

                        reader.ExecWholeRow((count, record) => {
                            int itemId = record.ToInt("item_id");
                            string itemName = record["item_name"];

                            childVM.ChildrenVMList.Add(new HierarchicalItemViewModel() {
                                Kind = HierarchicalKind.Item,
                                Id = itemId,
                                Name = itemName,
                                ParentVM = childVM,
                                RelationVMList = new ObservableCollection<RelationViewModel>(),
                                ShopNameList = new ObservableCollection<string>(),
                                RemarkList = new ObservableCollection<string>()
                            });
                        });

                        foreach (HierarchicalItemViewModel vm2 in childVM.ChildrenVMList) {
                            reader = dao.ExecQuery(@"
SELECT B.book_id AS BookId, B.book_name, RBI.book_id IS NULL AS IsNotRelated
FROM mst_book B
LEFT JOIN (SELECT book_id FROM rel_book_item WHERE del_flg = 0 AND item_id = @{0}) RBI ON RBI.book_id = B.book_id
ORDER BY B.sort_order;", vm2.Id);

                            reader.ExecWholeRow((count2, record2) => {
                                int bookId = record2.ToInt("BookId");
                                string bookName = record2["book_name"];
                                bool isRelated = !record2.ToBoolean("IsNotRelated");

                                vm2.RelationVMList.Add(new RelationViewModel() {
                                    Id = bookId,
                                    Name = bookName,
                                    IsRelated = isRelated
                                });
                            });

                            // 店舗名の一覧を取得する
                            reader = dao.ExecQuery(@"
SELECT shop_name
FROM hst_shop
WHERE del_flg = 0 AND item_id = @{0}
ORDER BY used_time DESC;", vm2.Id);

                            reader.ExecWholeRow((count2, record2) => {
                                string shopName = record2["shop_name"];

                                vm2.ShopNameList.Add(shopName);
                            });

                            // 備考の一覧を取得する
                            reader = dao.ExecQuery(@"
SELECT remark
FROM hst_remark
WHERE del_flg = 0 AND item_id = @{0}
ORDER BY used_time DESC;", vm2.Id);

                            reader.ExecWholeRow((count2, record2) => {
                                string remark = record2["remark"];

                                vm2.RemarkList.Add(remark);
                            });
                        }
                    }
                }
            }

            return vmList;
        }

        /// <summary>
        /// 帳簿VM(設定用)リストを取得する
        /// </summary>
        /// <returns>帳簿VM(設定用)リスト</returns>
        private ObservableCollection<BookSettingViewModel> LoadBookSettingViewModelList() {
            ObservableCollection<BookSettingViewModel> vmList = new ObservableCollection<BookSettingViewModel>();

            using(DaoBase dao = builder.Build()) {
                // 帳簿一覧を取得する
                DaoReader reader = dao.ExecQuery(@"
SELECT book_id, book_name, pay_day, initial_value
FROM mst_book
WHERE del_flg = 0
ORDER BY sort_order;");

                reader.ExecWholeRow((count, record) => {
                    int bookId = record.ToInt("book_id");
                    string bookName = record["book_name"];
                    int initialValue = record.ToInt("initial_value");
                    int? payDay = record.ToNullableInt("pay_day");

                    vmList.Add(new BookSettingViewModel() {
                        Id = bookId,
                        Name = bookName,
                        InitialValue = initialValue,
                        PayDay = payDay
                    });
                });

                // 項目との関係の一覧を取得する(移動を除く)
                foreach(BookSettingViewModel vm in vmList) {
                    reader = dao.ExecQuery(@"
SELECT I.item_id AS ItemId, I.item_name, C.category_name, RBI.item_id IS NULL AS IsNotRelated
FROM mst_item I
INNER JOIN (SELECT category_id, category_name FROM mst_category WHERE del_flg = 0) C ON C.category_id = I.category_id
LEFT JOIN (SELECT item_id FROM rel_book_item WHERE del_flg = 0 AND book_id = @{0}) RBI ON RBI.item_id = I.item_id
WHERE del_flg = 0 AND move_flg = 0
ORDER BY I.sort_order;", vm.Id);

                    vm.RelationVMList = new ObservableCollection<RelationViewModel>();
                    reader.ExecWholeRow((count, record) => {
                        int itemId = record.ToInt("ItemId");
                        string name = string.Format(@"{0} - {1}", record["category_name"], record["item_name"]);
                        bool isRelated = !record.ToBoolean("IsNotRelated");

                        vm.RelationVMList.Add(new RelationViewModel() {
                            Id = itemId,
                            Name = name,
                            IsRelated = isRelated
                        });
                    });
                }
            }

            return vmList;
        }
        #endregion

        #region 設定反映用の関数
        /// <summary>
        /// 設定を読み込む
        /// </summary>
        private void LoadSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (Properties.Settings.Default.MainWindow_Left != -1) {
                Left = settings.MainWindow_Left;
            }
            if (Properties.Settings.Default.MainWindow_Top != -1) {
                Top = settings.MainWindow_Top;
            }
            if (Properties.Settings.Default.MainWindow_Width != -1) {
                Width = settings.MainWindow_Width;
            }
            if (Properties.Settings.Default.MainWindow_Height != -1) {
                Height = settings.MainWindow_Height;
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        private void SaveSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (this.WindowState == WindowState.Normal) {
                settings.MainWindow_Left = Left;
                settings.MainWindow_Top = Top;
                settings.MainWindow_Width = Width;
                settings.MainWindow_Height = Height;
                settings.MainWindow_SelectedBookId = this.MainWindowVM.SelectedBookVM.Id.HasValue ? this.MainWindowVM.SelectedBookVM.Id.Value : -1;
                settings.Save();
            }
        }
        #endregion
    }
}
