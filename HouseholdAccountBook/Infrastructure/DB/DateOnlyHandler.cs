using Dapper;
using System;
using System.Data;

namespace HouseholdAccountBook.Infrastructure.DB
{
    /// <summary>
    /// Dapper で <see cref="DateOnly"/> を扱えるようにするためのハンドラ
    /// </summary>
    public class DateOnlyHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value) => parameter.Value = value.ToDateTime(TimeOnly.MinValue);

        public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);
    }
}
