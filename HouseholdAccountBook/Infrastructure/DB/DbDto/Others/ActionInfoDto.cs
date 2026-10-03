using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;
using System;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto.Others
{
    /// <summary>
    /// 帳簿項目情報DTO
    /// </summary>
    public class ActionInfoDto : VirTableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActionInfoDto() : base() { }

        /// <summary>
        /// 帳簿項目ID
        /// </summary>
        public int ActionId { get; set; } = -1;
        /// <summary>
        /// 項目日時
        /// </summary>
        public DateTime ActTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 帳簿ID
        /// </summary>
        public int BookId { get; set; } = -1;
        /// <summary>
        /// 分類ID
        /// </summary>
        public int CategoryId { get; set; } = -1;
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = -1;
        /// <summary>
        /// 帳簿名
        /// </summary>
        public string BookName { get; set; } = string.Empty;
        /// <summary>
        /// 分類名
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; } = string.Empty;
        /// <summary>
        /// 帳簿項目のアセットID
        /// </summary>
        public int? AssetId { get; set; }
        /// <summary>
        /// 店舗名
        /// </summary>
        public string ShopName { get; set; } = string.Empty;
        /// <summary>
        /// グループID
        /// </summary>
        public int? GroupId { get; set; }
        /// <summary>
        /// 備考
        /// </summary>
        public string Remark { get; set; } = string.Empty;
        /// <summary>
        /// 一致フラグ
        /// </summary>
        public int IsMatch { get; set; }

        /// <summary>
        /// デフォルトに紐づく金額
        /// </summary>
        public AmountDto DefaultAmount { get; set; } = new();
        private decimal MainActValue {
            set => this.DefaultAmount.MainValue = value;
        }
        private int ActAssetId {
            set => this.DefaultAmount.AssetId = value;
        }

        /// <summary>
        /// 帳簿に紐づく金額
        /// </summary>
        public AmountDto BookAmount { get; set; } = new();
        private decimal MainActValueBook {
            set => this.BookAmount.MainValue = value;
        }
        private int ActAssetIdBook {
            set => this.BookAmount.AssetId = value;
        }

        /// <summary>
        /// 項目に紐づく金額
        /// </summary>
        public AmountDto ItemAmount { get; set; } = new();
        private decimal MainActValueItem {
            set => this.ItemAmount.MainValue = value;
        }
        private int ActAssetIdItem {
            set => this.ItemAmount.AssetId = value;
        }

        /// <summary>
        /// 帳簿項目に紐づく金額
        /// </summary>
        public AmountDto ActionAmount { get; set; } = new();
        private decimal MainActValueAction {
            set => this.ActionAmount.MainValue = value;
        }
        private int ActAssetIdAction {
            set => this.ActionAmount.AssetId = value;
        }
    }
}
