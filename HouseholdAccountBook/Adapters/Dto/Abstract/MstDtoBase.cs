namespace HouseholdAccountBook.Adapters.Dto.Abstract
{
    /// <summary>
    /// マスタテーブルのDTOのベースクラス
    /// </summary>
    public abstract class MstDtoBase : CommonTableDtoBase, ISequentialIDDto
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MstDtoBase() : base() { }

        /// <summary>
        /// コンストラクタ(KHCbmDtoBaseからの変換)
        /// </summary>
        /// <param name="dto">記帳風月のレコード</param>
        public MstDtoBase(KHCbmDtoBase dto) : base(dto) => this.SortOrder = dto.SORT_KEY;

        public abstract int GetId();

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; set; }
    }
}
