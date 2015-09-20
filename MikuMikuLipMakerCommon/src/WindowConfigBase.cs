using System;
using System.Runtime.Serialization;
using System.Windows;

namespace ruche.mmd.tools
{
    /// <summary>
    /// ウィンドウ設定の抽象クラス。
    /// </summary>
    [DataContract(Namespace = "")]
    public abstract class WindowConfigBase : IExtensibleDataObject
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        protected WindowConfigBase()
        {
            this.Left = null;
            this.Top = null;
            this.Width = null;
            this.Height = null;
            this.IsMaximized = null;
        }

        /// <summary>
        /// ウィンドウの左端位置を取得または設定する。
        /// </summary>
        [DataMember]
        public double? Left { get; set; }

        /// <summary>
        /// ウィンドウの上端位置を取得または設定する。
        /// </summary>
        [DataMember]
        public double? Top { get; set; }

        /// <summary>
        /// ウィンドウの幅を取得または設定する。
        /// </summary>
        [DataMember]
        public double? Width { get; set; }

        /// <summary>
        /// ウィンドウの高さを取得または設定する。
        /// </summary>
        [DataMember]
        public double? Height { get; set; }

        /// <summary>
        /// ウィンドウが最大化されているか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool? IsMaximized { get; set; }

        /// <summary>
        /// ウィンドウから値をコピーする。
        /// </summary>
        /// <param name="window">コピー元のウィンドウ。</param>
        public void CopyFrom(Window window)
        {
            this.Left = window.Left;
            this.Top = window.Top;
            this.Width = window.Width;
            this.Height = window.Height;
            this.IsMaximized = (window.WindowState == WindowState.Maximized);
        }

        /// <summary>
        /// ウィンドウに値を適用する。
        /// </summary>
        /// <param name="window">適用先のウィンドウ。</param>
        public void ApplyTo(Window window)
        {
            if (this.Left.HasValue)
            {
                window.Left = this.Left.Value;
            }
            if (this.Top.HasValue)
            {
                window.Top = this.Top.Value;
            }
            if (this.Width.HasValue)
            {
                window.Width = this.Width.Value;
            }
            if (this.Height.HasValue)
            {
                window.Height = this.Height.Value;
            }
            if (this.IsMaximized.HasValue)
            {
                window.WindowState =
                    this.IsMaximized.Value ? WindowState.Maximized : WindowState.Normal;
            }
        }

        #region IExtensibleDataObject の明示的実装

        ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }

        #endregion
    }
}
