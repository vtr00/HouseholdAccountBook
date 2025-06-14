using HouseholdAccountBook.DbHandler;
using System;

namespace HouseholdAccountBook.Dto.Abstract
{
    /// <summary>
    /// DTOのベースクラス
    /// </summary>
    public class DtoBase
    {
        public DtoBase(string jsonCode)
        {
            this.JsonCode = jsonCode;
        }

        public DtoBase(DbReader.Record record)
        {
            this.DelFlag = record.ToBoolean("del_flg");
            this.UpdateTime = record.ToDateTime("update_time");
            this.Updater = record["updater"];
            this.InsertTime = record.ToDateTime("insert_time");
            this.Inserter = record["inserter"];
            this.JsonCode = record["json_code"];
        }

        /// <summary>
        /// 削除フラグ
        /// </summary>
        public bool DelFlag { get; private set; } = false;
        /// <summary>
        /// 更新日
        /// </summary>
        public DateTime UpdateTime { get; private set; } = DateTime.Now;
        /// <summary>
        /// 更新者
        /// </summary>
        public string Updater { get; private set; } = ConstValue.ConstValue.Updater;
        /// <summary>
        /// 挿入日
        /// </summary>
        public DateTime InsertTime { get; private set; } = DateTime.Now;
        /// <summary>
        /// 挿入者
        /// </summary>
        public string Inserter { get; private set; } = ConstValue.ConstValue.Inserter;
        /// <summary>
        /// JSONコード
        /// </summary>
        public string JsonCode { get; private set; } = string.Empty;
    }
}
