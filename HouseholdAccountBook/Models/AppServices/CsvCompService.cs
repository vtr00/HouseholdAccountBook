using HouseholdAccountBook.Infrastructure.CSV;
using HouseholdAccountBook.Infrastructure.DB.DbDao.Compositions;
using HouseholdAccountBook.Infrastructure.DB.DbDao.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.Others;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.AppServices
{
    /// <summary>
    /// CSV比較サービス
    /// </summary>
    /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
    public class CsvCompService(DbHandlerFactory dbHandlerFactory)
    {
        /// <summary>
        /// DBハンドラファクトリ
        /// </summary>
        private readonly DbHandlerFactory mDbHandlerFactory = dbHandlerFactory;

        /// <summary>
        /// 帳簿Model(比較用)を取得する
        /// </summary>
        /// <returns>帳簿Model</returns>
        public async Task<IEnumerable<AccountModel>> UpdateAccountCompListAsync()
        {
            using FuncLog funcLog = new();
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            List<AccountModel> amList = [];

            MstBookDao mstBookDao = new(dbHandler);
            IEnumerable<MstBookDto> dtoList = await mstBookDao.FindIfJsonCodeExistsAsync();
            foreach (MstBookDto dto in dtoList) {
                MstBookDto.JsonDto jsonObj = dto.JsonCode == null ? null : new(dto.JsonCode);
                if (jsonObj is null) { continue; }

                AccountModel vm = new(dto.BookId, dto.BookName) {
                    CsvFolderPath = jsonObj.CsvFolderPath == string.Empty ? null : jsonObj.CsvFolderPath,
                    TextEncoding = jsonObj.TextEncoding,
                    ActDateIndex = jsonObj.CsvActDateIndex + 1,
                    ExpensesIndex = jsonObj.CsvOutgoIndex + 1,
                    ItemNameIndex = jsonObj.CsvItemNameIndex + 1
                };
                if (vm.CsvFolderPath == null || vm.ActDateIndex == null || vm.ExpensesIndex == null || vm.ItemNameIndex == null) { continue; }

                amList.Add(vm);
            }

            return amList;
        }

        /// <summary>
        /// 帳簿項目CSV Modelリストを取得する
        /// </summary>
        /// <param name="csvFilePathList">読み込むCSVファイルのパスリスト</param>
        /// <param name="actDateIndex">日付インデックス</param>
        /// <param name="itemNameIndex">項目名インデックス</param>
        /// <param name="expensesIndex">支出インデックス</param>
        /// <param name="encoding">エンコーディング</param>
        /// <returns>読込結果</returns>
        public async Task<IEnumerable<ActionCsvModel>> LoadCsvCompListAsync(IEnumerable<string> csvFilePathList, int actDateIndex, int itemNameIndex, int expensesIndex, Encoding encoding)
        {
            IEnumerable<ActionCsvDto> modelList = await CSVFileDao.LoadCsvCompListAsync(csvFilePathList, actDateIndex, itemNameIndex, expensesIndex, encoding);

            AssetModel asset = AssetService.Instance.GetDefaultAssetModel();
            IEnumerable<ActionCsvModel> vmList = modelList.Select(x => new ActionCsvModel() {
                Date = x.Date,
                Value = new(x.Value, asset.Scale),
                Name = x.Name
            });

            return vmList;
        }

        /// <summary>
        /// 一致する帳簿項目Modelリストを取得する
        /// </summary>
        /// <param name="accountId">帳簿ID</param>
        /// <param name="dateTime">日付</param>
        /// <param name="value">支出</param>
        /// <returns>帳簿Modelリスト</returns>
        public async Task<IEnumerable<ActionModel>> LoadMatchedActionAsync(AccountIdObj accountId, DateTime dateTime, AmountObj value)
        {
            using FuncLog funcLog = new(new { accountId, dateTime, value });

            AssetModel asset = AssetService.Instance.GetDefaultAssetModel(); // 帳簿に紐づくモデルに変更予定
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            ActionCompInfoDao actionCompInfoDao = new(dbHandler);
            IEnumerable<ActionCompInfoDto> dtoList = await actionCompInfoDao.FindMatchesWithCsvAsync(accountId.Id, (int)UserSettingService.Instance.DefaultAssetId, dateTime, value.MainValue);
            IEnumerable<ActionModel> actionList = [.. dtoList.Select(dto => new ActionModel() {
                    GroupId = dto.GroupId,
                    Item = new(dto.ItemId, dto.ItemName),
                    Base = new(dto.ActionId, dto.ActTime, new(dto.ActMainValue, asset.Scale)),
                    Shop = new(dto.ShopName),
                    Remark = new(dto.Remark),
                    IsMatch = dto.IsMatch == 1
                })];

            funcLog.Returns = actionList.Select(static m => m.ActionId);
            return actionList;
        }
    }
}
