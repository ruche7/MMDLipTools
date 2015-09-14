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

        /// <summary>
        /// COM API での処理成功を表す値。
        /// </summary>
        private const int S_OK = 0;

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
            Type langComType = Type.GetTypeFromProgID("MSIME.Japan");
            if (langComType == null)
            {
                throw new NotSupportedException();
            }

            // IUnknown COMオブジェクト生成
            var langIfUnk = Activator.CreateInstance(langComType);

            IFELanguage langIf = null;
            bool opened = false;
            string dest = null;
            try
            {
                // IFELanguage COMオブジェクト生成
                langIf = langIfUnk as IFELanguage;
                if (langIf == null)
                {
                    // STAThread でメインスレッドから呼ばないとここで失敗する
                    throw new InvalidOperationException();
                }

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
                // 閉じる
                if (opened)
                {
                    langIf.Close();
                }

                // IUnknown COMオブジェクト解放
                Marshal.ReleaseComObject(langIfUnk);
            }

            return dest;
        }
    }
}
