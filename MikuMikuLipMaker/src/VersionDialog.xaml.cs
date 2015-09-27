using System;
using System.Reflection;
using System.Windows;

namespace MikuMikuLipMaker
{
    /// <summary>
    /// VersionDialog の View クラス。
    /// </summary>
    public partial class VersionDialog : Window
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public VersionDialog()
        {
            InitializeComponent();

            var name = Assembly.GetExecutingAssembly().GetName();

            // バージョン情報設定
            this.DataContext =
                new
                {
                    Name = name.Name,
                    Version = name.Version.ToString(3),
                    Author = @"ルーチェ",
                    Address = @"http://www.ruche-home.net",
                };
        }
    }
}
