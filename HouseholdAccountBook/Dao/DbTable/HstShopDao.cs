using HouseholdAccountBook.Dao.Abstract;
using HouseholdAccountBook.DbHandler.Abstract;
using HouseholdAccountBook.Dto.Abstract;
using HouseholdAccountBook.Dto.DbTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Dao.DbTable
{
    /// <summary>
    /// 店舗名テーブルDAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class HstShopDao(DbHandlerBase dbHandler) : ReadWriteTableDaoBase<HstShopDto>(dbHandler)
    {
        /// <summary>
        /// 全レコードを取得する
        /// </summary>
        /// <returns>DTOリスト</returns>
        public override async Task<IEnumerable<HstShopDto>> FindAllAsync()
        {
            var dtoList = await this.dbHandler.QueryAsync<HstShopDto>(@"
SELECT * 
FROM hst_shop
WHERE del_flg = 0;");

            return dtoList;
        }

        /// <summary>
        /// <see cref="HstShopDto.ShopName"/> と <see cref="HstShopDto.ItemId"/> に基づいて、レコードを取得する
        /// </summary>
        /// <param name="dto"><see cref="HstShopDto"/></param>
        /// <param name="includeDeleted"><see cref="TableDtoBase.DelFlg">=1も含めるか</param>
        /// <returns>DTO</returns>
        public async Task<HstShopDto> FindByItemIdAndShopNameAsync(string shopName, int itemId, bool includeDeleted = false)
        {
            var dto = includeDeleted
                ? await this.dbHandler.QuerySingleOrDefaultAsync<HstShopDto>(@"
SELECT * FROM hst_shop
WHERE item_id = @ItemId AND shop_name = @ShopName;",
new HstShopDto { ShopName = shopName, ItemId = itemId })
                : await this.dbHandler.QuerySingleOrDefaultAsync<HstShopDto>(@"
SELECT * FROM hst_shop
WHERE item_id = @ItemId AND shop_name = @ShopName AND del_flg = 0;",
new HstShopDto { ShopName = shopName, ItemId = itemId });

            return dto;
        }

        /// <summary>
        /// <see cref="HstShopDto"/> を挿入する
        /// </summary>
        /// <param name="dto"><see cref="HstRemarkDto"/></param>
        /// <returns>挿入行数</returns>
        public override async Task<int> InsertAsync(HstShopDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
INSERT INTO hst_shop (item_id, shop_name, used_time, json_code, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemId, @ShopName, @UsedTime, @JsonCode, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter);", dto);

            return count;
        }

        /// <summary>
        /// <see cref="HstShopDto.ItemId"/> と <see cref="HstShopDto.ShopName"/> が一致するレコードを更新する
        /// </summary>
        /// <param name="dto"><see cref="HstShopDto"/></param>
        /// <returns>更新行数</returns>
        public override async Task<int> UpdateAsync(HstShopDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE hst_shop
SET used_time = @UsedTime, json_code = @JsonCode, del_flg = @DelFlg, update_time = @UpdateTime, updater = @Updater
WHERE item_id = @ItemId AND shop_name = @ShopName AND used_time < @UsedTime;", dto);

            return count;
        }

        /// <summary>
        /// <see cref="HstShopDto"/> を挿入または更新する
        /// </summary>
        /// <param name="dto"><see cref="HstShopDto"/></param>
        /// <returns>挿入/更新行数</returns>
        /// <remarks>PostgreSQLとSQLiteで挙動が変わる可能性があるため変更時は要動作確認</remarks>
        public override async Task<int> UpsertAsync(HstShopDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
INSERT INTO hst_shop (item_id, shop_name, used_time, json_code, del_flg, update_time, updater, insert_time, inserter)
VALUES (@ItemId, @ShopName, @UsedTime, @JsonCode, @DelFlg, @UpdateTime, @Updater, @InsertTime, @Inserter)
ON CONFLICT (item_id, shop_name) DO UPDATE
SET used_time = @UsedTime, json_code = @JsonCode, del_flg = @DelFlg, update_time = @UpdateTime, updater = @Updater
WHERE hst_shop.item_id = @ItemId AND hst_shop.shop_name = @ShopName AND hst_shop.used_time < @UsedTime;", dto);

            return count;
        }

        /// <summary>
        /// <see cref="HstShopDto"/> の全てのレコードを削除する
        /// </summary>
        /// <returns>削除行数</returns>
        public override async Task<int> DeleteAllAsync()
        {
            int count = await this.dbHandler.ExecuteAsync(@"DELETE FROM hst_shop;");

            return count;
        }

        /// <summary>
        /// <see cref="HstShopDto.ItemId"/> と <see cref="HstShopDto.ShopName"/> が一致するレコードを削除する
        /// </summary>
        /// <param name="dto"><see cref="HstShopDto"/></param>
        /// <returns>削除行数</returns>
        public async Task<int> DeleteAsync(HstShopDto dto)
        {
            int count = await this.dbHandler.ExecuteAsync(@"
UPDATE hst_shop SET del_flg = 1, update_time = @UpdateTime, updater = @Updater
WHERE shop_name = @ShopName AND item_id = @ItemId;", dto);

            return count;
        }
    }
}
