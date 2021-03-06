﻿using HouseholdAccountBook.Dao;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.UserControls;
using HouseholdAccountBook.ViewModels;
using Microsoft.Win32;
using Notifications.Wpf;
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
        #endregion

        /// <summary>
        /// <see cref="MainWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="builder">DAOビルダ</param>
        public MainWindow(DaoBuilder builder)
        {
            this.builder = builder;

            this.InitializeComponent();
            this.LoadWindowSetting();
        }

        #region イベントハンドラ
        #region コマンド
        #region ファイル
        /// <summary>
        /// 記帳風月のDBを取り込み可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportKichoHugetsuDbCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true; // TODO: 設定で無効化できるようにする
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

            this.Cursor = Cursors.Wait;

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
                // 帳簿リスト更新
                await this.UpdateBookListAsync();

                await this.UpdateAsync(isScroll: true, isUpdateActDateLastEdited: true);

                this.Cursor = null;

                MessageBox.Show(MessageText.FinishToImport, MessageTitle.Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                this.Cursor = null;
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
            e.CanExecute = Properties.Settings.Default.App_Postgres_RestoreExePath != string.Empty;
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

            this.Cursor = Cursors.Wait;

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
                // 帳簿リスト更新
                await this.UpdateBookListAsync();

                await this.UpdateAsync(isScroll: true, isUpdateActDateLastEdited: true);

                this.Cursor = null;

                MessageBox.Show(MessageText.FinishToImport, MessageTitle.Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                this.Cursor = null;

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
            e.CanExecute = Properties.Settings.Default.App_Postgres_DumpExePath != string.Empty;
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

            this.Cursor = Cursors.Wait;

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
            this.Cursor = null;

            if (process.ExitCode == 0) {
                MessageBox.Show(MessageText.FinishToExport, MessageTitle.Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                MessageBox.Show(MessageText.FoultToExport, MessageTitle.Error, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// 手動バックアップを行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackUpCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            this.CreateBackUpFile();

            this.Cursor = null;

            MessageBox.Show(MessageText.FinishToBackUp, MessageTitle.Information);
        }

        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 編集
        /// <summary>
        /// 移動操作可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveToBookCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択していて、帳簿が2つ以上存在していて、選択されている帳簿が存在する
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab && this.WVM.BookVMList?.Count >= 2 && this.WVM.SelectedBookVM != null;
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveToBookCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveRegistrationWindow mrw = new MoveRegistrationWindow(this.builder,
                this.WVM.SelectedBookVM.Id, this.WVM.SelectedActionVM?.ActTime ?? this.WVM.DisplayedMonth) { Owner = this };
            // 登録時イベントを登録する
            mrw.Registrated += async (sender2, e2) => {
                // 帳簿一覧タブを更新する
                await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                FocusManager.SetFocusedElement(this, this.actionDataGrid);
                this.actionDataGrid.Focus();
            };
            mrw.ShowDialog();
        }

        /// <summary>
        /// 項目追加可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択していて、選択されている帳簿が存在する
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab && this.WVM.SelectedBookVM != null;
        }

        /// <summary>
        /// 項目追加処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ActionRegistrationWindow arw = new ActionRegistrationWindow(this.builder,
                this.WVM.SelectedBookVM.Id, this.WVM.SelectedActionVM?.ActTime) { Owner = this };
            // 登録時イベントを登録する
            arw.Registrated += async (sender2, e2) => {
                // 帳簿一覧タブを更新する
                await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                FocusManager.SetFocusedElement(this, this.actionDataGrid);
                this.actionDataGrid.Focus();
            };
            arw.ShowDialog();
        }

        /// <summary>
        /// 項目まとめて追加可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionListCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択していて、選択されている帳簿が存在する
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab && this.WVM.SelectedBookVM != null;
        }

        /// <summary>
        /// 項目まとめて追加処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddActionListCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ActionListRegistrationWindow alrw = new ActionListRegistrationWindow(this.builder,
                this.WVM.SelectedBookVM.Id, this.WVM.SelectedActionVM?.ActTime) { Owner = this };
            // 登録時イベントを登録する
            alrw.Registrated += async (sender2, e2) => {
                // 帳簿一覧タブを更新する
                await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                FocusManager.SetFocusedElement(this, this.actionDataGrid);
                this.actionDataGrid.Focus();
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
            // 帳簿タブを選択していて、選択されている帳簿項目が1つだけ存在していて、選択している帳簿項目のIDが0より大きい
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab &&
                           this.WVM.SelectedActionVMList.Count == 1 && this.WVM.SelectedActionVMList[0].ActionId > 0;
        }

        /// <summary>
        /// 項目編集処理
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
                    MoveRegistrationWindow mrw = new MoveRegistrationWindow(this.builder, this.WVM.SelectedBookVM.Id, this.WVM.SelectedActionVM.GroupId.Value) { Owner = this };
                    // 登録時イベントを登録する
                    mrw.Registrated += async (sender2, e2) => {
                        // 帳簿一覧タブを更新する
                        await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                        FocusManager.SetFocusedElement(this, this.actionDataGrid);
                        this.actionDataGrid.Focus();
                    };
                    mrw.ShowDialog();
                    break;
                case (int)GroupKind.ListReg:
                    // リスト登録された帳簿項目の編集時の処理
                    ActionListRegistrationWindow alrw = new ActionListRegistrationWindow(this.builder, this.WVM.SelectedActionVM.GroupId.Value) { Owner = this };
                    // 登録時イベントを登録する
                    alrw.Registrated += async (sender2, e2) => {
                        // 帳簿一覧タブを更新する
                        await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                        FocusManager.SetFocusedElement(this, this.actionDataGrid);
                        this.actionDataGrid.Focus();
                    };
                    alrw.ShowDialog();
                    break;
                case (int)GroupKind.Repeat:
                default:
                    // 移動・リスト登録以外の帳簿項目の編集時の処理
                    ActionRegistrationWindow arw = new ActionRegistrationWindow(this.builder, this.WVM.SelectedActionVM.ActionId) { Owner = this };
                    // 登録時イベントを登録する
                    arw.Registrated += async (sender2, e2) => {
                        // 帳簿一覧タブを更新する
                        await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                        FocusManager.SetFocusedElement(this, this.actionDataGrid);
                        this.actionDataGrid.Focus();
                    };
                    arw.ShowDialog();
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
            // 帳簿タブを選択していて、選択されている帳簿項目が1つだけ存在していて、選択している帳簿項目のIDが0より大きい
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab &&
                           this.WVM.SelectedActionVMList.Count == 1 && this.WVM.SelectedActionVMList[0].ActionId > 0;
        }

        /// <summary>
        /// 項目複製処理
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
                ActionRegistrationWindow arw = new ActionRegistrationWindow(this.builder, this.WVM.SelectedActionVM.ActionId, RegistrationMode.Copy) { Owner = this };
                // 登録時イベントを登録する
                arw.Registrated += async (sender2, e2) => {
                    // 帳簿一覧タブを更新する
                    await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                    FocusManager.SetFocusedElement(this, this.actionDataGrid);
                    this.actionDataGrid.Focus();
                };
                arw.ShowDialog();
            }
            else {
                // 移動の複製時の処理
                MoveRegistrationWindow mrw = new MoveRegistrationWindow(this.builder, this.WVM.SelectedBookVM.Id, this.WVM.SelectedActionVM.GroupId.Value, RegistrationMode.Copy) { Owner = this };
                // 登録時イベントを登録する
                mrw.Registrated += async (sender2, e2) => {
                    // 帳簿一覧タブを更新する
                    await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                    FocusManager.SetFocusedElement(this, this.actionDataGrid);
                    this.actionDataGrid.Focus();
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
            // 帳簿タブを選択していて、選択している帳簿項目が存在していて、選択している帳簿項目にIDが0より大きいものが存在する
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab &&
                           this.WVM.SelectedActionVMList.Where((vm) => { return vm.ActionId > 0; }).Count() != 0;
        }

        /// <summary>
        /// 項目削除処理
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
            // 帳簿タブを選択していて、選択している帳簿項目が存在していて、選択している帳簿項目にIDが0より大きいものが存在する
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab &&
                           this.WVM.SelectedActionVMList.Where((vm) => { return vm.ActionId > 0; }).Count() != 0;
        }

        /// <summary>
        /// CSV一致チェック処理
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

        /// <summary>
        /// 画面表示を更新する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            await this.UpdateAsync();

            this.Cursor = null;
        }

        #region 月間表示
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
            this.Cursor = Cursors.Wait;

            switch (this.WVM.DisplayedTermKind) {
                case TermKind.Monthly:
                    this.WVM.DisplayedMonth = this.WVM.DisplayedMonth.Value.AddMonths(-1);
                    await this.UpdateAsync(isScroll: true);
                    break;
            }

            this.Cursor = null;
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
            this.Cursor = Cursors.Wait;

            this.WVM.DisplayedMonth = DateTime.Now.GetFirstDateOfMonth();
            await this.UpdateAsync(isScroll: true);

            this.Cursor = null;
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
            this.Cursor = Cursors.Wait;

            switch (this.WVM.DisplayedTermKind) {
                case TermKind.Monthly:
                    this.WVM.DisplayedMonth = this.WVM.DisplayedMonth.Value.AddMonths(1);
                    await this.UpdateAsync(isScroll: true);
                    break;
            }

            this.Cursor = null;
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
                    stw = new TermWindow(this.WVM.DisplayedMonth.Value) { Owner = this };
                    break;
                case TermKind.Selected:
                    stw = new TermWindow(this.WVM.StartDate, this.WVM.EndDate) { Owner = this };
                    break;
            }

            if (stw.ShowDialog() == true) {
                this.Cursor = Cursors.Wait;

                this.WVM.StartDate = stw.WVM.StartDate;
                this.WVM.EndDate = stw.WVM.EndDate;

                await this.UpdateAsync(isScroll: true);

                this.Cursor = null;
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
            this.Cursor = Cursors.Wait;

            this.WVM.DisplayedYear = this.WVM.DisplayedYear.AddYears(-1);
            await this.UpdateAsync();

            this.Cursor = null;
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
            this.Cursor = Cursors.Wait;

            this.WVM.DisplayedYear = DateTime.Now.GetFirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);
            await this.UpdateAsync();

            this.Cursor = null;
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
            this.Cursor = Cursors.Wait;

            this.WVM.DisplayedYear = this.WVM.DisplayedYear.AddYears(1);
            await this.UpdateAsync();

            this.Cursor = null;
        }
        #endregion
        #endregion

        #region ツール
        /// <summary>
        /// 設定ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OpenSettingsWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow(this.builder) { Owner = this };
            if (sw.ShowDialog() == true) {
                this.Cursor = Cursors.Wait;

                await this.UpdateBookListAsync();

                await this.UpdateAsync();

                this.Cursor = null;
            }
        }

        /// <summary>
        /// CSV比較ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenCsvComparisonWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CsvComparisonWindow ccw = new CsvComparisonWindow(this.builder, this.WVM.SelectedBookVM.Id);
            // 帳簿項目の一致を確認時のイベントを登録する
            ccw.IsMatchChanged += async (sender2, e2) => {
                ActionViewModel vm = this.WVM.ActionVMList.FirstOrDefault((tmpVM) => { return tmpVM.ActionId == e2.Value; });
                if (vm != null) {
                    using (DaoBase dao = this.builder.Build()) {
                        DaoReader reader = await dao.ExecQueryAsync(@"
SELECT is_match
FROM hst_action
WHERE action_id = @{0};", vm.ActionId);

                        bool isMatch = false;
                        reader.ExecARow((record) => {
                            isMatch = record.ToInt("is_match") == 1;
                        });
                        vm.IsMatch = isMatch;
                    }
                }
            };
            // 複数の帳簿項目変更時のイベントを登録する
            ccw.ActionsStatusChanged += async (sender3, e3) => {
                await this.UpdateBookTabDataAsync(isScroll: false);
            };
            ccw.Show();
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
            vw.ShowDialog();
        }
        #endregion
        #endregion

        #region ウィンドウ
        /// <summary>
        /// ウィンドウ読込完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            // 帳簿リスト更新
            await this.UpdateBookListAsync(settings.MainWindow_SelectedBookId);

            // タブ選択
            if (settings.MainWindow_SelectedTabIndex != -1) {
                this.WVM.SelectedTabIndex = settings.MainWindow_SelectedTabIndex;
            }
            // グラフ種別選択
            if (settings.MainWindow_SelectedGraphKindIndex != -1) {
                this.WVM.SelectedGraphKindIndex = settings.MainWindow_SelectedGraphKindIndex;
            }

            await this.UpdateAsync(isScroll: true, isUpdateActDateLastEdited: true);

            // イベントハンドラ設定
            this.WVM.SelectedTabChanged += async () => {
                this.Cursor = Cursors.Wait;

                settings.MainWindow_SelectedTabIndex = this.WVM.SelectedTabIndex;
                settings.Save();

                await this.UpdateAsync(isScroll: true);

                this.Cursor = null;
            };
            this.WVM.SelectedBookChanged += async () => {
                this.Cursor = Cursors.Wait;

                settings.MainWindow_SelectedBookId = this.WVM.SelectedBookVM.Id ?? -1;
                settings.Save();

                await this.UpdateAsync(isScroll: true);

                this.Cursor = null;
            };
            this.WVM.SelectedGraphKindChanged += async () => {
                this.Cursor = Cursors.Wait;

                settings.MainWindow_SelectedGraphKindIndex = this.WVM.SelectedGraphKindIndex;
                settings.Save();

                await this.UpdateAsync();

                this.Cursor = null;
            };
        }

        /// <summary>
        /// ウィンドウクローズ時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
        }

        /// <summary>
        /// ウィンドウ状態変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_StateChanegd(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (settings.App_BackUpFlagAtMinimizing) {
                if (this.WindowState == WindowState.Minimized) {
                    this.CreateBackUpFile();
                }
            }
        }
        #endregion
        #endregion

        #region 画面更新用の関数
        private async Task UpdateAsync(bool isScroll = false, bool isUpdateActDateLastEdited = false)
        {
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
            int? tmpBookId = bookId ?? this.WVM.SelectedBookVM?.Id;

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
                    BookViewModel vm = new BookViewModel() { Id = record.ToInt("book_id"), Name = record["book_name"] };
                    bookVMList.Add(vm);

                    if (vm.Id == tmpBookId) {
                        selectedBookVM = vm;
                    }
                    return true;
                });
            }

            this.WVM.BookVMList = bookVMList;
            this.WVM.SelectedBookVM = selectedBookVM;
        }

        #region VM取得用の関数
        /// <summary>
        /// 月内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">月内の時刻</param>
        /// <returns>帳簿項目VMリスト</returns>
        private async Task<ObservableCollection<ActionViewModel>> LoadActionViewModelListWithinMonthAsync(int? bookId, DateTime includedTime)
        {
            DateTime startTime = includedTime.GetFirstDateOfMonth();
            DateTime endTime = startTime.AddMonths(1).AddMilliseconds(-1);
            return await this.LoadActionViewModelListAsync(bookId, startTime, endTime);
        }

        /// <summary>
        /// 期間内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>帳簿項目VMリスト</returns>
        private async Task<ObservableCollection<ActionViewModel>> LoadActionViewModelListAsync(int? bookId, DateTime startTime, DateTime endTime)
        {
            ObservableCollection<ActionViewModel> actionVMList = new ObservableCollection<ActionViewModel>();
            using (DaoBase dao = this.builder.Build()) {
                DaoReader reader;
                if (bookId == null) {
                    // 全帳簿
                    reader = await dao.ExecQueryAsync(@"
-- 繰越残高
SELECT -1 AS action_id, @{1} AS act_time, -1 AS category_id, -1 AS item_id, '繰越残高' AS item_name, 0 AS act_value, (
  -- 残高
  SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT COALESCE(SUM(initial_value), 0) FROM mst_book WHERE del_flg = 0)
  FROM hst_action AA
  INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
  WHERE AA.del_flg = 0 AND AA.act_time < @{1}
) AS balance, NULL AS shop_name, NULL AS group_id, NULL AS remark, 0 AS is_match
UNION
-- 各帳簿項目
SELECT A.action_id AS action_id, A.act_time AS act_time, C.category_id AS category_id, I.item_id AS item_id, I.item_name AS item_name, A.act_value AS act_value, 0 AS balance, 
       A.shop_name AS shop_name, A.group_id AS group_id, A.remark AS remark, A.is_match AS is_match
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
ORDER BY act_time, action_id;", null, startTime, endTime);
                }
                else {
                    // 各帳簿
                    reader = await dao.ExecQueryAsync(@"
-- 繰越残高
SELECT -1 AS action_id, @{1} AS act_time, -1 AS category_id, -1 AS item_id, '繰越残高' AS item_name, 0 AS act_value, (
  -- 残高
  SELECT COALESCE(SUM(AA.act_value), 0) + (SELECT initial_value FROM mst_book WHERE book_id = @{0})
  FROM hst_action AA
  INNER JOIN (SELECT * FROM mst_book WHERE del_flg = 0) BB ON BB.book_id = AA.book_id
  WHERE AA.book_id = @{0} AND AA.del_flg = 0 AND AA.act_time < @{1}
) AS balance, NULL AS shop_name, NULL AS group_id, NULL AS remark, 0 AS is_match
UNION
-- 各帳簿項目
SELECT A.action_id AS action_id, A.act_time AS act_time, C.category_id AS category_id, I.item_id AS item_id, I.item_name AS item_name, A.act_value AS act_value, 0 AS balance,
       A.shop_name AS shop_name, A.group_id AS group_id, A.remark AS remark, A.is_match AS is_match
FROM hst_action A
INNER JOIN (SELECT * FROM mst_item WHERE (move_flg = 1 OR item_id IN (
  SELECT item_id FROM rel_book_item WHERE book_id = @{0} AND del_flg = 0
)) AND del_flg = 0) I ON I.item_id = A.item_id
INNER JOIN (SELECT * FROM mst_category WHERE del_flg = 0) C ON I.category_id = C.category_id
WHERE A.book_id = @{0} AND A.del_flg = 0 AND @{1} <= A.act_time AND A.act_time <= @{2}
ORDER BY act_time, action_id;", bookId, startTime, endTime);
                }
                int balance = 0;
                reader.ExecWholeRow((count, record) => {
                    int actionId = record.ToInt("action_id");
                    DateTime actTime = record.ToDateTime("act_time");
                    int categoryId = record.ToInt("category_id");
                    int itemId = record.ToInt("item_id");
                    string itemName = record["item_name"];
                    int actValue = record.ToInt("act_value");
                    balance = count == 0 ? record.ToInt("balance") : balance + actValue;
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
                        CategoryId = categoryId,
                        ItemId = itemId,
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
            DateTime startTime = includedTime.GetFirstDateOfMonth();
            DateTime endTime = startTime.AddMonths(1).AddMilliseconds(-1);
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
                        CategoryId = categoryId,
                        CategoryName = categoryName,
                        ItemId = itemId,
                        ItemName = string.Format("  {0}", itemName),
                        Summary = summary
                    });
                    return true;
                });
            }

            // 差引損益
            int total = summaryVMList.Sum((obj) => obj.Summary);
            // 収入/支出
            List<SummaryViewModel> totalAsBalanceKindList = new List<SummaryViewModel>();
            // カテゴリ小計
            List<SummaryViewModel> totalAsCategoryList = new List<SummaryViewModel>();

            // 収支別に計算する
            foreach (var g1 in summaryVMList.GroupBy((obj) => obj.BalanceKind)) {
                // 収入/支出の小計を計算する
                totalAsBalanceKindList.Add(new SummaryViewModel() {
                    BalanceKind = g1.Key,
                    CategoryId = -1,
                    CategoryName = string.Empty,
                    ItemId = -1,
                    ItemName = BalanceKindStr[(BalanceKind)g1.Key],
                    Summary = g1.Sum((obj) => obj.Summary)
                });
                // カテゴリ別の小計を計算する
                foreach (var g2 in g1.GroupBy((obj) => obj.CategoryId)) {
                    totalAsCategoryList.Add(new SummaryViewModel() {
                        BalanceKind = g1.Key,
                        CategoryId = g2.Key,
                        CategoryName = string.Empty,
                        ItemId = -1,
                        ItemName = g2.First().CategoryName,
                        Summary = g2.Sum((obj) => obj.Summary)
                    });
                }
            }

            // 差引損益を追加する
            summaryVMList.Insert(0, new SummaryViewModel() {
                BalanceKind = -1,
                CategoryId = -1,
                CategoryName = string.Empty,
                ItemId = -1,
                ItemName = "差引損益",
                Summary = total
            });
            // 収入/支出の小計を追加する
            foreach (SummaryViewModel svm in totalAsBalanceKindList) {
                summaryVMList.Insert(summaryVMList.IndexOf(summaryVMList.First(obj => obj.BalanceKind == svm.BalanceKind)), svm);
            }
            // カテゴリ別の小計を追加する
            foreach (SummaryViewModel svm in totalAsCategoryList) {
                summaryVMList.Insert(summaryVMList.IndexOf(summaryVMList.First(obj => obj.CategoryId == svm.CategoryId)), svm);
            }

            return summaryVMList;
        }

        /// <summary>
        /// 月内日別概要VMリストを取得する(日別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">月内の時間</param>
        /// <returns>月内日別概要VMリスト</returns>
        private async Task<ObservableCollection<SeriesViewModel>> LoadDailySummaryViewModelListWithinMonthAsync(int? bookId, DateTime includedTime)
        {
            DateTime startTime = includedTime.GetFirstDateOfMonth();
            DateTime endTime = startTime.AddMonths(1).AddMilliseconds(-1);

            return await this.LoadDailySummaryViewModelListAsync(bookId, startTime, endTime);
        }

        /// <summary>
        /// 期間内日別概要VMリストを取得する(日別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>日別概要VMリスト</returns>
        private async Task<ObservableCollection<SeriesViewModel>> LoadDailySummaryViewModelListAsync(int? bookId, DateTime startTime, DateTime endTime)
        {
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
                    BalanceKind = -1,
                    CategoryId = -1,
                    CategoryName = string.Empty,
                    ItemId = -1,
                    ItemName = "残高",
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
                SeriesViewModel vm = new SeriesViewModel() {
                    BalanceKind = summaryVM.BalanceKind,
                    CategoryId = summaryVM.CategoryId,
                    CategoryName = summaryVM.CategoryName,
                    ItemId = summaryVM.ItemId,
                    ItemName = summaryVM.ItemName,
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
        /// 年度内月別概要VMリストを取得する(月別一覧/月別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">年度内の時間</param>
        /// <returns>年度内月別概要VMリスト</returns>
        private async Task<ObservableCollection<SeriesViewModel>> LoadMonthlySummaryViewModelListWithinYearAsync(int? bookId, DateTime includedTime)
        {
            DateTime startTime = includedTime.GetFirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth);
            DateTime endTime = startTime.AddYears(1).AddMilliseconds(-1);

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
                    BalanceKind = -1,
                    CategoryId = -1,
                    CategoryName = string.Empty,
                    ItemId = -1,
                    ItemName = "残高",
                    Values = new List<int>()
                }
            };
            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の月の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.AddMonths(1).AddMilliseconds(-1);
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Summary;
            vmList[0].Values.Add(balance); // 残高
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Summary;
                SeriesViewModel vm = new SeriesViewModel() {
                    BalanceKind = summaryVM.BalanceKind,
                    CategoryId = summaryVM.CategoryId,
                    CategoryName = summaryVM.CategoryName,
                    ItemId = summaryVM.ItemId,
                    ItemName = summaryVM.ItemName,
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
                tmpEndTime = tmpStartTime.AddMonths(1).AddMilliseconds(-1);

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
        /// 10年内年別概要VMリストを取得する(年別一覧/年別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>年別概要VMリスト</returns>
        private async Task<ObservableCollection<SeriesViewModel>> LoadYearlySummaryViewModelListWithinDecadeAsync(int? bookId)
        {
            DateTime startTime = DateTime.Now.GetFirstDateOfFiscalYear(Properties.Settings.Default.App_StartMonth).AddYears(-9);
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
                    BalanceKind = -1,
                    CategoryId = -1,
                    CategoryName = string.Empty,
                    ItemId = -1,
                    ItemName = "残高",
                    Values = new List<int>()
                }
            };
            int averageCount = 0; // 平均値計算に使用する年数(去年まで)

            // 最初の年の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.AddYears(1).AddMilliseconds(-1);
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Summary;
            vmList[0].Values.Add(balance); // 残高
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Summary;
                SeriesViewModel vm = new SeriesViewModel() {
                    BalanceKind = summaryVM.BalanceKind,
                    CategoryId = summaryVM.CategoryId,
                    CategoryName = summaryVM.CategoryName,
                    ItemId = summaryVM.ItemId,
                    ItemName = summaryVM.ItemName,
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
                tmpEndTime = tmpStartTime.AddYears(1).AddMilliseconds(-1);

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
        /// <param name="isScroll">スクロールするか</param>
        /// <param name="isUpdateActDateLastEdited">最後に操作した帳簿項目を更新するか</param>
        private async Task UpdateBookTabDataAsync(List<int> actionIdList = null, bool isScroll = false, bool isUpdateActDateLastEdited = false)
        {
            if (this.WVM.SelectedTab != Tabs.BooksTab) return;

            // 指定がなければ、更新前の帳簿項目の選択を維持する
            List<int> tmpActionIdList = actionIdList ?? new List<int>(this.WVM.SelectedActionVMList.Select((tmp) => tmp.ActionId));
            this.WVM.SelectedActionVMList.Clear();
            // サマリーの選択を維持する
            SummaryViewModel tmpSelectedSVM = this.WVM.SelectedSummaryVM;

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
            IEnumerable<ActionViewModel> query1 = this.WVM.ActionVMList.Where((avm) => { return tmpActionIdList.Contains(avm.ActionId); });
            foreach (var tmp in query1) { this.WVM.SelectedActionVMList.Add(tmp); }

            // 更新前のサマリーの選択を維持する
            IEnumerable<SummaryViewModel> query2 = this.WVM.SummaryVMList.Where((svm) => {
                return svm.BalanceKind == tmpSelectedSVM?.BalanceKind && svm.CategoryId == tmpSelectedSVM?.CategoryId && svm.ItemId == tmpSelectedSVM?.ItemId;
            });
            this.WVM.SelectedSummaryVM = query2.Count() == 0 ? null : query2.First();

            if (isScroll) {
                if (this.WVM.DisplayedTermKind == TermKind.Monthly &&
                    this.WVM.DisplayedMonth.Value.GetFirstDateOfMonth() < DateTime.Today &&
                    DateTime.Today < this.WVM.DisplayedMonth.Value.GetFirstDateOfMonth().AddMonths(1).AddMilliseconds(-1)) {
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

            // 全項目
            this.WVM.WholeItemDailyGraphModel.Axes.Clear();
            this.WVM.WholeItemDailyGraphModel.Series.Clear();

            // 横軸 - 日軸
            CategoryAxis cAxis1 = new CategoryAxis() { Unit = "日", Position = AxisPosition.Bottom };
            cAxis1.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する日の文字列を作成する
            for (DateTime tmp = this.WVM.StartDate; tmp <= this.WVM.EndDate; tmp = tmp.AddDays(1)) {
                cAxis1.Labels.Add(tmp.ToString("%d"));
            }
            this.WVM.WholeItemDailyGraphModel.Axes.Add(cAxis1);

            // 縦軸 - 線形軸
            LinearAxis lAxis1 = new LinearAxis() {
                Unit = "円",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0"
            };
            switch (this.WVM.SelectedGraphKind) {
                case GraphKind.IncomeAndOutgo: {
                        lAxis1.Title = "収支";
                    }
                    break;
                case GraphKind.Balance: {
                        lAxis1.Title = "残高";
                    }
                    break;
            }
            this.WVM.WholeItemDailyGraphModel.Axes.Add(lAxis1);

            this.WVM.WholeItemDailyGraphModel.InvalidatePlot(true);

            // 選択項目
            this.WVM.SelectedItemDailyGraphModel.Axes.Clear();
            this.WVM.SelectedItemDailyGraphModel.Series.Clear();

            // 横軸 - 日軸
            CategoryAxis cAxis2 = new CategoryAxis() { Unit = "日", Position = AxisPosition.Bottom };
            cAxis2.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する日の文字列を作成する
            for (DateTime tmp = this.WVM.StartDate; tmp <= this.WVM.EndDate; tmp = tmp.AddDays(1)) {
                cAxis2.Labels.Add(tmp.ToString("%d"));
            }
            this.WVM.SelectedItemDailyGraphModel.Axes.Add(cAxis2);

            // 縦軸 - 線形軸
            LinearAxis lAxis2 = new LinearAxis() {
                Unit = "円",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0"
            };
            switch (this.WVM.SelectedGraphKind) {
                case GraphKind.IncomeAndOutgo: {
                        lAxis2.Title = "収支";
                    }
                    break;
                case GraphKind.Balance: {
                        lAxis2.Title = "残高";
                    }
                    break;
            }
            this.WVM.SelectedItemDailyGraphModel.Axes.Add(lAxis2);

            this.WVM.SelectedItemDailyGraphModel.InvalidatePlot(true);
        }

        /// <summary>
        /// 日別グラフタブに表示するデータを更新する
        /// </summary>
        private async Task UpdateDailyGraphTabDataAsync()
        {
            if (this.WVM.SelectedTab != Tabs.DailyGraphTab) return;

            switch (this.WVM.SelectedGraphKind) {
                case GraphKind.IncomeAndOutgo: {
                        ObservableCollection<SeriesViewModel> vmList = null;
                        switch (this.WVM.DisplayedTermKind) {
                            case TermKind.Monthly:
                                vmList = await this.LoadDailySummaryViewModelListWithinMonthAsync(this.WVM.SelectedBookVM?.Id, this.WVM.DisplayedMonth.Value);
                                break;
                            case TermKind.Selected:
                                vmList = await this.LoadDailySummaryViewModelListAsync(this.WVM.SelectedBookVM?.Id, this.WVM.StartDate, this.WVM.EndDate);
                                break;
                        }
                        List<int> sumPlus = new List<int>(); // 日ごとの合計収入
                        List<int> sumMinus = new List<int>(); // 日ごとの合計支出

                        this.WVM.WholeItemDailyGraphModel.Series.Clear();
                        foreach (SeriesViewModel tmpVM in vmList) {
                            if (tmpVM.ItemId == -1) { continue; }

                            CustomColumnSeries cSeries1 = new CustomColumnSeries() {
                                IsStacked = true,
                                Title = tmpVM.ItemName,
                                ItemsSource = tmpVM.Values.Select((value, index) => {
                                    return new SeriesItemViewModel {
                                        Value = value,
                                        Number = index,
                                        ItemId = tmpVM.ItemId,
                                        CategoryId = tmpVM.CategoryId
                                    };
                                }),
                                ValueField = "Value",
                                TrackerFormatString = "{0}\n{1}日: {2:#,0}"
                            };
                            // 全項目月間グラフの項目を選択した時のイベントを登録する
                            cSeries1.TrackerHitResultChanged += (sender, e) => {
                                if (e.Value == null) return;

                                SeriesItemViewModel itemVM = e.Value.Item as SeriesItemViewModel;
                                SeriesViewModel vm = vmList.FirstOrDefault((tmp) => tmp.ItemId == itemVM.ItemId);

                                this.WVM.SelectedItemDailyGraphModel.Series.Clear();

                                CustomColumnSeries cSeries2 = new CustomColumnSeries() {
                                    IsStacked = true,
                                    Title = vm.ItemName,
                                    FillColor = (e.Value.Series as CustomColumnSeries).ActualFillColor,
                                    ItemsSource = vm.Values.Select((value, index) => {
                                        return new SeriesItemViewModel {
                                            Value = value,
                                            Number = index,
                                            ItemId = vm.ItemId,
                                            CategoryId = vm.CategoryId
                                        };
                                    }),
                                    ValueField = "Value",
                                    TrackerFormatString = "{1}日: {2:#,0}" //日付: 金額
                                };

                                this.WVM.SelectedItemDailyGraphModel.Series.Add(cSeries2);

                                foreach (Axis axis in this.WVM.SelectedItemDailyGraphModel.Axes) {
                                    if (axis.Position == AxisPosition.Left) {
                                        this.SetAxisRange(axis, vm.Values.Min(), vm.Values.Max(), 4, true);
                                        break;
                                    }
                                }

                                this.WVM.SelectedItemDailyGraphModel.InvalidatePlot(true);
                            };

                            this.WVM.WholeItemDailyGraphModel.Series.Add(cSeries1);

                            // 全項目の日毎の合計を計算する
                            for (int i = 0; i < tmpVM.Values.Count; ++i) {
                                if (sumPlus.Count <= i) { sumPlus.Add(0); sumMinus.Add(0); }

                                if (tmpVM.Values[i] < 0) sumMinus[i] += tmpVM.Values[i];
                                else sumPlus[i] += tmpVM.Values[i];
                            }
                        }

                        // Y軸の範囲を設定する
                        foreach (Axis axis in this.WVM.WholeItemDailyGraphModel.Axes) {
                            if (axis.Position == AxisPosition.Left) {
                                this.SetAxisRange(axis, sumMinus.Min(), sumPlus.Max(), 10, true);
                                break;
                            }
                        }
                    }
                    break;
                case GraphKind.Balance: {
                        LineSeries cSeries = new LineSeries() {
                            Title = "残高",
                            TrackerFormatString = "{2}日: {4:#,0}" //日付: 金額
                        };
                        ObservableCollection<SeriesViewModel> vmList = null;
                        switch (this.WVM.DisplayedTermKind) {
                            case TermKind.Monthly:
                                vmList = await this.LoadDailySummaryViewModelListWithinMonthAsync(this.WVM.SelectedBookVM?.Id, this.WVM.DisplayedMonth.Value);
                                break;
                            case TermKind.Selected:
                                vmList = await this.LoadDailySummaryViewModelListAsync(this.WVM.SelectedBookVM?.Id, this.WVM.StartDate, this.WVM.EndDate);
                                break;
                        }
                        cSeries.Points.AddRange(new List<int>(vmList[0].Values).Select((value, index) => new DataPoint(index, value)));

                        this.WVM.WholeItemDailyGraphModel.Series.Clear();
                        this.WVM.WholeItemDailyGraphModel.Series.Add(cSeries);

                        // Y軸の範囲を設定する
                        foreach (Axis axis in this.WVM.WholeItemDailyGraphModel.Axes) {
                            if (axis.Position == AxisPosition.Left) {
                                this.SetAxisRange(axis, cSeries.Points.Min((value) => value.Y), cSeries.Points.Max((value) => value.Y), 10, true);
                                break;
                            }
                        }
                    }
                    break;
            }

            this.WVM.WholeItemDailyGraphModel.InvalidatePlot(true);

            this.WVM.SelectedItemDailyGraphModel.Series.Clear();
            this.WVM.SelectedItemDailyGraphModel.InvalidatePlot(true);
        }
        #endregion

        #region 月別一覧タブ更新用の関数
        /// <summary>
        /// 月別一覧タブに表示するデータを更新する
        /// </summary>
        private async Task UpdateMonthlyListTabDataAsync()
        {
            if (this.WVM.SelectedTab != Tabs.MonthlyListTab) return;

            int startMonth = Properties.Settings.Default.App_StartMonth;

            // 表示する月の文字列を作成する
            ObservableCollection<string> displayedMonths = new ObservableCollection<string>();
            for (int i = startMonth; i < startMonth + 12; ++i) {
                displayedMonths.Add(string.Format("{0}月", (i - 1) % 12 + 1));
            }
            this.WVM.DisplayedMonths = displayedMonths;
            this.WVM.MonthlySummaryVMList = await this.LoadMonthlySummaryViewModelListWithinYearAsync(this.WVM.SelectedBookVM.Id, this.WVM.DisplayedYear);
        }
        #endregion

        #region 月別グラフタブ更新用の関数
        /// <summary>
        /// 月別グラフタブに表示するデータを初期化する
        /// </summary>
        private void InitializeMonthlyGraphTabData()
        {
            if (this.WVM.SelectedTab != Tabs.MonthlyGraphTab) return;

            int startMonth = Properties.Settings.Default.App_StartMonth;
            // 全項目
            this.WVM.WholeItemMonthlyGraphModel.Axes.Clear();
            this.WVM.WholeItemMonthlyGraphModel.Series.Clear();

            // 横軸 - 月軸
            CategoryAxis cAxis1 = new CategoryAxis() { Unit = "月", Position = AxisPosition.Bottom };
            cAxis1.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
                                   // 表示する月の文字列を作成する
            for (int i = startMonth; i < startMonth + 12; ++i) {
                cAxis1.Labels.Add(string.Format("{0}", (i - 1) % 12 + 1));
            }
            this.WVM.WholeItemMonthlyGraphModel.Axes.Add(cAxis1);

            // 縦軸 - 線形軸
            LinearAxis lAxis1 = new LinearAxis() {
                Unit = "円",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0"
            };
            switch (this.WVM.SelectedGraphKind) {
                case GraphKind.IncomeAndOutgo: {
                        lAxis1.Title = "収支";
                    }
                    break;
                case GraphKind.Balance: {
                        lAxis1.Title = "残高";
                    }
                    break;
            }
            this.WVM.WholeItemMonthlyGraphModel.Axes.Add(lAxis1);

            this.WVM.WholeItemMonthlyGraphModel.InvalidatePlot(true);

            // 選択項目
            this.WVM.SelectedItemMonthlyGraphModel.Axes.Clear();
            this.WVM.SelectedItemMonthlyGraphModel.Series.Clear();

            // 横軸 - 月軸
            CategoryAxis cAxis2 = new CategoryAxis() { Unit = "月", Position = AxisPosition.Bottom };
            cAxis2.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
                                   // 表示する月の文字列を作成する
            for (int i = startMonth; i < startMonth + 12; ++i) {
                cAxis2.Labels.Add(string.Format("{0}", (i - 1) % 12 + 1));
            }
            this.WVM.SelectedItemMonthlyGraphModel.Axes.Add(cAxis2);

            // 縦軸 - 線形軸
            LinearAxis lAxis2 = new LinearAxis() {
                Unit = "円",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0"
            };
            switch (this.WVM.SelectedGraphKind) {
                case GraphKind.IncomeAndOutgo: {
                        lAxis2.Title = "収支";
                    }
                    break;
                case GraphKind.Balance: {
                        lAxis2.Title = "残高";
                    }
                    break;
            }
            this.WVM.SelectedItemMonthlyGraphModel.Axes.Add(lAxis2);

            this.WVM.SelectedItemMonthlyGraphModel.InvalidatePlot(true);
        }

        /// <summary>
        /// 月別グラフタブに表示するデータを更新する
        /// </summary>
        private async Task UpdateMonthlyGraphTabDataAsync()
        {
            if (this.WVM.SelectedTab != Tabs.MonthlyGraphTab) return;

            int startMonth = Properties.Settings.Default.App_StartMonth;

            switch (this.WVM.SelectedGraphKind) {
                case GraphKind.IncomeAndOutgo: {
                        ObservableCollection<SeriesViewModel> vmList = await this.LoadMonthlySummaryViewModelListWithinYearAsync(this.WVM.SelectedBookVM.Id, this.WVM.DisplayedYear);
                        List<int> sumPlus = new List<int>(); // 月ごとの合計収入
                        List<int> sumMinus = new List<int>(); // 月ごとの合計支出

                        this.WVM.WholeItemMonthlyGraphModel.Series.Clear();
                        foreach (SeriesViewModel tmpVM in vmList) {
                            if (tmpVM.ItemId == -1) { continue; }

                            CustomColumnSeries cSeries1 = new CustomColumnSeries() {
                                IsStacked = true,
                                Title = tmpVM.ItemName,
                                ItemsSource = tmpVM.Values.Select((value, index) => new SeriesItemViewModel {
                                    Value = value,
                                    Number = index + startMonth,
                                    ItemId = tmpVM.ItemId,
                                    CategoryId = tmpVM.CategoryId
                                }),
                                ValueField = "Value",
                                TrackerFormatString = "{0}\n{1}月: {2:#,0}"
                            };
                            // 全項目年間グラフの項目を選択した時のイベントを登録する
                            cSeries1.TrackerHitResultChanged += (sender, e) => {
                                if (e.Value == null) return;

                                SeriesItemViewModel itemVM = e.Value.Item as SeriesItemViewModel;
                                SeriesViewModel vm = vmList.FirstOrDefault((tmp) => tmp.ItemId == itemVM.ItemId);

                                this.WVM.SelectedItemMonthlyGraphModel.Series.Clear();

                                CustomColumnSeries cSeries2 = new CustomColumnSeries() {
                                    IsStacked = true,
                                    Title = vm.ItemName,
                                    FillColor = (e.Value.Series as CustomColumnSeries).ActualFillColor,
                                    ItemsSource = vm.Values.Select((value, index) => new SeriesItemViewModel {
                                        Value = value,
                                        Number = index + startMonth,
                                        ItemId = vm.ItemId,
                                        CategoryId = vm.CategoryId
                                    }),
                                    ValueField = "Value",
                                    TrackerFormatString = "{1}月: {2:#,0}" //月: 金額
                                };
                                this.WVM.SelectedItemMonthlyGraphModel.Series.Add(cSeries2);

                                foreach (Axis axis in this.WVM.SelectedItemMonthlyGraphModel.Axes) {
                                    if (axis.Position == AxisPosition.Left) {
                                        this.SetAxisRange(axis, vm.Values.Min(), vm.Values.Max(), 4, true);
                                        break;
                                    }
                                }

                                this.WVM.SelectedItemMonthlyGraphModel.InvalidatePlot(true);
                            };
                            this.WVM.WholeItemMonthlyGraphModel.Series.Add(cSeries1);

                            // 全項目の月毎の合計を計算する
                            for (int i = 0; i < tmpVM.Values.Count; ++i) {
                                if (sumPlus.Count <= i) { sumPlus.Add(0); sumMinus.Add(0); }

                                if (tmpVM.Values[i] < 0) sumMinus[i] += tmpVM.Values[i];
                                else sumPlus[i] += tmpVM.Values[i];
                            }
                        }

                        // Y軸の範囲を設定する
                        foreach (Axis axis in this.WVM.WholeItemMonthlyGraphModel.Axes) {
                            if (axis.Position == AxisPosition.Left) {
                                this.SetAxisRange(axis, sumMinus.Min(), sumPlus.Max(), 10, true);
                                break;
                            }
                        }
                    }
                    break;
                case GraphKind.Balance: {
                        LineSeries cSeries = new LineSeries() {
                            Title = "残高",
                            TrackerFormatString = "{2}月: {4:#,0}" //月: 金額
                        };
                        ObservableCollection<SeriesViewModel> vmList = await this.LoadMonthlySummaryViewModelListWithinYearAsync(this.WVM.SelectedBookVM.Id, this.WVM.DisplayedYear);
                        cSeries.Points.AddRange(new List<int>(vmList[0].Values).Select((value, index) => new DataPoint(index, value)));

                        this.WVM.WholeItemMonthlyGraphModel.Series.Clear();
                        this.WVM.WholeItemMonthlyGraphModel.Series.Add(cSeries);

                        // Y軸の範囲を設定する
                        foreach (Axis axis in this.WVM.WholeItemMonthlyGraphModel.Axes) {
                            if (axis.Position == AxisPosition.Left) {
                                this.SetAxisRange(axis, cSeries.Points.Min((value) => value.Y), cSeries.Points.Max((value) => value.Y), 10, true);
                                break;
                            }
                        }
                    }
                    break;
            }
            this.WVM.WholeItemMonthlyGraphModel.InvalidatePlot(true);

            this.WVM.SelectedItemMonthlyGraphModel.Series.Clear();
            this.WVM.SelectedItemMonthlyGraphModel.InvalidatePlot(true);
        }
        #endregion

        #region 年別一覧タブ更新用の関数
        /// <summary>
        /// 年別一覧タブに表示するデータを更新する
        /// </summary>
        private async Task UpdateYearlyListTabDataAsync()
        {
            if (this.WVM.SelectedTab != Tabs.YearlyListTab) return;

            int startYear = DateTime.Now.Year - 9;

            // 表示する月の文字列を作成する
            ObservableCollection<string> displayedYears = new ObservableCollection<string>();
            for (int i = startYear; i < startYear + 10; ++i) {
                displayedYears.Add(string.Format("{0}年", i));
            }
            this.WVM.DisplayedYears = displayedYears;
            this.WVM.YearlySummaryVMList = await this.LoadYearlySummaryViewModelListWithinDecadeAsync(this.WVM.SelectedBookVM.Id);
        }
        #endregion

        #region 年別グラフタブ更新用の関数
        /// <summary>
        /// 年別グラフタブに表示するデータを初期化する
        /// </summary>
        private void InitializeYearlyGraphTabData()
        {
            if (this.WVM.SelectedTab != Tabs.YearlyGraphTab) return;

            int startYear = DateTime.Now.Year - 9;

            // 全項目
            this.WVM.WholeItemYearlyGraphModel.Axes.Clear();
            this.WVM.WholeItemYearlyGraphModel.Series.Clear();

            // 横軸 - 年軸
            CategoryAxis cAxis1 = new CategoryAxis() { Unit = "年", Position = AxisPosition.Bottom };
            cAxis1.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
                                   // 表示する年の文字列を作成する
            for (int i = startYear; i < startYear + 10; ++i) {
                cAxis1.Labels.Add(string.Format("{0}", i));
            }
            this.WVM.WholeItemYearlyGraphModel.Axes.Add(cAxis1);

            // 縦軸 - 線形軸
            LinearAxis lAxis1 = new LinearAxis() {
                Unit = "円",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0"
            };
            switch (this.WVM.SelectedGraphKind) {
                case GraphKind.IncomeAndOutgo: {
                        lAxis1.Title = "収支";
                    }
                    break;
                case GraphKind.Balance: {
                        lAxis1.Title = "残高";
                    }
                    break;
            }
            this.WVM.WholeItemYearlyGraphModel.Axes.Add(lAxis1);

            this.WVM.WholeItemYearlyGraphModel.InvalidatePlot(true);

            // 選択項目
            this.WVM.SelectedItemYearlyGraphModel.Axes.Clear();
            this.WVM.SelectedItemYearlyGraphModel.Series.Clear();

            // 横軸 - 年軸
            CategoryAxis cAxis2 = new CategoryAxis() { Unit = "年", Position = AxisPosition.Bottom };
            cAxis2.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
                                   // 表示する年の文字列を作成する
            for (int i = startYear; i < startYear + 10; ++i) {
                cAxis2.Labels.Add(string.Format("{0}", i));
            }
            this.WVM.SelectedItemYearlyGraphModel.Axes.Add(cAxis2);

            // 縦軸 - 線形軸
            LinearAxis lAxis2 = new LinearAxis() {
                Unit = "円",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                StringFormat = "#,0"
            };
            switch (this.WVM.SelectedGraphKind) {
                case GraphKind.IncomeAndOutgo: {
                        lAxis2.Title = "収支";
                    }
                    break;
                case GraphKind.Balance: {
                        lAxis2.Title = "残高";
                    }
                    break;
            }
            this.WVM.SelectedItemYearlyGraphModel.Axes.Add(lAxis2);

            this.WVM.SelectedItemYearlyGraphModel.InvalidatePlot(true);
        }

        /// <summary>
        /// 年別グラフタブに表示するデータを更新する
        /// </summary>
        private async Task UpdateYearlyGraphTabDataAsync()
        {
            if (this.WVM.SelectedTab != Tabs.YearlyGraphTab) return;

            int startYear = DateTime.Now.Year - 9;

            switch (this.WVM.SelectedGraphKind) {
                case GraphKind.IncomeAndOutgo: {
                        ObservableCollection<SeriesViewModel> vmList = await this.LoadYearlySummaryViewModelListWithinDecadeAsync(this.WVM.SelectedBookVM.Id);
                        List<int> sumPlus = new List<int>(); // 年ごとの合計収入
                        List<int> sumMinus = new List<int>(); // 年ごとの合計支出

                        this.WVM.WholeItemYearlyGraphModel.Series.Clear();
                        foreach (SeriesViewModel tmpVM in vmList) {
                            if (tmpVM.ItemId == -1) { continue; }

                            CustomColumnSeries cSeries1 = new CustomColumnSeries() {
                                IsStacked = true,
                                Title = tmpVM.ItemName,
                                ItemsSource = tmpVM.Values.Select((value, index) => new SeriesItemViewModel {
                                    Value = value,
                                    Number = index + startYear,
                                    ItemId = tmpVM.ItemId,
                                    CategoryId = tmpVM.CategoryId
                                }),
                                ValueField = "Value",
                                TrackerFormatString = "{0}\n{1}年: {2:#,0}"
                            };
                            // 全項目年間グラフの項目を選択した時のイベントを登録する
                            cSeries1.TrackerHitResultChanged += (sender, e) => {
                                if (e.Value == null) return;

                                SeriesItemViewModel itemVM = e.Value.Item as SeriesItemViewModel;
                                SeriesViewModel vm = vmList.FirstOrDefault((tmp) => tmp.ItemId == itemVM.ItemId);

                                this.WVM.SelectedItemYearlyGraphModel.Series.Clear();

                                CustomColumnSeries cSeries2 = new CustomColumnSeries() {
                                    IsStacked = true,
                                    Title = vm.ItemName,
                                    FillColor = (e.Value.Series as CustomColumnSeries).ActualFillColor,
                                    ItemsSource = vm.Values.Select((value, index) => new SeriesItemViewModel {
                                        Value = value,
                                        Number = index + startYear,
                                        ItemId = vm.ItemId,
                                        CategoryId = vm.CategoryId
                                    }),
                                    ValueField = "Value",
                                    TrackerFormatString = "{1}年: {2:#,0}" //年: 金額
                                };
                                this.WVM.SelectedItemYearlyGraphModel.Series.Add(cSeries2);

                                foreach (Axis axis in this.WVM.SelectedItemYearlyGraphModel.Axes) {
                                    if (axis.Position == AxisPosition.Left) {
                                        this.SetAxisRange(axis, vm.Values.Min(), vm.Values.Max(), 4, true);
                                        break;
                                    }
                                }

                                this.WVM.SelectedItemYearlyGraphModel.InvalidatePlot(true);
                            };
                            this.WVM.WholeItemYearlyGraphModel.Series.Add(cSeries1);

                            // 全項目の月毎の合計を計算する
                            for (int i = 0; i < tmpVM.Values.Count; ++i) {
                                if (sumPlus.Count <= i) { sumPlus.Add(0); sumMinus.Add(0); }

                                if (tmpVM.Values[i] < 0) sumMinus[i] += tmpVM.Values[i];
                                else sumPlus[i] += tmpVM.Values[i];
                            }
                        }

                        // Y軸の範囲を設定する
                        foreach (Axis axis in this.WVM.WholeItemYearlyGraphModel.Axes) {
                            if (axis.Position == AxisPosition.Left) {
                                this.SetAxisRange(axis, sumMinus.Min(), sumPlus.Max(), 10, true);
                                break;
                            }
                        }
                    }
                    break;
                case GraphKind.Balance: {
                        LineSeries cSeries = new LineSeries() {
                            Title = "残高",
                            TrackerFormatString = "{2}年: {4:#,0}" //年: 金額
                        };
                        ObservableCollection<SeriesViewModel> vmList = await this.LoadYearlySummaryViewModelListWithinDecadeAsync(this.WVM.SelectedBookVM.Id);
                        cSeries.Points.AddRange(new List<int>(vmList[0].Values).Select((value, index) => new DataPoint(index, value)));

                        this.WVM.WholeItemYearlyGraphModel.Series.Clear();
                        this.WVM.WholeItemYearlyGraphModel.Series.Add(cSeries);

                        // Y軸の範囲を設定する
                        foreach (Axis axis in this.WVM.WholeItemYearlyGraphModel.Axes) {
                            if (axis.Position == AxisPosition.Left) {
                                this.SetAxisRange(axis, cSeries.Points.Min((value) => value.Y), cSeries.Points.Max((value) => value.Y), 10, true);
                                break;
                            }
                        }
                    }
                    break;
            }
            this.WVM.WholeItemYearlyGraphModel.InvalidatePlot(true);

            this.WVM.SelectedItemYearlyGraphModel.Series.Clear();
            this.WVM.SelectedItemYearlyGraphModel.InvalidatePlot(true);
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
        private void LoadWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (0 <= settings.MainWindow_Left) {
                this.Left = settings.MainWindow_Left;
            }
            if (0 <= settings.MainWindow_Top) {
                this.Top = settings.MainWindow_Top;
            }
            if (settings.MainWindow_Width != -1) {
                this.Width = settings.MainWindow_Width;
            }
            if (settings.MainWindow_Height != -1) {
                this.Height = settings.MainWindow_Height;
            }
            if (settings.MainWindow_WindowState != -1) {
                this.WindowState = (WindowState)settings.MainWindow_WindowState;
            }
        }

        /// <summary>
        /// ウィンドウ設定を保存する
        /// </summary>
        private void SaveWindowSetting()
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (this.WindowState != WindowState.Minimized) {
                settings.MainWindow_WindowState = (int)this.WindowState;
            }
            settings.MainWindow_Left = this.Left;
            settings.MainWindow_Top = this.Top;
            settings.MainWindow_Width = this.Width;
            settings.MainWindow_Height = this.Height;

            settings.Save();
        }
        #endregion

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
                                string.Format(@"{0}/{1}.backup", tmpBackUpFolderPath, DateTime.Now.ToString("yyyyMMddHHmmss")),
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
