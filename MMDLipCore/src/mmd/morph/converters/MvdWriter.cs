using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ruche.mmd.morph.converters
{
    /// <summary>
    /// モーフのキーフレーム列挙からMVDフォーマットデータを作成して書き出すクラス。
    /// </summary>
    public class MvdWriter : MotionDataWriterBase
    {
        /// <summary>
        /// ヘッダの固定データ。
        /// </summary>
        private static readonly byte[] HeaderData =
            MakeStringBytes("Motion Vector Data file", 30);

        /// <summary>
        /// 文字列からCodePage932エンコードのバイト列を作成する。
        /// </summary>
        /// <param name="src">文字列。</param>
        /// <param name="fixedSize">固定サイズ。固定しないならば -1 。</param>
        /// <returns>CodePage932エンコードのバイト列。</returns>
        private static byte[] MakeStringBytes(string src, int fixedSize = -1)
        {
            return MakeStringBytes(src, Encoding.UTF8, fixedSize);
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MvdWriter()
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="fps">キーフレーム列挙のFPS値。</param>
        public MvdWriter(float fps)
        {
            this.Fps = fps;
        }

        /// <summary>
        /// MVDフォーマットデータに書き出すモデル名。
        /// </summary>
        public string ModelName
        {
            get { return _modelName; }
            set { _modelName = value ?? ""; }
        }
        private string _modelName = "";

        /// <summary>
        /// MVDフォーマットデータに書き出す英語モデル名。
        /// </summary>
        public string EnglishModelName
        {
            get { return _englishModelName; }
            set { _englishModelName = value ?? ""; }
        }
        private string _englishModelName = "";

        /// <summary>
        /// キーフレーム列挙のFPS値を取得または設定する。
        /// </summary>
        public float Fps
        {
            get { return _fps; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _fps = value;
            }
        }
        private float _fps = 30;

        /// <summary>
        /// Write メソッドの実書き出し処理を行う。
        /// </summary>
        /// <param name="stream">ストリーム。</param>
        /// <param name="keyFrames">基準フレーム補正済みのキーフレーム列挙。</param>
        protected override void WriteCore(Stream stream, IEnumerable<KeyFrame> keyFrames)
        {
            // モーフ名別キーフレーム列挙作成
            var frameTable = (from f in keyFrames group f by f.MorphName).ToArray();

            // モデル名のバイト列作成
            var modelNameBytes = MakeStringBytes(this.ModelName);
            var englishModelNameBytes = MakeStringBytes(this.EnglishModelName);

            // 書き出し
            using (var writer = new BinaryWriter(stream))
            {
                // ヘッダ
                writer.Write(HeaderData);
                writer.Write(1.0f);     // version
                writer.Write((byte)1);  // encoding (0:UTF-16LE, 1:UTF-8)
                writer.Write(modelNameBytes.Length);
                writer.Write(modelNameBytes);
                writer.Write(englishModelNameBytes.Length);
                writer.Write(englishModelNameBytes);
                writer.Write(this.Fps);
                writer.Write(0);        // reserved size

                // モーフ名リスト
                writer.Write((byte)0);  // name-list tag
                writer.Write((byte)0);  // name-list minor type
                writer.Write(0);        // reserved
                writer.Write(0);        // reserved
                writer.Write(frameTable.Length);
                writer.Write(0);        // reserved
                for (int i = 0; i < frameTable.Length; ++i)
                {
                    var nameBytes = MakeStringBytes(frameTable[i].Key);

                    writer.Write(i);    // id
                    writer.Write(nameBytes.Length);
                    writer.Write(nameBytes);
                }

                // モーフデータ
                writer.Write((byte)32); // morph-data tag
                writer.Write((byte)1);  // morph-data minor type
                for (int i = 0; i < frameTable.Length; ++i)
                {
                    var frames = frameTable[i].ToArray();

                    writer.Write(i);    // id
                    writer.Write(16);   // item size
                    writer.Write(frames.Length);
                    writer.Write(0);    // reserved
                    foreach (var f in frames)
                    {
                        writer.Write(f.Frame);
                        writer.Write(f.Weight);
                        writer.Write((byte)20);     // interpolation1 x
                        writer.Write((byte)20);     // interpolation1 y
                        writer.Write((byte)107);    // interpolation2 x
                        writer.Write((byte)107);    // interpolation2 y
                    }
                }

                // EOF
                writer.Write((byte)255);    // EOF tag
                writer.Write((byte)0);      // EOF minor type
            }
        }
    }
}
