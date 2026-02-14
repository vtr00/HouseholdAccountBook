using HouseholdAccountBook.Adapters.Dao.Abstract;
using HouseholdAccountBook.Adapters.DbHandlers.Abstract;
using HouseholdAccountBook.Adapters.Dto.Others;
using HouseholdAccountBook.Extensions;
using System;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Adapters.Dao.Compositions
{
    /// <summary>
    /// 日時情報DAO
    /// </summary>
    /// <param name="dbHandler">DBハンドラ</param>
    public class DateTimeInfoDao(DbHandlerBase dbHandler) : TableDaoBase(dbHandler)
    {
        /// <summary>
        /// テーブル内のデータの更新日時を取得する
        /// </summary>
        /// <param name="dto">対象のテーブル情報</param>
        /// <returns>テーブル内のデータの更新日時</returns>
        /// <exception cref="ArgumentException">テーブル名が不正</exception>
        public async Task<DateTime> GetUpdateTime(TableInfoDto dto)
        {
            if (!dto.TableName.IsValidDBIdentifier()) {
                throw new ArgumentException($"Invalid table name: {dto.TableName}");
            }
            return await this.mDbHandler.QuerySingleAsync<DateTime>($"SELECT MAX(update_time) FROM {dto.TableName};");
        }

        /// <summary>
        /// テーブル内のデータの挿入日時を取得する
        /// </summary>
        /// <param name="dto">対象のテーブル情報</param>
        /// <returns>テーブル内のデータの挿入日時</returns>
        /// <exception cref="ArgumentException">テーブル名が不正</exception>
        public async Task<DateTime> GetInsertTime(TableInfoDto dto)
        {
            if (!dto.TableName.IsValidDBIdentifier()) {
                throw new ArgumentException($"Invalid table name: {dto.TableName}");
            }

            return await this.mDbHandler.QuerySingleAsync<DateTime>($"SELECT MAX(insert_time) FROM {dto.TableName};");
        }
    }
}
