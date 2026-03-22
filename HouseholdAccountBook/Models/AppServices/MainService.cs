using HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions;
using HouseholdAccountBook.Infrastructure.DB.DbDao.DbTable;
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
    /// <summary>
    /// メインサービス
    /// </summary>
    /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
    public class MainService(DbHandlerFactory dbHandlerFactory)
    {
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory mDbHandlerFactory = dbHandlerFactory;

        /// <summary>
        /// 帳簿項目を削除する
        /// </summary>
        /// <param name="targetList">削除対象の帳簿項目IDとグループIDのペア</param>
        /// <returns></returns>
        public async Task DeleteAction(IEnumerable<(ActionIdObj, GroupIdObj)> targetList)
        {
            using FuncLog funcLog = new(new { targetList });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            await dbHandler.ExecTransactionAsync(async () => {
                HstActionDao hstActionDao = new(dbHandler);
                HstGroupDao hstGroupDao = new(dbHandler);

                List<GroupIdObj> groupIdList = [];
                // 帳簿項目IDが0を超える項目についてループ
                foreach ((ActionIdObj, GroupIdObj) target in targetList) {
                    ActionIdObj actionId = target.Item1;
                    GroupIdObj groupId = target.Item2;

                    // 対象となる帳簿項目を削除する
                    _ = await hstActionDao.DeleteByIdAsync((int)actionId);

                    // グループIDがない場合は次の項目へ
                    if (groupId is null) { continue; }

                    var groupDto = await hstGroupDao.FindByIdAsync((int)groupId);
                    groupIdList.Add((int)groupId);
                    int groupKind = groupDto.GroupKind;

                    switch (groupKind) {
                        case (int)GroupKind.Move: {
                            // 移動の場合、削除項目と同じグループIDを持つ帳簿項目を削除する
                            _ = await hstActionDao.DeleteByGroupIdAsync((int)groupId);
                        }
                        break;
                        case (int)GroupKind.Repeat: {
                            // 繰返しの場合、削除項目の日時以降の同じグループIDを持つ帳簿項目を削除する
                            _ = await hstActionDao.DeleteInGroupAfterDateByIdAsync((int)actionId, false);
                        }
                        break;
                    }

                    // 削除対象と同じグループIDを持つ帳簿項目が1つだけの場合にグループIDをクリアする(移動以外の場合に該当する)
                    var actionDtoList = await hstActionDao.FindByGroupIdAsync((int)groupId);
                    if (actionDtoList.Count() == 1) {
                        _ = await hstActionDao.ClearGroupIdByIdAsync(actionDtoList.First().ActionId);
                    }
                }

                foreach (GroupIdObj groupId in groupIdList) {
                    // 同じグループIDを持つ帳簿項目が存在しなくなる場合にグループを削除する
                    var actionDtoList = await hstActionDao.FindByGroupIdAsync((int)groupId);
                    if (!actionDtoList.Any()) {
                        _ = await hstGroupDao.DeleteByIdAsync((int)groupId);
                    }
                }
            });
        }

        /// <summary>
        /// 繰越残高を取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="startDate">開始日付</param>
        /// <returns>繰越残高</returns>
        public async Task<decimal> LoadEndingBalance(BookIdObj bookId, DateOnly startDate)
        {
            using FuncLog funcLog = new(new { bookId, startDate });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            EndingBalanceInfoDao endingBalanceInfoDao = new(dbHandler);
            EndingBalanceInfoDto dto = bookId == -1
                ? await endingBalanceInfoDao.Find(startDate) // 全帳簿の繰越残高
                : await endingBalanceInfoDao.FindByBookId(bookId.Value, startDate); // 各帳簿の繰越残高
            decimal balance = dto.EndingBalance;

            return balance;
        }

        /// <summary>
        /// 月内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">月内の時刻</param>
        /// <returns>帳簿項目VMリスト</returns>
        public async Task<IEnumerable<ActionWithBalanceModel>> LoadActionListAsync(BookIdObj bookId, DateOnly includedTime)
        {
            using FuncLog funcLog = new(new { bookId, includedTime });

            DateOnly startTime = includedTime.GetFirstDateOfMonth();
            DateOnly endTime = startTime.GetLastDateOfMonth();
            return await this.LoadActionListAsync(bookId, (PeriodObj<DateOnly>)new(startTime, endTime));
        }

        /// <summary>
        /// 期間内帳簿項目VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="period">期間</param>
        /// <returns>帳簿項目VMリスト</returns>
        public async Task<IEnumerable<ActionWithBalanceModel>> LoadActionListAsync(BookIdObj bookId, PeriodObj<DateOnly> period)
        {
            using FuncLog funcLog = new(new { bookId, period });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            List<ActionWithBalanceModel> amList = [];
            decimal balance = await this.LoadEndingBalance(bookId, period.Start);

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
            IEnumerable<ActionInfoDto> dtoList = bookId == -1
                ? await actionInfoDao.FindAllWithinTerm(period.Start, period.End) // 全帳簿項目
                : await actionInfoDao.FindByBookIdWithinTerm(bookId.Value, period.Start, period.End); // 各帳簿項目

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

            return amList;
        }

        /// <summary>
        /// 帳簿項目内の店舗名と備考を更新する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <param name="shopName">店舗名</param>
        /// <param name="remark">備考</param>
        /// <returns></returns>
        public async Task UpdateShopNameAndRemarkInActionAsync(ActionIdObj actionId, string shopName, string remark)
        {
            using FuncLog funcLog = new(new { actionId, shopName, remark });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            HstActionDao hstActionDao = new(dbHandler);
            _ = await hstActionDao.UpdateShopNameAndRemarkByIdAsync(actionId.Value, shopName, remark);
        }

        /// <summary>
        /// 帳簿項目の日付を取得する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <returns>日付</returns>
        public async Task<DateOnly> LoadActDateAsync(ActionIdObj actionId)
        {
            using FuncLog funcLog = new(new { actionId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            HstActionDao hstActionDao = new(dbHandler);
            var dto = await hstActionDao.FindByIdAsync(actionId.Value);
            DateOnly actDate = DateOnly.FromDateTime(dto.ActTime);

            return actDate;
        }

        /// <summary>
        /// 月内概要VMリストを取得する(帳簿タブ)
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="includedTime">月内の時間</param>
        /// <returns>概要VMリスト</returns>
        public async Task<IEnumerable<SummaryModel>> LoadSummaryListAsync(BookIdObj bookId, DateOnly includedTime)
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
        /// <param name="period">期間</param>
        /// <returns>概要VMリスト</returns>
        public async Task<IEnumerable<SummaryModel>> LoadSummaryListAsync(BookIdObj bookId, PeriodObj<DateOnly> period)
        {
            using FuncLog funcLog = new(new { bookId, period });

            List<SummaryModel> smList = [];
            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
                SummaryInfoDao summaryInfoDao = new(dbHandler);
                IEnumerable<SummaryInfoDto> dtoList = bookId == -1
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
