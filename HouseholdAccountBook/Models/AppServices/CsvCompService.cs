using HouseholdAccountBook.Infrastructure.DB.DbDao.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers.Abstract;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models.UiDto;
using System.Collections.ObjectModel;
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
        public async Task<ObservableCollection<BookModel>> UpdateBookCompListAsync()
        {
            using FuncLog funcLog = new();

            ObservableCollection<BookModel> bmList = [];

            await using (DbHandlerBase dbHandler = await this.mDbHandlerFactory.CreateAsync()) {
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

                    bmList.Add(vm);
                }
            }

            return bmList;
        }
    }
}
