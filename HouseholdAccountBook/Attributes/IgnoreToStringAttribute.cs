using System;

namespace HouseholdAccountBook.Attributes
{
    /// <summary>
    /// 文字列化を無視する属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class IgnoreToStringAttribute : Attribute
    {
        /// <summary>
        /// 代替テキスト
        /// </summary>
        public string Alternative { get; set; } = "***";
    }
}
