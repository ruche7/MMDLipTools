using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ruche.mmd.morph.lip.converters
{
    /// <summary>
    /// 入力文から口パク用の読み仮名を作成するクラス。
    /// </summary>
    public class LipKanaMaker
    {
        /// <summary>
        /// IMEによって読み仮名を取得してもよい文字列にマッチする正規表現。
        /// </summary>
        private static readonly Regex rexToPhonetic =
            new Regex("[^～‥…―゛ﾞ\u3099゜ﾟ\u309Aゝゞヽヾ]+");

        /// <summary>
        /// 読み仮名コンバータ。
        /// </summary>
        private readonly PhoneticConverter _phoneConv = new PhoneticConverter();

        /// <summary>
        /// 数字列読み仮名コンバータ。
        /// </summary>
        private readonly DigitKanaConverter _digitConv = new DigitKanaConverter();

        /// <summary>
        /// 英字＆記号読み仮名コンバータ。
        /// </summary>
        private readonly AlphaGlyphKanaConverter _alpGlyConv =
            new AlphaGlyphKanaConverter();

        /// <summary>
        /// カタカナコンバータ。
        /// </summary>
        private readonly KatakanaConverter _kataConv = new KatakanaConverter();

        /// <summary>
        /// 入力文から口パク用の読み仮名を作成する。
        /// </summary>
        /// <param name="src">入力文。</param>
        /// <returns>口パク用の読み仮名。</returns>
        /// <remarks>
        /// メインスレッドから呼び出さなければ一部処理が失敗する。
        /// </remarks>
        public string Make(string src)
        {
            return this.StartMake(src).Result;
        }

        /// <summary>
        /// 入力文から口パク用の読み仮名を非同期で作成開始する。
        /// </summary>
        /// <param name="src">入力文。</param>
        /// <returns>口パク用の読み仮名作成タスク。</returns>
        /// <remarks>
        /// メインスレッドから呼び出さなければ一部処理が失敗する。
        /// </remarks>
        public Task<string> StartMake(string src)
        {
            // 読み仮名取得だけはメインスレッドで処理
            var text = rexToPhonetic.Replace(src, m => GetPhonetic(m.Value));

            return
                Task.Factory
                    .StartNew(
                        () =>
                        {
                            // 数字列を読み仮名に変換
                            var dest = _digitConv.ConvertFrom(text);

                            // 英字と記号を読み仮名に変換
                            dest = _alpGlyConv.ConvertFrom(dest);

                            // カタカナに変換。
                            return _kataConv.ConvertFrom(dest);
                        });
        }

        /// <summary>
        /// 文字列の読み仮名を取得する。
        /// </summary>
        /// <param name="src">文字列。</param>
        /// <returns>文字列の読み仮名。</returns>
        private string GetPhonetic(string src)
        {
            string kana = null;

            try
            {
                kana = _phoneConv.ConvertFrom(src);
                if (kana == null)
                {
                    kana = src;
                }
            }
            catch
            {
                kana = src;
            }

            return kana;
        }
    }
}
