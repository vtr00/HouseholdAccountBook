using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Infrastructure.Utilities.Args.RequestEventArgs;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Loaders;
using HouseholdAccountBook.ViewModels.Settings;
using HouseholdAccountBook.Views;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HouseholdAccountBook.ViewModels.WindowsParts
{
    /// <summary>
    /// 帳簿設定タブVM
    /// </summary>
    public class SettingsWindowBookTabViewModel : WindowPartViewModelBase
    {
        #region フィールド
        /// <summary>
        /// アプリサービス
        /// </summary>
        private AppService mAppService;
        /// <summary>
        /// 設定サービス
        /// </summary>
        private SettingService mSettingService;
        #endregion

        #region イベント
        /// <summary>
        /// 更新必要時イベント
        /// </summary>
        public event EventHandler NeedToUpdateChanged;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 帳簿セレクタVM
        /// </summary>
        public SelectorViewModel<BookModel, BookIdObj> BookSelectorVM { get; } = new(static vm => vm?.Id);
        /// <summary>
        /// 表示された帳簿設定VM
        /// </summary>
        public BookSettingViewModel DisplayedBookSettingVM {
            get;
            set => this.SetProperty(ref field, value);
        }

        #region コマンド
        /// <summary>
        /// 帳簿追加コマンド
        /// </summary>
        public ICommand AddBookCommand => new RelayCommand(this.AddBookCommand_Executed);
        /// <summary>
        /// 帳簿削除コマンド
        /// </summary>
        public ICommand DeleteBookCommand => new RelayCommand(this.DeleteBookCommand_Executed, this.DeleteBookCommand_CanExecute);
        /// <summary>
        /// 帳簿表示順上昇コマンド
        /// </summary>
        public ICommand RaiseBookSortOrderCommand => new RelayCommand(this.RaiseBookSortOrderCommand_Executed, this.RaiseBookSortOrderCommand_CanExecute);
        /// <summary>
        /// 帳簿表示順下降コマンド
        /// </summary>
        public ICommand DropBookSortOrderCommand => new RelayCommand(this.DropBookSortOrderCommand_Executed, this.DropBookSortOrderCommand_CanExecute);
        /// <summary>
        /// 帳簿情報保存コマンド
        /// </summary>
        public ICommand SaveBookInfoCommand => new RelayCommand(this.SaveBookInfoCommand_Executed, this.SaveBookInfoCommand_CanExecute);
        /// <summary>
        /// フォルダパス選択コマンド
        /// </summary>
        public override ICommand SelectFolderPathCommand => new RelayCommand<FolderPathKind>(this.SelectFolderPathCommand_Executed);
        /// <summary>
        /// 帳簿-項目関係変更コマンド
        /// </summary>
        public ICommand ChangeBookRelationCommand => new RelayCommand<object>(this.ChangeBookRelationCommand_Executed);
        #endregion
        #endregion

        #region コマンドイベントハンドラ
        /// <summary>
        /// 帳簿追加コマンド処理
        /// </summary>
        private async void AddBookCommand_Executed()
        {
            using WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create();

            BookIdObj bookId = await this.mSettingService.AddBookAsync();
            await this.LoadAsync(bookId);

            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 帳簿削除コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool DeleteBookCommand_CanExecute() => this.BookSelectorVM.SelectedItem != null;

        /// <summary>
        /// 帳簿削除コマンド処理
        /// </summary>
        private async void DeleteBookCommand_Executed()
        {
            using WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create();

            if (await this.mSettingService.DeleteBookAsync(this.BookSelectorVM.SelectedKey)) {
                await this.LoadAsync();
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
            else {
                _ = MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInBook, Properties.Resources.Title_Error);
            }
        }

        /// <summary>
        /// 帳簿表示順上昇コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool RaiseBookSortOrderCommand_CanExecute() => 0 < this.BookSelectorVM.SelectedIndex;

        /// <summary>
        /// 帳簿表示順上昇コマンド処理
        /// </summary>
        private async void RaiseBookSortOrderCommand_Executed()
        {
            using WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create();

            int index = this.BookSelectorVM.SelectedIndex;
            BookIdObj changingId = this.BookSelectorVM.ItemList[index].Id;
            BookIdObj changedId = this.BookSelectorVM.ItemList[index - 1].Id;

            await this.mSettingService.SwapBookSortOrderAsync(changingId, changedId);
            await this.LoadAsync(changingId);

            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 帳簿表示順下降コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool DropBookSortOrderCommand_CanExecute()
        {
            int index = this.BookSelectorVM.SelectedIndex;
            bool canExecute = index != -1 && index < this.BookSelectorVM.Count - 1;
            return canExecute;
        }

        /// <summary>
        /// 帳簿表示順下降コマンド処理
        /// </summary>
        private async void DropBookSortOrderCommand_Executed()
        {
            using WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create();

            int index = this.BookSelectorVM.SelectedIndex;
            BookIdObj changingId = this.BookSelectorVM.ItemList[index].Id;
            BookIdObj changedId = this.BookSelectorVM.ItemList[index + 1].Id;

            await this.mSettingService.SwapBookSortOrderAsync(changingId, changedId);
            await this.LoadAsync(changingId);

            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 帳簿情報保存コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool SaveBookInfoCommand_CanExecute() => !string.IsNullOrWhiteSpace(this.DisplayedBookSettingVM?.InputedName);

        /// <summary>
        /// 帳簿情報保存コマンド処理
        /// </summary>
        private async void SaveBookInfoCommand_Executed()
        {
            using WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create();

            BookSettingViewModel vm = this.DisplayedBookSettingVM;
            BookModel book = new(vm.Id, vm.InputedName) {
                BookKind = vm.BookKindSelectorVM.SelectedKey,
                Remark = vm.InputedRemark ?? string.Empty,
                InitialValue = vm.InputedInitialValue,
                StartDateExists = vm.SelectedIfStartDateExists,
                EndDateExists = vm.SelectedIfEndDateExists,
                Period = vm.InputedPeriod,
                DebitBookId = vm.DebitBookSelectorVM.SelectedKey,
                PayDay = vm.InputedPayDay,
                CsvFolderPath = vm.InputedCsvFolderPath != string.Empty ? Path.GetFullPath(vm.InputedCsvFolderPath, App.GetCurrentDir()) : null,
                TextEncoding = vm.TextEncodingSelectorVM.SelectedKey,
                ActDateIndex = vm.InputedActDateIndex,
                ExpensesIndex = vm.InputedExpensesIndex,
                ItemNameIndex = vm.InputedItemNameIndex
            };
            await this.mSettingService.SaveBookAsync(book);
            await this.LoadAsync(vm.Id);

            _ = MessageBox.Show(Properties.Resources.Message_FinishToSave, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information);
            this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// フォルダパス選択コマンド処理
        /// </summary>
        private void SelectFolderPathCommand_Executed(FolderPathKind kind)
        {
            Properties.Settings settings = Properties.Settings.Default;

            string folderFullPath = string.Empty;
            string title = string.Empty;
            switch (kind) {
                case FolderPathKind.CsvFolder:
                    if (string.IsNullOrWhiteSpace(this.DisplayedBookSettingVM.InputedCsvFolderPath)) {
                        folderFullPath = Path.GetDirectoryName(settings.App_CsvFilePath);
                    }
                    else {
                        (string folderPath, string fileName) = PathUtil.GetSeparatedPath(this.DisplayedBookSettingVM.InputedCsvFolderPath, App.GetCurrentDir());
                        folderFullPath = Path.Combine(folderPath, fileName);
                    }
                    title = Properties.Resources.Title_CsvFolderSelection;
                    break;
                default:
                    break;
            }

            OpenFolderDialogRequestEventArgs e = new() {
                InitialDirectory = folderFullPath,
                Title = title
            };
            if (this.OpenFolderDialogRequest(e)) {
                switch (kind) {
                    case FolderPathKind.CsvFolder:
                        this.DisplayedBookSettingVM.InputedCsvFolderPath = PathUtil.GetSmartPath(App.GetCurrentDir(), e.FolderName);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 帳簿-項目関係変更コマンド処理
        /// </summary>
        /// <param name="viewModel">チェックされた対象の<see cref="RelationModel"/></param>
        private async void ChangeBookRelationCommand_Executed(object viewModel)
        {
            using WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create();

            BookSettingViewModel vm = this.DisplayedBookSettingVM;
            vm.RelationSelectorVM.SelectedItem = viewModel as RelationViewModel; // チェックボックスを変更しただけでは変更されないため、引数で受け取る

            if (await this.mSettingService.SaveBookItemRemationAsync(vm.Id, (int)vm.RelationSelectorVM.SelectedKey, vm.RelationSelectorVM.SelectedItem.IsRelated)) {
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
            else {
                vm.RelationSelectorVM.SelectedItem.IsRelated = !vm.RelationSelectorVM.SelectedItem.IsRelated; // 選択前の状態に戻す
                _ = MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInItemWithinBook, Properties.Resources.Title_Error);
            }
        }
        #endregion

        public SettingsWindowBookTabViewModel()
        {
            using FuncLog funcLog = new();

            this.BookSelectorVM.SetLoader(async _ => await this.mAppService.LoadBookListAsync());
        }

        public override void Initialize(WaitCursorManagerFactory waitCursorManagerFactory, DbHandlerFactory dbHandlerFactory)
        {
            base.Initialize(waitCursorManagerFactory, dbHandlerFactory);

            this.mAppService = new(this.mDbHandlerFactory);
            this.mSettingService = new(this.mDbHandlerFactory);
        }

        public override async Task LoadAsync() => await this.LoadAsync(null);

        /// <summary>
        /// 帳簿設定タブに表示するデータを読み込む
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        public async Task LoadAsync(BookIdObj bookId = null)
        {
            using FuncLog funcLog = new(new { bookId });

            // InitializeComponent内で呼ばれる場合があるため、nullチェックを行う
            if (this.mAppService == null) {
                return;
            }

            await this.BookSelectorVM.LoadAsync(bookId);
        }

        public override void AddEventHandlers()
        {
            this.BookSelectorVM.SelectionChanged += async (sender, e) => {
                if (e.NewValue != null) {
                    using WaitCursorManager wcm = this.mWaitCursorManagerFactory.Create();

                    SettingViewModelLoader loader = new(this.mAppService, this.mSettingService);
                    this.DisplayedBookSettingVM = await loader.LoadBookSettingViewModelAsync(e.NewValue);
                }
                else {
                    this.DisplayedBookSettingVM = null;
                }
            };
        }
    }
}
