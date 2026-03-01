using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace HouseholdAccountBook.Models.DomainModels
{
    /// <summary>
    /// 店舗Model
    /// </summary>
    /// <param name="name">店名</param>
    public class ShopModel(string name)
    {
        /// <summary>
        /// 店名
        /// </summary>
        public string Name { get; init; } = name;

        public override string ToString() => this.Name;

        public static implicit operator string(ShopModel shop) => shop.Name;
        public static implicit operator ShopModel(string name) => new(name);
    }
}
