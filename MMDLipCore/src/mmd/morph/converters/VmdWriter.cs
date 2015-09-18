using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ruche.mmd.morph.converters
{
    /// <summary>
    /// モーフのキーフレーム列挙からVMDフォーマットデータを作成して書き出すクラス。
    /// </summary>
    public class VmdWriter : MotionDataWriterBase
    {
        /// <summary>
        /// CodePage932エンコード。
        /// </summary>
        private static readonly Encoding CP932 = Encoding.GetEncoding(932);

        /// <summary>
        /// ヘッダの固定データ。
        /// </summary>
        private static readonly byte[] HeaderData =
            MakeStringBytes("Vocaloid Motion Data 0002", 30);

        /// <summary>
        /// 文字列からCodePage932エンコードのバイト列を作成する。
        /// </summary>
        /// <param name="src">文字列。</param>
        /// <param name="fixedSize">固定サイズ。固定しないならば -1 。</param>
        /// <returns>CodePage932エンコードのバイト列。</returns>
        private static byte[] MakeStringBytes(string src, int fixedSize = -1)
        {
            return MakeStringBytes(src, CP932, fixedSize);
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public VmdWriter()
        {
        }

        /// <summary>
        /// VMDフォーマットデータに書き出すモデル名。
        /// </summary>
        public string ModelName
        {
            get { return _modelName; }
            set { _modelName = value ?? ""; }
        }
        private string _modelName = "";

        /// <summary>
        /// Write メソッドの実書き出し処理を行う。
        /// </summary>
        /// <param name="stream">ストリーム。</param>
        /// <param name="keyFrames">基準フレーム補正済みのキーフレーム列挙。</param>
        protected override void WriteCore(Stream stream, IEnumerable<KeyFrame> keyFrames)
        {
            var frames = keyFrames.ToList();

            // uint.MaxValue より大きいフレーム値は受け付けない
            if (frames.Count > 0 && frames[frames.Count - 1].Frame > uint.MaxValue)
            {
                throw new ArgumentException(
                    "Frames with too large value are contained in `keyFrames`.",
                    "keyFrames");
            }

            // 書き出し
            using (var writer = new BinaryWriter(stream))
            {
                // ヘッダ
                writer.Write(HeaderData);
                writer.Write(MakeStringBytes(this.ModelName, 20));

                // ボーン(要素数0)
                writer.Write(0U);

                // モーフ
                writer.Write((uint)frames.Count);
                frames.ForEach(
                    src =>
                    {
                        writer.Write(MakeStringBytes(src.MorphName, 15));
                        writer.Write((uint)src.Frame);
                        writer.Write(src.Weight);
                    });

                // カメラ、照明、セルフシャドウ、IK(それぞれ要素数0)
                writer.Write(0U);
                writer.Write(0U);
                writer.Write(0U);
                writer.Write(0U);
            }
        }
    }
}
