using HouseholdAccountBook.Models.Dto.Abstract;
using HouseholdAccountBook.Models.Dto.KHDbTable;
using System;

namespace HouseholdAccountBook.Models.Dto.DbTable
{
    /// <summary>
    /// 備考テーブルDTO
    /// </summary>
    public class HstRemarkDto : TableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HstRemarkDto() { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dto">記帳風月DTO</param>
        public HstRemarkDto(CbtNoteDto dto) : base(dto)
        {
            this.ItemId = dto.ITEM_ID;
            this.Remark = dto.NOTE_NAME;
        }

        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = 0;
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// 備考種別(未使用)
        /// </summary>
        public int RemarkKind { get; set; } = 0;
        /// <summary>
        /// 使用日時
        /// </summary>
        public DateTime UsedTime { get; set; } = DateTime.Now;
    }
}
