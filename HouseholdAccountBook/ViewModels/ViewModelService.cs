using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.Infrastructure;
using HouseholdAccountBook.Models.Infrastructure.DbDao.Compositions;
using HouseholdAccountBook.Models.Infrastructure.DbDao.DbTable;
using HouseholdAccountBook.Models.Infrastructure.DbHandlers;
using HouseholdAccountBook.Models.Infrastructure.DbHandlers.Abstract;
using HouseholdAccountBook.Models.Infrastructure.DbDto.DbTable;
using HouseholdAccountBook.Models.Infrastructure.DbDto.Others;
using HouseholdAccountBook.Models.Infrastructure.Logger;
using HouseholdAccountBook.Models.Utilities.Extensions;
using HouseholdAccountBook.ViewModels.Component;
using HouseholdAccountBook.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HouseholdAccountBook.Models.Infrastructure.EncodingUtil;
using static HouseholdAccountBook.ViewModels.UiConstants;
using HouseholdAccountBook.Models.DomainModels;
using HouseholdAccountBook.Models.ValueObjects;
using System.Windows;

namespace HouseholdAccountBook.ViewModels
{
    /// <summary>
    /// ViewModelサービス
    /// </summary>
    public class ViewModelService(DbHandlerFactory dbHandlerFactory)
    {
        private DbHandlerFactory DbHandlerFactory { get; } = dbHandlerFactory;

