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
        /// 金額を文字列表現に変換する
        /// </summary>
        /// <param name="value">DB管理値(補助単位値)</param>
        /// <param name="assetId">アセットID</param>
        /// <param name="kind">単位種別</param>
        /// <param name="position">単位位置</param>
        /// <returns>金額の文字列表現</returns>
        public string ToAssetString(decimal value, AssetIdObj assetId, UnitKind kind, UnitPosition position)
        {
            using FuncLog funcLog = new(new { value, assetId, kind, position }, Log.LogLevel.Trace);

            AssetModel asset = this.mAssets.FirstOrDefault(asset => asset.Id == assetId);
            if (asset is null) { return value.ToString(); }

            string valueStr = value.ToString();
            string ret = value.ToString();

            switch (kind) {
                case UnitKind.MainUnit:
                    valueStr = (value / (decimal)Math.Pow(10, asset.Scale)).ToString($"N{asset.Scale}"); // 数値のみの文字列表現
                    ret = position switch {
                        UnitPosition.Pre => $"{asset.Prefix}{valueStr}",
                        UnitPosition.Post => $"{valueStr}{asset.Suffix}",
                        UnitPosition.Both => $"{asset.Prefix}{valueStr}{asset.Suffix}",
                        _ => $"{valueStr}"
                    };
                    break;
                case UnitKind.SubUnit:
                    valueStr = value.ToString();
                    ret = position switch {
                        UnitPosition.Pre => $"{asset.SubPrefix}{valueStr}",
                        UnitPosition.Post => $"{valueStr}{asset.SubSuffix}",
                        UnitPosition.Both => $"{asset.SubPrefix}{valueStr}{asset.SubSuffix}",
                        _ => $"{valueStr}"
                    };
                    break;
            }

            funcLog.Returns = new { ret };
            return ret;
        }

        /// <summary>
        /// 変換元のDB管理値(補助単位値)を変換先のDB管理値(補助単位値)に変換する
        /// </summary>
        /// <param name="value">変換元のDB管理値(補助単位値)</param>
        /// <param name="srcAssetId">変換元のアセットID</param>
        /// <param name="dstAssetId">変換先のアセットID。未指定の場合はデフォルトアセット</param>
        /// <returns>変換先のDB管理値(補助単位値)</returns>
        public decimal Convert(decimal value, AssetIdObj srcAssetId, AssetIdObj dstAssetId = null)
        {
            dstAssetId ??= UserSettingService.Instance.DefaultAssetId;

            AssetModel srcAsset = this.mAssets.FirstOrDefault(asset => asset.Id == srcAssetId);
            AssetModel dstAsset = this.mAssets.FirstOrDefault(asset => asset.Id == dstAssetId);

            decimal srcSubValue = value;
            decimal srcMainValue = srcSubValue / (decimal)Math.Pow(10, srcAsset.Scale);
            decimal dstMainValue = srcMainValue * srcAsset.BaseRate / dstAsset.BaseRate;
            decimal dstSubValue = dstMainValue * (decimal)Math.Pow(10, dstAsset.Scale);

            return dstSubValue;
        }
    }
}
