using System;

namespace HouseholdAccountBook.Adapters.Dto.Abstract
{
    /// <summary>
    /// 汎用テーブルDTOのベースクラス
    /// </summary>
    public class CommonTableDtoBase : PhyTableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CommonTableDtoBase() : base() { }

        /// <summary>
        /// コンストラクタ(KHDtoBaseからの変換)
        /// </summary>
        /// <param name="dto">記帳風月のレコード</param>
        public CommonTableDtoBase(KHDtoBase dto) : base() => this.DelFlg = dto.DEL_FLG ? 1 : 0;

        /// <summary>
        /// JSONコード
        /// </summary>
        public string JsonCode { get; set; }
        /// <summary>
        /// 削除フラグ
        /// </summary>
        public int DelFlg { get; set; }
        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdateTime {
            get => this.mUpdateTime ??= this.Now;
            set => this.mUpdateTime = value;
        }
        private DateTime? mUpdateTime;
        /// <summary>
        /// 更新者
        /// </summary>
        public string Updater { get; set; } = DbConstants.Updater;
        /// <summary>
        /// 挿入日時
        /// </summary>
        public DateTime InsertTime {
            get => this.mInsertTime ??= this.Now;
            set => this.mInsertTime = value;
        }
        private DateTime? mInsertTime;
        /// <summary>
        /// 挿入者
        /// </summary>
        public string Inserter { get; set; } = DbConstants.Inserter;

        /// <summary>
        /// <see cref="UpdateTime"/> か <see cref="InsertTime"/> のうち先に取得した時刻
        /// </summary>
        private DateTime Now => this.mNow ??= DateTime.Now;
        private DateTime? mNow;
    }
}