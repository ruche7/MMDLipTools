using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ruche.mmd.morph.converters
{
    /// <summary>
    /// モーフのキーフレーム列挙からモーションデータを作成して書き出す抽象クラス。
    /// </summary>
    public abstract class MotionDataWriterBase
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        protected MotionDataWriterBase()
        {
        }

        /// <summary>
        /// キーフレーム列挙からモーションデータを作成し、ストリームへ書き出す。
        /// </summary>
        /// <param name="stream">ストリーム。</param>
        /// <param name="keyFrames">キーフレーム列挙。</param>
        /// <returns>書き出されたキーフレーム数。</returns>
        /// <remarks>
        /// フレーム位置が負数のキーフレームは書き出されない。
        /// </remarks>
        public int Write(Stream stream, IEnumerable<KeyFrame> keyFrames)
        {
            return this.Write(stream, keyFrames, 0);
        }

        /// <summary>
        /// キーフレーム列挙からモーションデータを作成し、ストリームへ書き出す。
        /// </summary>
        /// <param name="stream">ストリーム。</param>
        /// <param name="keyFrames">キーフレーム列挙。</param>
        /// <param name="baseFrame">基準フレーム位置。</param>
        /// <returns>書き出されたキーフレーム数。</returns>
        /// <remarks>
        /// 基準フレーム位置を 0 フレーム目として書き出す。
        /// 基準フレーム位置よりも前のキーフレームは書き出されない。
        /// </remarks>
        public int Write(
            Stream stream,
            IEnumerable<KeyFrame> keyFrames,
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
            var frames =
                from f in keyFrames
                where f.Frame >= baseFrame
                orderby f.Frame
                select new KeyFrame(f.MorphName, f.Frame - baseFrame, f.Weight);

            // 書き出し
            this.WriteCore(stream, frames);

            return frames.Count();
        }

        /// <summary>
        /// Write メソッドの実書き出し処理を行う。
        /// </summary>
        /// <param name="stream">ストリーム。</param>
        /// <param name="keyFrames">基準フレーム補正済みのキーフレーム列挙。</param>
        protected abstract void WriteCore(
            Stream stream,
            IEnumerable<KeyFrame> keyFrames);

        /// <summary>
        /// 文字列から指定エンコードのバイト列を作成する。
        /// </summary>
        /// <param name="src">文字列。</param>
        /// <param name="encoding">エンコード。</param>
        /// <returns>バイト列。</returns>
        protected static byte[] MakeStringBytes(string src, Encoding encoding)
        {
            return MakeStringBytes(src, encoding, -1);
        }

        /// <summary>
        /// 文字列から指定エンコードのバイト列を作成する。
        /// </summary>
        /// <param name="src">文字列。</param>
        /// <param name="encoding">エンコード。</param>
        /// <param name="fixedSize">固定サイズ。固定しないならば -1 。</param>
        /// <returns>バイト列。</returns>
        protected static byte[] MakeStringBytes(
            string src,
            Encoding encoding,
            int fixedSize)
        {
            if (src == null)
            {
                throw new ArgumentNullException("src");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            var dest = encoding.GetBytes(src);

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
