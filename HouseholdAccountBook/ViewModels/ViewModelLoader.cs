using HouseholdAccountBook.Adapters.Dao.Compositions;
using HouseholdAccountBook.Adapters.Dao.DbTable;
using HouseholdAccountBook.Adapters.DbHandlers;
using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Dto.DbTable;
using HouseholdAccountBook.Adapters.Dto.Others;
using HouseholdAccountBook.Adapters.Logger;
using HouseholdAccountBook.Enums;
using HouseholdAccountBook.Extensions;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HouseholdAccountBook.Extensions.EncodingExtensions;
using static HouseholdAccountBook.ViewModels.Settings.HierarchicalSettingViewModel;
using static HouseholdAccountBook.Views.UiConstants;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// ViewModel読込サービス
    /// </summary>
    public class ViewModelLoader(DbHandlerFactory dbHandlerFactory)
    {
        private DbHandlerFactory DbHandlerFactory { get; } = dbHandlerFactory;

        #region RegistrationWindow
        /// <summary>
        /// 帳簿リストを取得する
        /// </summary>
        /// <param name="initialName">1番目に追加する項目の名称(空文字の場合は追加しない)</param>
        /// <param name="start">開始日</param>
        /// <param name="end">終了日</param>
        public async Task<ObservableCollection<BookViewModel>> LoadBookListAsync(string initialName = "", DateTime? start = null, DateTime? end = null)
        {
            using FuncLog funcLog = new(new { initialName, start, end });

            ObservableCollection<BookViewModel> bookVMList = [];

            // 1番目に表示する項目を追加する
            if (initialName != string.Empty) {
                bookVMList.Add(new BookViewModel() { Id = null, Name = initialName });
            }

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                MstBookDao mstBookDao = new(dbHandler);
                var dtoList = await mstBookDao.FindAllAsync();
                foreach (var dto in dtoList) {
                    MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : new(dto.JsonCode);

                    if (DateTimeExtensions.IsWithIn(start, end, jsonObj?.StartDate, jsonObj?.EndDate)) {
                        BookViewModel vm = new() {
                            Id = dto.BookId,
                            Name = dto.BookName,
                            Remark = jsonObj?.Remark ?? string.Empty,
                            BookKind = (BookKind)dto.BookKind,
                            DebitBookId = dto.DebitBookId,
                            PayDay = dto.PayDay
                        };
                        bookVMList.Add(vm);
                    }
                }
            }

            return bookVMList;
        }

        /// <summary>
        /// 分類リストを取得する
        /// </summary>
        /// <param name="bookId">絞り込み対象の帳簿ID</param>
        /// <param name="balanceKind">絞り込み対象の収支種別</param>
        /// <returns></returns>
        public async Task<ObservableCollection<CategoryViewModel>> LoadCategoryListAsync(int bookId, BalanceKind balanceKind)
        {
            using FuncLog funcLog = new(new { bookId, balanceKind });

            ObservableCollection<CategoryViewModel> categoryVMList = [
                new CategoryViewModel() { Id = -1, Name = Properties.Resources.ListName_NoSpecification }
            ];

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                MstCategoryWithinBookDao mstCategoryWithinBookDao = new(dbHandler);
                var dtoList = await mstCategoryWithinBookDao.FindByBookIdAndBalanceKindAsync(bookId, (int)balanceKind);
                foreach (MstCategoryDto dto in dtoList) {
                    CategoryViewModel vm = new() {
                        Id = dto.CategoryId,
                        Name = dto.CategoryName
                    };
                    categoryVMList.Add(vm);
                }
            }

            return categoryVMList;
        }

        /// <summary>
        /// 項目リストを取得する
        /// </summary>
        /// <param name="bookId">絞り込み対象の帳簿ID</param>
        /// <param name="balanceKind">絞り込み対象の収支種別</param>
        /// <param name="categoryId">絞り込み対象の分類ID</param>
        /// <returns></returns>
        public async Task<ObservableCollection<ItemViewModel>> LoadItemListAsync(int bookId, BalanceKind balanceKind, int categoryId)
        {
            using FuncLog funcLog = new(new { bookId, balanceKind, categoryId });

            ObservableCollection<ItemViewModel> itemVMList = [];

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                CategoryItemInfoDao categoryItemInfoDao = new(dbHandler);
                var dtoList = categoryId == -1
                    ? await categoryItemInfoDao.FindByBookIdAndBalanceKindAsync(bookId, (int)balanceKind)
                    : await categoryItemInfoDao.FindByBookIdAndCategoryIdAsync(bookId, categoryId);
                foreach (CategoryItemInfoDto dto in dtoList) {
                    ItemViewModel vm = new() {
                        Id = dto.ItemId,
                        Name = dto.ItemName,
                        CategoryName = categoryId == -1 ? dto.CategoryName : ""
                    };
                    itemVMList.Add(vm);
                }
            }

            return itemVMList;
        }

        /// <summary>
        /// 店舗リストを取得する
        /// </summary>
        /// <param name="itemId">絞り込み対象の項目ID</param>
        /// <returns></returns>
        public async Task<ObservableCollection<ShopViewModel>> LoadShopListAsync(int itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            ObservableCollection<ShopViewModel> shopNameVMList = [
                new ShopViewModel() { Name = string.Empty }
            ];

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                ShopInfoDao shopInfoDao = new(dbHandler);
                var dtoList = await shopInfoDao.FindByItemIdAsync(itemId);
                foreach (ShopInfoDto dto in dtoList) {
                    ShopViewModel vm = new() {
                        Name = dto.ShopName,
                        UsedCount = dto.Count,
                        UsedTime = dto.UsedTime
                    };
                    shopNameVMList.Add(vm);
                }
            }

            return shopNameVMList;
        }

        /// <summary>
        /// 備考リストを取得する
        /// </summary>
        /// <param name="itemId">絞り込み対象の項目ID</param>
        /// <returns></returns>
        public async Task<ObservableCollection<RemarkViewModel>> LoadRemarkListAsync(int itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            ObservableCollection<RemarkViewModel> remarkVMList = [
                    new RemarkViewModel() { Remark = string.Empty }
            ];

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                RemarkInfoDao remarkInfoDao = new(dbHandler);
                var dtoList = await remarkInfoDao.FindByItemIdAsync(itemId);
                foreach (RemarkInfoDto dto in dtoList) {
                    RemarkViewModel vm = new() {
                        Remark = dto.Remark,
                        UsedCount = dto.Count,
                        UsedTime = dto.UsedTime
                    };
                    remarkVMList.Add(vm);
                }
            }

            return remarkVMList;
        }
        #endregion

        #region MainWindow
        /// <summary>
        /// 月内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="targetBookId">帳簿ID</param>
        /// <param name="includedTime">月内の時刻</param>
        /// <returns>帳簿項目VMリスト</returns>
        public async Task<ObservableCollection<ActionViewModel>> LoadActionViewModelListWithinMonthAsync(int? targetBookId, DateTime includedTime)
        {
            using FuncLog funcLog = new(new { targetBookId, includedTime });

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
        public async Task<ObservableCollection<ActionViewModel>> LoadActionViewModelListAsync(int? targetBookId, DateTime startTime, DateTime endTime)
        {
            using FuncLog funcLog = new(new { targetBookId, startTime, endTime });

            ObservableCollection<ActionViewModel> actionVMList = [];
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
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
                        Income = aDto.ActValue < 0 ? null : aDto.ActValue,
                        Expenses = aDto.ActValue < 0 ? -aDto.ActValue : null,
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
        public async Task<ObservableCollection<SummaryViewModel>> LoadSummaryViewModelListWithinMonthAsync(int? bookId, DateTime includedTime)
        {
            using FuncLog funcLog = new(new { bookId, includedTime });

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
        public async Task<ObservableCollection<SummaryViewModel>> LoadSummaryViewModelListAsync(int? bookId, DateTime startTime, DateTime endTime)
        {
            using FuncLog funcLog = new(new { bookId, startTime, endTime });

            ObservableCollection<SummaryViewModel> summaryVMList = [];
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
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
            int total = summaryVMList.Sum(obj => obj.Total);
            // 収入/支出
            List<SummaryViewModel> totalAsBalanceKindList = [];
            // 分類小計
            List<SummaryViewModel> totalAsCategoryList = [];

            // 収支別に計算する
            foreach (var g1 in summaryVMList.GroupBy(obj => obj.BalanceKind)) {
                // 収入/支出の小計を計算する
                totalAsBalanceKindList.Add(new SummaryViewModel() {
                    BalanceKind = g1.Key,
                    BalanceName = BalanceKindStr[(BalanceKind)g1.Key],
                    Total = g1.Sum(obj => obj.Total)
                });
                // 分類別の小計を計算する
                foreach (var g2 in g1.GroupBy(obj => obj.CategoryId)) {
                    totalAsCategoryList.Add(new SummaryViewModel() {
                        BalanceKind = g1.Key,
                        BalanceName = BalanceKindStr[(BalanceKind)g1.Key],
                        CategoryId = g2.Key,
                        CategoryName = g2.First().CategoryName,
                        Total = g2.Sum(obj => obj.Total)
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
        public async Task<ObservableCollection<SeriesViewModel>> LoadDailySeriesViewModelListWithinMonthAsync(int? bookId, DateTime includedTime)
        {
            using FuncLog funcLog = new(new { bookId, includedTime });

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
        public async Task<ObservableCollection<SeriesViewModel>> LoadDailySeriesViewModelListAsync(int? bookId, DateTime startTime, DateTime endTime)
        {
            using FuncLog funcLog = new(new { bookId, startTime, endTime });

            // 開始日までの収支を取得する
            int balance = 0;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
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
                    Values = [],
                    StartDates = [],
                    EndDates = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の日の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.AddDays(1).AddMilliseconds(-1);
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高
            vmList[0].StartDates.Add(tmpStartTime);
            vmList[0].EndDates.Add(tmpEndTime);

            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [],
                    StartDates = [],
                    EndDates = [],
                    Total = value,
                    Average = endTime < DateTime.Now ? value : 0 // 平均値は過去のデータのみで計算する
                };
                vm.Values.Add(value);
                vm.StartDates.Add(tmpStartTime);
                vm.EndDates.Add(tmpEndTime);
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
                vmList[0].StartDates.Add(tmpStartTime);
                vmList[0].EndDates.Add(tmpEndTime);
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    int value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);
                    vmList[j + 1].StartDates.Add(tmpStartTime);
                    vmList[j + 1].EndDates.Add(tmpEndTime);

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
        /// <param name="startMonth">年度開始月</param>
        /// <returns>年度内月別系列VMリスト</returns>
        public async Task<ObservableCollection<SeriesViewModel>> LoadMonthlySeriesViewModelListWithinYearAsync(int? bookId, DateTime includedTime, int startMonth)
        {
            using FuncLog funcLog = new(new { bookId, includedTime, startMonth });

            DateTime startTime = includedTime.GetFirstDateOfFiscalYear(startMonth);
            DateTime endTime = startTime.GetLastDateOfFiscalYear(startMonth);

            // 開始日までの収支を取得する
            int balance = 0;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = bookId == null
                    ? await endingBalanceInfoDao.Find(startTime) // 全帳簿
                    : await endingBalanceInfoDao.FindByBookId(bookId.Value, startTime); // 各帳簿
                balance = dto.EndingBalance;
            }

            ObservableCollection<SeriesViewModel> vmList = [
                new SeriesViewModel() {
                    OtherName = Properties.Resources.ListName_Balance,
                    Values = [],
                    StartDates = [],
                    EndDates = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の月の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.GetLastDateOfMonth();
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高
            vmList[0].StartDates.Add(tmpStartTime);
            vmList[0].EndDates.Add(tmpEndTime);
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [],
                    StartDates = [],
                    EndDates = [],
                    Total = value,
                    Average = tmpEndTime < DateTime.Now ? value : 0
                };
                vm.Values.Add(value);
                vm.StartDates.Add(tmpStartTime);
                vm.EndDates.Add(tmpEndTime);
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
                vmList[0].StartDates.Add(tmpStartTime);
                vmList[0].EndDates.Add(tmpEndTime);
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    int value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);
                    vmList[j + 1].StartDates.Add(tmpStartTime);
                    vmList[j + 1].EndDates.Add(tmpEndTime);

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
        public async Task<ObservableCollection<SeriesViewModel>> LoadYearlySeriesViewModelListWithinDecadeAsync(int? bookId, DateTime startYear, int startMonth)
        {
            using FuncLog funcLog = new(new { bookId, startYear, startMonth });

            DateTime startTime = startYear;

            // 開始日までの収支を取得する
            int balance = 0;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = bookId == null
                    ? await endingBalanceInfoDao.Find(startTime) // 全帳簿
                    : await endingBalanceInfoDao.FindByBookId(bookId.Value, startTime); // 各帳簿
                balance = dto.EndingBalance;
            }

            ObservableCollection<SeriesViewModel> vmList = [
                new SeriesViewModel() {
                    OtherName = Properties.Resources.ListName_Balance,
                    Values = [],
                    StartDates = [],
                    EndDates = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する年数(去年まで)

            // 最初の年の分を取得する
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = tmpStartTime.GetLastDateOfFiscalYear(startMonth);
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高
            vmList[0].StartDates.Add(tmpStartTime);
            vmList[0].EndDates.Add(tmpEndTime);
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                int value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [],
                    StartDates = [],
                    EndDates = [],
                    Total = value,
                    Average = tmpEndTime < DateTime.Now ? value : 0
                };
                vm.Values.Add(value);
                vm.StartDates.Add(tmpStartTime);
                vm.EndDates.Add(tmpEndTime);
                vmList.Add(vm);
            }
            if (tmpEndTime < DateTime.Now) {
                ++averageCount;
            }

            // 最初以外の年の分を取得する
            int years = 10;
            for (int i = 1; i < years; ++i) {
                tmpStartTime = tmpStartTime.AddYears(1);
                tmpEndTime = tmpStartTime.GetLastDateOfFiscalYear(startMonth);

                summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpStartTime, tmpEndTime);
                balance += summaryVMList[0].Total;
                vmList[0].Values.Add(balance); // 残高
                vmList[0].StartDates.Add(tmpStartTime);
                vmList[0].EndDates.Add(tmpEndTime);
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    int value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);
                    vmList[j + 1].StartDates.Add(tmpStartTime);
                    vmList[j + 1].EndDates.Add(tmpEndTime);

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

        #region CsvComparisonWindow
        /// <summary>
        /// 帳簿VM(比較用)を取得する
        /// </summary>
        public async Task<ObservableCollection<BookComparisonViewModel>> UpdateBookCompListAsync()
        {
            using FuncLog funcLog = new();

            ObservableCollection<BookComparisonViewModel> bookCompVMList = [];

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                MstBookDao mstBookDao = new(dbHandler);
                var dtoList = await mstBookDao.FindIfJsonCodeExistsAsync();
                foreach (MstBookDto dto in dtoList) {
                    MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : new(dto.JsonCode);
                    if (jsonObj is null) { continue; }

                    BookComparisonViewModel vm = new() {
                        Id = dto.BookId,
                        Name = dto.BookName,
                        CsvFolderPath = jsonObj.CsvFolderPath == string.Empty ? null : jsonObj.CsvFolderPath,
                        TextEncoding = jsonObj.TextEncoding,
                        ActDateIndex = jsonObj.CsvActDateIndex + 1,
                        ExpensesIndex = jsonObj.CsvOutgoIndex + 1,
                        ItemNameIndex = jsonObj.CsvItemNameIndex + 1
                    };
                    if (vm.CsvFolderPath == null || vm.ActDateIndex == null || vm.ExpensesIndex == null || vm.ItemNameIndex == null) { continue; }

                    bookCompVMList.Add(vm);
                }
            }

            return bookCompVMList;
        }
        #endregion

        #region SettingsWindow
        /// <summary>
        /// 帳簿設定VMを取得する
        /// </summary>
        /// <param name="bookId">表示対象の帳簿ID</param>
        /// <returns>帳簿設定VM</returns>
        public async Task<BookSettingViewModel> LoadBookSettingViewModelAsync(int bookId)
        {
            using FuncLog funcLog = new(new { bookId });

            BookSettingViewModel vm = null;

            ViewModelLoader loader = new(this.DbHandlerFactory);
            ObservableCollection<BookViewModel> vmList = await loader.LoadBookListAsync(Properties.Resources.ListName_None);

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                // 帳簿一覧を取得する
                BookInfoDao bookInfoDao = new(dbHandler);
                var dto = await bookInfoDao.FindByBookId(bookId);

                MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : new(dto.JsonCode);

                vm = new BookSettingViewModel() {
                    Id = bookId,
                    SortOrder = dto.SortOrder,
                    Name = dto.BookName,
                    SelectedBookKind = (BookKind)dto.BookKind,
                    Remark = jsonObj?.Remark ?? string.Empty,
                    InitialValue = dto.InitialValue,
                    StartDateExists = jsonObj?.StartDate != null,
                    StartDate = jsonObj?.StartDate ?? dto.StartDate ?? DateTime.Today,
                    EndDateExists = jsonObj?.EndDate != null,
                    EndDate = jsonObj?.EndDate ?? dto.EndDate ?? DateTime.Today,
                    DebitBookVMList = new ObservableCollection<BookViewModel>(vmList.Where(tmpVM => tmpVM.Id != bookId)),
                    PayDay = dto.PayDay,
                    CsvFolderPath = jsonObj is null ? "" : PathExtensions.GetSmartPath(App.GetCurrentDir(), jsonObj.CsvFolderPath),
                    TextEncodingList = GetTextEncodingList(),
                    SelectedTextEncoding = jsonObj?.TextEncoding ?? Encoding.UTF8.CodePage,
                    ActDateIndex = jsonObj?.CsvActDateIndex + 1,
                    ExpensesIndex = jsonObj?.CsvOutgoIndex + 1,
                    ItemNameIndex = jsonObj?.CsvItemNameIndex + 1,
                    RelationVMList = await LoadRelationViewModelListFromBookIdAsync(dbHandler, bookId)
                };
                vm.SelectedDebitBookVM = vm.DebitBookVMList.FirstOrElementAtOrDefault(tmpVM => tmpVM.Id == dto.DebitBookId, 0);
            }

            return vm;
        }

        /// <summary>
        /// 関連VMリスト(帳簿主体)を取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>関連VMリスト</returns>
        private static async Task<ObservableCollection<RelationViewModel>> LoadRelationViewModelListFromBookIdAsync(DbHandlerBase dbHandler, int bookId)
        {
            using FuncLog funcLog = new(new { bookId });

            ItemRelFromBookInfoDao itemRelFromBookInfoDao = new(dbHandler);
            var dtoList = await itemRelFromBookInfoDao.FindByBookIdAsync(bookId);

            ObservableCollection<RelationViewModel> rvmList = [];
            foreach (ItemRelFromBookInfoDto dto in dtoList) {
                RelationViewModel rvm = new() {
                    Id = dto.ItemId,
                    Name = $"{BalanceKindStr[(BalanceKind)dto.BalanceKind]} > {dto.CategoryName} > {dto.ItemName}",
                    IsRelated = dto.IsRelated
                };
                rvmList.Add(rvm);
            }
            return rvmList;
        }

        /// <summary>
        /// 階層構造項目VMリストを取得する
        /// </summary>
        /// <returns>階層構造項目VMリスト</returns>
        public async Task<ObservableCollection<HierarchicalViewModel>> LoadHierarchicalViewModelListAsync()
        {
            using FuncLog funcLog = new();

            ObservableCollection<HierarchicalViewModel> vmList = [
                new () {
                    Depth = (int)HierarchicalKind.Balance,
                    Id = (int)BalanceKind.Income,
                    SortOrder = -1,
                    Name = Properties.Resources.BalanceKind_Income,
                    ParentVM = null,
                    ChildrenVMList = []
                },
                new () {
                    Depth = (int)HierarchicalKind.Balance,
                    Id = (int)BalanceKind.Expenses,
                    SortOrder = -1,
                    Name = Properties.Resources.BalanceKind_Expenses,
                    ParentVM = null,
                    ChildrenVMList = []
                }
            ];

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                foreach (HierarchicalViewModel vm in vmList) {
                    // 分類
                    MstCategoryDao mstCategoryDao = new(dbHandler);
                    var cDtoList = await mstCategoryDao.FindByBalanceKindAsync(vm.Id);

                    foreach (MstCategoryDto dto in cDtoList) {
                        vm.ChildrenVMList.Add(new HierarchicalViewModel() {
                            Depth = (int)HierarchicalKind.Category,
                            Id = dto.CategoryId,
                            SortOrder = dto.SortOrder,
                            Name = dto.CategoryName,
                            ParentVM = vm,
                            ChildrenVMList = []
                        });
                    }

                    // 項目
                    MstItemDao mstItemDao = new(dbHandler);
                    foreach (HierarchicalViewModel categoryVM in vm.ChildrenVMList) {
                        var iDtoList = await mstItemDao.FindByCategoryIdAsync(categoryVM.Id);

                        foreach (MstItemDto dto in iDtoList) {
                            categoryVM.ChildrenVMList.Add(new HierarchicalViewModel() {
                                Depth = (int)HierarchicalKind.Item,
                                Id = dto.ItemId,
                                SortOrder = dto.SortOrder,
                                Name = dto.ItemName,
                                ParentVM = categoryVM
                            });
                        }
                    }
                }
            }

            return vmList;
        }

        /// <summary>
        /// 階層構造設定VMを取得する
        /// </summary>
        /// <param name="kind">表示対象の階層種別</param>
        /// <param name="id">表示対象のID</param>
        /// <returns>階層構造設定VM</returns>
        public async Task<HierarchicalSettingViewModel> LoadHierarchicalSettingViewModelAsync(HierarchicalKind kind, int id)
        {
            using FuncLog funcLog = new(new { kind, id });

            HierarchicalSettingViewModel vm = null;

            switch (kind) {
                case HierarchicalKind.Balance: {
                    vm = new HierarchicalSettingViewModel() {
                        Kind = HierarchicalKind.Balance,
                        Id = -1,
                        SortOrder = -1,
                        Name = string.Empty
                    };
                    break;
                }
                case HierarchicalKind.Category: {
                    // 分類
                    await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                        MstCategoryDao mstCategoryDao = new(dbHandler);
                        var dto = await mstCategoryDao.FindByIdAsync(id);

                        vm = new HierarchicalSettingViewModel() {
                            Kind = HierarchicalKind.Category,
                            Id = id,
                            SortOrder = dto.SortOrder,
                            Name = dto.CategoryName
                        };
                    }
                    break;
                }
                case HierarchicalKind.Item: {
                    // 項目
                    await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                        MstItemDao mstItemDao = new(dbHandler);
                        var dto = await mstItemDao.FindByIdAsync(id);

                        vm = new HierarchicalSettingViewModel {
                            Kind = HierarchicalKind.Item,
                            Id = id,
                            SortOrder = dto.SortOrder,
                            Name = dto.ItemName,
                            RelationVMList = await LoadRelationViewModelListAsync(dbHandler, id),
                            ShopVMList = await LoadShopViewModelListAsync(dbHandler, id),
                            RemarkVMList = await LoadRemarkViewModelListAsync(dbHandler, id)
                        };
                    }
                    break;
                }
            }

            return vm;
        }

        /// <summary>
        /// 関連VMリスト(項目主体)を取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>関連VMリスト</returns>
        private static async Task<ObservableCollection<RelationViewModel>> LoadRelationViewModelListAsync(DbHandlerBase dbHandler, int itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            BookRelFromItemInfoDao bookRelFromItemInfoDao = new(dbHandler);
            var dtoList = await bookRelFromItemInfoDao.FindByItemIdAsync(itemId);

            ObservableCollection<RelationViewModel> rvmList = [];
            foreach (BookRelFromItemInfoDto dto in dtoList) {
                RelationViewModel rvm = new() {
                    Id = dto.BookId,
                    Name = dto.BookName,
                    IsRelated = dto.IsRelated
                };
                rvmList.Add(rvm);
            }
            return rvmList;
        }

        /// <summary>
        /// 店舗VMリストを取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>店舗VMリスト</returns>
        private static async Task<ObservableCollection<ShopViewModel>> LoadShopViewModelListAsync(DbHandlerBase dbHandler, int itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            ObservableCollection<ShopViewModel> svmList = [];
            ShopInfoDao shopInfoDao = new(dbHandler);
            var dtoList = await shopInfoDao.FindByItemIdAsync(itemId);

            foreach (ShopInfoDto dto in dtoList) {
                ShopViewModel svm = new() {
                    Name = dto.ShopName,
                    UsedCount = dto.Count,
                    UsedTime = dto.UsedTime
                };
                svmList.Add(svm);
            }
            return svmList;
        }

        /// <summary>
        /// 備考VMリストを取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="itemId">項目ID</param>
        /// <returns>備考VMリスト</returns>
        private static async Task<ObservableCollection<RemarkViewModel>> LoadRemarkViewModelListAsync(DbHandlerBase dbHandler, int itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            ObservableCollection<RemarkViewModel> rvmList = [];
            RemarkInfoDao remarkInfoDao = new(dbHandler);
            var dtoList = await remarkInfoDao.FindByItemIdAsync(itemId);

            foreach (RemarkInfoDto dto in dtoList) {
                RemarkViewModel rvm = new() {
                    Remark = dto.Remark,
                    UsedCount = dto.Count,
                    UsedTime = dto.UsedTime
                };
                rvmList.Add(rvm);
            }
            return rvmList;
        }
        #endregion
    }
}
