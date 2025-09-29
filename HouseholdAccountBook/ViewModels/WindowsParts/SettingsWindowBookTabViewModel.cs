using HouseholdAccountBook.Adapters.Dao.DbTable;
using HouseholdAccountBook.Adapters.DbHandler.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.Others;
using HouseholdAccountBook.Others.RequestEventArgs;
using HouseholdAccountBook.ViewModels.Abstract;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Settings;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        #region イベント
        /// <summary>
        /// 選択された帳簿VM変更時のイベント
        /// </summary>
        public event EventHandler<EventArgs<BookViewModel>> SelectedBookVMChanged;

        /// <summary>
        /// 更新必要時イベント
        /// </summary>
        public event EventHandler NeedToUpdateChanged;
        #endregion

        #region Bindingプロパティ
        /// <summary>
        /// 帳簿VMリスト
        /// </summary>
        #region BookVMList
        public ObservableCollection<BookViewModel> BookVMList
        {
            get => this._BookVMList;
            set {
                this._BookVMList.Clear();
                foreach (BookViewModel vm in value) {
                    this._BookVMList.Add(vm);
                }
                this.RaisePropertyChanged(nameof(this.BookVMList));
            }
        }
        private readonly ObservableCollection<BookViewModel> _BookVMList = [];
        #endregion
        /// <summary>
        /// 選択された帳簿VM
        /// </summary>
        #region SelectedBookVM
        public BookViewModel SelectedBookVM
        {
            get => this._SelectedBookVM;
            set {
                if (this.SetProperty(ref this._SelectedBookVM, value)) {
                    this.SelectedBookVMChanged?.Invoke(this, new EventArgs<BookViewModel>(value));
                }
            }
        }
        private BookViewModel _SelectedBookVM = default;
        #endregion
        /// <summary>
        /// 表示された帳簿設定VM
        /// </summary>
        #region DisplayedBookSettingVM
        public BookSettingViewModel DisplayedBookSettingVM
        {
            get => this._DisplayedBookSettingVM;
            set => this.SetProperty(ref this._DisplayedBookSettingVM, value);
        }
        private BookSettingViewModel _DisplayedBookSettingVM = default;
        #endregion

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
        public ICommand SelectCsvFolderPathCommand => new RelayCommand(this.SelectCsvFolderPathCommand_Executed);
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
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                int bookId = -1;
                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    MstBookDao mstBookDao = new(dbHandler);
                    bookId = await mstBookDao.InsertReturningIdAsync(new MstBookDto { });
                }

                await this.LoadAsync(bookId);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 帳簿削除コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool DeleteBookCommand_CanExecute() => this.SelectedBookVM != null;

        /// <summary>
        /// 帳簿削除コマンド処理
        /// </summary>
        private async void DeleteBookCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    await dbHandler.ExecTransactionAsync(async () => {
                        HstActionDao hstActionDao = new(dbHandler);
                        var dtoList = await hstActionDao.FindByBookIdAsync(this.SelectedBookVM.Id.Value);
                        if (dtoList.Any()) {
                            _ = MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInBook, Properties.Resources.Title_Error);
                            return;
                        }

                        MstBookDao hstBookDao = new(dbHandler);
                        _ = await hstBookDao.DeleteByIdAsync(this.SelectedBookVM.Id.Value);
                    });
                }

                await this.LoadAsync();
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 帳簿表示順上昇コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool RaiseBookSortOrderCommand_CanExecute()
        {
            bool canExecute = this.BookVMList != null;
            if (canExecute) {
                int index = this.BookVMList.IndexOf(this.SelectedBookVM);
                canExecute = index > 0;
            }
            return canExecute;
        }

        /// <summary>
        /// 帳簿表示順上昇コマンド処理
        /// </summary>
        private async void RaiseBookSortOrderCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                int index = this.BookVMList.IndexOf(this.SelectedBookVM);
                int changingId = this.BookVMList[index].Id.Value;
                int changedId = this.BookVMList[index - 1].Id.Value;

                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    MstBookDao mstBookDao = new(dbHandler);
                    _ = await mstBookDao.SwapSortOrderAsync(changingId, changedId);
                }

                await this.LoadAsync(changingId);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 帳簿表示順下降コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool DropBookSortOrderCommand_CanExecute()
        {
            bool canExecute = this.BookVMList != null;
            if (canExecute) {
                int index = this.BookVMList.IndexOf(this.SelectedBookVM);
                canExecute = index != -1 && index < this.BookVMList.Count - 1;
            }
            return canExecute;
        }

        /// <summary>
        /// 帳簿表示順下降コマンド処理
        /// </summary>
        private async void DropBookSortOrderCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                int index = this.BookVMList.IndexOf(this.SelectedBookVM);
                int changingId = this.BookVMList[index].Id.Value;
                int changedId = this.BookVMList[index + 1].Id.Value;

                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    MstBookDao mstBookDao = new(dbHandler);
                    _ = await mstBookDao.SwapSortOrderAsync(changingId, changedId);
                }

                await this.LoadAsync(changingId);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 帳簿情報保存コマンド実行可能か
        /// </summary>
        /// <returns></returns>
        private bool SaveBookInfoCommand_CanExecute() => this.SelectedBookVM != null && !string.IsNullOrWhiteSpace(this.SelectedBookVM.Name);

        /// <summary>
        /// 帳簿情報保存コマンド処理
        /// </summary>
        private async void SaveBookInfoCommand_Executed()
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                BookSettingViewModel vm = this.DisplayedBookSettingVM;
                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    MstBookDto.JsonDto jsonObj = new() {
                        StartDate = vm.StartDateExists ? vm.StartDate : null,
                        EndDate = vm.EndDateExists ? vm.EndDate : null,
                        CsvFolderPath = vm.CsvFolderPath != string.Empty ? Path.GetFullPath(vm.CsvFolderPath, App.GetCurrentDir()) : null,
                        TextEncoding = vm.SelectedTextEncoding,
                        CsvActDateIndex = vm.ActDateIndex - 1,
                        CsvOutgoIndex = vm.ExpensesIndex - 1,
                        CsvItemNameIndex = vm.ItemNameIndex - 1
                    };
                    string jsonCode = jsonObj.ToJson();

                    MstBookDao mstBookDao = new(dbHandler);
                    _ = await mstBookDao.UpdateSetableAsync(new MstBookDto {
                        BookName = vm.Name,
                        BookKind = (int)vm.SelectedBookKind,
                        InitialValue = vm.InitialValue,
                        DebitBookId = vm.SelectedDebitBookVM.Id,
                        PayDay = vm.PayDay,
                        JsonCode = jsonCode,
                        BookId = vm.Id.Value
                    });
                }

                await this.LoadAsync(vm.Id);
                _ = MessageBox.Show(Properties.Resources.Message_FinishToSave, Properties.Resources.Title_Information, MessageBoxButton.OK, MessageBoxImage.Information);
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// CSVフォルダパス選択コマンド処理
        /// </summary>
        private void SelectCsvFolderPathCommand_Executed()
        {
            Properties.Settings settings = Properties.Settings.Default;

            string folderFullPath;
            if (string.IsNullOrWhiteSpace(this.DisplayedBookSettingVM.CsvFolderPath)) {
                folderFullPath = Path.GetDirectoryName(settings.App_CsvFilePath);
            }
            else {
                (string folderPath, string fileName) = PathExtensions.GetSeparatedPath(this.DisplayedBookSettingVM.CsvFolderPath, App.GetCurrentDir());
                folderFullPath = Path.Combine(folderPath, fileName);
            }

            var e = new OpenFolderDialogRequestEventArgs() {
                InitialDirectory = folderFullPath,
                Title = Properties.Resources.Title_CsvFolderSelection
            };
            if (this.OpenFolderDialogRequest(e)) {
                this.DisplayedBookSettingVM.CsvFolderPath = PathExtensions.GetSmartPath(App.GetCurrentDir(), e.FolderName);
            }
        }

        /// <summary>
        /// 帳簿-項目関係変更コマンド処理
        /// </summary>
        private async void ChangeBookRelationCommand_Executed(object viewModel)
        {
            using (WaitCursorManager wcm = this.waitCursorManagerFactory.Create()) {
                BookSettingViewModel vm = this.DisplayedBookSettingVM;
                vm.SelectedRelationVM = viewModel as RelationViewModel; // チェックボックスを変更しただけでは変更されないため、引数で受け取る

                await using (DbHandlerBase dbHandler = await this.dbHandlerFactory.CreateAsync()) {
                    await dbHandler.ExecTransactionAsync(async () => {
                        HstActionDao hstActionDao = new(dbHandler);
                        var hstActionDtoList = await hstActionDao.FindByBookIdAndItemIdAsync(vm.Id.Value, vm.SelectedRelationVM.Id);

                        if (hstActionDtoList.Any()) {
                            vm.SelectedRelationVM.IsRelated = !vm.SelectedRelationVM.IsRelated; // 選択前の状態に戻す
                            _ = MessageBox.Show(Properties.Resources.Message_CantDeleteBecauseActionItemExistsInItemWithinBook, Properties.Resources.Title_Error);
                            return;
                        }

                        RelBookItemDao relBookItemDao = new(dbHandler);
                        var dtoList = await relBookItemDao.UpsertAsync(new RelBookItemDto {
                            BookId = vm.Id.Value,
                            ItemId = vm.SelectedRelationVM.Id,
                            DelFlg = vm.SelectedRelationVM.IsRelated ? 0 : 1
                        });
                    });
                }
                this.NeedToUpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion

        protected override void AddEventHandlers()
        {
            this.SelectedBookVMChanged += async (sender, e) => {
                if (e.Value != null) {
                    ViewModelLoader loader = new(this.dbHandlerFactory);
                    this.DisplayedBookSettingVM = await loader.LoadBookSettingViewModelAsync(e.Value.Id.Value);
                }
                else {
                    this.DisplayedBookSettingVM = null;
                }
            };
        }

        public override async Task LoadAsync()
        {
            await this.LoadAsync(null);
        }

        /// <summary>
        /// 帳簿設定タブに表示するデータを読み込む
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        public async Task LoadAsync(int? bookId = null)
        {
            Log.Info();

            // InitializeComponent内で呼ばれる場合があるため、nullチェックを行う
            if (this.dbHandlerFactory == null) {
                return;
            }

            await this.UpdateBookListAsync(bookId);
        }

        /// <summary>
        /// 帳簿リストを更新する
        /// </summary>
        /// <param name="bookId">選択対象の帳簿ID</param>
        private async Task UpdateBookListAsync(int? bookId = null)
        {
            ViewModelLoader loader = new(this.dbHandlerFactory);
            int? tmpBookId = bookId ?? this.SelectedBookVM?.Id;
            this.BookVMList = await loader.LoadBookListAsync();
            this.SelectedBookVM = this.BookVMList.FirstOrElementAtOrDefault(vm => vm.Id == tmpBookId, 0);
        }
    }
}
