using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Extentions;
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

            LoadSetting();

#if !DEBUG
            actionIdColumn.Visibility = Visibility.Collapsed;
            groupIdColumn.Visibility = Visibility.Collapsed;
#endif
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
                this.MainWindowVM.SelectedBookVM.BookId, this.MainWindowVM.SelectedActionVM?.ActTime);
            mrw.Registrated += (sender2, e2) => {
                UpdateBookData(e2.Id);
                actionDataGrid.Focus();
            };
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
                this.MainWindowVM.SelectedBookVM.BookId, this.MainWindowVM.SelectedActionVM?.ActTime);
            arw.Registrated += (sender2, e2)=> {
                UpdateBookData(e2.Id);
                actionDataGrid.Focus();
            };
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
                this.MainWindowVM.SelectedBookVM.BookId, this.MainWindowVM.SelectedActionVM?.ActTime);
            alrw.Registrated += (sender2, e2) => {
                UpdateBookData(e2.Id);
                actionDataGrid.Focus();
            };
            alrw.ShowDialog();
        }

        /// <summary>
        /// 項目編集可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditActionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択していて、選択されている帳簿が存在していて、選択している帳簿項目のIDが0より大きい
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
                    groupKind = record.ToNumerableInt("group_kind");
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
                MoveRegistrationWindow mrw = new MoveRegistrationWindow(builder, this.MainWindowVM.SelectedBookVM.BookId, this.MainWindowVM.SelectedActionVM.GroupId.Value);
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
            // 帳簿タブを選択していて、選択している帳簿が存在していて、選択している帳簿項目のIDが0より大きい
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

            if (MessageBox.Show("選択した項目を削除しますか？", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
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
            e.CanExecute = this.MainWindowVM.SelectedTab != Tab.SettingTab && false;
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
            // 帳簿タブを選択している
            e.CanExecute = this.MainWindowVM.SelectedTab == Tab.BookTab;
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

            this.MainWindowVM.DisplayedMonth = DateTime.Now.FirstDateOfMonth();
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
            // 帳簿タブを選択している
            e.CanExecute = this.MainWindowVM.SelectedTab == Tab.ListTab;
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

            this.MainWindowVM.DisplayedYear = DateTime.Now.FirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);
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

            Cursor = cCursor;
        }

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
                        break;
                    case Tab.SettingTab:
                        break;
                }
                Cursor = cCursor;
            }
            oldSelectedTab = this.MainWindowVM.SelectedTab;
        }
        #endregion

        #region 画面更新用の関数
        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        private void UpdateBookList(int? bookId = null)
        {
            ObservableCollection<BookViewModel> bookVMList = new ObservableCollection<BookViewModel>() {
                new BookViewModel() { BookId = null, BookName = "一覧" }
            };
            BookViewModel selectedBookVM = bookVMList[0];
            using (DaoBase dao = builder.Build()) {
                DaoReader reader = dao.ExecQuery(@"
SELECT * 
FROM mst_book 
WHERE del_flg = 0 
ORDER BY sort_order;");
                reader.ExecWholeRow((count, record) => {
                    BookViewModel vm = new BookViewModel() { BookId = record.ToInt("book_id"), BookName = record["book_name"] };
                    bookVMList.Add(vm);

                    if(vm.BookId == bookId) {
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
                    this.MainWindowVM.SelectedBookVM?.BookId, this.MainWindowVM.DisplayedMonth.FirstDateOfMonth(),
                    this.MainWindowVM.DisplayedMonth.FirstDateOfMonth().AddMonths(1).AddMilliseconds(-1));

                this.MainWindowVM.SummaryVMList = LoadSummaryViewModelList(
                    this.MainWindowVM.SelectedBookVM?.BookId, this.MainWindowVM.DisplayedMonth.FirstDateOfMonth(),
                    this.MainWindowVM.DisplayedMonth.FirstDateOfMonth().AddMonths(1).AddMilliseconds(-1));

                IEnumerable<ActionViewModel> query = this.MainWindowVM.ActionVMList.Where((avm) => { return avm.ActionId == actionId; });
                this.MainWindowVM.SelectedActionVM = query.Count() == 0 ? null : query.First();
            }
        }

        /// <summary>
        /// 年間一覧タブに表示するデータを更新する
        /// </summary>
        private void UpdateYearsListData()
        {
            if (this.MainWindowVM.SelectedTab == Tab.ListTab) {
                int startMonth = Properties.Settings.Default.App_StartMonth;

                // 表示する月の文字列を作成する
                this.MainWindowVM.DisplayedMonths = new ObservableCollection<string>();
                for (int i = startMonth; i < startMonth + 12; ++i) {
                    this.MainWindowVM.DisplayedMonths.Add(string.Format("{0}月", (i - 1) % 12 + 1));
                }
                this.MainWindowVM.SummaryWithinYearVMList = LoadSummaryWithinYearViewModelList(this.MainWindowVM.SelectedBookVM.BookId, this.MainWindowVM.DisplayedYear);
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
                        GroupId = record.ToNumerableInt("group_id"),
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
                totalAsBalanceKind.Add(new SummaryViewModel() {
                    BalanceKind = g1.Key, CategoryId = -1, CategoryName = String.Empty,
                    ItemId = -1, ItemName = BalanceStr[(BalanceKind)g1.Key],
                    Summary = g1.Sum(obj => obj.Summary)
                });
                foreach (IGrouping<int, SummaryViewModel> g2 in g1.GroupBy(obj => obj.CategoryId)) {
                    totalAsCategory.Add(new SummaryViewModel() {
                        BalanceKind = g1.Key, CategoryId = g2.Key, CategoryName = String.Empty,
                        ItemId = -1, ItemName = g2.First().CategoryName,
                        Summary = g2.Sum(obj => obj.Summary)
                    });
                }
            }
            
            summaryVMList.Insert(0, new SummaryViewModel() {
                BalanceKind = -1, CategoryId = -1, CategoryName = String.Empty,
                ItemId = -1, ItemName = "差引損益", Summary = total });
            foreach(SummaryViewModel svm in totalAsBalanceKind) {
                summaryVMList.Insert(summaryVMList.IndexOf(summaryVMList.First(obj => obj.BalanceKind == svm.BalanceKind)), svm);
            }
            foreach(SummaryViewModel svm in totalAsCategory) {
                summaryVMList.Insert(summaryVMList.IndexOf(summaryVMList.First(obj => obj.CategoryId == svm.CategoryId)), svm);
            }

            return summaryVMList;
        }

        /// <summary>
        /// 年度内合計項目VMリストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="year">表示年</param>
        /// <returns></returns>
        private ObservableCollection<SummaryWithinYearViewModel> LoadSummaryWithinYearViewModelList(int? bookId, DateTime year)
        {
            DateTime startTime = new DateTime(year.Year, Properties.Settings.Default.App_StartMonth, 1);
            DateTime endTime = startTime.AddMonths(1).AddMilliseconds(-1);

            // 開始月までの収支を取得する
            int balance = 0;
            using(DaoBase dao = builder.Build()) {
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
                BalanceKind = -1, CategoryId = -1, CategoryName = String.Empty,
                ItemId = -1, ItemName = "残高", Values = new List<int>() });

            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の月の分を取得する
            ObservableCollection<SummaryViewModel> summaryVMList = this.LoadSummaryViewModelList(bookId, startTime, endTime);
            balance = balance + summaryVMList[0].Summary;
            vmList[0].Values.Add(balance); // 残高
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Summary;
                SummaryWithinYearViewModel vm = new SummaryWithinYearViewModel() {
                    BalanceKind = summaryVM.BalanceKind, CategoryId = summaryVM.CategoryId, CategoryName = summaryVM.CategoryName,
                    ItemId = summaryVM.ItemId, ItemName = summaryVM.ItemName, Values = new List<int>(), Summary = value };
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

            // 最初以外の月の分を取得
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

            foreach(SummaryWithinYearViewModel vm in vmList) {
                if (vm.Average != null) {
                    vm.Average /= averageCount;
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
            if (Properties.Settings.Default.MainWindow_Left != -1) {
                Left = Properties.Settings.Default.MainWindow_Left;
            }
            if (Properties.Settings.Default.MainWindow_Top != -1) {
                Top = Properties.Settings.Default.MainWindow_Top;
            }
            if (Properties.Settings.Default.MainWindow_Width != -1) {
                Width = Properties.Settings.Default.MainWindow_Width;
            }
            if (Properties.Settings.Default.MainWindow_Height != -1) {
                Height = Properties.Settings.Default.MainWindow_Height;
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        private void SaveSetting()
        {
            if (this.WindowState == WindowState.Normal) {
                Properties.Settings.Default.MainWindow_Left = Left;
                Properties.Settings.Default.MainWindow_Top = Top;
                Properties.Settings.Default.MainWindow_Width = Width;
                Properties.Settings.Default.MainWindow_Height = Height;
                Properties.Settings.Default.MainWindow_SelectedBookId = this.MainWindowVM.SelectedBookVM.BookId.HasValue ? this.MainWindowVM.SelectedBookVM.BookId.Value : -1;
                Properties.Settings.Default.Save();
            }
        }
        #endregion
    }

    #region ViewModel
    /// <summary>
    /// 帳簿VM
    /// </summary>
    public partial class BookViewModel
    {
        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int? BookId { get; set; }
        /// <summary>
        /// 帳簿名
        /// </summary>
        public String BookName { get; set; }
    }

    /// <summary>
    /// 帳簿項目VM
    /// </summary>
    public class ActionViewModel
    {
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public int ActionId { get; set; }
        /// <summary>
        /// 時刻
        /// </summary>
        public DateTime ActTime { get; set; }
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 収入
        /// </summary>
        public int? Income { get; set; }
        /// <summary>
        /// 支出
        /// </summary>
        public int? Outgo { get; set; }
        /// <summary>
        /// 残高
        /// </summary>
        public int Balance { get; set; }
        /// <summary>
        /// 店舗名
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// グループID
        /// </summary>
        public int? GroupId { get; set; }

        /// <summary>
        /// 文字色
        /// </summary>
        public SolidColorBrush ColorBrush {
            get {
                if (ActTime <= DateTime.Now) {
                    return new SolidColorBrush(Colors.Black);
                }
                return new SolidColorBrush(Colors.Gray);
            }
        }
    }

    /// <summary>
    /// 合計VM
    /// </summary>
    public class SummaryViewModel
    {
        /// <summary>
        /// 収支種別
        /// </summary>
        public int BalanceKind { get; set; }
        /// <summary>
        /// カテゴリID
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// カテゴリ名
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 合計
        /// </summary>
        public int Summary { get; set; }
        
        /// <summary>
        /// 背景色
        /// </summary>
        public SolidColorBrush ColorBrush {
            get {
                if(BalanceKind == -1) {
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0xB0, 0xB0));
                }
                if(CategoryId == -1) {
                    return new SolidColorBrush(Color.FromRgb(0xB0, 0xFF, 0xB0));
                }
                if(ItemId == -1) {
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xB0));
                }
                return new SolidColorBrush(Color.FromRgb(0xEE, 0xE3, 0xFB));
            }
        }
    }
    
    /// <summary>
    /// 年内合計VM
    /// </summary>
    public class SummaryWithinYearViewModel
    {
        /// <summary>
        /// 収支種別
        /// </summary>
        public int BalanceKind { get; set; }
        /// <summary>
        /// カテゴリID
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// カテゴリ名
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 値
        /// </summary>
        public List<int> Values { get; set; }
        /// <summary>
        /// 平均
        /// </summary>
        public int? Average { get; set; }
        /// <summary>
        /// 合計
        /// </summary>
        public int? Summary { get; set; }

        /// <summary>
        /// 背景色
        /// </summary>
        public SolidColorBrush ColorBrush
        {
            get {
                if (BalanceKind == -1) {
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0xB0, 0xB0));
                }
                if (CategoryId == -1) {
                    return new SolidColorBrush(Color.FromRgb(0xB0, 0xFF, 0xB0));
                }
                if (ItemId == -1) {
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xB0));
                }
                return new SolidColorBrush(Color.FromRgb(0xEE, 0xE3, 0xFB));
            }
        }
    }

    /// <summary>
    /// メインウィンドウVM
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 更新中か
        /// </summary>
        bool onUpdate = false;

        /// <summary>
        /// 選択されたタブインデックス
        /// </summary>
        #region SelectedTabIndex
        public int SelectedTabIndex {
            get { return _SelectedTabIndex; }
            set {
                if (_SelectedTabIndex != value) {
                    _SelectedTabIndex = value;
                    PropertyChanged?.Raise(this, _nameSelectedTabIndex);

                    SelectedTab = (Tab)value;
                }
            }
        }
        private int _SelectedTabIndex;
        internal static readonly string _nameSelectedTabIndex = PropertyName<MainWindowViewModel>.Get(x => x.SelectedTabIndex);
        #endregion
        /// <summary>
        /// 選択されたタブ種別
        /// </summary>
        #region SelectedTab
        public Tab SelectedTab
        {
            get { return _SelectedTab; }
            set {
                if (_SelectedTab != value) {
                    _SelectedTab = value;
                    PropertyChanged?.Raise(this, _nameSelectedTab);

                    SelectedTabIndex = (int)value;
                }
            }
        }
        private Tab _SelectedTab = default(Tab);
        internal static readonly string _nameSelectedTab = PropertyName<MainWindowViewModel>.Get(x => x.SelectedTab);
        #endregion

        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get { return _BookVMList; }
            set
            {
                if (_BookVMList != value) {
                    _BookVMList = value;
                    PropertyChanged?.Raise(this, _nameBookVMList);
                }
            }
        }
        private ObservableCollection<BookViewModel> _BookVMList;
        internal static readonly string _nameBookVMList = PropertyName<MainWindowViewModel>.Get(x => x.BookVMList);
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get { return _SelectedBookVM; }
            set
            {
                if (_SelectedBookVM != value) {
                    _SelectedBookVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedBookVM);
                }
            }
        }
        private BookViewModel _SelectedBookVM;
        internal static readonly string _nameSelectedBookVM = PropertyName<MainWindowViewModel>.Get(x => x.SelectedBookVM);
        #endregion

        #region 帳簿タブ
        /// <summary>
        /// 表示月
        /// </summary>
        #region DisplayedMonth
        public DateTime DisplayedMonth
        {
            get { return _DisplayedMonth; }
            set
            {
                if (_DisplayedMonth != value) {
                    _DisplayedMonth = value;
                    PropertyChanged?.Raise(this, _nameDisplayedMonth);

                    if (!onUpdate) {
                        onUpdate = true;
                        // 表示月の年度の最初の月を表示年とする
                        DisplayedYear = value.FirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);
                        onUpdate = false;
                    }
                }
            }
        }
        private DateTime _DisplayedMonth;
        internal static readonly string _nameDisplayedMonth = PropertyName<MainWindowViewModel>.Get(x => x.DisplayedMonth);
        #endregion
        
        /// <summary>
        /// 帳簿項目VMリスト
        /// </summary>
        #region ActionVMList
        public ObservableCollection<ActionViewModel> ActionVMList
        {
            get { return _ActionVMList; }
            set
            {
                if (_ActionVMList != value) {
                    _ActionVMList = value;
                    PropertyChanged?.Raise(this, _nameActionVMList);
                }
            }
        }
        private ObservableCollection<ActionViewModel> _ActionVMList;
        internal static readonly string _nameActionVMList = PropertyName<MainWindowViewModel>.Get(x => x.ActionVMList);
        #endregion
        /// <summary>
        /// 選択された帳簿項目VM
        /// </summary>
        #region SelectedActionVM
        public ActionViewModel SelectedActionVM
        {
            get { return _SelectedActionVM; }
            set {
                if (_SelectedActionVM != value) {
                    _SelectedActionVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedActionVM);
                }
            }
        }
        private ActionViewModel _SelectedActionVM = default(ActionViewModel);
        internal static readonly string _nameSelectedActionVM = PropertyName<MainWindowViewModel>.Get(x => x.SelectedActionVM);
        #endregion
        
        /// <summary>
        /// 合計項目VMリスト
        /// </summary>
        #region SummaryVMList
        public ObservableCollection<SummaryViewModel> SummaryVMList
        {
            get { return _SummaryVMList; }
            set
            {
                if (_SummaryVMList != value) {
                    _SummaryVMList = value;
                    PropertyChanged?.Raise(this, _nameSummaryVMList);
                }
            }
        }
        private ObservableCollection<SummaryViewModel> _SummaryVMList;
        internal static readonly string _nameSummaryVMList = PropertyName<MainWindowViewModel>.Get(x => x.SummaryVMList);
        #endregion
        /// <summary>
        /// 選択された合計項目VM
        /// </summary>
        #region SelectedSummaryVM
        public SummaryViewModel SelectedSummaryVM
        {
            get { return _SelectedSummaryVM; }
            set {
                if (_SelectedSummaryVM != value) {
                    _SelectedSummaryVM = value;
                    PropertyChanged?.Raise(this, _nameSelectedSummaryVM);
                }
            }
        }
        private SummaryViewModel _SelectedSummaryVM = default(SummaryViewModel);
        internal static readonly string _nameSelectedSummaryVM = PropertyName<MainWindowViewModel>.Get(x => x.SelectedSummaryVM);
        #endregion
        #endregion

        #region 年間一覧タブ
        /// <summary>
        /// 表示年
        /// </summary>
        #region DisplayedYear
        public DateTime DisplayedYear
        {
            get { return _DisplayedYear; }
            set {
                if (_DisplayedYear != value) {
                    int yearDiff = value.Year - _DisplayedYear.Year;

                    _DisplayedYear = value;
                    PropertyChanged?.Raise(this, _nameDisplayedYear);

                    if (!onUpdate) {
                        onUpdate = true;
                        // 表示年の差分を表示月に反映する
                        DisplayedMonth = DisplayedMonth.AddYears(yearDiff);
                        onUpdate = false;
                    }
                }
            }
        }
        private DateTime _DisplayedYear;
        internal static readonly string _nameDisplayedYear = PropertyName<MainWindowViewModel>.Get(x => x.DisplayedYear);
        #endregion
        
        /// <summary>
        /// 表示月リスト
        /// </summary>
        #region DisplayedMonths
        public ObservableCollection<string> DisplayedMonths
        {
            get { return _DisplayedMonths; }
            set {
                if (_DisplayedMonths != value) {
                    _DisplayedMonths = value;
                    PropertyChanged?.Raise(this, _nameDisplayedMonths);
                }
            }
        }
        private ObservableCollection<string> _DisplayedMonths = default(ObservableCollection<string>);
        internal static readonly string _nameDisplayedMonths = PropertyName<MainWindowViewModel>.Get(x => x.DisplayedMonths);
        #endregion

        /// <summary>
        /// 年内合計項目VMリスト
        /// </summary>
        #region SummaryWithinYearVMList
        public ObservableCollection<SummaryWithinYearViewModel> SummaryWithinYearVMList
        {
            get { return _SummaryWithinYearVMList; }
            set {
                if (_SummaryWithinYearVMList != value) {
                    _SummaryWithinYearVMList = value;
                    PropertyChanged?.Raise(this, _nameSummaryWithinYearVMList);
                }
            }
        }
        private ObservableCollection<SummaryWithinYearViewModel> _SummaryWithinYearVMList = default(ObservableCollection<SummaryWithinYearViewModel>);
        internal static readonly string _nameSummaryWithinYearVMList = PropertyName<MainWindowViewModel>.Get(x => x.SummaryWithinYearVMList);
        #endregion
        #endregion

        /// <summary>
        /// プロパティ変更イベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
    #endregion
}
