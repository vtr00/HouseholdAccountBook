using HouseholdAccountBook.Infrastructure.Utilities;
using HouseholdAccountBook.Models;
using HouseholdAccountBook.Models.AppServices;
using HouseholdAccountBook.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace HouseholdAccountBook.Views.Converters
{
    /// <summary>
    /// DateTime - 表示年度変換
    /// </summary>
    public class DateTimeToDisplayedFiscalYearConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter = null, CultureInfo culture = null)
        {
            if (value == null) {
                return null;
            }

            UnitType unitType = EnumUtil.SafeCastEnum(parameter, UnitType.Both);

            string unit_pre = unitType switch {
                UnitType.Pre or UnitType.Both => UnitUtil.GetYearPreUnit(UserSettingService.Instance.FiscalStartMonth),
                UnitType.None or UnitType.Post => string.Empty,
                _ => string.Empty
            };
            string unit_post = unitType switch {
                UnitType.Post or UnitType.Both => UnitUtil.GetYearPostUnit(UserSettingService.Instance.FiscalStartMonth),
                UnitType.None or UnitType.Pre => string.Empty,
                _ => string.Empty
            };

            if (value is DateTime dateTime) {
                return $"{unit_pre}{dateTime:yyyy}{unit_post}";
            }
            else if (value is DateOnly dateOnly) {
                return $"{unit_pre}{dateOnly:yyyy}{unit_post}";
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
