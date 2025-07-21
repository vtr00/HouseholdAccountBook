namespace HouseholdAccountBook.Dto.Abstract
{
    /// <summary>
    /// マスタテーブルのDTOのベースクラス
    /// </summary>
    public class MstDtoBase : TableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MstDtoBase() : base() { }

        public MstDtoBase(KHCbmDtoBase dto) : base(dto)
        {
            this.SortOrder = dto.SORT_KEY;
        }

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }
}
