using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

namespace ruche.util
{
    /// <summary>
    /// 各プロパティの属性を基に設定値の読み書きを行う静的クラス。
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// 設定値ディクショナリを作成する。
        /// </summary>
        /// <param name="src">設定値の読み取り元。</param>
        /// <returns>設定値ディクショナリ。</returns>
        public static ResourceDictionary Make(object src)
        {
            var dest = new ResourceDictionary();

            var props =
                src.GetType().GetProperties(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (prop.IsDefined(typeof(ConfigValueAttribute), false))
                {
                    var value = prop.GetValue(src, null);
                    dest.Add(
                        prop.Name,
                        (value is ICloneable) ? ((ICloneable)value).Clone() : value);
                }
                else if (prop.IsDefined(typeof(ConfigValueContainerAttribute), false))
                {
                    dest.Add(prop.Name, Make(prop.GetValue(src, null)));
                }
            }

            return dest;
        }

        /// <summary>
        /// 設定値ディクショナリの内容を適用する。
        /// </summary>
        /// <param name="src">設定値ディクショナリ。</param>
        /// <param name="dest">設定値の適用先。</param>
        public static void Apply(ResourceDictionary src, object dest)
        {
            var props =
                src.GetType().GetProperties(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (prop.IsDefined(typeof(ConfigValueAttribute), false))
                {
                    if (src.Contains(prop.Name))
                    {
                        prop.SetValue(src, src[prop.Name], null);
                    }
                }
                else if (prop.IsDefined(typeof(ConfigValueContainerAttribute), false))
                {
                    if (
                        src.Contains(prop.Name) &&
                        (src[prop.Name] is ResourceDictionary))
                    {
                        var value = prop.GetValue(src, null);
                        Apply(src[prop.Name] as ResourceDictionary, value);
                        prop.SetValue(src, value, null);
                    }
                }
            }
        }

        /// <summary>
        /// 設定値をファイルへ書き出す。
        /// </summary>
        /// <param name="src">設定値の読み取り元。</param>
        /// <param name="filePath">書き出し先ファイルパス。</param>
        public static void Save(object src, string filePath)
        {
            // 親ディレクトリ作成
            var parentDirPath = Path.GetDirectoryName(Path.GetFullPath(filePath));
            if (!Directory.Exists(parentDirPath))
            {
                Directory.CreateDirectory(parentDirPath);
            }

            // 保存
            using (var output = File.Create(filePath))
            {
                Save(src, output);
            }
        }

        /// <summary>
        /// 設定値をストリームへ書き出す。
        /// </summary>
        /// <param name="src">設定値の読み取り元。</param>
        /// <param name="output">書き出し先ストリーム。</param>
        public static void Save(object src, Stream output)
        {
            var xmlSettings = new XmlWriterSettings { Indent = true };
            using (var writer = XmlWriter.Create(output, xmlSettings))
            {
                XamlWriter.Save(Make(src), writer);
            }
        }

        /// <summary>
        /// 設定値をファイルから読み取る。
        /// </summary>
        /// <param name="filePath">読み取り元ファイル。</param>
        /// <param name="dest">設定値の適用先。</param>
        /// <returns>適用できたならば true 。そうでなければ false 。</returns>
        public static bool Load(string filePath, object dest)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            using (var input = File.OpenRead(filePath))
            {
                return Load(input, dest);
            }
        }

        /// <summary>
        /// 設定値をストリームから読み取る。
        /// </summary>
        /// <param name="input">読み取り元ストリーム。</param>
        /// <param name="dest">設定値の適用先。</param>
        /// <returns>適用できたならば true 。そうでなければ false 。</returns>
        public static bool Load(Stream input, object dest)
        {
            ResourceDictionary src = null;

            try
            {
                src = XamlReader.Load(input) as ResourceDictionary;
            }
            catch
            {
                return false;
            }

            if (src == null)
            {
                return false;
            }
            Apply(src, dest);

            return true;
        }
    }
}
