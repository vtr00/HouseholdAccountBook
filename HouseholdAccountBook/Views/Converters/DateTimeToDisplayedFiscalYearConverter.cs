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

            UnitPosition unitPosition = EnumUtil.SafeCastEnum(parameter, UnitPosition.Both);

            string unit_pre = unitPosition switch {
                UnitPosition.Pre or UnitPosition.Both => UnitUtil.GetYearPreUnit(UserSettingService.Instance.FiscalStartMonth),
                UnitPosition.None or UnitPosition.Post => string.Empty,
                _ => string.Empty
            };
            string unit_post = unitPosition switch {
                UnitPosition.Post or UnitPosition.Both => UnitUtil.GetYearPostUnit(UserSettingService.Instance.FiscalStartMonth),
                UnitPosition.None or UnitPosition.Pre => string.Empty,
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
