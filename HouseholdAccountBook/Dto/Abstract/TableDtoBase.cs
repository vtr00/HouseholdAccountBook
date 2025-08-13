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
        /// 更新日時
        /// </summary>
        public DateTime UpdateTime
        {
            get => this._UpdateTime ??= this.Now;
            set => this._UpdateTime = value;
        }
        private DateTime? _UpdateTime;
        /// <summary>
        /// 更新者
        /// </summary>
        public string Updater { get; set; } = DbConstants.Updater;
        /// <summary>
        /// 挿入日時
        /// </summary>
        public DateTime InsertTime
        {
            get => this._InsertTime ??= this.Now;
            set => this._InsertTime = value;
        }
        private DateTime? _InsertTime;
        /// <summary>
        /// 挿入者
        /// </summary>
        public string Inserter { get; set; } = DbConstants.Inserter;

        /// <summary>
        /// <see cref="UpdateTime"/> か <see cref="InsertTime"/> を取得した時刻
        /// </summary>
        private DateTime Now => this._Now ??= DateTime.Now;
        private DateTime? _Now = null;
    }
}