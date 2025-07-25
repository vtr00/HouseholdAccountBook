﻿using HouseholdAccountBook.Dto.Abstract;
using System;

namespace HouseholdAccountBook.Dto.Others
{
    /// <summary>
    /// 移動情報DTO
    /// </summary>
    public class MoveActionInfoDto : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MoveActionInfoDto() : base() { }

        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int BookId { get; set; } = -1;
        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public int ActionId { get; set; } = -1;
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = -1;
        /// <summary>
        /// 項目日時
        /// </summary>
        public DateTime ActTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 項目値
        /// </summary>
        public int ActValue { get; set; } = 0;
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// 移動フラグ
        /// </summary>
        public int MoveFlg { get; set; } = 0;
    }
}
