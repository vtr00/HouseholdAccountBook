using HouseholdAccountBook.Models.Infrastructure.DbDto.Abstract;
using System;

namespace HouseholdAccountBook.Models.Infrastructure.DbDto.DbTable
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
