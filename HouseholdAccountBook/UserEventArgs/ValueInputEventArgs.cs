using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdAccountBook.UserEventArgs
{
    public class ValueInputEventArgs
    {
        /// <summary>
        /// 入力された値
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="value">入力された値</param>
        public ValueInputEventArgs(int value)
        {
            this.Value = value;
        }
    }
}
