using System;

namespace HouseholdAccountBook.Dto.Json
{
    /// <summary>
    /// mst_bookテーブルでのJsonCodeに対応するDTO
    /// </summary>
    public class MstBookJsonDto
    {
        public MstBookJsonDto(DateTime? startDate, DateTime? endDate, string csvFolderPath, int? csvActDateIndex, int? csvOutgoIndex, int? csvItemNameIndex)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.CsvFolderPath = csvFolderPath;
            this.CsvActDateIndex = csvActDateIndex;
            this.CsvOutgoIndex = csvOutgoIndex;
            this.CsvItemNameIndex = csvItemNameIndex;
        }

        /// <summary>
        /// 開始日
        /// </summary>
        public DateTime? StartDate { get; private set; } = null;
        /// <summary>
        /// 終了日
        /// </summary>
        public DateTime? EndDate { get; private set; } = null;

        /// <summary>
        /// CSVフォルダパス
        /// </summary>
        public string CsvFolderPath { get; private set; } = null;

        /// <summary>
        /// CSV内での日付のインデックス
        /// </summary>
        public int? CsvActDateIndex { get; private set; } = null;
        /// <summary>
        /// CSV内での出費のインデックス
        /// </summary>
        public int? CsvOutgoIndex { get; private set; } = null;
        /// <summary>
        /// CSV内での項目名のインデックス
        /// </summary>
        public int? CsvItemNameIndex { get; private set; } = null;
    }
}