        #region RegistrationWindow
        /// <summary>
        /// 帳簿リストを取得する
        /// </summary>
        /// <param name="initialName">1番目に追加する項目の名称(空文字の場合は追加しない)</param>
        /// <param name="start">開始日</param>
        /// <param name="end">終了日</param>
        /// <returns>帳簿リスト</returns>
        public async Task<ObservableCollection<BookModel>> LoadBookListAsync(string initialName = "", PeriodObj<DateOnly> period = null)
        {
            using FuncLog funcLog = new(new { initialName, period });

            ObservableCollection<BookModel> bookVMList = [];

            // 1番目に表示する項目を追加する
            if (initialName != string.Empty) {
                bookVMList.Add(new(null, initialName));
            }

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                MstBookDao mstBookDao = new(dbHandler);
                var dtoList = await mstBookDao.FindAllAsync();
                foreach (var dto in dtoList) {
                    MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : new(dto.JsonCode);

                    if (DateOnlyExtensions.IsWithIn(period?.Start, period?.End, jsonObj?.StartDate?.ToDateOnly(), jsonObj?.EndDate?.ToDateOnly())) {
                        BookModel vm = new(dto.BookId, dto.BookName) {
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
        /// <returns>分類リスト</returns>
        public async Task<ObservableCollection<CategoryModel>> LoadCategoryListAsync(BookIdObj bookId, BalanceKind balanceKind)
        {
            using FuncLog funcLog = new(new { bookId, balanceKind });

            ObservableCollection<CategoryModel> categoryVMList = [
                new CategoryModel(-1, Properties.Resources.ListName_NoSpecification, BalanceKind.Others)
            ];

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                MstCategoryWithinBookDao mstCategoryWithinBookDao = new(dbHandler);
                IEnumerable<MstCategoryDto> dtoList = await mstCategoryWithinBookDao.FindByBookIdAndBalanceKindAsync((int)bookId, (int)balanceKind);
                foreach (MstCategoryDto dto in dtoList) {
                    categoryVMList.Add(new(dto.CategoryId, dto.CategoryName, (BalanceKind)dto.BalanceKind));
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
        /// <returns>項目リスト</returns>
        public async Task<ObservableCollection<ItemModel>> LoadItemListAsync(BookIdObj bookId, BalanceKind balanceKind, CategoryIdObj categoryId)
        {
            using FuncLog funcLog = new(new { bookId, balanceKind, categoryId });

            ObservableCollection<ItemModel> itemVMList = [];

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                CategoryItemInfoDao categoryItemInfoDao = new(dbHandler);
                IEnumerable<CategoryItemInfoDto> dtoList = (int)categoryId == -1
                    ? await categoryItemInfoDao.FindByBookIdAndBalanceKindAsync((int)bookId, (int)balanceKind)
                    : await categoryItemInfoDao.FindByBookIdAndCategoryIdAsync((int)bookId, (int)categoryId);
                foreach (CategoryItemInfoDto dto in dtoList) {
                    ItemModel vm = new(dto.ItemId, dto.ItemName) {
                        CategoryName = (int)categoryId == -1 ? dto.CategoryName : ""
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
        /// <returns>店舗リスト</returns>
        public async Task<ObservableCollection<ShopViewModel>> LoadShopListAsync(ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            ObservableCollection<ShopViewModel> shopNameVMList = [
                new ShopViewModel()
            ];

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                ShopInfoDao shopInfoDao = new(dbHandler);
                IEnumerable<ShopInfoDto> dtoList = await shopInfoDao.FindByItemIdAsync((int)itemId);
                foreach (ShopInfoDto dto in dtoList) {
                    ShopViewModel vm = new() {
                        Shop = dto.ShopName,
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
        /// <returns>備考リスト</returns>
        public async Task<ObservableCollection<RemarkViewModel>> LoadRemarkListAsync(ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            ObservableCollection<RemarkViewModel> remarkVMList = [
                    new RemarkViewModel()
            ];

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                RemarkInfoDao remarkInfoDao = new(dbHandler);
                IEnumerable<RemarkInfoDto> dtoList = await remarkInfoDao.FindByItemIdAsync((int)itemId);
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
        public async Task<ObservableCollection<ActionViewModel>> LoadActionViewModelListWithinMonthAsync(BookIdObj targetBookId, DateOnly includedTime)
        {
            using FuncLog funcLog = new(new { targetBookId, includedTime });

            DateOnly startTime = includedTime.GetFirstDateOfMonth();
            DateOnly endTime = startTime.GetLastDateOfMonth();
            return await this.LoadActionViewModelListAsync(targetBookId, new (startTime, endTime));
        }

        /// <summary>
        /// 期間内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="targetBookId">帳簿ID</param>
        /// <param name="period">期間</param>
        /// <returns>帳簿項目VMリスト</returns>
        public async Task<ObservableCollection<ActionViewModel>> LoadActionViewModelListAsync(BookIdObj targetBookId, PeriodObj<DateOnly> period)
        {
            using FuncLog funcLog = new(new { targetBookId, period });

            ObservableCollection<ActionViewModel> actionVMList = [];
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = targetBookId == null
                    ? await endingBalanceInfoDao.Find(period.Start) // 全帳簿の繰越残高
                    : await endingBalanceInfoDao.FindByBookId(targetBookId.Value, period.Start); // 各帳簿の繰越残高

                // 繰越残高を追加
                decimal balance = dto.EndingBalance;
                {
                    ActionViewModel avm = new() {
                        ActionWithBalance = new() {
                            Action = new() {
                                Book = new(-1, string.Empty),
                                Category = new(-1, string.Empty, BalanceKind.Others),
                                Item = new(-1, Properties.Resources.ListName_CarryForward),
                                Base = new(-1, period.Start.ToDateTime(TimeOnly.MinValue), 0),
                                Shop = null,
                                Remark = null
                            },
                            Balance = balance
                        },
                        IsMatch = false
                    };
                    actionVMList.Add(avm);
                }

                ActionInfoDao actionInfoDao = new(dbHandler);
                IEnumerable<ActionInfoDto> dtoList = targetBookId == null
                    ? await actionInfoDao.FindAllWithinTerm(period.Start, period.End) // 全帳簿項目
                    : await actionInfoDao.FindByBookIdWithinTerm(targetBookId.Value, period.Start, period.End); // 各帳簿項目

                foreach (ActionInfoDto aDto in dtoList) {
                    balance += aDto.ActValue;

                    ActionViewModel avm = new() {
                        ActionWithBalance = new() {
                            Action = new() {
                                GroupId = aDto.GroupId,
                                Book = new(aDto.BookId, aDto.BookName),
                                Category = new(aDto.CategoryId, aDto.CategoryName, aDto.ActValue < 0 ? BalanceKind.Expenses : BalanceKind.Income),
                                Item = new(aDto.ItemId, aDto.ItemName),
                                Base = new(aDto.ActionId, aDto.ActTime, aDto.ActValue),
                                Shop = new(aDto.ShopName),
                                Remark = new(aDto.Remark)
                            },
                            Balance = balance
                        },
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
        public async Task<ObservableCollection<SummaryViewModel>> LoadSummaryViewModelListWithinMonthAsync(BookIdObj bookId, DateOnly includedTime)
        {
            using FuncLog funcLog = new(new { bookId, includedTime });

            DateOnly startTime = includedTime.GetFirstDateOfMonth();
            DateOnly endTime = startTime.GetLastDateOfMonth();
            return await this.LoadSummaryViewModelListAsync(bookId, new(startTime, endTime));
        }

        /// <summary>
        /// 期間内概要VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>概要VMリスト</returns>
        public async Task<ObservableCollection<SummaryViewModel>> LoadSummaryViewModelListAsync(BookIdObj bookId, PeriodObj<DateOnly> period)
        {
            using FuncLog funcLog = new(new { bookId, period });

            ObservableCollection<SummaryViewModel> summaryVMList = [];
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                SummaryInfoDao summaryInfoDao = new(dbHandler);
                IEnumerable<SummaryInfoDto> dtoList = bookId == null
                    ? await summaryInfoDao.FindAllWithinPeriod(period.Start, period.End)
                    : await summaryInfoDao.FindByBookIdWithinPeriod(bookId.Value, period.Start, period.End);

                foreach (SummaryInfoDto dto in dtoList) {
                    summaryVMList.Add(new() {
                        Category = new(dto.CategoryId, dto.CategoryName, (BalanceKind)dto.BalanceKind),
                        Item = new(dto.ItemId, dto.ItemName),
                        Total = dto.Total
                    });
                }
            }

            // 差引損益
            decimal total = summaryVMList.Sum(obj => obj.Total);
            // 収入/支出
            List<SummaryViewModel> totalAsBalanceKindList = [];
            // 分類小計
            List<SummaryViewModel> totalAsCategoryList = [];

            // 収支別に計算する
            foreach (var g1 in summaryVMList.GroupBy(obj => obj.Category.BalanceKind)) {
                // 収入/支出の小計を計算する
                totalAsBalanceKindList.Add(new() {
                    Category = new(-1, string.Empty, g1.Key),
                    Total = g1.Sum(obj => obj.Total)
                });
                // 分類別の小計を計算する
                foreach (var g2 in g1.GroupBy(obj => (int)obj.Category.Id)) {
                    totalAsCategoryList.Add(new() {
                        Category = new(g2.Key, g2.First().Category.Name, g1.Key),
                        Total = g2.Sum(obj => obj.Total)
                    });
                }
            }

            // 差引損益を追加する
            summaryVMList.Insert(0, new() {
                OtherName = Properties.Resources.ListName_profitAndLoss,
                Total = total
            });
            // 収入/支出の小計を追加する
            foreach (SummaryViewModel svm in totalAsBalanceKindList) {
                summaryVMList.Insert(summaryVMList.IndexOf(summaryVMList.First(obj => obj.Category.BalanceKind == svm.Category.BalanceKind)), svm);
            }
            // 分類別の小計を追加する
            foreach (SummaryViewModel svm in totalAsCategoryList) {
                summaryVMList.Insert(summaryVMList.IndexOf(summaryVMList.First(obj => obj.Category.Id == svm.Category.Id)), svm);
            }

            return summaryVMList;
        }

        /// <summary>
        /// 月内日別系列VMリストを取得する(日別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">月内の時間</param>
        /// <returns>月内日別系列VMリスト</returns>
        public async Task<ObservableCollection<SeriesViewModel>> LoadDailySeriesViewModelListWithinMonthAsync(BookIdObj bookId, DateOnly includedTime)
        {
            using FuncLog funcLog = new(new { bookId, includedTime });

            DateOnly startTime = includedTime.GetFirstDateOfMonth();
            DateOnly endTime = startTime.GetLastDateOfMonth();

            return await this.LoadDailySeriesViewModelListAsync(bookId, new(startTime, endTime));
        }

        /// <summary>
        /// 期間内日別系列VMリストを取得する(日別グラフタブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>日別系列VMリスト</returns>
        public async Task<ObservableCollection<SeriesViewModel>> LoadDailySeriesViewModelListAsync(BookIdObj bookId, PeriodObj<DateOnly> period)
        {
            using FuncLog funcLog = new(new { bookId, period });

            // 開始日までの収支を取得する
            decimal balance = 0;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = bookId == null
                    ? await endingBalanceInfoDao.Find(period.Start) // 全帳簿
                    : await endingBalanceInfoDao.FindByBookId(bookId.Value, period.Start); // 各帳簿
                balance = dto.EndingBalance;
            }

            // 系列データ
            ObservableCollection<SeriesViewModel> vmList = [
                new SeriesViewModel() {
                    OtherName = Properties.Resources.ListName_Balance,
                    Values = [],
                    Periods = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の日の分を取得する
            PeriodObj<DateOnly> tmpPeriod;
            {
                DateOnly tmpStartTime = period.Start;
                DateOnly tmpEndTime = tmpStartTime;
                tmpPeriod = new(tmpStartTime, tmpEndTime);
            }
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpPeriod);
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高
            vmList[0].Periods.Add(tmpPeriod);

            foreach (SummaryViewModel summaryVM in summaryVMList) {
                decimal value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [value],
                    Periods = [tmpPeriod],
                    Total = value,
                    Average = period.End < DateOnlyExtensions.Today ? value : 0 // 平均値は過去のデータのみで計算する
                };
                vmList.Add(vm);
            }
            if (period.End < DateOnly.FromDateTime(DateTime.Now)) {
                ++averageCount;
            }

            // 最初以外の日の分を取得する
            int days = period.End.DayNumber - period.Start.AddDays(-1).DayNumber;
            for (int i = 1; i < days; ++i) {
                {
                    DateOnly tmpStartTime = tmpPeriod.Start.AddDays(1);
                    DateOnly tmpEndTime = tmpStartTime;
                    tmpPeriod = new(tmpStartTime, tmpEndTime);
                }
                summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpPeriod);
                balance += summaryVMList[0].Total;
                vmList[0].Values.Add(balance); // 残高
                vmList[0].Periods.Add(tmpPeriod);
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    decimal value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);
                    vmList[j + 1].Periods.Add(tmpPeriod);

                    if (tmpPeriod.End.IsPost()) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Total += value;
                }
                if (tmpPeriod.End.IsPost()) {
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
        public async Task<ObservableCollection<SeriesViewModel>> LoadMonthlySeriesViewModelListWithinYearAsync(BookIdObj bookId, DateOnly includedTime, int startMonth)
        {
            using FuncLog funcLog = new(new { bookId, includedTime, startMonth });

            PeriodObj<DateOnly> period;
            {
                DateOnly startTime = includedTime.GetFirstDateOfFiscalYear(startMonth);
                DateOnly endTime = startTime.GetLastDateOfFiscalYear(startMonth);
                period = new(startTime, endTime);
            }

            // 開始日までの収支を取得する
            decimal balance = 0;
            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = bookId == null
                    ? await endingBalanceInfoDao.Find(period.Start) // 全帳簿
                    : await endingBalanceInfoDao.FindByBookId(bookId.Value, period.Start); // 各帳簿
                balance = dto.EndingBalance;
            }

            ObservableCollection<SeriesViewModel> vmList = [
                new SeriesViewModel() {
                    OtherName = Properties.Resources.ListName_Balance,
                    Values = [],
                    Periods = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する月数(先月まで)

            // 最初の月の分を取得する
            PeriodObj<DateOnly> tmpPeriod;
            {
                DateOnly tmpStartTime = period.Start;
                DateOnly tmpEndTime = tmpStartTime.GetLastDateOfMonth();
                tmpPeriod = new(tmpStartTime, tmpEndTime);
            }
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpPeriod);
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高
            vmList[0].Periods.Add(tmpPeriod);
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                decimal value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [value],
                    Periods = [tmpPeriod],
                    Total = value,
                    Average = tmpPeriod.End.IsPost() ? value : 0
                };
                vmList.Add(vm);
            }
            if (tmpPeriod.End.IsPost()) {
                ++averageCount;
            }

            // 最初以外の月の分を取得する
            int monthes = (period.End.Year * 12) + period.End.Month - ((period.Start.Year * 12) + period.Start.Month - 1);
            for (int i = 1; i < monthes; ++i) {
                {
                    DateOnly tmpStartTime = tmpPeriod.Start.AddMonths(1);
                    DateOnly tmpEndTime = tmpStartTime.GetLastDateOfMonth();
                    tmpPeriod = new(tmpStartTime, tmpEndTime);
                }
                summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpPeriod);
                balance += summaryVMList[0].Total;
                vmList[0].Values.Add(balance); // 残高
                vmList[0].Periods.Add(tmpPeriod);
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    decimal value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);
                    vmList[j + 1].Periods.Add(tmpPeriod);

                    if (tmpPeriod.End.IsPost()) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Total += value;
                }
                if (tmpPeriod.End.IsPost()) {
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
        public async Task<ObservableCollection<SeriesViewModel>> LoadYearlySeriesViewModelListWithinDecadeAsync(BookIdObj bookId, DateOnly startYear, int startMonth)
        {
            using FuncLog funcLog = new(new { bookId, startYear, startMonth });

            DateOnly startTime = startYear;

            // 開始日までの収支を取得する
            decimal balance = 0;
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
                    Periods = []
                }
            ];
            int averageCount = 0; // 平均値計算に使用する年数(去年まで)

            // 最初の年の分を取得する
            PeriodObj<DateOnly> tmpPeriod;
            {
                DateOnly tmpStartTime = startTime;
                DateOnly tmpEndTime = tmpStartTime.GetLastDateOfFiscalYear(startMonth);
                tmpPeriod = new(tmpStartTime, tmpEndTime);
            }
            ObservableCollection<SummaryViewModel> summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpPeriod);
            balance += summaryVMList[0].Total;
            vmList[0].Values.Add(balance); // 残高
            vmList[0].Periods.Add(tmpPeriod);
            foreach (SummaryViewModel summaryVM in summaryVMList) {
                decimal value = summaryVM.Total;
                SeriesViewModel vm = new(summaryVM) {
                    Values = [],
                    Periods = [],
                    Total = value,
                    Average = tmpPeriod.End.IsPost() ? value : 0
                };
                vm.Values.Add(value);
                vm.Periods.Add(tmpPeriod);
                vmList.Add(vm);
            }
            if (tmpPeriod.End.IsPost()) {
                ++averageCount;
            }

            // 最初以外の年の分を取得する
            int years = 10;
            for (int i = 1; i < years; ++i) {
                {
                    DateOnly tmpStartTime = tmpPeriod.Start.AddYears(1);
                    DateOnly tmpEndTime = tmpStartTime.GetLastDateOfFiscalYear(startMonth);
                    tmpPeriod = new(tmpStartTime, tmpEndTime);
                }

                summaryVMList = await this.LoadSummaryViewModelListAsync(bookId, tmpPeriod);
                balance += summaryVMList[0].Total;
                vmList[0].Values.Add(balance); // 残高
                vmList[0].Periods.Add(tmpPeriod);
                for (int j = 0; j < summaryVMList.Count; ++j) {
                    decimal value = summaryVMList[j].Total;

                    vmList[j + 1].Values.Add(value);
                    vmList[j + 1].Periods.Add(tmpPeriod);

                    if (tmpPeriod.End.IsPost()) {
                        vmList[j + 1].Average += value;
                    }
                    vmList[j + 1].Total += value;
                }
                if (tmpPeriod.End.IsPost()) {
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
        public async Task<ObservableCollection<BookModel>> UpdateBookCompListAsync()
        {
            using FuncLog funcLog = new();

            ObservableCollection<BookModel> bookCompVMList = [];

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                MstBookDao mstBookDao = new(dbHandler);
                var dtoList = await mstBookDao.FindIfJsonCodeExistsAsync();
                foreach (MstBookDto dto in dtoList) {
                    MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : new(dto.JsonCode);
                    if (jsonObj is null) { continue; }

                    BookModel vm = new(dto.BookId, dto.BookName) {
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
        public async Task<BookSettingViewModel> LoadBookSettingViewModelAsync(BookIdObj bookId)
        {
            using FuncLog funcLog = new(new { bookId });

            BookSettingViewModel vm = null;

            ViewModelService service = new(this.DbHandlerFactory);
            ObservableCollection<BookModel> vmList = await service.LoadBookListAsync(Properties.Resources.ListName_None);

            await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                // 帳簿一覧を取得する
                BookInfoDao bookInfoDao = new(dbHandler);
                var dto = await bookInfoDao.FindByBookId((int)bookId);

                MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : new(dto.JsonCode);

                vm = new BookSettingViewModel() {
                    Id = bookId,
                    SortOrder = dto.SortOrder,
                    InputedName = dto.BookName,
                    SelectedBookKind = (BookKind)dto.BookKind,
                    InputedRemark = jsonObj?.Remark ?? string.Empty,
                    InputedInitialValue = dto.InitialValue,
                    SelectedIfStartDateExists = jsonObj?.StartDate != null,
                    SelectedIfEndDateExists = jsonObj?.EndDate != null,
                    InputedPeriod = new(jsonObj?.StartDate?.ToDateOnly() ?? dto.StartDate?.ToDateOnly() ?? DateOnlyExtensions.Today, 
                                        jsonObj?.EndDate?.ToDateOnly() ?? dto.EndDate?.ToDateOnly() ?? DateOnlyExtensions.Today),
                    DebitBookVMList = new ObservableCollection<BookModel>(vmList.Where(tmpVM => tmpVM.Id != bookId)),
                    InputedPayDay = dto.PayDay,
                    InputedCsvFolderPath = jsonObj is null ? "" : PathUtil.GetSmartPath(App.GetCurrentDir(), jsonObj.CsvFolderPath),
                    TextEncodingList = GetTextEncodingList(),
                    SelectedTextEncoding = jsonObj?.TextEncoding ?? Encoding.UTF8.CodePage,
                    InputedActDateIndex = jsonObj?.CsvActDateIndex + 1,
                    InputedExpensesIndex = jsonObj?.CsvOutgoIndex + 1,
                    InputedItemNameIndex = jsonObj?.CsvItemNameIndex + 1,
                    RelationVMList = await LoadRelationViewModelListFromBookIdAsync(dbHandler, bookId)
                };
                vm.SelectedDebitBookVM = vm.DebitBookVMList.FirstOrElementAtOrDefault(tmpVM => (int?)tmpVM.Id == dto.DebitBookId, 0);
            }

            return vm;
        }

        /// <summary>
        /// 関連VMリスト(帳簿主体)を取得する
        /// </summary>
        /// <param name="dbHandler">DBハンドラ</param>
        /// <param name="bookId">帳簿ID</param>
        /// <returns>関連VMリスト</returns>
        private static async Task<ObservableCollection<RelationViewModel>> LoadRelationViewModelListFromBookIdAsync(DbHandlerBase dbHandler, BookIdObj bookId)
        {
            using FuncLog funcLog = new(new { bookId });

            ItemRelFromBookInfoDao itemRelFromBookInfoDao = new(dbHandler);
            var dtoList = await itemRelFromBookInfoDao.FindByBookIdAsync((int)bookId);

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
        /// 項目ツリーVMリストを取得する
        /// </summary>
        /// <returns>階層構造項目VMリスト</returns>
        public async Task<ObservableCollection<ItemTreeViewModel>> LoadItemTreeVMListAsync()
        {
            using FuncLog funcLog = new();

            ObservableCollection<ItemTreeViewModel> vmList = [
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
                foreach (ItemTreeViewModel balanceVM in vmList) {
                    // 分類
                    MstCategoryDao mstCategoryDao = new(dbHandler);
                    var cDtoList = await mstCategoryDao.FindByBalanceKindAsync((int)balanceVM.Id);

                    foreach (MstCategoryDto dto in cDtoList) {
                        balanceVM.ChildrenVMList.Add(new ItemTreeViewModel() {
                            Depth = (int)HierarchicalKind.Category,
                            Id = dto.CategoryId,
                            SortOrder = dto.SortOrder,
                            Name = dto.CategoryName,
                            ParentVM = balanceVM,
                            ChildrenVMList = []
                        });
                    }

                    // 項目
                    MstItemDao mstItemDao = new(dbHandler);
                    foreach (ItemTreeViewModel categoryVM in balanceVM.ChildrenVMList) {
                        var iDtoList = await mstItemDao.FindByCategoryIdAsync((int)categoryVM.Id);

                        foreach (MstItemDto dto in iDtoList) {
                            categoryVM.ChildrenVMList.Add(new ItemTreeViewModel() {
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
        /// 分類/項目設定VMを取得する
        /// </summary>
        /// <param name="kind">表示対象の階層種別</param>
        /// <param name="id">表示対象のID</param>
        /// <returns>分類/項目設定VM</returns>
        public async Task<ItemSettingViewModel> LoadItemSettingVMAsync(HierarchicalKind kind, IdObj id)
        {
            using FuncLog funcLog = new(new { kind, id });

            ItemSettingViewModel vm = null;

            switch (kind) {
                case HierarchicalKind.Balance: {
                    vm = new ItemSettingViewModel() {
                        Kind = HierarchicalKind.Balance,
                        Id = -1,
                        SortOrder = -1,
                        InputedName = string.Empty
                    };
                    break;
                }
                case HierarchicalKind.Category: {
                    // 分類
                    CategoryIdObj tmpId = new((int)id);
                    await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                        MstCategoryDao mstCategoryDao = new(dbHandler);
                        var dto = await mstCategoryDao.FindByIdAsync((int)tmpId);

                        vm = new ItemSettingViewModel() {
                            Kind = HierarchicalKind.Category,
                            Id = id,
                            SortOrder = dto.SortOrder,
                            InputedName = dto.CategoryName
                        };
                    }
                    break;
                }
                case HierarchicalKind.Item: {
                    // 項目
                    ItemIdObj tmpId = new((int)id);
                    await using (DbHandlerBase dbHandler = await this.DbHandlerFactory.CreateAsync()) {
                        MstItemDao mstItemDao = new(dbHandler);
                        var dto = await mstItemDao.FindByIdAsync((int)tmpId);

                        vm = new ItemSettingViewModel {
                            Kind = HierarchicalKind.Item,
                            Id = id,
                            SortOrder = dto.SortOrder,
                            InputedName = dto.ItemName,
                            RelationVMList = await LoadRelationViewModelListAsync(dbHandler, tmpId),
                            ShopVMList = await LoadShopViewModelListAsync(dbHandler, tmpId),
                            RemarkVMList = await LoadRemarkViewModelListAsync(dbHandler, tmpId)
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
        private static async Task<ObservableCollection<RelationViewModel>> LoadRelationViewModelListAsync(DbHandlerBase dbHandler, ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            BookRelFromItemInfoDao bookRelFromItemInfoDao = new(dbHandler);
            var dtoList = await bookRelFromItemInfoDao.FindByItemIdAsync((int)itemId);

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
        private static async Task<ObservableCollection<ShopViewModel>> LoadShopViewModelListAsync(DbHandlerBase dbHandler, ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            ObservableCollection<ShopViewModel> svmList = [];
            ShopInfoDao shopInfoDao = new(dbHandler);
            var dtoList = await shopInfoDao.FindByItemIdAsync((int)itemId);

            foreach (ShopInfoDto dto in dtoList) {
                ShopViewModel svm = new() {
                    Shop = new(dto.ShopName),
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
        private static async Task<ObservableCollection<RemarkViewModel>> LoadRemarkViewModelListAsync(DbHandlerBase dbHandler, ItemIdObj itemId)
        {
            using FuncLog funcLog = new(new { itemId });

            ObservableCollection<RemarkViewModel> rvmList = [];
            RemarkInfoDao remarkInfoDao = new(dbHandler);
            var dtoList = await remarkInfoDao.FindByItemIdAsync((int)itemId);

            foreach (RemarkInfoDto dto in dtoList) {
                RemarkViewModel rvm = new() {
                    Remark = new(dto.Remark),
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
