using HouseholdAccountBook.Infrastructure.DB.DbDto.Abstract;

namespace HouseholdAccountBook.Infrastructure.DB.DbDto.Others
{
    /// <summary>
    /// 概要情報DTO
    /// </summary>
    public class SummaryInfoDto : VirTableDtoBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SummaryInfoDto() : base() { }

        /// <summary>
        /// 収支種別
        /// </summary>
        public int BalanceKind { get; set; } = (int)Models.BalanceKind.Others;
        /// <summary>
        /// 分類ID
        /// </summary>
        public int CategoryId { get; set; } = -1;
        /// <summary>
        /// 分類名
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>
        /// 項目ID
        /// </summary>
        public int ItemId { get; set; } = -1;
        /// <summary>
        /// 項目名
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// デフォルトに紐づく合計金額
        /// </summary>
        public AmountDto DefaultTotalAmount { get; set; } = new();
        private decimal MainActValue {
            set => this.DefaultTotalAmount.MainValue = value;
        }
        private int ActAssetId {
            set => this.DefaultTotalAmount.AssetId = value;
        }

        /// <summary>
        /// 帳簿に紐づく合計金額
        /// </summary>
        public AmountDto BookTotalAmount { get; set; } = new();
        private decimal MainActValueBook {
            set => this.BookTotalAmount.MainValue = value;
        }
        private int ActAssetIdBook {
            set => this.BookTotalAmount.AssetId = value;
        }

        /// <summary>
        /// 項目に紐づく合計金額
        /// </summary>
        public AmountDto ItemTotalAmount { get; set; } = new();
        private decimal MainActValueItem {
            set => this.ItemTotalAmount.MainValue = value;
        }
        private int ActAssetIdItem {
            set => this.ItemTotalAmount.AssetId = value;
        }
    }
}
