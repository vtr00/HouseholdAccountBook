namespace HouseholdAccountBook.Dto
{
    /// <summary>
    /// mst_bookテーブルでのJsonCodeに対応するオブジェクト
    /// </summary>
    public class MstBookJsonObject
    {
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
