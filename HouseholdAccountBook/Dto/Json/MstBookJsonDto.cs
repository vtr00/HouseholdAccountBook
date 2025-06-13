using System;

namespace HouseholdAccountBook.Dto.Json
{
    /// <summary>
    /// mst_bookテーブルでのJsonCodeに対応するDTO
    /// </summary>
    public class MstBookJsonDto
    {
        /// <summary>
        /// 開始日
        /// </summary>
        public DateTime? StartDate { get; set; } = null;
        /// <summary>
        /// 終了日
        /// </summary>
        public DateTime? EndDate { get; set; } = null;

        /// <summary>
        /// CSVフォルダパス
        /// </summary>
        public string CsvFolderPath { get; set; } = null;

        /// <summary>
        /// CSV内での日付のインデックス
        /// </summary>
        public int? CsvActDateIndex { get; set; } = null;
        /// <summary>
        /// CSV内での出費のインデックス
        /// </summary>
        public int? CsvOutgoIndex { get; set; } = null;
        /// <summary>
        /// CSV内での項目名のインデックス
        /// </summary>
        public int? CsvItemNameIndex { get; set; } = null;
    }
}
