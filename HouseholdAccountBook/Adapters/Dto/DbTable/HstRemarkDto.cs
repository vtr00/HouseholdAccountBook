using HouseholdAccountBook.Adapters.Dto.Abstract;
using HouseholdAccountBook.Adapters.Dto.KHDbTable;
using System;

namespace HouseholdAccountBook.Adapters.Dto.DbTable
{
    /// <summary>
    /// 備考DTO
    /// </summary>
    public class HstRemarkDto : CommonTableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HstRemarkDto() { }
        /// <summary>
        /// コンストラクタ(記帳風月の備考DTOからのコピー)
        /// </summary>
        /// <param name="dto">記帳風月の備考DTO</param>
        public HstRemarkDto(CbtNoteDto dto) : base(dto)
        {
            this.ItemId = dto.ITEM_ID;
            this.Remark = dto.NOTE_NAME;
            this.RemarkKind = 0;
        }

        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; }
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// 備考種別(未使用)
        /// </summary>
        public int RemarkKind { get; set; }
        /// <summary>
        /// 使用日時
        /// </summary>
        public DateTime UsedTime { get; set; } = DateTime.Now;
    }
}
