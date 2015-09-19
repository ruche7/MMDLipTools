using System;
using System.Windows.Controls.Primitives;

namespace ruche.mmd.tools
{
    /// <summary>
    /// MikuMikuLipStatusBar の View クラス。
    /// </summary>
    public partial class MikuMikuLipStatusBar : StatusBar
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MikuMikuLipStatusBar()
        {
            InitializeComponent();

            this.DataContextChanged += MikuMikuLipConfigViewUtil.OnDataContextChanged;
        }
    }
}
