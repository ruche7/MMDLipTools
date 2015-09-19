using System;
using System.Windows.Controls;

namespace ruche.mmd.tools
{
    /// <summary>
    /// MikuMikuLipMenu の View クラス。
    /// </summary>
    public partial class MikuMikuLipMenu : Menu
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MikuMikuLipMenu()
        {
            InitializeComponent();

            this.DataContextChanged += MikuMikuLipConfigViewUtil.OnDataContextChanged;
        }
    }
}
