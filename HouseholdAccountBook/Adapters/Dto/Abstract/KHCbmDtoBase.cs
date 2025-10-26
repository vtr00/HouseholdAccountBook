namespace HouseholdAccountBook.Adapters.Dto.Abstract
{
    /// <summary>
    /// 記帳風月のマスタテーブル向けのDTOのベースクラス
    /// </summary>
    public abstract class KHCbmDtoBase : KHDtoBase, ISequentialIDDto
    {
        public KHCbmDtoBase() { }

        public abstract int GetId();

        /// <summary>
        /// ソートキー
        /// </summary>
        public int SORT_KEY { get; set; }
    }
}
