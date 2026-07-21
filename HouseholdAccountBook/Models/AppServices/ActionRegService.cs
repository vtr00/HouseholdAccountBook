using HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions;
using HouseholdAccountBook.Infrastructure.DB.DbDao.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Infrastructure.Utilities.Extensions;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.AppServices
{
    /// <summary>
    /// 帳簿項目登録サービス
    /// </summary>
    public class ActionRegService(DbHandlerFactory dbHandlerFactory)
    {
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory mDbHandlerFactory = dbHandlerFactory;

        #region 帳簿項目
        /// <summary>
        /// 帳簿項目Modelを取得する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <returns>帳簿項目Model</returns>
        public async Task<ActionModel> LoadActionAsync(ActionIdObj actionId)
        {
            using FuncLog funcLog = new(new { actionId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            ActionInfoDao dao = new(dbHandler);
            ActionInfoDto dto = await dao.FindByActionIdAsync((int)actionId, (int)UserSettingService.Instance.DefaultAssetId);

            ActionModel action = new() {
                Base = new(dto.ActionId, dto.ActTime, new(dto.OrgMainActValue, dto.OrgActAssetId)),
                AssetId = dto.AssetId ?? AssetIdObj.System,
                GroupId = dto.GroupId,
                Account = new(dto.BookId, dto.BookName),
                Category = new(dto.CategoryId, dto.CategoryName, 0 < Math.Sign(dto.OrgMainActValue) ? BalanceKind.Income : BalanceKind.Expenses),
                Item = new(dto.ItemId, dto.ItemName),
                Shop = new(dto.ShopName),
                Remark = new(dto.Remark),
                IsMatch = dto.IsMatch == 1
            };
            return action;
        }

        /// <summary>
        /// 繰り返しグループの繰り返し回数を取得する
        /// </summary>
        /// <param name="actionId">帳簿項目ID</param>
        /// <returns>繰り返し回数</returns>
        public async Task<int> LoadRepeatCountAsync(ActionIdObj actionId)
        {
            using FuncLog funcLog = new(new { actionId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            GroupInfoDao groupInfoDao = new(dbHandler);
            GroupInfoDto dto = await groupInfoDao.FindByActionIdAsync((int)actionId);

            GroupKind groupKind = EnumUtil.SafeCastEnum(dto.GroupKind, GroupKind.NotInOne);

            int count = 1;
            if (groupKind == GroupKind.Repeat) {
                HstActionDao hstActionDao = new(dbHandler);
                IEnumerable<HstActionDto> dtoList = await hstActionDao.FindInGroupAfterDateByIdAsync(actionId.Id);

                count = Math.Max(1, dtoList.Count());
            }
            return count;
        }

        /// <summary>
        /// 帳簿項目Modelを保存する
        /// </summary>
        /// <param name="action">帳簿項目Model</param>
        /// <param name="count">繰り返し回数</param>
        /// <param name="isLink">連動して変更するか</param>
        /// <param name="holidaySettingKind">休日設定種別</param>
        /// <returns>登録した帳簿項目ID</returns>
        public async Task<ActionIdObj> SaveActionAsync(ActionModel action, int count, bool isLink, HolidaySettingKind holidaySettingKind)
        {
            using FuncLog funcLog = new(new { action, count, isLink, holidaySettingKind });

            ActionIdObj resActionId = null;

            // 休日設定を考慮した日付を取得する関数
            static DateTime GetDateTimeWithHolidaySettingKind(HolidaySettingKind holidaySettingKind, DateTime tmpDateTime)
            {
                switch (holidaySettingKind) {
                    case HolidaySettingKind.BeforeHoliday:
                        while (HolidayService.Instance.IsNationalHoliday(tmpDateTime.ToDateOnly()) || tmpDateTime.DayOfWeek == DayOfWeek.Saturday || tmpDateTime.DayOfWeek == DayOfWeek.Sunday) {
                            tmpDateTime = tmpDateTime.AddDays(-1);
                        }
                        break;
                    case HolidaySettingKind.AfterHoliday:
                        while (HolidayService.Instance.IsNationalHoliday(tmpDateTime.ToDateOnly()) || tmpDateTime.DayOfWeek == DayOfWeek.Saturday || tmpDateTime.DayOfWeek == DayOfWeek.Sunday) {
                            tmpDateTime = tmpDateTime.AddDays(1);
                        }
                        break;
                }
                return tmpDateTime;
            }

            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();
            await dbHandler.ExecTransactionAsync(async () => {
                HstActionDao hstActionDao = new(dbHandler);
                HstGroupDao hstGroupDao = new(dbHandler);

                if (action.ActionId is null) {
                    #region 帳簿項目を追加する
                    // 新規グループIDを取得する
                    GroupIdObj assingedGroupId = count == 1 ? null : await hstGroupDao.InsertReturningIdAsync(new HstGroupDto { GroupKind = (int)GroupKind.Repeat });

                    // 帳簿項目を追加する
                    for (int i = 0; i < count; ++i) {
                        // 日付を取得する
                        DateTime tmpActTime = count == 1 ? action.ActTime : GetDateTimeWithHolidaySettingKind(holidaySettingKind, action.ActTime.AddMonths(i)); // 登録日付
                        ActionIdObj actionId = await hstActionDao.InsertReturningIdAsync(new HstActionDto {
                            BookId = (int)action.Account.Id,
                            ItemId = (int)action.Item.Id,
                            ActTime = tmpActTime,
                            ActValue = action.Amount.SubValue,
                            AssetId = null, // TODO: 将来の拡張用
                            ShopName = action.Shop,
                            GroupId = (int?)assingedGroupId,
                            Remark = action.Remark
                        });

                        // 繰り返しの最初の1回を選択するようにする
                        if (i == 0) {
                            resActionId = actionId;
                        }
                    }
                    #endregion
                }
                else {
                    #region 帳簿項目を編集する
                    if (count == 1) {
                        #region 繰返し回数が1回
                        if (action.GroupId == null) {
                            #region グループに属していない
                            _ = await hstActionDao.UpdateAsync(new HstActionDto {
                                BookId = (int)action.Account.Id,
                                ItemId = (int)action.Item.Id,
                                ActTime = action.ActTime,
                                ActValue = action.Amount.SubValue,
                                AssetId = null, // TODO: 将来の拡張用
                                ShopName = action.Shop,
                                GroupId = null,
                                Remark = action.Remark,
                                IsMatch = action.IsMatch ? 1 : 0,
                                ActionId = (int)action.ActionId
                            });
                            #endregion
                        }
                        else {
                            #region グループに属している
                            // この帳簿項目以降の繰返し分のレコードを削除する
                            _ = await hstActionDao.DeleteInGroupAfterDateByIdAsync((int)action.ActionId, false);

                            // グループに属する項目の個数を調べる
                            IEnumerable<HstActionDto> dtoList = await hstActionDao.FindByGroupIdAsync((int)action.GroupId);

                            if (dtoList.Count() <= 1) {
                                #region グループに属する項目が1項目以下
                                // この帳簿項目のグループIDをクリアする
                                _ = await hstActionDao.UpdateAsync(new HstActionDto {
                                    BookId = (int)action.Account.Id,
                                    ItemId = (int)action.Item.Id,
                                    ActTime = action.ActTime,
                                    ActValue = action.Amount.SubValue,
                                    AssetId = null, // TODO: 将来の拡張用
                                    ShopName = action.Shop,
                                    GroupId = null,
                                    Remark = action.Remark,
                                    IsMatch = action.IsMatch ? 1 : 0,
                                    ActionId = (int)action.ActionId
                                });

                                // グループを削除する
                                _ = await hstGroupDao.DeleteByIdAsync((int)action.GroupId);
                                #endregion
                            }
                            else {
                                #region グループに属する項目が2項目以上
                                // この帳簿項目のグループIDをクリアせずに残す(過去分と同じグループIDになる)
                                _ = await hstActionDao.UpdateWithoutIsMatchAsync(new HstActionDto {
                                    BookId = (int)action.Account.Id,
                                    ItemId = (int)action.Item.Id,
                                    ActTime = action.ActTime,
                                    ActValue = action.Amount.SubValue,
                                    AssetId = null, // TODO: 将来の拡張用
                                    ShopName = action.Shop,
                                    GroupId = (int?)action.GroupId,
                                    Remark = action.Remark,
                                    IsMatch = 0, // 変更しない
                                    ActionId = (int)action.ActionId
                                });
                                #endregion
                            }
                            #endregion
                        }
                        #endregion
                    }
                    else {
                        #region 繰返し回数が2回以上
                        List<ActionIdObj> actionIdList = [];

                        GroupIdObj assignedGroupId = action.GroupId;
                        if (assignedGroupId == null) {
                            #region グループIDが未割当て
                            // グループIDを取得する
                            assignedGroupId = await hstGroupDao.InsertReturningIdAsync(new HstGroupDto { GroupKind = (int)GroupKind.Repeat });
                            actionIdList.Add(action.ActionId);
                            #endregion
                        }
                        else {
                            #region グループIDが割当て済
                            // 変更の対象となる帳簿項目を洗い出す
                            IEnumerable<HstActionDto> dtoList = await hstActionDao.FindInGroupAfterDateByIdAsync((int)action.ActionId);
                            dtoList.Select(dto => dto.ActionId).ToList().ForEach(id => actionIdList.Add(id));
                            #endregion
                        }

                        DateTime tmpActTime = GetDateTimeWithHolidaySettingKind(holidaySettingKind, action.ActTime);

                        // この帳簿項目にだけis_matchを反映する
                        Debug.Assert(actionIdList[0] == action.ActionId);
                        _ = await hstActionDao.UpdateAsync(new HstActionDto {
                            BookId = (int)action.Account.Id,
                            ItemId = (int)action.Item.Id,
                            ActTime = tmpActTime,
                            ActValue = action.Amount.SubValue,
                            AssetId = null, // TODO: 将来の拡張用
                            ShopName = action.Shop,
                            GroupId = (int?)assignedGroupId,
                            Remark = action.Remark,
                            IsMatch = action.IsMatch ? 1 : 0,
                            ActionId = (int)action.ActionId
                        });

                        // 既存のレコードを更新する
                        for (int i = 1; i < actionIdList.Count; ++i) {
                            ActionIdObj targetActionId = actionIdList[i];
                            // 日付を取得する
                            tmpActTime = GetDateTimeWithHolidaySettingKind(holidaySettingKind, action.ActTime.AddMonths(i)); // 登録日付

                            if (i < count) { // 繰返し回数の範囲内のレコードを更新する
                                if (isLink) { // 連動して編集時のみ変更する
                                    _ = await hstActionDao.UpdateWithoutIsMatchAsync(new HstActionDto {
                                        BookId = (int)action.Account.Id,
                                        ItemId = (int)action.Item.Id,
                                        ActTime = tmpActTime,
                                        ActValue = action.Amount.SubValue,
                                        AssetId = null, // TODO: 将来の拡張用
                                        ShopName = action.Shop,
                                        GroupId = (int?)assignedGroupId,
                                        Remark = action.Remark,
                                        IsMatch = 0, // 変更しない
                                        ActionId = (int)targetActionId
                                    });
                                }
                            }
                            else { // 繰返し回数が帳簿項目数を下回っていた場合に、越えたレコードを削除する
                                _ = await hstActionDao.DeleteByIdAsync((int)targetActionId);
                            }
                        }

                        // 繰返し回数が帳簿項目数を越えていた場合に、新規レコードを追加する
                        for (int i = actionIdList.Count; i < count; ++i) {
                            // 日付を取得する
                            tmpActTime = GetDateTimeWithHolidaySettingKind(holidaySettingKind, action.ActTime.AddMonths(i));

                            _ = await hstActionDao.InsertReturningIdAsync(new HstActionDto {
                                BookId = (int)action.Account.Id,
                                ItemId = (int)action.Item.Id,
                                ActTime = tmpActTime,
                                ActValue = action.Amount.SubValue,
                                AssetId = null, // TODO: 将来の拡張用
                                ShopName = action.Shop,
                                GroupId = (int?)assignedGroupId,
                                Remark = action.Remark,
                                IsMatch = 0
                            });
                        }
                        #endregion
                    }

                    resActionId = action.ActionId;
                    #endregion
                }
            });

            return resActionId;
        }
        #endregion

        #region 帳簿項目リスト
        /// <summary>
        /// 帳簿項目Modelリストを取得する
        /// </summary>
        /// <param name="groupId">グループID</param>
        /// <returns>帳簿項目Modelリスト</returns>
        public async Task<IEnumerable<ActionModel>> LoadActionListAsync(GroupIdObj groupId)
        {
            using FuncLog funcLog = new(new { groupId });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            ActionInfoDao dao = new(dbHandler);
            IEnumerable<ActionInfoDto> dtoList = await dao.FindByGroupIdAsync((int)groupId, (int)UserSettingService.Instance.DefaultAssetId);

            List<ActionModel> actionList = [];
            foreach (ActionInfoDto dto in dtoList) {
                ActionModel action = new() {
                    Base = new(dto.ActionId, dto.ActTime, new(dto.OrgMainActValue, dto.OrgActAssetId)),
                    AssetId = dto.AssetId ?? AssetIdObj.System,
                    GroupId = groupId,
                    Account = new(dto.BookId, dto.BookName),
                    Category = new(dto.CategoryId, dto.CategoryName, 0 < Math.Sign(dto.OrgMainActValue) ? BalanceKind.Income : BalanceKind.Expenses),
                    Item = new(dto.ItemId, dto.ItemName),
                    Shop = new(dto.ShopName),
                    Remark = new(dto.Remark)
                };
                actionList.Add(action);
            }

            return actionList;
        }

        /// <summary>
        /// 帳簿項目Modelリストを保存する
        /// </summary>
        /// <param name="actionList">帳簿項目Modelリスト</param>
        /// <returns>帳簿項目ID</returns>
        public async Task<IEnumerable<ActionIdObj>> SaveActionListAsync(IEnumerable<ActionModel> actionList)
        {
            using FuncLog funcLog = new(new { actionList });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            List<ActionIdObj> resActionIdList = [];
            if (actionList.First().GroupId is null) {
                #region 帳簿項目を追加する
                await dbHandler.ExecTransactionAsync(async () => {
                    // グループIDを取得する
                    HstGroupDao hstGroupDao = new(dbHandler);
                    GroupIdObj assignedGroupId = await hstGroupDao.InsertReturningIdAsync(new HstGroupDto { GroupKind = (int)GroupKind.ListReg });

                    HstActionDao hstActionDao = new(dbHandler);
                    foreach (ActionModel action in actionList) {
                        int id = await hstActionDao.InsertReturningIdAsync(new HstActionDto {
                            BookId = (int)action.Account.Id,
                            ItemId = (int)action.Item.Id,
                            ActTime = action.ActTime,
                            ActValue = action.Amount.SubValue,
                            AssetId = null, // TODO: 将来の拡張用
                            ShopName = action.Shop,
                            GroupId = (int?)assignedGroupId,
                            Remark = action.Remark
                        });
                        resActionIdList.Add(id);
                    }
                });
                #endregion
            }
            else {
                #region 帳簿項目を編集する
                await dbHandler.ExecTransactionAsync(async () => {
                    HstActionDao hstActionDao = new(dbHandler);
                    foreach (ActionModel action in actionList) {
                        if (action.ActionId is not null) {
                            _ = await hstActionDao.UpdateWithoutIsMatchAsync(new HstActionDto {
                                BookId = (int)action.Account.Id,
                                ItemId = (int)action.Item.Id,
                                ActTime = action.ActTime,
                                ActValue = action.Amount.SubValue,
                                AssetId = null, // TODO: 将来の拡張用
                                ShopName = action.Shop,
                                Remark = action.Remark,
                                GroupId = (int?)action.GroupId,
                                ActionId = (int)action.ActionId
                            });

                            resActionIdList.Add(action.ActionId);
                        }
                        else {
                            ActionIdObj actionId = await hstActionDao.InsertReturningIdAsync(new HstActionDto {
                                BookId = (int)action.Account.Id,
                                ItemId = (int)action.Item.Id,
                                ActTime = action.ActTime,
                                ActValue = action.Amount.SubValue,
                                AssetId = null, // TODO: 将来の拡張用
                                ShopName = action.Shop,
                                Remark = action.Remark,
                                GroupId = (int?)action.GroupId
                            });
                            resActionIdList.Add(actionId);
                        }
                    }

                    // UI上から削除された帳簿項目を削除する
                    IEnumerable<HstActionDto> dtoList = await hstActionDao.FindByGroupIdAsync((int)actionList.First().GroupId);
                    IEnumerable<int> expected = dtoList.Select(dto => dto.ActionId).Except(resActionIdList.Select(tmp => (int)tmp));
                    foreach (int actionId in expected) {
                        _ = await hstActionDao.DeleteByIdAsync(actionId);
                    }
                });
                #endregion
            }

            return resActionIdList;
        }
        #endregion

        #region 移動
        /// <summary>
        /// 移動帳簿項目Modelを取得する
        /// </summary>
        /// <param name="groupId">グループID</param>
        /// <returns>移動元, 移動先, 手数料 帳簿項目Model</returns>
        public async Task<(ActionModel, ActionModel, ActionModel)> LoadMoveActionsAsync(GroupIdObj groupId)
        {
            using FuncLog funcLog = new(new { groupId });

            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            MoveActionInfoDao moveActionInfoDao = new(dbHandler);
            // 移動元、移動先、手数料の順に並び替え
            IEnumerable<MoveActionInfoDto> dtoList = await moveActionInfoDao.GetAllAsync((int)UserSettingService.Instance.DefaultAssetId, (int)groupId);
            dtoList = dtoList.OrderBy(dto => dto.MainActValue).OrderBy(dto => -dto.MoveFlg);

            List<ActionModel> actionList = [];
            foreach (MoveActionInfoDto dto in dtoList) {
                ActionModel action = new() {
                    Base = new(dto.ActionId, dto.ActTime, new(dto.MainActValue, dto.ActAssetId)),
                    AssetId = dto.AssetId ?? AssetIdObj.System,
                    GroupId = groupId,
                    Account = new(dto.BookId, string.Empty),
                    Category = null,
                    Item = new(dto.ItemId, string.Empty),
                    Shop = null,
                    Remark = new(dto.Remark)
                };
                actionList.Add(action);
            }
            ActionModel srcAction = actionList[0];
            ActionModel dstAction = actionList[1];
            ActionModel feeAction = actionList.ElementAtOrDefault(2);

            return (srcAction, dstAction, feeAction);
        }

        /// <summary>
        /// 移動帳簿項目Modelを保存する
        /// </summary>
        /// <param name="srcAction">移動元帳簿項目Model</param>
        /// <param name="dstAction">移動先帳簿項目Model</param>
        /// <param name="feeAction">手数料帳簿項目Model</param>
        /// <returns>対象の帳簿項目ID</returns>
        public async Task<IEnumerable<ActionIdObj>> SaveMoveActionsAsync(ActionModel srcAction, ActionModel dstAction, ActionModel feeAction)
        {
            using FuncLog funcLog = new(new { srcAction, dstAction, feeAction });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            List<ActionIdObj> resActionIdList = [];
            await dbHandler.ExecTransactionAsync(async () => {
                GroupIdObj assignedGroupId = srcAction.GroupId;

                if (srcAction.ActionId is null) {
                    #region 帳簿項目を追加する
                    // グループIDを取得する
                    HstGroupDao hstGroupDao = new(dbHandler);
                    assignedGroupId = await hstGroupDao.InsertReturningIdAsync(new HstGroupDto { GroupKind = (int)GroupKind.Move });

                    // 移動元
                    HstActionDao hstActionDao = new(dbHandler);
                    ActionIdObj srcActionId = await hstActionDao.InsertMoveActionReturningIdAsync(new HstActionDto {
                        BookId = (int)srcAction.Account.Id,
                        ActTime = srcAction.ActTime,
                        ActValue = srcAction.Amount.SubValue,
                        AssetId = null, // TODO: 将来の拡張用
                        GroupId = (int?)assignedGroupId
                    }, (int)BalanceKind.Expenses);
                    resActionIdList.Add(srcActionId);

                    // 移動先
                    ActionIdObj dstActionId = await hstActionDao.InsertMoveActionReturningIdAsync(new HstActionDto {
                        BookId = (int)dstAction.Account.Id,
                        ActTime = dstAction.ActTime,
                        ActValue = dstAction.Amount.SubValue,
                        AssetId = null, // TODO: 将来の拡張用
                        GroupId = (int?)assignedGroupId
                    }, (int)BalanceKind.Income);
                    resActionIdList.Add(dstActionId);
                    #endregion
                }
                else {
                    #region 帳簿項目を変更する
                    // 移動元
                    HstActionDao hstActionDao = new(dbHandler);
                    _ = await hstActionDao.UpdateMoveActionAsync(new HstActionDto {
                        BookId = (int)srcAction.Account.Id,
                        ActTime = srcAction.ActTime,
                        ActValue = srcAction.Amount.SubValue,
                        AssetId = null, // TODO: 将来の拡張用
                        ActionId = (int)srcAction.ActionId
                    });
                    resActionIdList.Add(srcAction.ActionId);

                    // 移動先
                    _ = await hstActionDao.UpdateMoveActionAsync(new HstActionDto {
                        BookId = (int)dstAction.Account.Id,
                        ActTime = dstAction.ActTime,
                        ActValue = dstAction.Amount.SubValue,
                        AssetId = null, // TODO: 将来の拡張用
                        ActionId = (int)dstAction.ActionId
                    });
                    resActionIdList.Add(dstAction.ActionId);
                    #endregion
                }

                if (feeAction.Amount.MainValue != 0) {
                    #region 手数料あり
                    if (feeAction.ActionId is null) {
                        // 手数料が未登録のとき追加する
                        HstActionDao hstActionDao = new(dbHandler);
                        ActionIdObj feeActionId = await hstActionDao.InsertReturningIdAsync(new HstActionDto {
                            BookId = (int)feeAction.Account.Id,
                            ItemId = (int)feeAction.Item.Id,
                            ActTime = feeAction.ActTime,
                            ActValue = feeAction.Amount.SubValue,
                            AssetId = null, // TODO: 将来の拡張用
                            Remark = feeAction.Remark,
                            GroupId = (int?)assignedGroupId
                        });
                        resActionIdList.Add(feeActionId);
                    }
                    else {
                        // 手数料が登録済のとき更新する
                        HstActionDao hstActionDao = new(dbHandler);
                        _ = await hstActionDao.UpdateAsync(new HstActionDto {
                            BookId = (int)feeAction.Account.Id,
                            ItemId = (int)feeAction.Item.Id,
                            ActTime = feeAction.ActTime,
                            ActValue = feeAction.Amount.SubValue,
                            AssetId = null, // TODO: 将来の拡張用
                            Remark = feeAction.Remark,
                            GroupId = (int?)assignedGroupId,
                            ActionId = (int)feeAction.ActionId
                        });
                        resActionIdList.Add(feeAction.ActionId);
                    }
                    #endregion
                }
                else {
                    #region 手数料なし
                    if (feeAction.ActionId is not null) {
                        HstActionDao hstActionDao = new(dbHandler);
                        _ = await hstActionDao.DeleteByIdAsync((int)feeAction.ActionId);
                    }
                    #endregion
                }
            });

            return resActionIdList;
        }
        #endregion

        /// <summary>
        /// 店舗情報を保存する
        /// </summary>
        /// <param name="shop">店舗情報</param>
        /// <returns></returns>
        public async Task SaveShopAsync(ShopModel shop)
        {
            using FuncLog funcLog = new(new { shop });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            // 店舗を追加/編集する
            HstShopDao hstShopDao = new(dbHandler);
            _ = await hstShopDao.UpsertAsync(new HstShopDto {
                ItemId = (int)shop.ItemId,
                ShopName = shop.Name,
                UsedTime = shop.CurrentActTime ?? DateTime.MinValue
            });
        }

        /// <summary>
        /// 備考情報を保存する
        /// </summary>
        /// <param name="remark">備考情報</param>
        /// <returns></returns>
        public async Task SaveRemarkAsync(RemarkModel remark)
        {
            using FuncLog funcLog = new(new { remark });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            // 備考を追加/編集する
            HstRemarkDao hstRemarkDao = new(dbHandler);
            _ = await hstRemarkDao.UpsertAsync(new HstRemarkDto {
                ItemId = (int)remark.ItemId,
                Remark = remark.Remark,
                UsedTime = remark.CurrentActTime ?? DateTime.MinValue
            });
        }
    }
}
