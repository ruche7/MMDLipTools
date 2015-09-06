using System;
using System.Runtime.InteropServices;

namespace ruche.mmd.morph.lip.converters
{
    /// <summary>
    /// 文字列を読み仮名に変換するクラス。
    /// </summary>
    public class PhoneticConverter
    {
        #region COM定義

        private const int S_OK = 0;
        private const int FELANG_REQ_REV = 0x00030000;
        private const int FELANG_CMODE_PINYIN = 0x00000100;
        private const int FELANG_CMODE_NOINVISIBLECHAR = 0x40000000;

        /// <summary>
        /// IFELanguage COMインタフェース。
        /// </summary>
        [ComImport]
        [Guid("019F7152-E6DB-11D0-83C3-00C04FDDB82E")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFELanguage
        {
            int Open();
            int Close();

            void dummy1();  // not use
            void dummy2();  // not use

            int GetPhonetic(
                [MarshalAs(UnmanagedType.BStr)] string src,
                int start,
                int length,
                [MarshalAs(UnmanagedType.BStr)] out string dest);
        }

        #endregion

        /// <summary>
        /// 文字列を読み仮名に変換する。
        /// </summary>
        /// <param name="src">文字列。</param>
        /// <returns>文字列の読み仮名。失敗した場合は null 。</returns>
        public string ConvertFrom(string src)
        {
            // IFELanguage COMインタフェース型取得
            Type langType = Type.GetTypeFromProgID("MSIME.Japan");
            if (langType == null)
            {
                throw new NotSupportedException();
            }

            // 型からインスタンスを生成
            var langIf = Activator.CreateInstance(langType) as IFELanguage;
            if (langIf == null)
            {
                throw new NotSupportedException();
            }

            string dest = null;
            bool opened = false;
            try
            {
                // 開く
                if (langIf.Open() != S_OK)
                {
                    throw new NotSupportedException();
                }
                opened = true;

                // 変換
                if (langIf.GetPhonetic(src, 1, -1, out dest) != S_OK)
                {
                    return null;
                }
            }
            finally
            {
                if (opened)
                {
                    // 閉じる
                    langIf.Close();
                }
            }

            return dest;
        }
    }
}
