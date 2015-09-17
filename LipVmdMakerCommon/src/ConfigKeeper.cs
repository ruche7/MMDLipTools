using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace ruche.mmd.tools
{
    /// <summary>
    /// 設定の読み書きを行うクラス。
    /// </summary>
    /// <typeparam name="T">設定値の型。</typeparam>
    public class ConfigKeeper<T>
    {
        /// <summary>
        /// 設定ファイルパスの既定値。
        /// </summary>
        private static readonly string DefaultFilePath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                @"ruche-home\MMDLipTools",
                typeof(T).FullName + ".config");

        /// <summary>
        /// シリアライザを生成する。
        /// </summary>
        /// <returns>シリアライザ。</returns>
        private static XmlObjectSerializer MakeSerializer()
        {
            return new DataContractJsonSerializer(typeof(T));
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public ConfigKeeper()
        {
            this.Value = default(T);
        }

        /// <summary>
        /// 設定値を取得または設定する。
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 設定ファイルパスを取得または設定する。
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value ?? DefaultFilePath; }
        }
        private string _filePath = DefaultFilePath;

        /// <summary>
        /// 設定を読み取る。
        /// </summary>
        /// <returns>成功したならば true 。失敗したならば false 。</returns>
        public bool Load()
        {
            // ファイルがなければ読み取れない
            if (!File.Exists(this.FilePath))
            {
                return false;
            }

            try
            {
                // 読み取り
                using (var stream = File.OpenRead(this.FilePath))
                {
                    var serializer = MakeSerializer();
                    var value = serializer.ReadObject(stream);
                    if (!(value is T))
                    {
                        return false;
                    }
                    this.Value = (T)value;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 設定を書き出す。
        /// </summary>
        /// <returns>成功したならば true 。失敗したならば false 。</returns>
        public bool Save()
        {
            try
            {
                // 親ディレクトリ作成
                var dirPath = Path.GetDirectoryName(Path.GetFullPath(this.FilePath));
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                // 書き出し
                using (var stream = File.Create(this.FilePath))
                {
                    var serializer = MakeSerializer();
                    serializer.WriteObject(stream, this.Value);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
