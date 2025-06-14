using HouseholdAccountBook.DbHandler;

namespace HouseholdAccountBook.Dto.Abstract
{
    /// <summary>
    /// マスタテーブルのDTOのベースクラス
    /// </summary>
    public class MstDtoBase : DtoBase
    {
        public MstDtoBase(int sortOrder, string jsonCode) : base(jsonCode)
        {
            this.SortOrder = sortOrder;
        }

        public MstDtoBase(string jsonCode) : base(jsonCode)
        {
        }

        public MstDtoBase(DbReader.Record record) : base(record)
        {
            this.SortOrder = record.ToInt("sort_order");
        }

        /// <summary>
        /// ソート順
        /// </summary>
        public int SortOrder { get; private set; } = 0;
    }
}
