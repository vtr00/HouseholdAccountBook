using System.Diagnostics;

namespace HouseholdAccountBook.Models.ValueObjects
{
    public interface IIdObj
    {
        int Value { get; init; }
    }

    /// <summary>
    /// ID VO
    /// </summary>
    /// <param name="Value">ID</param>
    [DebuggerDisplay("{Value}")]
    public sealed record class IdObj(int Value) : IIdObj
    {
        public static implicit operator IdObj(int value) => new(value);
        public static explicit operator int?(IdObj obj) => obj?.Value;
        public static explicit operator int(IdObj obj) => obj is null ? -1 : obj.Value;

        public override string ToString() => $"{this.Value}";
    }

    /// <summary>
    /// 帳簿項目ID VO
    /// </summary>
    /// <param name="Value">帳簿ID</param>
    /// <remarks>-1 は 繰越残高</remarks>
    [DebuggerDisplay("{Value}")]
    public sealed record class ActionIdObj(int Value) : IIdObj
    {
        public static implicit operator ActionIdObj(int id) => new(id);
        public static explicit operator int?(ActionIdObj obj) => obj?.Value;
        public static explicit operator int(ActionIdObj obj) => obj is null ? -1 : obj.Value;

        public override string ToString() => $"{this.Value}";
    }
    /// <summary>
    /// グループID VO
    /// </summary>
    /// <param name="Value">グループID</param>
    [DebuggerDisplay("{Value}")]
    public sealed record class GroupIdObj(int Value) : IIdObj
    {
        public static implicit operator GroupIdObj(int id) => new(id);
        public static explicit operator int?(GroupIdObj obj) => obj?.Value;
        public static explicit operator int(GroupIdObj obj) => obj is null ? -1 : obj.Value;

        public override string ToString() => $"{this.Value}";
    }

    /// <summary>
    /// 帳簿ID VO
    /// </summary>
    /// <param name="Value">帳簿ID</param>
    /// <remarks>-1 は 一覧 または 支払元帳簿:なし</remarks>
    [DebuggerDisplay("{Value}")]
    public sealed record class BookIdObj(int Value) : IIdObj
    {
        public static implicit operator BookIdObj(int id) => new(id);
        public static explicit operator int?(BookIdObj obj) => obj?.Value;
        public static explicit operator int(BookIdObj obj) => obj is null ? -1 : obj.Value;

        public override string ToString() => $"{this.Value}";
    }

    /// <summary>
    /// 分類ID VO
    /// </summary>
    /// <param name="Value">分類ID</param>
    /// <remarks>-1 は (指定なし)</remarks>
    [DebuggerDisplay("{Value}")]
    public record class CategoryIdObj(int Value) : IIdObj
    {
        public static implicit operator CategoryIdObj(int id) => new(id);
        public static explicit operator int?(CategoryIdObj obj) => obj?.Value;
        public static explicit operator int(CategoryIdObj obj) => obj is null ? -1 : obj.Value;
        public static implicit operator IdObj(CategoryIdObj obj) => obj?.Value;

        public override string ToString() => $"{this.Value}";
    }

    /// <summary>
    /// 項目ID VO
    /// </summary>
    /// <param name="Value">項目ID</param>
    [DebuggerDisplay("{Value}")]
    public sealed record class ItemIdObj(int Value) : IIdObj
    {
        public static implicit operator ItemIdObj(int id) => new(id);
        public static explicit operator int?(ItemIdObj obj) => obj?.Value;
        public static explicit operator int(ItemIdObj obj) => obj is null ? -1 : obj.Value;
        public static implicit operator IdObj(ItemIdObj obj) => obj?.Value;

        public override string ToString() => $"{this.Value}";
    }
}
