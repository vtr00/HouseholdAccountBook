namespace HouseholdAccountBook.Models.ValueObjects
{
    /// <summary>
    /// ID VO
    /// </summary>
    /// <param name="Value">ID</param>
    public record class IdObj(int Value)
    {
        public static implicit operator IdObj(int value) => new(value);
        public static explicit operator int?(IdObj obj) => obj?.Value;
        public static explicit operator int(IdObj obj) => obj is null ? -1 : obj.Value;

        public override string ToString() => $"{this.Value}";
    }

    /// <summary>
    /// 帳簿項目ID
    /// </summary>
    /// <param name="Value">帳簿ID</param>
    public record class ActionIdObj(int Value) : IdObj(Value)
    {
        public static implicit operator ActionIdObj(int id) => new(id);
        public static explicit operator int?(ActionIdObj obj) => obj?.Value;
        public static explicit operator int(ActionIdObj obj) => obj is null ? -1 : obj.Value;
    }
    /// <summary>
    /// グループID
    /// </summary>
    /// <param name="Value">グループID</param>
    public record class GroupIdObj(int Value) : IdObj(Value)
    {
        public static implicit operator GroupIdObj(int id) => new(id);
        public static explicit operator int?(GroupIdObj obj) => obj?.Value;
        public static explicit operator int(GroupIdObj obj) => obj is null ? -1 : obj.Value;
    }

    /// <summary>
    /// 帳簿ID
    /// </summary>
    /// <param name="Value">帳簿ID</param>
    public record class BookIdObj(int Value) : IdObj(Value)
    {
        public static implicit operator BookIdObj(int id) => new(id);
        public static explicit operator int?(BookIdObj obj) => obj?.Value;
        public static explicit operator int(BookIdObj obj) => obj is null ? -1 : obj.Value;
    }

    /// <summary>
    /// 分類ID
    /// </summary>
    /// <param name="Value">分類ID</param>
    public record class CategoryIdObj(int Value) : IdObj(Value)
    {
        public static implicit operator CategoryIdObj(int id) => new(id);
        public static explicit operator int?(CategoryIdObj obj) => obj?.Value;
        public static explicit operator int(CategoryIdObj obj) => obj is null ? -1 : obj.Value;
    }

    /// <summary>
    /// 項目ID
    /// </summary>
    /// <param name="Value">項目ID</param>
    public record class ItemIdObj(int Value) : IdObj(Value)
    {
        public static implicit operator ItemIdObj(int id) => new(id);
        public static explicit operator int?(ItemIdObj obj) => obj?.Value;
        public static explicit operator int(ItemIdObj obj) => obj is null ? -1 : obj.Value;
    }
}
