using HouseholdAccountBook.Models.Infrastructure.DbDto.Abstract;

namespace HouseholdAccountBook.Models.Infrastructure.DbDto.Others
{
    /// <summary>
    /// テーブル情報DTO
    /// </summary>
    public class TableInfoDto : VirTableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TableInfoDto() : base() { }

        /// <summary>
        /// テーブルスキーマ
        /// </summary>
        public string TableSchema { get; set; } = string.Empty;

        /// <summary>
        /// テーブル名
        /// </summary>
        public string TableName { get; set; } = string.Empty;
    }
}
