using HouseholdAccountBook.Infrastructure;
using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Infrastructure.Logger;
using HouseholdAccountBook.Models.UiDto;
using HouseholdAccountBook.Models.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouseholdAccountBook.Models.AppServices
{
    public class AssetService : SingletonBase<AssetService>
    {
        /// <summary>
        /// アセットリスト
        /// </summary>
        private IEnumerable<AssetModel> mAssets = [];

        /// <summary>
        /// スタティックコンストラクタ
        /// </summary>
        static AssetService() => Register(static () => new AssetService());
        /// <summary>
        /// プライベートコンストラクタ
        /// </summary>
        private AssetService() { }

        /// <summary>
        /// アセットリストを更新する
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        public async Task UpdateAssets(DbHandlerFactory dbHandlerFactory)
        {
            using FuncLog funcLog = new();

            AppCommonService service = new(dbHandlerFactory);
            this.mAssets = await service.LoadAssetListAsync();
        }

        /// <summary>
        /// デフォルトアセットモデルを取得する
        /// </summary>
        /// <returns>デフォルトアセットモデル</returns>
        public AssetModel GetDefaultAssetModel() => this.mAssets.FirstOrDefault(asset => asset.Id == UserSettingService.Instance.DefaultAssetId);
        /// <summary>
        /// アセットモデルを取得する
        /// </summary>
        /// <param name="assetId">アセットID</param>
        /// <returns>指定されたアセットIDのアセットモデル</returns>
        public AssetModel GetAssetModel(AssetIdObj assetId) => this.mAssets.FirstOrDefault(asset => asset.Id == assetId, this.GetDefaultAssetModel());

        /// <summary>
        /// 金額を文字列表現に変換する
        /// </summary>
        /// <param name="amount">金額VO</param>
        /// <param name="outputKind">出力値単位種別</param>
        /// <param name="position">単位位置</param>
        /// <returns>金額の文字列表現</returns>
        public string ToAssetString(AmountObj amount, UnitKind outputKind, UnitPosition position = UnitPosition.Both)
            => this.ToAssetString(amount.MainValue, amount.AssetId, UnitKind.MainUnit, outputKind, position);
        /// <summary>
        /// 金額を文字列表現に変換する
        /// </summary>
        /// <param name="value">金額</param>
        /// <param name="assetId">アセットID</param>
        /// <param name="inputKind">入力値単位種別</param>
        /// <param name="outputKind">出力値単位種別</param>
        /// <param name="position">単位位置</param>
        /// <returns>金額の文字列表現</returns>
        public string ToAssetString(decimal? value, AssetIdObj assetId, UnitKind inputKind, UnitKind outputKind, UnitPosition position = UnitPosition.Both)
        {
            AssetModel asset = this.GetAssetModel(assetId);
            if (asset is null) { return value.ToString(); }

            decimal tmpValue = value ?? 0;
            string signStr = Math.Sign(tmpValue) < 0 ? "-" : string.Empty;
            string absValueStr = Math.Abs(tmpValue).ToString();
            string ret = Math.Abs(tmpValue).ToString();

            switch (outputKind) {
                case UnitKind.MainUnit:
                    // 数値のみの文字列表現
                    switch (inputKind) {
                        case UnitKind.MainUnit:
                            absValueStr = Math.Abs(tmpValue).ToString($"N{asset.Scale}");
                            break;
                        case UnitKind.SubUnit:
                            absValueStr = Math.Abs(tmpValue / (decimal)Math.Pow(10, asset.Scale)).ToString($"N{asset.Scale}");
                            break;
                    }

                    ret = position switch {
                        UnitPosition.Pre => $"{signStr}{asset.Prefix}{absValueStr}",
                        UnitPosition.Post => $"{signStr}{absValueStr}{asset.Suffix}",
                        UnitPosition.Both => $"{signStr}{asset.Prefix}{absValueStr}{asset.Suffix}",
                        _ => $"{signStr}{absValueStr}"
                    };
                    break;
                case UnitKind.SubUnit:
                    // 数値のみの文字列表現
                    switch (inputKind) {
                        case UnitKind.MainUnit:
                            absValueStr = Math.Abs(tmpValue * (decimal)Math.Pow(10, asset.Scale)).ToString($"N0");
                            break;
                        case UnitKind.SubUnit:
                            absValueStr = Math.Abs(tmpValue).ToString($"N0");
                            break;
                    }

                    ret = position switch {
                        UnitPosition.Pre => $"{signStr}{asset.SubPrefix}{absValueStr}",
                        UnitPosition.Post => $"{signStr}{absValueStr}{asset.SubSuffix}",
                        UnitPosition.Both => $"{signStr}{asset.SubPrefix}{absValueStr}{asset.SubSuffix}",
                        _ => $"{signStr}{absValueStr}"
                    };
                    break;
            }

            return ret;
        }

        /// <summary>
        /// 変換元の金額VOを変換先の金額VOに変換する
        /// </summary>
        /// <param name="src">変換元の金額VO</param>
        /// <param name="dstAssetId">変換先のアセットID。未指定の場合はデフォルトアセット</param>
        /// <returns>変換先の金額VO</returns>
        public AmountObj Convert(AmountObj src, AssetIdObj dstAssetId = null)
        {
            dstAssetId ??= UserSettingService.Instance.DefaultAssetId;

            AssetModel srcAsset = this.GetAssetModel(src.AssetId);
            AssetModel dstAsset = this.GetAssetModel(dstAssetId);

            return new(src.MainValue * srcAsset.BaseRate / dstAsset.BaseRate, dstAssetId);
        }
    }
}
