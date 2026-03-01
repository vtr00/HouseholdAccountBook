using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace HouseholdAccountBook.Models.DomainModels
{
    /// <summary>
    /// 移動Model
    /// </summary>
    public class MoveModel
    {
        /// <summary>
        /// グループID
        /// </summary>
        public GroupIdObj GroupId { get; set; } = -1;

        /// <summary>
        /// 移動元帳簿項目
        /// </summary>
        public ActionModel MoveFrom { get; set; } = new();
        /// <summary>
        /// 移動先帳簿項目
        /// </summary>
        public ActionModel MoveTo { get; set; } = new();
        /// <summary>
        /// 手数料
        /// </summary>
        public ActionModel Comission { get; set; } = new();

        /// <summary>
        /// 備考
        /// </summary>
        public RemarkModel Remark { get; set; } = string.Empty;
    }
}
