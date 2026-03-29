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
        public async Task<IEnumerable<BookModel>> UpdateBookCompListAsync()
        {
            using FuncLog funcLog = new();
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            List<BookModel> bmList = [];

            MstBookDao mstBookDao = new(dbHandler);
            IEnumerable<MstBookDto> dtoList = await mstBookDao.FindIfJsonCodeExistsAsync();
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

                bmList.Add(vm);
            }

            return bmList;
        }

        /// <summary>
        /// 一致する帳簿項目Modelリストを取得する
        /// </summary>
        /// <param name="bookId">帳簿ID</param>
        /// <param name="dateTime">日付</param>
        /// <param name="value">支出</param>
        /// <returns>帳簿Modelリスト</returns>
        public async Task<IEnumerable<ActionModel>> LoadMatchedActionAsync(BookIdObj bookId, DateTime dateTime, decimal value)
        {
            using FuncLog funcLog = new(new { bookId, dateTime, value });
            await using DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync();

            ActionCompInfoDao actionCompInfoDao = new(dbHandler);
            IEnumerable<ActionCompInfoDto> dtoList = await actionCompInfoDao.FindMatchesWithCsvAsync(bookId.Id, dateTime, (int)value);
            IEnumerable<ActionModel> actionList = [.. dtoList.Select(static dto => new ActionModel() {
                    GroupId = dto.GroupId,
                    Item = new(dto.ItemId, dto.ItemName),
                    Base = new(dto.ActionId, dto.ActTime, dto.ActValue),
                    Shop = new(dto.ShopName),
                    Remark = new(dto.Remark),
                    IsMatch = dto.IsMatch == 1
                })];

            return actionList;
        }
    }
}
