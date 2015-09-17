using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ruche.mmd.morph.converters
{
    /// <summary>
    /// モーフのVMDフォーマットデータを作成して書き出す静的クラス。
    /// </summary>
    public static class VmdWriter
    {
        /// <summary>
        /// CodePage932エンコード。
        /// </summary>
        private static readonly Encoding CP932 = Encoding.GetEncoding(932);

        /// <summary>
        /// ヘッダの固定データ。
        /// </summary>
        private static readonly byte[] HeaderData =
            MakeStringData("Vocaloid Motion Data 0002", 30);

        /// <summary>
        /// キーフレーム列挙からVMDフォーマットデータを作成し、ストリームへ書き出す。
        /// </summary>
        /// <param name="stream">ストリーム。</param>
        /// <param name="keyFrames">キーフレーム列挙。</param>
        /// <param name="modelName">モデル名。 null ならば空文字列扱い。</param>
        /// <returns>書き出されたキーフレーム数。</returns>
        /// <remarks>
        /// フレーム位置が負数のキーフレームは書き出されない。
        /// </remarks>
        public static int Write(
            Stream stream,
            IEnumerable<KeyFrame> keyFrames,
            string modelName)
        {
            return Write(stream, keyFrames, modelName, 0);
        }

        /// <summary>
        /// キーフレーム列挙からVMDフォーマットデータを作成し、ストリームへ書き出す。
        /// </summary>
        /// <param name="stream">ストリーム。</param>
        /// <param name="keyFrames">キーフレーム列挙。</param>
        /// <param name="modelName">モデル名。 null ならば空文字列扱い。</param>
        /// <param name="baseFrame">基準フレーム位置。</param>
        /// <returns>書き出されたキーフレーム数。</returns>
        /// <remarks>
        /// 基準フレーム位置を 0 フレーム目として書き出す。
        /// 基準フレーム位置よりも前のキーフレームは書き出されない。
        /// </remarks>
        public static int Write(
            Stream stream,
            IEnumerable<KeyFrame> keyFrames,
            string modelName,
            long baseFrame)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (!stream.CanWrite)
            {
                throw new ArgumentException("`stream` is not writable.", "stream");
            }
            if (keyFrames == null)
            {
                throw new ArgumentNullException("keyFrames");
            }

            // baseFrame 基準のキーフレームリスト作成
            var srcs = (
                from f in keyFrames
                where f.Frame >= baseFrame
                orderby f.Frame
                select new KeyFrame(f.MorphName, f.Frame - baseFrame, f.Weight))
                .ToList();
            if (srcs.Count > 0 && srcs[srcs.Count - 1].Frame > uint.MaxValue)
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
                writer.Write(MakeStringData(modelName ?? "", 20));

                // ボーン(要素数0)
                writer.Write(0U);

                // モーフ
                writer.Write((uint)srcs.Count);
                srcs.ForEach(
                    src =>
                    {
                        writer.Write(MakeStringData(src.MorphName, 15));
                        writer.Write((uint)src.Frame);
                        writer.Write(src.Weight);
                    });

                // カメラ、照明、セルフシャドウ、IK(それぞれ要素数0)
                writer.Write(0U);
                writer.Write(0U);
                writer.Write(0U);
                writer.Write(0U);
            }

            return srcs.Count;
        }

        /// <summary>
        /// 文字列からCodePage932エンコードのバイト列を作成する。
        /// </summary>
        /// <param name="src">文字列。</param>
        /// <param name="fixedSize">固定サイズ。固定しないならば -1 。</param>
        /// <returns>CodePage932エンコードのバイト列。</returns>
        private static byte[] MakeStringData(string src, int fixedSize = -1)
        {
            var dest = CP932.GetBytes(src);

            if (fixedSize >= 0 && dest.Length != fixedSize)
            {
                var temp = new byte[fixedSize];
                Buffer.BlockCopy(dest, 0, temp, 0, Math.Min(fixedSize, dest.Length));
                dest = temp;
            }

            return dest;
        }
    }
}
