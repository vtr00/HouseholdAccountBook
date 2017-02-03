using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdAccountBook.UserEventArgs
{
    public class RegistrateEventArgs
    {
        /// <summary>
        /// 入力された値
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="value">登録されたID</param>
        public RegistrateEventArgs(int? id)
        {
            this.Id = id;
        }
    }
}
