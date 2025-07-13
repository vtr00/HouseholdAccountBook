namespace HouseholdAccountBook.Dto.Abstract
{
    /// <summary>
    /// マスタテーブルのDTOのベースクラス
    /// </summary>
    public class MstDtoBase : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MstDtoBase() : base() { }

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }
}
