using HouseholdAccountBook.Others;
using System;

namespace HouseholdAccountBook.Dto.Abstract
{
    /// <summary>
    /// テーブルDTOのベースクラス
    /// </summary>
    public class TableDtoBase : DtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TableDtoBase() : base() { }

        public TableDtoBase(KHDtoBase dto) : base()
        {
            this.DelFlg = dto.DEL_FLG ? 1 : 0;
        }

        /// <summary>
        /// JSONコード
        /// </summary>
        public string JsonCode { get; set; } = null;
        /// <summary>
        /// 削除フラグ
        /// </summary>
        public int DelFlg { get; set; } = 0;
        /// <summary>
        /// 更新日
        /// </summary>
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 更新者
        /// </summary>
        public string Updater { get; set; } = DbConstants.Updater;
        /// <summary>
        /// 挿入日
        /// </summary>
        public DateTime InsertTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 挿入者
        /// </summary>
        public string Inserter { get; set; } = DbConstants.Inserter;
    }
}