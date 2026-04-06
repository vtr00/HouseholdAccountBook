using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;
using HouseholdAccountBook.Infrastructure.DB.DbDto.KHDbTable;
using HouseholdAccountBook.Infrastructure.Logger;
using System;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto.DbTable
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
            using FuncLog funcLog = new(new { dto }, Log.LogLevel.Trace);

            this.ItemId = dto.ITEM_ID;
            this.Remark = dto.NOTE_NAME;
            this.RemarkKind = 0;
            // 使用日時は記帳風月の管理外なので最小値で埋める
            this.UsedTime = DateTime.MinValue;
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
        public DateTime UsedTime { get; set; }
    }
}
