using System;
using System.Collections.Generic;
using System.Text;

namespace HouseholdAccountBook.Models.DomainModels
{
    /// <summary>
    /// 備考Model
    /// </summary>
    /// <param name="text">備考</param>
    public class RemarkModel(string text)
    {
        /// <summary>
        /// テキスト
        /// </summary>
        public string Text { get; init; } = text;

        public override string ToString() => this.Text;

        public static implicit operator string(RemarkModel remark) => remark.Text;
        public static implicit operator RemarkModel(string remark) => new(remark);
    }
}
