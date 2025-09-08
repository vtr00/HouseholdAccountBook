namespace HouseholdAccountBook.Models.Dto.Abstract
{
    public interface ISequentialIDDto
    {
        /// <summary>
        /// シーケンスとなるIDを取得する
        /// </summary>
        /// <returns></returns>
        public int GetId();
    }
}
