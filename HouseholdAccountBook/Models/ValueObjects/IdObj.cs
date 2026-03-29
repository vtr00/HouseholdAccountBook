using System;
using System.Diagnostics;

namespace HouseholdAccountBook.Models.ValueObjects
{
    public interface IIdObj
    {
        /// <summary>
        /// ID
        /// </summary>
        int Id { get; init; }
    }

    /// <summary>
    /// ID VO
    /// </summary>
    [DebuggerDisplay("{Id}")]
    public sealed record class IdObj : IIdObj
    {
        public int Id { get; init; }

        /// <summary>
        /// 内部管理値
        /// </summary>
        public static IdObj System { get; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private IdObj() => this.Id = -1;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">ID</param>
        public IdObj(int id)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(id, nameof(id));
            this.Id = id;
        }

        public static implicit operator IdObj(int id) => new(id);
        public static explicit operator int?(IdObj obj) => obj?.Id;
        public static explicit operator int(IdObj obj) => obj is null ? -1 : obj.Id;
        public static implicit operator BookIdObj(IdObj obj) => obj?.Id;
        public static implicit operator ItemIdObj(IdObj obj) => obj?.Id;

        public override string ToString() => $"{this.Id}";
    }

    /// <summary>
    /// 帳簿ID VO
    /// </summary>
    [DebuggerDisplay("{Id}")]
    public sealed record class BookIdObj : IIdObj
    {
        public int Id { get; init; }

        /// <summary>
        /// 内部管理値
        /// </summary>
        public static BookIdObj System { get; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private BookIdObj() => this.Id = -1;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">帳簿ID</param>
        public BookIdObj(int id)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(id, nameof(id));
            this.Id = id;
        }

        public static implicit operator BookIdObj(int id) => new(id);
        public static explicit operator int?(BookIdObj obj) => obj?.Id;
        public static explicit operator int(BookIdObj obj) => obj is null ? -1 : obj.Id;
        public static implicit operator IdObj(BookIdObj obj) => obj?.Id;

        public override string ToString() => $"{this.Id}";
    }

    /// <summary>
    /// 分類ID VO
    /// </summary>
    [DebuggerDisplay("{Id}")]
    public record class CategoryIdObj : IIdObj
    {
        public int Id { get; init; }

        /// <summary>
        /// 内部管理値
        /// </summary>
        public static CategoryIdObj System { get; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private CategoryIdObj() => this.Id = -1;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">分類ID</param>
        public CategoryIdObj(int id)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(id, nameof(id));
            this.Id = id;
        }

        public static implicit operator CategoryIdObj(int id) => new(id);
        public static explicit operator int?(CategoryIdObj obj) => obj?.Id;
        public static explicit operator int(CategoryIdObj obj) => obj is null ? -1 : obj.Id;
        public static implicit operator IdObj(CategoryIdObj obj) => obj?.Id;

        public override string ToString() => $"{this.Id}";
    }

    /// <summary>
    /// 項目ID VO
    /// </summary>
    [DebuggerDisplay("{Id}")]
    public sealed record class ItemIdObj : IIdObj
    {
        public int Id { get; init; }

        /// <summary>
        /// 内部管理値
        /// </summary>
        public static ItemIdObj System { get; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ItemIdObj() => this.Id = -1;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">項目ID</param>
        public ItemIdObj(int id)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(id, nameof(id));
            this.Id = id;
        }

        public static implicit operator ItemIdObj(int id) => new(id);
        public static explicit operator int?(ItemIdObj obj) => obj?.Id;
        public static explicit operator int(ItemIdObj obj) => obj is null ? -1 : obj.Id;
        public static implicit operator IdObj(ItemIdObj obj) => obj?.Id;

        public override string ToString() => $"{this.Id}";
    }

    /// <summary>
    /// 帳簿項目ID VO
    /// </summary>
    [DebuggerDisplay("{Id}")]
    public sealed record class ActionIdObj : IIdObj
    {
        public int Id { get; init; }

        /// <summary>
        /// 内部管理値
        /// </summary>
        public static ActionIdObj System { get; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ActionIdObj() => this.Id = -1;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">帳簿項目ID</param>
        public ActionIdObj(int id)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(id, nameof(id));
            this.Id = id;
        }

        public static implicit operator ActionIdObj(int id) => new(id);
        public static explicit operator int?(ActionIdObj obj) => obj?.Id;
        public static explicit operator int(ActionIdObj obj) => obj is null ? -1 : obj.Id;

        public override string ToString() => $"{this.Id}";
    }
    /// <summary>
    /// グループID VO
    /// </summary>
    [DebuggerDisplay("{Id}")]
    public sealed record class GroupIdObj : IIdObj
    {
        public int Id { get; init; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private GroupIdObj() => this.Id = -1;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">グループID</param>
        public GroupIdObj(int id)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(id, nameof(id));
            this.Id = id;
        }

        public static implicit operator GroupIdObj(int id) => new(id);
        public static explicit operator int?(GroupIdObj obj) => obj?.Id;
        public static explicit operator int(GroupIdObj obj) => obj is null ? -1 : obj.Id;

        public override string ToString() => $"{this.Id}";
    }
}
