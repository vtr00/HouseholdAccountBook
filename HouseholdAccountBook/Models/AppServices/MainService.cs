using HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.AppServices
{
    public class MainService(DbHandlerFactory dbHandlerFactory)
    {
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory mDbHandlerFactory = dbHandlerFactory;

        /// <summary>
        /// 繰越残高を取得する
        /// </summary>
        /// <param name="targetBookId">帳簿ID</param>
        /// <param name="startDate">開始日付</param>
        /// <returns>繰越残高</returns>
        public async Task<decimal> LoadEndingBalance(BookIdObj targetBookId, DateOnly startDate)
        {
            decimal balance;
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
                EndingBalanceInfoDto dto = targetBookId == null
                    ? await endingBalanceInfoDao.Find(startDate) // 全帳簿の繰越残高
                    : await endingBalanceInfoDao.FindByBookId(targetBookId.Value, startDate); // 各帳簿の繰越残高
                balance = dto.EndingBalance;
            }
            return balance;
        }

        /// <summary>
        /// 月内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="targetBookId">帳簿ID</param>
        /// <param name="includedTime">月内の時刻</param>
        /// <returns>帳簿項目VMリスト</returns>
        public async Task<List<ActionWithBalanceModel>> LoadActionListAsync(BookIdObj targetBookId, DateOnly includedTime)
        {
            using FuncLog funcLog = new(new { targetBookId, includedTime });

            DateOnly startTime = includedTime.GetFirstDateOfMonth();
            DateOnly endTime = startTime.GetLastDateOfMonth();
            return await this.LoadActionListAsync(targetBookId, (PeriodObj<DateOnly>)new(startTime, endTime));
        }

        /// <summary>
        /// 期間内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="targetBookId">帳簿ID</param>
        /// <param name="period">期間</param>
        /// <returns>帳簿項目VMリスト</returns>
        public async Task<List<ActionWithBalanceModel>> LoadActionListAsync(BookIdObj targetBookId, PeriodObj<DateOnly> period)
        {
            using FuncLog funcLog = new(new { targetBookId, period });

            List<ActionWithBalanceModel> amList = [];
            decimal balance = await this.LoadEndingBalance(targetBookId, period.Start);

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                // 繰越残高を追加
                {
                    ActionWithBalanceModel am = new() {
                        Action = new() {
                            Book = new(-1, string.Empty),
                            Category = new(-1, string.Empty, BalanceKind.Others),
                            Item = new(-1, Properties.Resources.ListName_CarryForward),
                            Base = new(-1, period.Start.ToDateTime(TimeOnly.MinValue), 0),
                            Shop = null,
                            Remark = null,
                            IsMatch = false
                        },
                        Balance = balance
                    };
                    amList.Add(am);
                }

                ActionInfoDao actionInfoDao = new(dbHandler);
                IEnumerable<ActionInfoDto> dtoList = targetBookId == null
                    ? await actionInfoDao.FindAllWithinTerm(period.Start, period.End) // 全帳簿項目
                    : await actionInfoDao.FindByBookIdWithinTerm(targetBookId.Value, period.Start, period.End); // 各帳簿項目

                foreach (ActionInfoDto aDto in dtoList) {
                    balance += aDto.ActValue;

                    ActionWithBalanceModel am = new() {
                        Action = new() {
                            GroupId = aDto.GroupId,
                            Book = new(aDto.BookId, aDto.BookName),
                            Category = new(aDto.CategoryId, aDto.CategoryName, aDto.ActValue < 0 ? BalanceKind.Expenses : BalanceKind.Income),
                            Item = new(aDto.ItemId, aDto.ItemName),
                            Base = new(aDto.ActionId, aDto.ActTime, aDto.ActValue),
                            Shop = new(aDto.ShopName),
                            Remark = new(aDto.Remark),
                            IsMatch = aDto.IsMatch == 1
                        },
                        Balance = balance
                    };
                    amList.Add(am);
                }
            }

            return amList;
        }

        /// <summary>
        /// 月内概要VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">月内の時間</param>
        /// <returns>概要VMリスト</returns>
        public async Task<List<SummaryModel>> LoadSummaryListAsync(BookIdObj bookId, DateOnly includedTime)
        {
            using FuncLog funcLog = new(new { bookId, includedTime });

            DateOnly startTime = includedTime.GetFirstDateOfMonth();
            DateOnly endTime = startTime.GetLastDateOfMonth();
            return await this.LoadSummaryListAsync(bookId, (PeriodObj<DateOnly>)new(startTime, endTime));
        }

        /// <summary>
        /// 期間内概要VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endTime">終了時刻</param>
        /// <returns>概要VMリスト</returns>
        public async Task<List<SummaryModel>> LoadSummaryListAsync(BookIdObj bookId, PeriodObj<DateOnly> period)
        {
            using FuncLog funcLog = new(new { bookId, period });

            List<SummaryModel> smList = [];
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                SummaryInfoDao summaryInfoDao = new(dbHandler);
                IEnumerable<SummaryInfoDto> dtoList = bookId == null
                    ? await summaryInfoDao.FindAllWithinPeriod(period.Start, period.End)
                    : await summaryInfoDao.FindByBookIdWithinPeriod(bookId.Value, period.Start, period.End);

                foreach (SummaryInfoDto dto in dtoList) {
                    smList.Add(new() {
                        Category = new(dto.CategoryId, dto.CategoryName, (BalanceKind)dto.BalanceKind),
                        Item = new(dto.ItemId, dto.ItemName),
                        Total = dto.Total
                    });
                }
            }

            // 差引損益
            decimal total = smList.Sum(obj => obj.Total);
            // 収入/支出
            List<SummaryModel> totalAsBalanceKindList = [];
            // 分類小計
            List<SummaryModel> totalAsCategoryList = [];

            // 収支別に計算する
            foreach (var g1 in smList.GroupBy(obj => obj.Category.BalanceKind)) {
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
            smList.Insert(0, new() {
                OtherName = Properties.Resources.ListName_profitAndLoss,
                Total = total
            });
            // 収入/支出の小計を追加する
            foreach (SummaryModel svm in totalAsBalanceKindList) {
                smList.Insert(smList.IndexOf(smList.First(obj => obj.Category.BalanceKind == svm.Category.BalanceKind)), svm);
            }
            // 分類別の小計を追加する
            foreach (SummaryModel svm in totalAsCategoryList) {
                smList.Insert(smList.IndexOf(smList.First(obj => obj.Category.Id == svm.Category.Id)), svm);
            }

            return smList;
        }
    }
}
