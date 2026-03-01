using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;
using System;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable
{
    /// <summary>
    /// スキーマバージョンテーブルDTO
    /// </summary>
    /// <remarks>PostgreSQLのみ</remarks>
    public class MtdSchemaVersionDto : PhyTableDtoBase
    {
        /// <summary>
        /// スキーマバージョン
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
