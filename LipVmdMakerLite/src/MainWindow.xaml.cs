using System;
using System.Windows;
using ruche.mmd.tools;

namespace LipVmdMakerLite
{
    /// <summary>
    /// MainWindow の View クラス。
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ViewModel を取得または設定する。
        /// </summary>
        public LipVmdControlViewModel ViewModel
        {
            get { return this.DataContext as LipVmdControlViewModel; }
            set { this.DataContext = value ?? (new LipVmdControlViewModel()); }
        }
    }
}
