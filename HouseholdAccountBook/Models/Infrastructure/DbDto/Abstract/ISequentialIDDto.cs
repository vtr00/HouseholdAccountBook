namespace HouseholdAccountBook.Models.Infrastructure.DbDto.Abstract
{
    /// <summary>
    /// シーケンスを持つDTOのインターフェース
    /// </summary>
    public interface ISequentialIDDto
    {
        /// <summary>
        /// シーケンスとなるIDを取得する
        /// </summary>
        /// <returns>ID</returns>
        public int GetId();
    }
}
