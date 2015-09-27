using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MikuMikuLipMaker
{
    /// <summary>
    /// LicenseDialog の View クラス。
    /// </summary>
    public partial class LicenseDialog : Window
    {
        /// <summary>
        /// ライセンス情報クラス。
        /// </summary>
        private class Info
        {
            /// <summary>
            /// コンストラクタ。
            /// </summary>
            /// <param name="assembly">アセンブリ。</param>
            /// <param name="license">ライセンス情報。</param>
            /// <param name="address">アドレス情報。</param>
            public Info(Assembly assembly, string license, string address)
            {
                var name = assembly.GetName();
                var productAttrs =
                    assembly.GetCustomAttributes(
                        typeof(AssemblyProductAttribute),
                        false)
                        as AssemblyProductAttribute[];

                this.Name =
                    (productAttrs == null || productAttrs.Length == 0) ?
                        name.Name : productAttrs[0].Product;
                this.Version = name.Version.ToString(3);
                this.License = license;
                this.Address = address;
            }

            /// <summary>
            /// 名前を取得する。
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// バージョン情報を取得する。
            /// </summary>
            public string Version { get; private set; }

            /// <summary>
            /// ライセンス情報を取得する。
            /// </summary>
            public string License { get; private set; }

            /// <summary>
            /// ライセンス情報が有効であるか否かを取得する。
            /// </summary>
            public bool IsLicenseEnabled
            {
                get { return !string.IsNullOrWhiteSpace(this.License); }
            }

            /// <summary>
            /// アドレス情報を取得する。
            /// </summary>
            public string Address { get; private set; }

            /// <summary>
            /// アドレス情報が有効であるか否かを取得する。
            /// </summary>
            public bool IsAddressEnabled
            {
                get { return !string.IsNullOrWhiteSpace(this.Address); }
            }
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public LicenseDialog()
        {
            InitializeComponent();

            // ライセンス表記情報設定
            this.DataContext =
                new[]
                {
                    new Info(
                        Assembly.GetAssembly(typeof(Xceed.Wpf.Toolkit.DecimalUpDown)),
                        @"Microsoft Public License (Ms-PL)",
                        @"http://wpftoolkit.codeplex.com"),
                    new Info(
                        Assembly.GetAssembly(
                            typeof(Microsoft.WindowsAPICodePack.Dialogs.TaskDialog)),
                        @"The MIT License",
                        @"https://github.com/devkimchi/Windows-API-Code-Pack-1.1"),
                };
        }
    }
}
