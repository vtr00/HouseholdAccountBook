namespace HouseholdAccountBook.Models.Dto.Abstract
{
    /// <summary>
    /// マスタテーブルのDTOのベースクラス
    /// </summary>
    public abstract class MstDtoBase : TableDtoBase, ISequentialIDDto
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MstDtoBase() : base() { }

        public MstDtoBase(KHCbmDtoBase dto) : base(dto)
        {
            this.SortOrder = dto.SORT_KEY;
        }

        public abstract int GetId();

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }
}
