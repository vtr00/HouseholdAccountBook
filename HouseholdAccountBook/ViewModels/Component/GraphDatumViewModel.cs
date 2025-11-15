using HouseholdAccountBook.ViewModels.Abstract;
using System;

namespace HouseholdAccountBook.ViewModels.Component
{
    /// <summary>
    /// グラフデータVM
    /// </summary>
    public class GraphDatumViewModel : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 値(Y軸用)
        /// </summary>
        #region Value
        public int Value {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 日付(Tracker用)
        /// </summary>
        #region Date
        public DateTime Date {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// インデックス(LineSeriesのX軸用)
        /// </summary>
        #region Index
        public int Index {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 項目ID
        /// </summary>
        #region ItemId
        public int ItemId {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion

        /// <summary>
        /// 分類ID
        /// </summary>
        #region CategoryId
        public int CategoryId {
            get;
            set => this.SetProperty(ref field, value);
        }
        #endregion
        #endregion
    }
}
