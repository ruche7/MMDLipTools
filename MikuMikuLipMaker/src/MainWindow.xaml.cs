using System;
using System.Windows;
using System.Windows.Input;
using ruche.mmd.tools;

namespace MikuMikuLipMaker
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
        public MikuMikuLipConfigViewModel ViewModel
        {
            get { return this.DataContext as MikuMikuLipConfigViewModel; }
            set { this.DataContext = value ?? (new MikuMikuLipConfigViewModel()); }
        }

        /// <summary>
        /// Close コマンドの実行時に呼び出される。
        /// </summary>
        private void OnCloseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}
