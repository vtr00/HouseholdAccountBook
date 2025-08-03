using HouseholdAccountBook.Dao.Compositions;
using HouseholdAccountBook.Dao.DbTable;
using HouseholdAccountBook.Dao.KHDbTable;
using HouseholdAccountBook.DbHandler;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.DbTable;
using HouseholdAccountBook.Dto.KHDbTable;
using HouseholdAccountBook.Dto.Others;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Properties;
using HouseholdAccountBook.UserControls;
using HouseholdAccountBook.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using Notification.Wpf;
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
using static HouseholdAccountBook.Extensions.FrameworkElementExtensions;
using static HouseholdAccountBook.Others.DbConstants;
using static HouseholdAccountBook.Others.FileConstants;
using static HouseholdAccountBook.Others.LogicConstants;
using static HouseholdAccountBook.Others.UiConstants;

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
        private readonly DbHandlerFactory dbHandlerFactory;

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
        /// ウィンドウの境界(最終補正値)
        /// </summary>
        private Rect lastBounds = new();

        /// <summary>
        /// ウィンドウログ
        /// </summary>
        private readonly WindowLog windowLog = null;
        #endregion

        /// <summary>
        /// 子ウィンドウを開いているか
        /// </summary>
        private bool ChildrenWindowOpened => this.mrw != null || this.arw != null || this.alrw != null || (this.ccw != null && this.ccw.IsVisible);
        /// <summary>
        /// 登録ウィンドウを開いているか
        /// </summary>
        private bool RegistrationWindowOpened => this.mrw != null || this.arw != null || this.alrw != null;

        /// <summary>
        /// <see cref="MainWindow"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="dbHandlerFactory">DAOハンドラファクトリ</param>
        public MainWindow(DbHandlerFactory dbHandlerFactory)
        {
            this.Name = "Main";
            this.dbHandlerFactory = dbHandlerFactory;

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
            Log.Info();

            Properties.Settings settings = Properties.Settings.Default;

            string directory = string.Empty; // ディレクトリ
            string fileName = string.Empty; // ファイル名
            // 過去に読み込んだときのフォルダとファイルをデフォルトにする
            if (settings.App_KichoDBFilePath != string.Empty) {
                directory = Path.GetDirectoryName(settings.App_KichoDBFilePath);
                fileName = Path.GetFileName(settings.App_KichoDBFilePath);
            }

            OpenFileDialog ofd = new() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_MdbFile + "|*.mdb"
            };

            if (ofd.ShowDialog(this) == false) return;

            if (MessageBox.Show(Properties.Resources.Message_DeleteOldDataNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            settings.App_KichoDBFilePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            settings.Save();

            bool isOpen = false;
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                OleDbHandler.ConnectInfo info = new() {
                    Provider = settings.App_Access_Provider,
                    FilePath = ofd.FileName
                };

                using (OleDbHandler dbHandlerOle = new(info)) {
                    isOpen = dbHandlerOle.IsOpen;

                    if (isOpen) {
                        CbmBookDao cbmBookDao = new(dbHandlerOle);
                        CbmCategoryDao cbmCategoryDao = new(dbHandlerOle);
                        CbmItemDao cbmItemDao = new(dbHandlerOle);
                        CbtActDao cbtActDao = new(dbHandlerOle);
                        CbtNoteDao cbtNoteDao = new(dbHandlerOle);
                        CbrBookDao cbrBookDao = new(dbHandlerOle);

                        var cbmBookDtoList = await cbmBookDao.FindAllAsync();
                        var cbmCategoryDtoList = await cbmCategoryDao.FindAllAsync();
                        var cbmItemDtoList = await cbmItemDao.FindAllAsync();
                        var cbtActDtoList = await cbtActDao.FindAllAsync();
                        var cbtNoteDtoList = await cbtNoteDao.FindAllAsync();
                        var cbrBookDtoList = await cbrBookDao.FindAllAsync();

                        using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                            await dbHandler.ExecTransactionAsync(async () => {
                                MstBookDao mstBookDao = new(dbHandler);
                                MstCategoryDao mstCategoryDao = new(dbHandler);
                                MstItemDao mstItemDao = new(dbHandler);
                                HstActionDao hstActionDao = new(dbHandler);
                                HstGroupDao hstGroupDao = new(dbHandler);
                                HstShopDao hstShopDao = new(dbHandler);
                                HstRemarkDao hstRemarkDao = new(dbHandler);
                                RelBookItemDao relBookItemDao = new(dbHandler);

                                // 既存のデータを削除する
                                _ = await mstBookDao.DeleteAllAsync();
                                _ = await mstCategoryDao.DeleteAllAsync();
                                _ = await mstItemDao.DeleteAllAsync();
                                _ = await hstActionDao.DeleteAllAsync();
                                _ = await hstGroupDao.DeleteAllAsync();
                                _ = await hstShopDao.DeleteAllAsync();
                                _ = await hstRemarkDao.DeleteAllAsync();
                                _ = await relBookItemDao.DeleteAllAsync();

                                // 帳簿IDのシーケンスを更新する
                                await mstBookDao.SetIdSequenceAsync(cbmBookDtoList.Last().BOOK_ID);
                                // 帳簿テーブルのレコードを作成する
                                foreach (CbmBookDto cbmBookDto in cbmBookDtoList) {
                                    _ = await mstBookDao.InsertAsync(new MstBookDto(cbmBookDto));
                                }

                                // 分類IDのシーケンスを更新する
                                await mstCategoryDao.SetIdSequenceAsync(cbmCategoryDtoList.Last().CATEGORY_ID);
                                // 分類テーブルのレコードを作成する
                                foreach (CbmCategoryDto dto in cbmCategoryDtoList) {
                                    _ = await mstCategoryDao.InsertAsync(new MstCategoryDto(dto));
                                }

                                // 項目IDのシーケンスを更新する
                                await mstItemDao.SetIdSequenceAsync(cbmItemDtoList.Last().ITEM_ID);
                                // 項目テーブルのレコードを作成する
                                foreach (CbmItemDto dto in cbmItemDtoList) {
                                    _ = await mstItemDao.InsertAsync(new MstItemDto(dto));
                                }

                                // 帳簿項目IDのシーケンスを更新する
                                await hstActionDao.SetIdSequenceAsync(cbtActDtoList.Last().ACT_ID);
                                int maxGroupId = 0; // グループIDの最大値
                                // 帳簿テーブルのレコードを作成する
                                foreach (CbtActDto cbtActDto in cbtActDtoList) {
                                    HstActionDto dto = new(cbtActDto);
                                    _ = await hstActionDao.InsertAsync(dto);

                                    // groupId が存在しないなら次のレコードへ
                                    if (dto.GroupId == null) { continue; }
                                    int groupId = dto.GroupId.Value;

                                    // グループIDの最大値を更新する
                                    if (maxGroupId < groupId) { maxGroupId = groupId; }

                                    var gDto = await hstGroupDao.FindByIdAsync(groupId);
                                    // groupId のレコードが登録されていないとき
                                    if (gDto is null) {
                                        // グループの種類を調べる
                                        var cbtActDtoList2 = await cbtActDao.FindByGroupIdAsync(groupId);
                                        GroupKind groupKind = GroupKind.Repeat;
                                        int? tmpBookId = null;
                                        foreach (CbtActDto cbtActDto2 in cbtActDtoList2) {
                                            if (tmpBookId == null) { // 1つ目のレコードの帳簿IDを記録する
                                                tmpBookId = cbtActDto2.BOOK_ID;
                                            }
                                            else { // 2つ目のレコードの帳簿IDが1つ目と一致するか
                                                if (tmpBookId != cbtActDto2.BOOK_ID) {
                                                    // 帳簿が一致しない場合は移動
                                                    groupKind = GroupKind.Move;
                                                }
                                                else {
                                                    // 帳簿が一致する場合は繰り返し
                                                    groupKind = GroupKind.Repeat;
                                                }
                                                break;
                                            }
                                        }

                                        // グループテーブルのレコードを作成する
                                        _ = await hstGroupDao.InsertAsync(new HstGroupDto {
                                            GroupId = groupId,
                                            GroupKind = (int)groupKind
                                        });
                                    }
                                }
                                // グループIDのシーケンスを更新する
                                await hstGroupDao.SetIdSequenceAsync(maxGroupId);

                                // 備考テーブルのレコードを作成する
                                foreach (CbtNoteDto dto in cbtNoteDtoList) {
                                    _ = await hstRemarkDao.InsertAsync(new HstRemarkDto(dto));
                                }

                                // 帳簿-項目テーブルのレコードを作成する
                                foreach (CbrBookDto dto in cbrBookDtoList) {
                                    _ = await relBookItemDao.InsertAsync(new RelBookItemDto(dto));
                                }
                            });
                        }
                    }
                }

                if (isOpen) {
                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
                }
            }

            if (isOpen) {
                _ = MessageBox.Show(Properties.Resources.Message_FinishToImport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
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
            Log.Info();

            Properties.Settings settings = Properties.Settings.Default;

            string directory = string.Empty;
            string fileName = string.Empty;
            // 過去に読み込んだときのフォルダとファイルをデフォルトにする
            if (settings.App_CustomFormatFilePath != string.Empty) {
                directory = Path.GetDirectoryName(settings.App_CustomFormatFilePath);
                fileName = Path.GetFileName(settings.App_CustomFormatFilePath);
            }

            OpenFileDialog ofd = new() {
                CheckFileExists = true,
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_CustomFormatFile + "|*.*",
                CheckPathExists = true
            };

            if (ofd.ShowDialog(this) == false) return;

            if (MessageBox.Show(Properties.Resources.Message_DeleteOldDataNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                return;
            }

            settings.App_CustomFormatFilePath = Path.Combine(ofd.InitialDirectory, ofd.FileName);
            settings.Save();

            int exitCode = -1;
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    var mstBookDao = new MstBookDao(dbHandler);
                    var mstCategoryDao = new MstCategoryDao(dbHandler);
                    var mstItemDao = new MstItemDao(dbHandler);
                    var hstActionDao = new HstActionDao(dbHandler);
                    var hstGroupDao = new HstGroupDao(dbHandler);
                    var hstShopDao = new HstShopDao(dbHandler);
                    var hstRemarkDao = new HstRemarkDao(dbHandler);
                    var relBookItemDao = new RelBookItemDao(dbHandler);

                    // 既存のデータを削除する
                    _ = await mstBookDao.DeleteAllAsync();
                    _ = await mstCategoryDao.DeleteAllAsync();
                    _ = await mstItemDao.DeleteAllAsync();
                    _ = await hstActionDao.DeleteAllAsync();
                    _ = await hstGroupDao.DeleteAllAsync();
                    _ = await hstShopDao.DeleteAllAsync();
                    _ = await hstRemarkDao.DeleteAllAsync();
                    _ = await relBookItemDao.DeleteAllAsync();
                }

                exitCode = await this.ExecuteRestore(ofd.FileName);

                if (exitCode == 0) {
                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true, isUpdateActDateLastEdited: true);
                }
            }

            if (exitCode == 0) {
                _ = MessageBox.Show(Properties.Resources.Message_FinishToImport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_FoultToImport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
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
        private async void ExportCustomFileCommand_Excuted(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            Properties.Settings settings = Properties.Settings.Default;

            string directory;
            string fileName = string.Empty;
            // 過去に読み込んだときのフォルダとファイルをデフォルトにする
            if (settings.App_CustomFormatFilePath != string.Empty) {
                directory = Path.GetDirectoryName(settings.App_CustomFormatFilePath);
                fileName = Path.GetFileName(settings.App_CustomFormatFilePath);
            }
            else {
                directory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }

            SaveFileDialog sfd = new() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_CustomFormatFile + "|*.*"
            };

            if (sfd.ShowDialog(this) == false) return;

            settings.App_CustomFormatFilePath = Path.Combine(sfd.InitialDirectory, sfd.FileName);
            settings.Save();

            int? exitCode = -1;
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                exitCode = await this.ExecuteDump(sfd.FileName, PostgresFormat.Custom);
            }

            if (exitCode == 0) {
                _ = MessageBox.Show(Properties.Resources.Message_FinishToExport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_FoultToExport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        /// <summary>
        /// SQLファイル出力可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportSQLFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Properties.Settings.Default.App_Postgres_DumpExePath != string.Empty && !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// SQLファイルに出力する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ExportSQLFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            Properties.Settings settings = Properties.Settings.Default;

            string directory;
            string fileName = string.Empty;
            // 過去に読み込んだときのフォルダとファイルをデフォルトにする
            if (settings.App_SQLFilePath != string.Empty) {
                directory = Path.GetDirectoryName(settings.App_SQLFilePath);
                fileName = Path.GetFileName(settings.App_SQLFilePath);
            }
            else {
                directory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }

            SaveFileDialog sfd = new() {
                InitialDirectory = directory,
                FileName = fileName,
                Title = Properties.Resources.Title_FileSelection,
                Filter = Properties.Resources.FileSelectFilter_SqlFile + "|*.sql"
            };

            if (sfd.ShowDialog(this) == false) return;

            settings.App_SQLFilePath = Path.Combine(sfd.InitialDirectory, sfd.FileName);
            settings.Save();

            int? exitCode = -1;
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                exitCode = await this.ExecuteDump(sfd.FileName, PostgresFormat.Plain);
            }

            if (exitCode == 0) {
                _ = MessageBox.Show(Properties.Resources.Message_FinishToExport, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_FoultToExport, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
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
        private async void BackUpCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            bool result = false;
            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                result = await this.CreateBackUpFileAsync();
            }

            if (result) {
                _ = MessageBox.Show(Properties.Resources.Message_FinishToBackup, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_FoultToBackup, Properties.Resources.Title_Conformation, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
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
            Log.Info();

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
        /// 検索欄を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowFindBoxCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab;
        }

        /// <summary>
        /// 検索欄を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowFindBoxCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            this.WVM.SelectedFindKind = FindKind.Find;
        }

        /// <summary>
        /// 置換欄を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowReplaceBoxCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 帳簿タブを選択
            e.CanExecute = this.WVM.SelectedTab == Tabs.BooksTab;
        }

        /// <summary>
        /// 置換欄を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowReplaceBoxCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            this.WVM.SelectedFindKind = FindKind.Replace;
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
            Log.Info();

            this.mrw = new MoveRegistrationWindow(this.dbHandlerFactory, this.WVM.SelectedBookVM.Id,
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
                _ = this.Activate();
                _ = this.actionDataGrid.Focus();
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
            Log.Info();

            this.arw = new ActionRegistrationWindow(this.dbHandlerFactory, this.WVM.SelectedBookVM.Id,
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
                _ = this.Activate();
                _ = this.actionDataGrid.Focus();
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
            Log.Info();

            this.alrw = new ActionListRegistrationWindow(this.dbHandlerFactory, this.WVM.SelectedBookVM.Id,
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
                _ = this.Activate();
                _ = this.actionDataGrid.Focus();
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
                           this.WVM.SelectedActionVMList.Count == 1 && 0 < this.WVM.SelectedActionVMList[0].ActionId && !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 項目編集ウィンドウを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EditActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            // グループ種別を特定する
            int? groupKind = null;
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                GroupInfoDao groupInfoDao = new(dbHandler);
                var dto = await groupInfoDao.FindByActionId(this.WVM.SelectedActionVM.ActionId);
                groupKind = dto.GroupKind;
            }

            switch (groupKind) {
                case (int)GroupKind.Move:
                    // 移動の編集時の処理
                    this.mrw = new MoveRegistrationWindow(this.dbHandlerFactory, this.WVM.SelectedBookVM.Id, this.WVM.SelectedActionVM.GroupId.Value) { Owner = this };
                    this.mrw.LoadWindowSetting();

                    // 登録時イベントを登録する
                    this.mrw.Registrated += async (sender2, e2) => {
                        // 帳簿一覧タブを更新する
                        await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                    };
                    // クローズ時イベントを登録する
                    this.mrw.Closed += (sender3, e3) => {
                        this.mrw = null;
                        _ = this.Activate();
                        _ = this.actionDataGrid.Focus();
                    };
                    this.mrw.Show();
                    break;
                case (int)GroupKind.ListReg:
                    // リスト登録された帳簿項目の編集時の処理
                    this.alrw = new ActionListRegistrationWindow(this.dbHandlerFactory, this.WVM.SelectedActionVM.GroupId.Value) { Owner = this };
                    this.alrw.LoadWindowSetting();

                    // 登録時イベントを登録する
                    this.alrw.Registrated += async (sender2, e2) => {
                        // 帳簿一覧タブを更新する
                        await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                    };
                    // クローズ時イベントを登録する
                    this.alrw.Closed += (sender3, e3) => {
                        this.alrw = null;
                        _ = this.Activate();
                        _ = this.actionDataGrid.Focus();
                    };
                    this.alrw.Show();
                    break;
                case (int)GroupKind.Repeat:
                default:
                    // 移動・リスト登録以外の帳簿項目の編集時の処理
                    this.arw = new ActionRegistrationWindow(this.dbHandlerFactory, this.WVM.SelectedActionVM.ActionId) { Owner = this };
                    this.arw.LoadWindowSetting();

                    // 登録時イベントを登録する
                    this.arw.Registrated += async (sender2, e2) => {
                        // 帳簿一覧タブを更新する
                        await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                    };

                    this.arw.Closed += (sender3, e3) => {
                        this.arw = null;
                        _ = this.Activate();
                        _ = this.actionDataGrid.Focus();
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
            Log.Info();

            // グループ種別を特定する
            int? groupKind = null;
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                GroupInfoDao groupInfoDao = new(dbHandler);
                var dto = await groupInfoDao.FindByActionId(this.WVM.SelectedActionVM.ActionId);
                groupKind = dto.GroupKind;
            }

            if (groupKind == null || groupKind == (int)GroupKind.Repeat) {
                // 移動以外の帳簿項目の複製時の処理
                this.arw = new ActionRegistrationWindow(this.dbHandlerFactory, this.WVM.SelectedActionVM.ActionId, RegistrationKind.Copy) { Owner = this };
                this.arw.LoadWindowSetting();

                // 登録時イベントを登録する
                this.arw.Registrated += async (sender2, e2) => {
                    // 帳簿一覧タブを更新する
                    await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                };
                this.arw.Closed += (sender3, e3) => {
                    this.arw = null;
                    _ = this.Activate();
                    _ = this.actionDataGrid.Focus();
                };
                this.arw.Show();
            }
            else {
                // 移動の複製時の処理
                this.mrw = new MoveRegistrationWindow(this.dbHandlerFactory, this.WVM.SelectedBookVM.Id, this.WVM.SelectedActionVM.GroupId.Value, RegistrationKind.Copy) { Owner = this };
                this.mrw.LoadWindowSetting();

                // 登録時イベントを登録する
                this.mrw.Registrated += async (sender2, e2) => {
                    // 帳簿一覧タブを更新する
                    await this.UpdateBookTabDataAsync(e2.Value, isUpdateActDateLastEdited: true);
                };
                this.mrw.Closed += (sender3, e3) => {
                    this.mrw = null;
                    _ = this.Activate();
                    _ = this.actionDataGrid.Focus();
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
                           this.WVM.SelectedActionVMList.Where((vm) => { return vm.ActionId > 0; }).Any() && !this.ChildrenWindowOpened;
        }

        /// <summary>
        /// 項目削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            if (MessageBox.Show(Properties.Resources.Message_DeleteNotification, Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK) {
                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    await dbHandler.ExecTransactionAsync(async () => {
                        HstActionDao hstActionDao = new(dbHandler);
                        HstGroupDao hstGroupDao = new(dbHandler);

                        List<int> groupIdList = [];
                        // 帳簿項目IDが0を超える項目についてループ
                        foreach (ActionViewModel vm in this.WVM.SelectedActionVMList.Where((vm) => { return 0 < vm.ActionId; })) {
                            int actionId = vm.ActionId;
                            int? groupId = vm.GroupId;

                            // 対象となる帳簿項目を削除する
                            _ = await hstActionDao.DeleteByIdAsync(actionId);

                            // グループIDがない場合は次の項目へ
                            if (!groupId.HasValue) { continue; }

                            var groupDto = await hstGroupDao.FindByIdAsync(groupId.Value);
                            groupIdList.Add(groupId.Value);
                            int groupKind = groupDto.GroupKind;

                            switch (groupKind) {
                                case (int)GroupKind.Move: {
                                    // 移動の場合、削除項目と同じグループIDを持つ帳簿項目を削除する
                                    _ = await hstActionDao.DeleteByGroupIdAsync(groupId.Value);
                                }
                                break;
                                case (int)GroupKind.Repeat: {
                                    // 繰返しの場合、削除項目の日時以降の同じグループIDを持つ帳簿項目を削除する
                                    _ = await hstActionDao.DeleteInGroupAfterDateByIdAsync(actionId, false);
                                }
                                break;
                            }

                            // 削除対象と同じグループIDを持つ帳簿項目が1つだけの場合にグループIDをクリアする(移動以外の場合に該当する)
                            var actionDtoList = await hstActionDao.FindByGroupIdAsync(groupId.Value);
                            if (actionDtoList.Count() == 1) {
                                _ = await hstActionDao.ClearGroupIdByIdAsync(actionDtoList.First().ActionId);
                            }
                        }

                        foreach (int groupId in groupIdList) {
                            // 同じグループIDを持つ帳簿項目が存在しなくなる場合にグループを削除する
                            var actionDtoList = await hstActionDao.FindByGroupIdAsync(groupId);
                            if (!actionDtoList.Any()) {
                                _ = await hstGroupDao.DeleteByIdAsync(groupId);
                            }
                        }
                    });
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
                           this.WVM.SelectedActionVMList.Where((vm) => { return vm.ActionId > 0; }).Any() && !this.RegistrationWindowOpened;
        }

        /// <summary>
        /// CSV一致チェック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeIsMatchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                // 帳簿項目IDが0を超える項目についてループ
                HstActionDao hstActionDao = new(dbHandler);
                foreach (ActionViewModel vm in this.WVM.SelectedActionVMList.Where((vm) => { return 0 < vm.ActionId; })) {
                    _ = await hstActionDao.UpdateIsMatchByIdAsync(vm.ActionId, vm.IsMatch ? 1 : 0);
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
        private void ShowBookTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedTab != Tabs.BooksTab;
        }

        /// <summary>
        /// 帳簿タブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowBookTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            this.WVM.SelectedTab = Tabs.BooksTab;
        }

        /// <summary>
        /// 日別グラフタブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowDailyGraphTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedTab != Tabs.DailyGraphTab;
        }

        /// <summary>
        /// 日別グラフタブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowDailyGraphTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            this.WVM.SelectedTab = Tabs.DailyGraphTab;
        }

        /// <summary>
        /// 月別一覧タブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowMonthlyListTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedTab != Tabs.MonthlyListTab;
        }

        /// <summary>
        /// 月別一覧タブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowMonthlyListTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            this.WVM.SelectedTab = Tabs.MonthlyListTab;
        }

        /// <summary>
        /// 月別グラフタブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowMonthlyGraphTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedTab != Tabs.MonthlyGraphTab;
        }

        /// <summary>
        /// 月別グラフタブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowMonthlyGraphTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            this.WVM.SelectedTab = Tabs.MonthlyGraphTab;
        }

        /// <summary>
        /// 年別一覧タブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowYearlyListTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedTab != Tabs.YearlyListTab;
        }

        /// <summary>
        /// 年別一覧タブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowYearlyListTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            this.WVM.SelectedTab = Tabs.YearlyListTab;
        }

        /// <summary>
        /// 年別グラフタブ表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowYearlyGraphTabCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WVM.SelectedTab != Tabs.YearlyGraphTab;
        }

        /// <summary>
        /// 年別グラフタブを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowYearlyGraphTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

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
            Log.Info();

            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                this.WVM.DisplayedMonth = this.WVM.DisplayedMonth.Value.AddMonths(-1);
                await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
            }
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
            Log.Info();

            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                this.WVM.DisplayedMonth = DateTime.Now.GetFirstDateOfMonth();
                await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
            }
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
            Log.Info();

            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                this.WVM.DisplayedMonth = this.WVM.DisplayedMonth.Value.AddMonths(1);
                await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
            }
        }

        /// <summary>
        /// 日毎期間を選択するウィンドウを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OpenSelectingDailyTermWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            TermWindow stw = null;
            switch (this.WVM.DisplayedTermKind) {
                case TermKind.Monthly:
                    stw = new TermWindow(this.dbHandlerFactory, this.WVM.DisplayedMonth.Value) { Owner = this };
                    break;
                case TermKind.Selected:
                    stw = new TermWindow(this.dbHandlerFactory, this.WVM.StartDate, this.WVM.EndDate) { Owner = this };
                    break;
            }
            stw.LoadWindowSetting();

            if (stw.ShowDialog() == true) {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    this.WVM.StartDate = stw.WVM.StartDate;
                    this.WVM.EndDate = stw.WVM.EndDate;

                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
                }
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
            // 月別一覧/月別グラフ/年別一覧/年別グラフタブを選択している
            e.CanExecute = this.WVM.SelectedTab is Tabs.MonthlyListTab or Tabs.MonthlyGraphTab or Tabs.YearlyListTab or Tabs.YearlyGraphTab;
        }

        /// <summary>
        /// (表示している年の)前年を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GoToLastYearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                this.WVM.DisplayedYear = this.WVM.DisplayedYear.AddYears(-1);
                await this.UpdateAsync(isUpdateBookList: true);
            }
        }

        /// <summary>
        /// 今年を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToThisYearCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DateTime thisYear = DateTime.Now.GetFirstDateOfFiscalYear(this.WVM.FiscalStartMonth);
            // 月別一覧/月別グラフ/年別一覧/年別グラフタブを選択している かつ 今年が表示されていない
            e.CanExecute = (this.WVM.SelectedTab is Tabs.MonthlyListTab or Tabs.MonthlyGraphTab or Tabs.YearlyListTab or Tabs.YearlyGraphTab) &&
                           !(thisYear <= this.WVM.DisplayedYear && this.WVM.DisplayedYear < thisYear.AddYears(1));
        }

        /// <summary>
        /// 今年を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GoToThisYearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                this.WVM.DisplayedYear = DateTime.Now.GetFirstDateOfFiscalYear(this.WVM.FiscalStartMonth);
                await this.UpdateAsync(isUpdateBookList: true);
            }
        }

        /// <summary>
        /// 来年を表示可能か
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToNextYearCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // 月別一覧/月別グラフ/年別一覧/年別グラフタブを選択している
            e.CanExecute = this.WVM.SelectedTab is Tabs.MonthlyListTab or Tabs.MonthlyGraphTab or Tabs.YearlyListTab or Tabs.YearlyGraphTab;
        }

        /// <summary>
        /// (表示している年の)翌年を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GoToNextYearCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                this.WVM.DisplayedYear = this.WVM.DisplayedYear.AddYears(1);
                await this.UpdateAsync(isUpdateBookList: true);
            }
        }
        #endregion

        /// <summary>
        /// 画面表示を更新する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                await this.UpdateAsync(isUpdateBookList: true);
            }
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
            Log.Info();

            SettingsWindow sw = new(this.dbHandlerFactory) { Owner = this };
            sw.LoadWindowSetting();

            if (sw.ShowDialog() == true) {
                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    this.WVM.FiscalStartMonth = Properties.Settings.Default.App_StartMonth;
                    await this.UpdateAsync(isUpdateBookList: true);
                }
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
            Log.Info();

            if (this.ccw is null) {
                this.ccw = new CsvComparisonWindow(this.dbHandlerFactory, this.WVM.SelectedBookVM.Id) { Owner = this };
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
                    using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                        // 帳簿一覧タブを更新する
                        await this.UpdateBookTabDataAsync(isScroll: false, isUpdateActDateLastEdited: true);
                    }
                };
                // 帳簿変更時のイベントを登録する
                this.ccw.BookChanged += (sender4, e4) => {
                    var selectedVM = this.WVM.BookVMList.FirstOrDefault((vm) => { return vm.Id == e4.Value; });
                    if (selectedVM != null) {
                        this.WVM.SelectedBookVM = selectedVM;
                    }
                };
                // ウィンドウ非表示時イベントを登録する
                this.ccw.Hided += (sender5, e5) => {
                    _ = this.Activate();
                };
            }

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
            Log.Info();

            VersionWindow vw = new() { Owner = this };
            vw.MoveOwnersCenter();

            _ = vw.ShowDialog();
        }
        #endregion

        #region 検索
        /// <summary>
        /// 検索欄を非表示にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideFindBoxCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            this.WVM.FindText = string.Empty;
            this.WVM.SelectedFindKind = FindKind.None;
        }

        /// <summary>
        /// 置換欄を非表示にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideReplaceBoxCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.WVM.SelectedFindKind = FindKind.Find;
        }

        /// <summary>
        /// 店名か備考を検索する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            this.WVM.FindText = this.WVM.FindInputText;
        }

        /// <summary>
        /// 店名と備考を置換する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ReplaceActionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Log.Info();

            if (this.WVM.FindInputText == string.Empty) return;
            if (this.WVM.FindInputText == this.WVM.ReplaceText) return;

            this.WVM.FindText = this.WVM.FindInputText;

            if (MessageBox.Show(string.Format(Properties.Resources.Message_ReplaceShopNameRemarkNotification, this.WVM.FindText, this.WVM.ReplaceText), Properties.Resources.Title_Conformation,
                MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) != MessageBoxResult.OK) {
                this.WVM.FindText = string.Empty; // 検索をクリアしておく
                return;
            }

            List<int> actionIdList = [];
            foreach (ActionViewModel vm in this.WVM.DisplayedActionVMList) {
                actionIdList.Add(vm.ActionId);

                string shopName = vm.ShopName.Replace(this.WVM.FindText, this.WVM.ReplaceText);
                string remark = vm.Remark.Replace(this.WVM.FindText, this.WVM.ReplaceText);

                using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                    HstActionDao hstActionDao = new(dbHandler);
                    _ = await hstActionDao.UpdateShopNameAndRemarkByIdAsync(vm.ActionId, shopName, remark);
                }
            }

            this.WVM.FindText = string.Empty; // 検索をクリアしておく
            await this.UpdateBookTabDataAsync(actionIdList);
        }
        #endregion
        #endregion

        #region ウィンドウ
        /// <summary>
        /// ラッパ関数の呼び出し回数
        /// </summary>
        private int wrapperCount = 0;
        /// <summary>
        /// ウィンドウのサイズと位置を変更する際のラッパ関数
        /// </summary>
        /// <param name="func">ウィンドウのサイズと位置を変更する関数</param>
        private bool ChangedLocationOrSizeWrapper(Func<bool> func)
        {
            if (this.wrapperCount == 0) {
                this.SizeChanged -= this.MainWindow_SizeChanged;
                this.LocationChanged -= this.MainWindow_LocationChanged;
            }
            this.wrapperCount++;

            bool ret = func.Invoke();

            this.wrapperCount--;
            if (this.wrapperCount == 0) {
                this.SizeChanged += this.MainWindow_SizeChanged;
                this.LocationChanged += this.MainWindow_LocationChanged;
            }

            return ret;
        }

        /// <summary>
        /// ウィンドウ初期化完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            this.windowLog.Log("Initialized", true);

            this.lastBounds = this.RestoreBounds;

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
            this.WVM.FiscalStartMonth = settings.App_StartMonth;

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

            Log.Info($"SelectedTabIndex:{this.WVM.SelectedTabIndex} SelectedGraphKind1Index:{this.WVM.SelectedGraphKind1Index} SelectedGraphKind2Index:{this.WVM.SelectedGraphKind2Index}");

            await this.UpdateAsync(isScroll: true, isUpdateActDateLastEdited: true);

            this.RegisterEventHandlerToWVM();
        }

        /// <summary>
        /// ウィンドウクローズ時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
                // 通知しても即座に終了するため通知しない
                _ = await this.CreateBackUpFileAsync();
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
        private async void MainWindow_StateChanged(object sender, EventArgs e)
        {
            Properties.Settings settings = Properties.Settings.Default;

            if (this.WindowState == WindowState.Minimized) {
                if (settings.App_BackUpFlagAtMinimizing) {
                    Log.Info(string.Format($"BackUpCurrentAtMinimizing: {settings.App_BackUpCurrentAtMinimizing}"));
                    DateTime nextBackup = settings.App_BackUpCurrentAtMinimizing.AddMinutes(settings.App_BackUpIntervalMinAtMinimizing);
                    Log.Info(string.Format($"NextBackup: {nextBackup}"));

                    if (nextBackup <= DateTime.Now) {
                        bool result = await this.CreateBackUpFileAsync(true);
                        if (result != false) {
                            settings.App_BackUpCurrentAtMinimizing = DateTime.Now;
                            settings.Save();
                            Log.Info(string.Format($"Update BackUpCurrentAtMinimizing: {settings.App_BackUpCurrentAtMinimizing}"));
                        }
                    }
                }
            }

            this.windowLog.Log("WindowStateChanged", true);
            _ = this.ModifyLocationOrSize();
        }

        /// <summary>
        /// ウィンドウ位置変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            this.windowLog.Log("WindowLocationChanged", true);
            _ = this.ModifyLocationOrSize();
        }

        /// <summary>
        /// ウィンドウサイズ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.windowLog.Log("WindowSizeChanged", true);
            _ = this.ModifyLocationOrSize();
        }
        #endregion

        /// <summary>
        /// 月別一覧ダブルクリック時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonthlyListDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Log.Info();

            if (this.WVM.SelectedTab != Tabs.MonthlyListTab) return;

            if (e.MouseDevice.DirectlyOver is FrameworkElement fe) {
                if (fe.Parent is DataGridCell cell) {
                    int col = cell.Column.DisplayIndex;

                    if (1 <= col && col <= 12) {
                        // 選択された月の帳簿タブを開く
                        Log.Info($"{this.WVM.DisplayedYear:yyyy-MM-dd} + month:{col - 1}");
                        this.WVM.DisplayedMonth = this.WVM.DisplayedYear.AddMonths(col - 1);
                        Log.Info($"{this.WVM.DisplayedMonth:yyyy-MM-dd}");
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
            Log.Info();

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
            Log.Info($"isUpdateBookList:{isUpdateBookList} isScroll:{isScroll} isUpdateActDateLastEdited:{isUpdateActDateLastEdited}");

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
            Log.Info($"bookId:{bookId}");

            int? tmpBookId = bookId ?? this.WVM.SelectedBookVM?.Id;
            Log.Info($"tmpBookId:{tmpBookId}");

            ObservableCollection<BookViewModel> bookVMList = [
                new BookViewModel() { Id = null, Name = Properties.Resources.ListName_AllBooks }
            ];
            BookViewModel selectedBookVM = bookVMList[0];
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                MstBookDao mstBookDao = new(dbHandler);
                var dtoList = await mstBookDao.FindAllAsync();
                foreach (var dto in dtoList) {
                    MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : JsonConvert.DeserializeObject<MstBookDto.JsonDto>(dto.JsonCode);

                    if (DateTimeExtensions.IsWithIn(this.WVM.DisplayedStart, this.WVM.DisplayedEnd, jsonObj?.StartDate, jsonObj?.EndDate)) {
                        BookViewModel vm = new() { Id = dto.BookId, Name = dto.BookName };
                        bookVMList.Add(vm);

                        if (vm.Id == tmpBookId) {
                            selectedBookVM = vm;
                            Log.Info($"select {selectedBookVM?.Id}");
                        }
                    }
                }
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
            Log.Info($"targetBookId:{targetBookId}, includedTime:{includedTime:yyyy-MM-dd}");

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
            Log.Info($"targetBookId:{targetBookId} startTime:{startTime:yyyy-MM-dd} endTime:{endTime:yyyy-MM-dd}");

            ObservableCollection<ActionViewModel> actionVMList = [];
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = targetBookId == null
                    ? await endingBalanceInfoDao.Find(startTime) // 全帳簿の繰越残高
                    : await endingBalanceInfoDao.FindByBookId(targetBookId.Value, startTime); // 各帳簿の繰越残高

                // 繰越残高を追加
                int balance = dto.EndingBalance;
                {
                    ActionViewModel avm = new() {
                        ActionId = -1,
                        ActTime = startTime,
                        BookId = -1,
                        CategoryId = -1,
                        ItemId = -1,
                        BookName = "",
                        CategoryName = "",
                        ItemName = Properties.Resources.ListName_CarryForward,
                        BalanceKind = BalanceKind.Others,
                        Income = null,
                        Expenses = null,
                        Balance = balance,
                        ShopName = null,
                        GroupId = null,
                        Remark = null,
                        IsMatch = false
                    };
                    actionVMList.Add(avm);
                }

                ActionInfoDao actionInfoDao = new(dbHandler);
                IEnumerable<ActionInfoDto> dtoList = targetBookId == null
                    ? await actionInfoDao.FindAllWithinTerm(startTime, endTime) // 全帳簿項目
                    : await actionInfoDao.FindByBookIdWithinTerm(targetBookId.Value, startTime, endTime); // 各帳簿項目

                foreach (ActionInfoDto aDto in dtoList) {
                    balance += aDto.ActValue;

                    ActionViewModel avm = new() {
                        ActionId = aDto.ActionId,
                        ActTime = aDto.ActTime,
                        BookId = aDto.BookId,
                        CategoryId = aDto.CategoryId,
                        ItemId = aDto.ItemId,
                        BookName = aDto.BookName,
                        CategoryName = aDto.CategoryName,
                        ItemName = aDto.ItemName,
                        BalanceKind = aDto.ActValue < 0 ? BalanceKind.Expenses : BalanceKind.Income,
                        Income = aDto.ActValue < 0 ? (int?)null : aDto.ActValue,
                        Expenses = aDto.ActValue < 0 ? -aDto.ActValue : (int?)null,
                        Balance = balance,
                        ShopName = aDto.ShopName,
                        GroupId = aDto.GroupId,
                        Remark = aDto.Remark,
                        IsMatch = aDto.IsMatch == 1
                    };
                    actionVMList.Add(avm);
                }
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
            Log.Info($"bookId:{bookId} includedTime:{includedTime:yyyy-MM-dd}");

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
            Log.Info($"bookId:{bookId} startTime:{startTime:yyyy-MM-dd} endTime:{endTime:yyyy-MM-dd}");

            ObservableCollection<SummaryViewModel> summaryVMList = [];

            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                SummaryInfoDao summaryInfoDao = new(dbHandler);
                IEnumerable<SummaryInfoDto> dtoList = bookId == null
                    ? await summaryInfoDao.FindAllWithinTerm(startTime, endTime)
                    : await summaryInfoDao.FindByBookIdWithinTerm(bookId.Value, startTime, endTime);

                foreach (SummaryInfoDto dto in dtoList) {
                    int balanceKind = dto.BalanceKind;
                    int categoryId = dto.CategoryId;
                    string categoryName = dto.CategoryName;
                    int itemId = dto.ItemId;
                    string itemName = dto.ItemName;
                    int summary = dto.Total;
                    summaryVMList.Add(new SummaryViewModel() {
                        BalanceKind = dto.BalanceKind,
                        BalanceName = BalanceKindStr[(BalanceKind)dto.BalanceKind],
                        CategoryId = dto.CategoryId,
                        CategoryName = dto.CategoryName,
                        ItemId = dto.ItemId,
                        ItemName = dto.ItemName,
                        Total = dto.Total
                    });
                }
            }

            // 差引損益
            int total = summaryVMList.Sum((obj) => obj.Total);
            // 収入/支出
            List<SummaryViewModel> totalAsBalanceKindList = [];
            // 分類小計
            List<SummaryViewModel> totalAsCategoryList = [];

            // 収支別に計算する
            foreach (var g1 in summaryVMList.GroupBy((obj) => obj.BalanceKind)) {
                // 収入/支出の小計を計算する
                totalAsBalanceKindList.Add(new SummaryViewModel() {
                    BalanceKind = g1.Key,
                    BalanceName = BalanceKindStr[(BalanceKind)g1.Key],
                    Total = g1.Sum((obj) => obj.Total)
                });
                // 分類別の小計を計算する
                foreach (var g2 in g1.GroupBy((obj) => obj.CategoryId)) {
                    totalAsCategoryList.Add(new SummaryViewModel() {
                        BalanceKind = g1.Key,
                        BalanceName = BalanceKindStr[(BalanceKind)g1.Key],
                        CategoryId = g2.Key,
                        CategoryName = g2.First().CategoryName,
                        Total = g2.Sum((obj) => obj.Total)
                    });
                }
            }

            // 差引損益を追加する
            summaryVMList.Insert(0, new SummaryViewModel() {
                OtherName = Properties.Resources.ListName_profitAndLoss,
                Total = total
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
            Log.Info($"bookId:{bookId} includedTime:{includedTime:yyyy-MM-dd}");

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
            Log.Info($"bookId:{bookId} startTime:{startTime:yyyy-MM-dd} endTime:{endTime:yyyy-MM-dd}");

            // 開始日までの収支を取得する
            int balance = 0;
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = bookId == null
                    ? await endingBalanceInfoDao.Find(startTime) // 全帳簿
                    : await endingBalanceInfoDao.FindByBookId(bookId.Value, startTime); // 各帳簿
                balance = dto.EndingBalance;
            }

            // 系列データ
            ObservableCollection<SeriesViewModel> vmList = [
                new SeriesViewModel() {
                    OtherName = Properties.Resources.ListName_Balance,
                    Values = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の日の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.AddDays(1).AddMilliseconds(-1);
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高

            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [],
                    Total = value,
                    Average = endTime < DateTime.Now ? value : 0 // 平均値は過去のデータのみで計算する
                };
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
                balance += summaryVMList[0].Total;
                vmList[0].Values.Add(balance); // 残高
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    int value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);

                    if (tmpEndTime < DateTime.Now) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Total += value;
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
        /// <param name="includedTime">年度内の日付</param>
        /// <returns>年度内月別系列VMリスト</returns>
        private async Task<ObservableCollection<SeriesViewModel>> LoadMonthlySeriesViewModelListWithinYearAsync(int? bookId, DateTime includedTime)
        {
            Log.Info($"bookId:{bookId} includedTime:{includedTime:yyyy-MM-dd}");

            DateTime startTime = includedTime.GetFirstDateOfFiscalYear(this.WVM.FiscalStartMonth);
            DateTime endTime = startTime.GetLastDateOfFiscalYear(this.WVM.FiscalStartMonth);

            // 開始日までの収支を取得する
            int balance = 0;
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = bookId == null
                    ? await endingBalanceInfoDao.Find(startTime) // 全帳簿
                    : await endingBalanceInfoDao.FindByBookId(bookId.Value, startTime); // 各帳簿
                balance = dto.EndingBalance;
            }

            ObservableCollection<SeriesViewModel> vmList = [
                new SeriesViewModel() {
                    OtherName = Properties.Resources.ListName_Balance,
                    Values = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の月の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.GetLastDateOfMonth();
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [],
                    Total = value,
                    Average = tmpEndTime < DateTime.Now ? value : 0
                };
                vm.Values.Add(value);
                vmList.Add(vm);
            }
            if (tmpEndTime < DateTime.Now) {
                ++averageCount;
            }

            // 最初以外の月の分を取得する
            int monthes = (endTime.Year * 12) + endTime.Month - ((startTime.Year * 12) + startTime.Month - 1);
            for (int i = 1; i < monthes; ++i) {
                tmpStartTime = tmpStartTime.AddMonths(1);
                tmpEndTime = tmpStartTime.GetLastDateOfMonth();

                summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
                balance += summaryVMList[0].Total;
                vmList[0].Values.Add(balance); // 残高
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    int value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);

                    if (tmpEndTime < DateTime.Now) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Total += value;
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
            Log.Info($"bookId:{bookId}");

            DateTime startTime = this.WVM.DisplayedStartYear;

            // 開始日までの収支を取得する
            int balance = 0;
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = bookId == null
                    ? await endingBalanceInfoDao.Find(startTime) // 全帳簿
                    : await endingBalanceInfoDao.FindByBookId(bookId.Value, startTime); // 各帳簿
                balance = dto.EndingBalance;
            }

            ObservableCollection<SeriesViewModel> vmList = [
                new SeriesViewModel() {
                    OtherName = Properties.Resources.ListName_Balance,
                    Values = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する年数(去年まで)

            // 最初の年の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.GetLastDateOfFiscalYear(this.WVM.FiscalStartMonth);
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [],
                    Total = value,
                    Average = tmpEndTime < DateTime.Now ? value : 0
                };
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
                tmpEndTime = tmpStartTime.GetLastDateOfFiscalYear(this.WVM.FiscalStartMonth);

                summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
                balance += summaryVMList[0].Total;
                vmList[0].Values.Add(balance); // 残高
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    int value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);

                    if (tmpEndTime < DateTime.Now) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Total += value;
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

            List<int> emptyIdList = [];
            Log.Info($"actionIdList:{string.Join(",", actionIdList ?? emptyIdList)} balanceKind:{balanceKind} categoryId:{categoryId} itemId:{itemId} isScroll:{isScroll} isUpdateActDateLastEdited:{isUpdateActDateLastEdited}");

            // 指定がなければ、更新前の帳簿項目の選択を維持する
            List<int> tmpActionIdList = actionIdList ?? new(this.WVM.SelectedActionVMList.Select((tmp) => tmp.ActionId));
            // 指定がなければ、更新前のサマリーの選択を維持する
            int? tmpBalanceKind = balanceKind ?? this.WVM.SelectedBalanceKind;
            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.WVM.SelectedItemId;
            Log.Info($"tmpActionIdList:{string.Join(",", tmpActionIdList ?? emptyIdList)} tmpBalanceKind:{tmpBalanceKind} tmpCategoryId:{tmpCategoryId} tmpItemId:{tmpItemId}");

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
            this.WVM.SelectedSummaryVM = this.WVM.SummaryVMList.FirstOrDefault((vm) => vm.BalanceKind == tmpBalanceKind && vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId);

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
                    using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                        HstActionDao hstActionDao = new(dbHandler);
                        var dto = await hstActionDao.FindByIdAsync(actionIdList[0]);
                        this.WVM.ActDateLastEdited = dto.ActTime;
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
            CategoryAxis horizontalAxis1 = new() {
                Unit = Properties.Resources.Unit_Day,
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
            LinearAxis verticalAxis1 = new() {
                Unit = Properties.Resources.Unit_Money,
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
            CategoryAxis horizontalAxis2 = new() {
                Unit = Properties.Resources.Unit_Day,
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
            LinearAxis verticalAxis2 = new() {
                Unit = Properties.Resources.Unit_Money,
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

            Log.Info($"categoryId:{categoryId} itemId:{itemId}");

            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.WVM.SelectedItemId ?? null;
            Log.Info($"tmpCategoryId:{tmpCategoryId} tmpItemId:{tmpItemId}");

            switch (this.WVM.SelectedGraphKind1) {
                case GraphKind1.IncomeAndExpensesGraph: {
                    List<int> sumPlus = []; // 日ごとの合計収入(Y軸範囲の計算に使用)
                    List<int> sumMinus = []; // 日ごとの合計支出(Y軸範囲の計算に使用)

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
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => vm.CategoryId != -1 && vm.ItemId == -1));
                            break;
                        case GraphKind2.ItemGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => vm.ItemId != -1));
                            break;
                    }
                    this.WVM.DailyGraphSeriesVMList = tmpVMList;

                    // グラフ表示データを設定する
                    this.WVM.DailyGraphPlotModel.Series.Clear();
                    foreach (SeriesViewModel tmpVM in this.WVM.DailyGraphSeriesVMList) {
                        CustomBarSeries wholeSeries = new() {
                            IsStacked = true,
                            Title = tmpVM.DisplayedName,
                            ItemsSource = tmpVM.Values.Select((value, index) => {
                                return new GraphDatumViewModel {
                                    Value = value,
                                    Number = index,
                                    ItemId = tmpVM.ItemId,
                                    CategoryId = tmpVM.CategoryId
                                };
                            }),
                            ValueField = "Value",
                            TrackerFormatString = "{0}\n{1}" + Properties.Resources.Unit_Day + ": {2:#,0}", //日付: 金額
                            XAxisKey = "Value",
                            YAxisKey = "Category"
                        };
                        // 全項目日別グラフの項目をマウスオーバーした時のイベントを登録する
                        wholeSeries.TrackerHitResultChanged += (sender, e) => {
                            if (e.Value == null) return;

                            GraphDatumViewModel datumVM = e.Value.Item as GraphDatumViewModel;
                            this.WVM.SelectedDailyGraphSeriesVM = this.WVM.DailyGraphSeriesVMList.FirstOrDefault((tmp) => tmp.CategoryId == datumVM.CategoryId && tmp.ItemId == datumVM.ItemId);
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
                            SetAxisRange(axis, sumMinus.Min(), sumPlus.Max(), 10, true);
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
                    LineSeries series = new() {
                        Title = Properties.Resources.GraphKind1_BalanceGraph,
                        TrackerFormatString = "{2}" + Properties.Resources.Unit_Day + ": {4:#,0}" //日付: 金額
                    };
                    series.Points.AddRange(new List<int>(this.WVM.DailyGraphSeriesVMList[0].Values).Select((value, index) => new DataPoint(index, value)));
                    this.WVM.DailyGraphPlotModel.Series.Add(series);

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.WVM.DailyGraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            SetAxisRange(axis, series.Points.Min((value) => value.Y), series.Points.Max((value) => value.Y), 10, true);
                            break;
                        }
                    }
                    break;
                }
            }
            this.WVM.DailyGraphPlotModel.InvalidatePlot(true);

            this.WVM.SelectedDailyGraphSeriesVM = this.WVM.DailyGraphSeriesVMList.FirstOrDefault((vm) => vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId);
        }

        /// <summary>
        /// 選択項目日別グラフを更新する
        /// </summary>
        private void UpdateSelectedDailyGraph()
        {
            if (this.WVM.SelectedGraphKind1 != GraphKind1.IncomeAndExpensesGraph) return;

            Log.Info();

            SeriesViewModel vm = this.WVM.SelectedDailyGraphSeriesVM;

            // グラフ表示データを設定する
            this.WVM.SelectedDailyGraphPlotModel.Series.Clear();
            if (vm != null) {
                CustomBarSeries slectedSeries = new() {
                    IsStacked = true,
                    Title = vm.DisplayedName,
                    FillColor = (this.WVM.DailyGraphPlotModel.Series.FirstOrDefault((s) => {
                        List<GraphDatumViewModel> datumVMList = new((s as CustomBarSeries).ItemsSource.Cast<GraphDatumViewModel>());
                        return vm.CategoryId == datumVMList[0].CategoryId && vm.ItemId == datumVMList[0].ItemId;
                    }) as CustomBarSeries).ActualFillColor,
                    ItemsSource = vm.Values.Select((value, index) => {
                        return new GraphDatumViewModel {
                            Value = value,
                            Number = index,
                            ItemId = vm.ItemId,
                            CategoryId = vm.CategoryId
                        };
                    }),
                    ValueField = "Value",
                    TrackerFormatString = "{1}" + Properties.Resources.Unit_Day + ": {2:#,0}", //日付: 金額
                    XAxisKey = "Value",
                    YAxisKey = "Category"
                };

                this.WVM.SelectedDailyGraphPlotModel.Series.Add(slectedSeries);

                foreach (Axis axis in this.WVM.SelectedDailyGraphPlotModel.Axes) {
                    if (axis.Position == AxisPosition.Left) {
                        SetAxisRange(axis, vm.Values.Min(), vm.Values.Max(), 4, true);
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

            Log.Info($"balanceKind:{balanceKind} categoryId:{categoryId} itemId:{itemId}");

            int? tmpBalanceKind = balanceKind ?? this.WVM.SelectedBalanceKind;
            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.WVM.SelectedItemId;
            Log.Info($"tmpBalanceKind:{tmpBalanceKind} tmpCategoryId:{tmpCategoryId} tmpItemId:{tmpItemId}");

            this.WVM.MonthlySeriesVMList = await this.LoadMonthlySeriesViewModelListWithinYearAsync(this.WVM.SelectedBookVM?.Id, this.WVM.DisplayedYear);

            this.WVM.SelectedMonthlySeriesVM = this.WVM.MonthlySeriesVMList.FirstOrDefault((vm) => vm.BalanceKind == tmpBalanceKind && vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId);
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

            int startMonth = this.WVM.FiscalStartMonth;

            #region 全項目
            this.WVM.MonthlyGraphPlotModel.Axes.Clear();
            this.WVM.MonthlyGraphPlotModel.Series.Clear();

            // 横軸 - 月軸
            CategoryAxis horizontalAxis1 = new() {
                Unit = Properties.Resources.Unit_Month,
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis1.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する月の文字列を作成する
            for (int i = startMonth; i < startMonth + 12; ++i) {
                horizontalAxis1.Labels.Add($"{((i - 1) % 12) + 1}");
            }
            this.WVM.MonthlyGraphPlotModel.Axes.Add(horizontalAxis1);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis1 = new() {
                Unit = Properties.Resources.Unit_Money,
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
            CategoryAxis horizontalAxis2 = new() {
                Unit = Properties.Resources.Unit_Month,
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis2.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する月の文字列を作成する
            for (int i = startMonth; i < startMonth + 12; ++i) {
                horizontalAxis2.Labels.Add($"{((i - 1) % 12) + 1}");
            }
            this.WVM.SelectedMonthlyGraphPlotModel.Axes.Add(horizontalAxis2);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis2 = new() {
                Unit = Properties.Resources.Unit_Money,
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

            Log.Info($"categoryId:{categoryId} itemId:{itemId}");

            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.WVM.SelectedItemId;
            Log.Info($"tmpCategoryId:{tmpCategoryId} tmpItemId:{tmpItemId}");

            Settings settings = Settings.Default;
            int startMonth = settings.App_StartMonth;

            switch (this.WVM.SelectedGraphKind1) {
                case GraphKind1.IncomeAndExpensesGraph: {
                    List<int> sumPlus = []; // 月ごとの合計収入
                    List<int> sumMinus = []; // 月ごとの合計支出

                    // グラフ表示データを取得する
                    ObservableCollection<SeriesViewModel> tmpVMList = await this.LoadMonthlySeriesViewModelListWithinYearAsync(this.WVM.SelectedBookVM?.Id, this.WVM.DisplayedYear);
                    // グラフ表示データを設定用に絞り込む
                    switch (this.WVM.SelectedGraphKind2) {
                        case GraphKind2.CategoryGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => vm.CategoryId != -1 && vm.ItemId == -1));
                            break;
                        case GraphKind2.ItemGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => vm.ItemId != -1));
                            break;
                    }
                    this.WVM.MonthlyGraphSeriesVMList = tmpVMList;

                    // グラフ表示データを設定する
                    this.WVM.MonthlyGraphPlotModel.Series.Clear();
                    foreach (SeriesViewModel tmpVM in this.WVM.MonthlyGraphSeriesVMList) {
                        CustomBarSeries wholeSeries = new() {
                            IsStacked = true,
                            Title = tmpVM.DisplayedName,
                            ItemsSource = tmpVM.Values.Select((value, index) => new GraphDatumViewModel {
                                Value = value,
                                Number = index + startMonth,
                                ItemId = tmpVM.ItemId,
                                CategoryId = tmpVM.CategoryId
                            }),
                            ValueField = "Value",
                            TrackerFormatString = "{0}\n{1}" + Properties.Resources.Unit_Month + ": {2:#,0}", //月: 金額
                            XAxisKey = "Value",
                            YAxisKey = "Category"
                        };
                        // 全項目年間グラフの項目をマウスオーバーした時のイベントを登録する
                        wholeSeries.TrackerHitResultChanged += (sender, e) => {
                            if (e.Value == null) return;

                            GraphDatumViewModel datumVM = e.Value.Item as GraphDatumViewModel;
                            this.WVM.SelectedMonthlyGraphSeriesVM = this.WVM.MonthlyGraphSeriesVMList.FirstOrDefault((tmp) => tmp.CategoryId == datumVM.CategoryId && tmp.ItemId == datumVM.ItemId);
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
                            SetAxisRange(axis, sumMinus.Min(), sumPlus.Max(), 10, true);
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
                    LineSeries series = new() {
                        Title = Properties.Resources.GraphKind1_BalanceGraph,
                        TrackerFormatString = "{2}" + Properties.Resources.Unit_Month + ": {4:#,0}" //月: 金額
                    };
                    series.Points.AddRange(new List<int>(this.WVM.MonthlyGraphSeriesVMList[0].Values).Select((value, index) => new DataPoint(index, value)));
                    this.WVM.MonthlyGraphPlotModel.Series.Add(series);

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.WVM.MonthlyGraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            SetAxisRange(axis, series.Points.Min((value) => value.Y), series.Points.Max((value) => value.Y), 10, true);
                            break;
                        }
                    }
                    break;
                }
            }
            this.WVM.MonthlyGraphPlotModel.InvalidatePlot(true);

            this.WVM.SelectedMonthlyGraphSeriesVM = this.WVM.MonthlyGraphSeriesVMList.FirstOrDefault((vm) => vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId);
        }

        /// <summary>
        /// 選択項目月別グラフを更新する
        /// </summary>
        private void UpdateSelectedMonthlyGraph()
        {
            if (this.WVM.SelectedGraphKind1 != GraphKind1.IncomeAndExpensesGraph) return;

            Log.Info();

            Settings settings = Settings.Default;
            int startMonth = settings.App_StartMonth;

            SeriesViewModel vm = this.WVM.SelectedMonthlyGraphSeriesVM;

            // グラフ表示データを設定する
            this.WVM.SelectedMonthlyGraphPlotModel.Series.Clear();
            if (vm != null) {
                CustomBarSeries selectedSeries = new() {
                    IsStacked = true,
                    Title = vm.DisplayedName,
                    FillColor = (this.WVM.MonthlyGraphPlotModel.Series.FirstOrDefault((series) => {
                        List<GraphDatumViewModel> datumVMList = new((series as CustomBarSeries).ItemsSource.Cast<GraphDatumViewModel>());
                        return vm.CategoryId == datumVMList[0].CategoryId && vm.ItemId == datumVMList[0].ItemId;
                    }) as CustomBarSeries).ActualFillColor,
                    ItemsSource = vm.Values.Select((value, index) => new GraphDatumViewModel {
                        Value = value,
                        Number = index + startMonth,
                        ItemId = vm.ItemId,
                        CategoryId = vm.CategoryId
                    }),
                    ValueField = "Value",
                    TrackerFormatString = "{1}" + Properties.Resources.Unit_Month + ": {2:#,0}", //月: 金額
                    XAxisKey = "Value",
                    YAxisKey = "Category"
                };
                this.WVM.SelectedMonthlyGraphPlotModel.Series.Add(selectedSeries);

                // Y軸の範囲を設定する
                foreach (Axis axis in this.WVM.SelectedMonthlyGraphPlotModel.Axes) {
                    if (axis.Position == AxisPosition.Left) {
                        SetAxisRange(axis, vm.Values.Min(), vm.Values.Max(), 4, true);
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

            Log.Info($"balanceKind:{balanceKind} categoryId:{categoryId} itemId:{itemId}");

            int? tmpBalanceKind = balanceKind ?? this.WVM.SelectedBalanceKind;
            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.WVM.SelectedItemId;
            Log.Info($"tmpBalanceKind:{tmpBalanceKind} tmpCategoryId:{tmpCategoryId} tmpItemId:{tmpItemId}");

            this.WVM.YearlySeriesVMList = await this.LoadYearlySeriesViewModelListWithinDecadeAsync(this.WVM.SelectedBookVM?.Id);
            this.WVM.SelectedYearlySeriesVM = this.WVM.YearlySeriesVMList.FirstOrDefault((vm) => vm.BalanceKind == tmpBalanceKind && vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId);
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

            int startYear = this.WVM.DisplayedStartYear.Year;

            #region 全項目
            this.WVM.YearlyGraphPlotModel.Axes.Clear();
            this.WVM.YearlyGraphPlotModel.Series.Clear();

            // 横軸 - 年軸
            CategoryAxis horizontalAxis1 = new() {
                Unit = this.WVM.FiscalStartMonth == 1 ? Properties.Resources.Unit_Year : Properties.Resources.Unit_FiscalYear,
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis1.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する年の文字列を作成する
            for (int i = startYear; i < startYear + 10; ++i) {
                horizontalAxis1.Labels.Add(this.WVM.FiscalStartMonth == 1 ? $"{i}" : $"'{i}");
            }
            this.WVM.YearlyGraphPlotModel.Axes.Add(horizontalAxis1);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis1 = new() {
                Unit = Properties.Resources.Unit_Money,
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
            CategoryAxis horizontalAxis2 = new() {
                Unit = this.WVM.FiscalStartMonth == 1 ? Properties.Resources.Unit_Year : Properties.Resources.Unit_FiscalYear,
                Position = AxisPosition.Bottom,
                Key = "Category"
            };
            horizontalAxis2.Labels.Clear(); // 内部的にLabelsの値が共有されているのか、正常な表示にはこのコードが必要
            // 表示する年の文字列を作成する
            for (int i = startYear; i < startYear + 10; ++i) {
                horizontalAxis2.Labels.Add(this.WVM.FiscalStartMonth == 1 ? $"{i}" : $"'{i}");
            }
            this.WVM.SelectedYearlyGraphPlotModel.Axes.Add(horizontalAxis2);

            // 縦軸 - 線形軸
            LinearAxis verticalAxis2 = new() {
                Unit = Properties.Resources.Unit_Money,
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

            Log.Info($"categoryId:{categoryId} itemId:{itemId}");

            int? tmpCategoryId = categoryId ?? this.WVM.SelectedCategoryId;
            int? tmpItemId = itemId ?? this.WVM.SelectedItemId;
            Log.Info($"tmpCategoryId:{tmpCategoryId} tmpItemId:{tmpItemId}");

            int startYear = this.WVM.DisplayedStartYear.Year;

            switch (this.WVM.SelectedGraphKind1) {
                case GraphKind1.IncomeAndExpensesGraph: {
                    List<int> sumPlus = []; // 年ごとの合計収入
                    List<int> sumMinus = []; // 年ごとの合計支出

                    // グラフ表示データを取得する
                    ObservableCollection<SeriesViewModel> tmpVMList = await this.LoadYearlySeriesViewModelListWithinDecadeAsync(this.WVM.SelectedBookVM?.Id);
                    // グラフ表示データを設定用に絞り込む
                    switch (this.WVM.SelectedGraphKind2) {
                        case GraphKind2.CategoryGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => vm.CategoryId != -1 && vm.ItemId == -1));
                            break;
                        case GraphKind2.ItemGraph:
                            tmpVMList = new ObservableCollection<SeriesViewModel>(tmpVMList.Where((vm) => vm.ItemId != -1));
                            break;
                    }
                    this.WVM.YearlyGraphSeriesVMList = tmpVMList;

                    // グラフ表示データを設定する
                    this.WVM.YearlyGraphPlotModel.Series.Clear();
                    foreach (SeriesViewModel tmpVM in this.WVM.YearlyGraphSeriesVMList) {
                        CustomBarSeries wholeSeries = new() {
                            IsStacked = true,
                            Title = tmpVM.DisplayedName,
                            ItemsSource = tmpVM.Values.Select((value, index) => new GraphDatumViewModel {
                                Value = value,
                                Number = index + startYear,
                                ItemId = tmpVM.ItemId,
                                CategoryId = tmpVM.CategoryId
                            }),
                            ValueField = "Value",
                            TrackerFormatString = "{0}\n{1}" + Properties.Resources.Unit_FiscalYear + ": {2:#,0}", //年度: 金額
                            XAxisKey = "Value",
                            YAxisKey = "Category"
                        };
                        // 全項目年間グラフの項目を選択した時のイベントを登録する
                        wholeSeries.TrackerHitResultChanged += (sender, e) => {
                            if (e.Value == null) return;

                            GraphDatumViewModel datumVM = e.Value.Item as GraphDatumViewModel;
                            this.WVM.SelectedYearlyGraphSeriesVM = this.WVM.YearlyGraphSeriesVMList.FirstOrDefault((tmp) => tmp.CategoryId == datumVM.CategoryId && tmp.ItemId == datumVM.ItemId);
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
                            SetAxisRange(axis, sumMinus.Min(), sumPlus.Max(), 10, true);
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
                    LineSeries series = new() {
                        Title = Properties.Resources.GraphKind1_BalanceGraph,
                        TrackerFormatString = "{2}" + Properties.Resources.Unit_FiscalYear + ": {4:#,0}" //年度: 金額
                    };
                    series.Points.AddRange(new List<int>(this.WVM.YearlyGraphSeriesVMList[0].Values).Select((value, index) => new DataPoint(index, value)));
                    this.WVM.YearlyGraphPlotModel.Series.Add(series);

                    // Y軸の範囲を設定する
                    foreach (Axis axis in this.WVM.YearlyGraphPlotModel.Axes) {
                        if (axis.Position == AxisPosition.Left) {
                            SetAxisRange(axis, series.Points.Min((value) => value.Y), series.Points.Max((value) => value.Y), 10, true);
                            break;
                        }
                    }
                    break;
                }
            }
            this.WVM.YearlyGraphPlotModel.InvalidatePlot(true);

            this.WVM.SelectedYearlyGraphSeriesVM = this.WVM.YearlyGraphSeriesVMList.FirstOrDefault((vm) => vm.CategoryId == tmpCategoryId && vm.ItemId == tmpItemId);
        }

        /// <summary>
        /// 選択項目年別グラフを更新する
        /// </summary>
        private void UpdateSelectedYearlyGraph()
        {
            if (this.WVM.SelectedGraphKind1 != GraphKind1.IncomeAndExpensesGraph) return;

            Log.Info();

            Settings settings = Settings.Default;
            int startYear = this.WVM.DisplayedStartYear.Year;

            SeriesViewModel vm = this.WVM.SelectedYearlyGraphSeriesVM;

            // グラフ表示データを設定する
            this.WVM.SelectedYearlyGraphPlotModel.Series.Clear();
            if (vm != null) {
                CustomBarSeries selectedSeries = new() {
                    IsStacked = true,
                    Title = vm.DisplayedName,
                    FillColor = (this.WVM.YearlyGraphPlotModel.Series.FirstOrDefault((series) => {
                        List<GraphDatumViewModel> datumVMList = new((series as CustomBarSeries).ItemsSource.Cast<GraphDatumViewModel>());
                        return vm.CategoryId == datumVMList[0].CategoryId && vm.ItemId == datumVMList[0].ItemId;
                    }) as CustomBarSeries).ActualFillColor,
                    ItemsSource = vm.Values.Select((value, index) => new GraphDatumViewModel {
                        Value = value,
                        Number = index + startYear,
                        ItemId = vm.ItemId,
                        CategoryId = vm.CategoryId
                    }),
                    ValueField = "Value",
                    TrackerFormatString = "{1}" + Properties.Resources.Unit_FiscalYear + ": {2:#,0}", //年度: 金額
                    XAxisKey = "Value",
                    YAxisKey = "Category"
                };
                this.WVM.SelectedYearlyGraphPlotModel.Series.Add(selectedSeries);

                // Y軸の範囲を設定する
                foreach (Axis axis in this.WVM.SelectedYearlyGraphPlotModel.Axes) {
                    if (axis.Position == AxisPosition.Left) {
                        SetAxisRange(axis, vm.Values.Min(), vm.Values.Max(), 4, true);
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
        private static void SetAxisRange(Axis axis, double minValue, double maxValue, int divNum, bool isDisplayZero)
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
            _ = this.ChangedLocationOrSizeWrapper(() => {
                Properties.Settings settings = Properties.Settings.Default;
                settings.App_InitSizeFlag = false;
                settings.Save();

                if (0 <= settings.MainWindow_Left && 0 <= settings.MainWindow_Top) {
                    this.Left = settings.MainWindow_Left;
                    this.Top = settings.MainWindow_Top;
                }
                if (40 < settings.MainWindow_Width && 40 < settings.MainWindow_Height) {
                    this.Width = settings.MainWindow_Width;
                    this.Height = settings.MainWindow_Height;
                }
                if (settings.MainWindow_WindowState != -1) {
                    this.WindowState = (WindowState)settings.MainWindow_WindowState;
                }

                this.windowLog.Log("SettingLoaded", true);
                _ = this.ModifyLocationOrSize();

                return true;
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
                    if (0 <= this.Left && 0 <= this.Top) {
                        settings.MainWindow_Left = this.Left;
                        settings.MainWindow_Top = this.Top;
                    }
                    if (40 < this.Width && 40 < this.Height) {
                        settings.MainWindow_Width = this.Width;
                        settings.MainWindow_Height = this.Height;
                    }
                }

                settings.Save();
                this.windowLog.Log("SettingSaved", true);
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
                Log.Info($"SelectedTabChanged SelectedTabIndex:{this.WVM.SelectedTabIndex}");

                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    settings.MainWindow_SelectedTabIndex = this.WVM.SelectedTabIndex;
                    settings.Save();

                    await this.UpdateAsync(isUpdateBookList: true, isScroll: true);
                }
            };
            // 帳簿選択変更時
            this.WVM.SelectedBookChanged += async () => {
                Log.Info($"SelectedBookChanged SelectedBookId:{this.WVM.SelectedBookVM?.Id}");

                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    settings.MainWindow_SelectedBookId = this.WVM.SelectedBookVM?.Id ?? -1;
                    settings.Save();

                    await this.UpdateAsync(isScroll: true);
                }
            };
            // グラフ種別選択変更時
            this.WVM.SelectedGraphKindChanged += async () => {
                Log.Info($"SelectedGraphKindChanged SelectedGraphKind1Index:{this.WVM.SelectedGraphKind1Index} SelectedGraphKind2Index:{this.WVM.SelectedGraphKind2Index}");

                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    settings.MainWindow_SelectedGraphKindIndex = this.WVM.SelectedGraphKind1Index;
                    settings.MainWindow_SelectedGraphKind2Index = this.WVM.SelectedGraphKind2Index;
                    settings.Save();

                    await this.UpdateAsync();
                }
            };
            // 系列選択変更時
            this.WVM.SelectedSeriesChanged += () => {
                Log.Info("SelectedSeriesChanged");

                using (WaitCursorUseObject wcuo = this.CreateWaitCorsorUseObject()) {
                    this.UpdateSelectedGraph();
                }
            };
        }

        /// <summary>
        /// ウィンドウ位置またはサイズを修正する
        /// </summary>
        private bool ModifyLocationOrSize()
        {
            bool ret = this.ChangedLocationOrSizeWrapper(() => {
                bool ret2 = true;

                /// 位置調整
                if (30000 < Math.Max(Math.Abs(this.Left), Math.Abs(this.Top))) {
                    double tmpTop = this.Top;
                    double tmpLeft = this.Left;
                    if (30000 < Math.Max(Math.Abs(this.lastBounds.Left), Math.Abs(this.lastBounds.Top))) {
                        this.Left = this.lastBounds.Left;
                        this.Top = this.lastBounds.Top;
                    }
                    else {
                        // ディスプレイの中央に移動する
                        this.MoveOwnersCenter();
                    }

                    if (tmpTop != this.Top || tmpLeft != this.Left) {
                        this.windowLog.Log("WindowLocationModified", true);
                    }
                    else {
                        this.windowLog.Log("FailedToModifyLocation", true);
                        ret2 = false;
                    }
                }

                /// サイズ調整
                if (this.Height < 40 || this.Width < 40) {
                    double tmpHeight = this.Height;
                    double tmpWidth = this.Width;
                    if (40 < this.lastBounds.Height && 40 < this.lastBounds.Width) {
                        this.Height = this.lastBounds.Height;
                        this.Width = this.lastBounds.Width;
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
                        ret2 = false;
                    }
                }

                return ret2;
            });

            this.lastBounds = this.RestoreBounds;

            return ret;
        }

        #region ダンプ/リストア
        /// <summary>
        /// バックアップファイルを作成する
        /// </summary>
        /// <param name="notifyResult">実行結果を通知する</param>
        /// <param name="waitForFinish">完了を待つか</param>
        /// <param name="backUpNum">バックアップ数</param>
        /// <param name="backUpFolderPath">バックアップ用フォルダパス</param>
        /// <returns>成功/失敗</returns>
        public async Task<bool> CreateBackUpFileAsync(bool notifyResult = false, bool waitForFinish = true, int? backUpNum = null, string backUpFolderPath = null)
        {
            Properties.Settings settings = Properties.Settings.Default;

            int tmpBackUpNum = backUpNum ?? settings.App_BackUpNum;
            string tmpBackUpFolderPath = backUpFolderPath ?? settings.App_BackUpFolderPath;

            int? exitCode = null;

            // 古いバックアップを削除する
            if (tmpBackUpFolderPath != string.Empty) {
                if (Directory.Exists(tmpBackUpFolderPath)) {
                    List<string> fileList = new(Directory.GetFiles(tmpBackUpFolderPath, "*.backup", SearchOption.TopDirectoryOnly));
                    if (fileList.Count >= tmpBackUpNum) {
                        fileList.Sort();

                        for (int i = 0; i <= fileList.Count - tmpBackUpNum; ++i) {
                            File.Delete(fileList[i]);
                        }
                    }
                }
            }

            if (0 < tmpBackUpNum) {
                if (tmpBackUpFolderPath != string.Empty) {
                    // フォルダが存在しなければ作成する
                    if (!Directory.Exists(tmpBackUpFolderPath)) {
                        _ = Directory.CreateDirectory(tmpBackUpFolderPath);
                    }

                    if (settings.App_Postgres_DumpExePath != string.Empty) {
                        string backupFilePath = Path.Combine(tmpBackUpFolderPath, BackupFileName);
                        exitCode = await this.ExecuteDump(backupFilePath, PostgresFormat.Custom, notifyResult, waitForFinish);
                    }
                }
            }
            else {
                // バックアップ不要
                exitCode = 0;
            }

            return exitCode == 0;
        }

        /// <summary>
        /// ダンプを実行する
        /// </summary>
        /// <param name="backupFilePath">バックアップファイルパス</param>
        /// <param name="format">ダンプフォーマット</param>
        /// <param name="notifyResult">実行結果を通知する</param>
        /// <param name="waitForFinish">処理の完了を待機する</param>
        /// <returns>成功/失敗/不明</returns>
        private async Task<int?> ExecuteDump(string backupFilePath, PostgresFormat format, bool notifyResult = false, bool waitForFinish = true)
        {
            Properties.Settings settings = Properties.Settings.Default;

            int? result = -1;
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                if (dbHandler is NpgsqlDbHandler npgsqlDbHandler) {
                    result = notifyResult
                        ? await npgsqlDbHandler.ExecuteDump(backupFilePath, settings.App_Postgres_DumpExePath, (PostgresPasswordInput)settings.App_Postgres_Password_Input, format,
                            (exitCode) => {
                                // ダンプ結果を通知する
                                if (exitCode == 0) {
                                    NotificationManager nm = new();
                                    NotificationContent nc = new() {
                                        Title = this.Title,
                                        Message = Properties.Resources.Message_FinishToBackup,
                                        Type = NotificationType.Success
                                    };
                                    nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10));
                                }
                                else if (exitCode != null) {
                                    NotificationManager nm = new();
                                    NotificationContent nc = new() {
                                        Title = this.Title,
                                        Message = Properties.Resources.Message_FoultToBackup,
                                        Type = NotificationType.Error
                                    };
                                    nm.Show(nc, expirationTime: new TimeSpan(0, 0, 10));
                                }
                            }, waitForFinish)
                        : await npgsqlDbHandler.ExecuteDump(backupFilePath, settings.App_Postgres_DumpExePath, (PostgresPasswordInput)settings.App_Postgres_Password_Input, format, null, waitForFinish);
                }
            }
            return result;
        }

        /// <summary>
        /// リストアを実行する
        /// </summary>
        /// <param name="backupFilePath">バックアップファイルパス</param>
        /// <returns>成功/失敗</returns>
        private async Task<int> ExecuteRestore(string backupFilePath)
        {
            Properties.Settings settings = Properties.Settings.Default;

            int result = -1;
            using (DbHandlerBase dbHandler = this.dbHandlerFactory.Create()) {
                if (dbHandler is NpgsqlDbHandler npgsqlDbHandler) {
                    result = await npgsqlDbHandler.ExecuteRestore(backupFilePath, settings.App_Postgres_RestoreExePath, (PostgresPasswordInput)settings.App_Postgres_Password_Input);
                }
            }
            return result;
        }
        #endregion
    }
}
