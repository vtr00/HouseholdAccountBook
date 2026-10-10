using HouseholdAccountBook.Infrastructure.DB.DbDto;
using HouseholdAccountBook.Models.ValueObjects;

namespace HouseholdAccountBook.Models.DtoMappers
{
    /// <summary>
    /// 金額クラス変換
    /// </summary>
    public static class AmountMapper
    {
        /// <summary>
        /// 金額DTO -> 金額VO
        /// </summary>
        /// <param name="dto">金額DTO</param>
        /// <returns>金額VO</returns>
        public static AmountObj ToValueObject(AmountDto dto) => new(dto.MainValue, dto.AssetId);
    }
}
