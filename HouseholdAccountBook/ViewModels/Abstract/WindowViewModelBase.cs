using HouseholdAccountBook.Infrastructure.DB.DbHandlers;
using HouseholdAccountBook.Models.AppServices;
using System;
using System.Windows;

namespace HouseholdAccountBook.ViewModels.Abstract
{
    /// <summary>
    /// WindowViewModelの基底クラス
    /// </summary>
    public abstract class WindowViewModelBase : WindowPartViewModelBase
    {
        #region プロパティ
        /// <summary>
        /// 処理中状態
        /// </summary>
        /// <remarks>Bind向け</remarks>
        public BusyService BusyState => this.mBusyService;

        /// <summary>
        /// ウィンドウのサイズ設定を取得/設定する
        /// </summary>
        public Size? WindowSizeSetting {
            get {
                var (width, height) = this.WindowSizeSettingRaw;
                return width <= 0 || height <= 0 ? null : new Size(width, height);
            }
            set {
                if (value is not null) {
                    this.WindowSizeSettingRaw = (value.Value.Width, value.Value.Height);
                }
            }
        }
        /// <summary>
        /// ウィンドウの位置設定を取得/設定する
        /// </summary>
        public abstract Point WindowPointSetting { get; set; }
        /// <summary>
        /// ウィンドウの状態設定を取得/設定する
        /// </summary>
        public virtual int WindowStateSetting { get; set; }

        /// <summary>
        /// ウィンドウのサイズ設定の生データを取得/設定する
        /// </summary>
        protected virtual (double width, double height) WindowSizeSettingRaw { get; set; }
        #endregion

        /// <summary>
        /// ViewModelの初期化を行う
        /// </summary>
        /// <param name="dbHandlerFactory">DBハンドラファクトリ</param>
        /// <remarks>
        /// <see cref="FrameworkElement">のコンストラクタで呼び出す.
        /// <see cref="BusyService"/>のインスタンスを生成する.
        /// </remarks>
        public virtual void Initialize(DbHandlerFactory dbHandlerFactory) => base.Initialize(new(), dbHandlerFactory);

        public override void Initialize(BusyService busyService, DbHandlerFactory dbHandlerFactory) => throw new InvalidOperationException();
    }
}
