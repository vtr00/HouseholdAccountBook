using DynamicExpresso.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace HouseholdAccountBook.Models.ValueObjects
{
    /// <summary>
    /// ID VO
    /// </summary>
    /// <typeparam name="T">IDの型</typeparam>
    /// <param name="id">ID</param>
    public class IdObj(int id)
    {
        /// <summary>
        /// 値
        /// </summary>
        public int Value { get; protected init; } = id;

        public static implicit operator IdObj(int id) => new(id);
        public static explicit operator int?(IdObj obj) => obj?.Value;
        public static bool operator ==(IdObj left, IdObj right) => left?.Value == right?.Value;
        public static bool operator !=(IdObj left, IdObj right) => left?.Value != right?.Value;
        public override bool Equals(object obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => this.Value.GetHashCode();

        public override string ToString() => $"{this.Value}";
    }

    /// <summary>
    /// 帳簿項目ID
    /// </summary>
    /// <param name="id">帳簿ID</param>
    public class ActionIdObj(int id) : IdObj(id)
    {
        public static implicit operator ActionIdObj(int id) => new(id);
        public static explicit operator int?(ActionIdObj obj) => obj?.Value;
    }
    /// <summary>
    /// グループID
    /// </summary>
    /// <param name="id">グループID</param>
    public class GroupIdObj(int id) : IdObj(id)
    {
        public static implicit operator GroupIdObj(int id) => new(id);
        public static explicit operator int?(GroupIdObj obj) => obj?.Value;
    }

    /// <summary>
    /// 帳簿ID
    /// </summary>
    /// <param name="id">帳簿ID</param>
    public class BookIdObj(int id) : IdObj(id)
    {
        public static implicit operator BookIdObj(int id) => new(id);
        public static explicit operator int?(BookIdObj obj) => obj?.Value;
    }

    /// <summary>
    /// 分類ID
    /// </summary>
    /// <param name="id">分類ID</param>
    public class CategoryIdObj(int id) : IdObj(id)
    {
        public static implicit operator CategoryIdObj(int id) => new(id);
        public static explicit operator int?(CategoryIdObj obj) => obj?.Value;
    }

    /// <summary>
    /// 項目ID
    /// </summary>
    /// <param name="id">項目ID</param>
    public class ItemIdObj(int id) : IdObj(id)
    {
        public static implicit operator ItemIdObj(int id) => new(id);
        public static explicit operator int?(ItemIdObj obj) => obj?.Value;
    }
}
