using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Markup;

// アセンブリに関する一般情報は以下の属性セットをとおして制御されます。
// アセンブリに関連付けられている情報を変更するには、
// これらの属性値を変更してください。
[assembly: AssemblyTitle("ruche.wpf")]
[assembly: AssemblyDescription("The library of utility classes for WPF/XAML.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("ruche-home")]
[assembly: AssemblyProduct("ruche.wpf")]
[assembly: AssemblyCopyright("Copyright (C) ruche")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// ComVisible を false に設定すると、その型はこのアセンブリ内で COM コンポーネントから 
// 参照不可能になります。COM からこのアセンブリ内の型にアクセスする場合は、
// その型の ComVisible 属性を true に設定してください。
[assembly: ComVisible(false)]

// 次の GUID は、このプロジェクトが COM に公開される場合の、typelib の ID です
[assembly: Guid("18cec359-7ca0-4c67-aca3-df674b92be14")]

// アセンブリのバージョン情報は、以下の 4 つの値で構成されています:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// すべての値を指定するか、下のように '*' を使ってビルドおよびリビジョン番号を 
// 既定値にすることができます:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.*")]

// XML名前空間
[assembly: XmlnsDefinition("http://ruche-home.net/xaml/wpf", "ruche.wpf")]
[assembly: XmlnsDefinition("http://ruche-home.net/xaml/wpf/converters", "ruche.wpf.converters")]
[assembly: XmlnsDefinition("http://ruche-home.net/xaml/wpf/view", "ruche.wpf.view")]
[assembly: XmlnsDefinition("http://ruche-home.net/xaml/wpf/viewModel", "ruche.wpf.viewModel")]
