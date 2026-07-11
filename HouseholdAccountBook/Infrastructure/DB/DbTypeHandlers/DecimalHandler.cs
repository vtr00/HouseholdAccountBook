using Dapper;
using System;
using System.Data;

namespace HouseholdAccountBook.Infrastructure.DB.DbTypeHandlers
{
    /// <summary>
    /// Dapper で <see cref="decimal"/> を扱えるようにするためのハンドラ.
    /// SQLite の数値型(Double等)を decimal へ変換するための Dapper TypeHandler
    /// </summary>
    public class DecimalHandler : SqlMapper.TypeHandler<decimal>
    {
        /// <summary>
        /// DbDataParameter に decimal 値を設定する
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="value">変換元の値</param>
        public override void SetValue(IDbDataParameter parameter, decimal value) => parameter.Value = value;

        /// <summary>
        /// DbDataReader から取得した値を decimal に変換する
        /// </summary>
        /// <param name="value">変換元の値</param>
        public override decimal Parse(object value)
        {
            return value switch {
                decimal d => d,
                double d => (decimal)d,
                float f => (decimal)f,
                long l => l,
                int i => i,
                DBNull => 0m,
                null => 0m,
                _ => Convert.ToDecimal(value)
            };
        }
    }
}
