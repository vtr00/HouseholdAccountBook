namespace HouseholdAccountBook.Adapters.Dto.Abstract
{
    /// <summary>
    /// 記帳風月のDTO向けのベースクラス
    /// </summary>
    public abstract class KHDtoBase : PhyTableDtoBase
    {
        /// <summary>
        /// 削除フラグ
        /// </summary>
        public bool DEL_FLG { get; set; }
    }
}
