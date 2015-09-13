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
        private const int CLSCTX_INPROC_SERVER = 1;
        private const int CLSCTX_LOCAL_SERVER = 4;
        private const int CLSCTX_SERVER = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER;

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

        /// <summary>
        /// COMオブジェクトを生成する。
        /// </summary>
        [DllImport("ole32.dll")]
        private static extern int CoCreateInstance(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
            IntPtr pUnkOuter,
            uint dwClsContext,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            out IntPtr ppv);

        #endregion

        /// <summary>
        /// 文字列を読み仮名に変換する。
        /// </summary>
        /// <param name="src">文字列。</param>
        /// <returns>文字列の読み仮名。失敗した場合は null 。</returns>
        /// <remarks>
        /// メインスレッドから呼び出さなければ失敗する。
        /// </remarks>
        public string ConvertFrom(string src)
        {
            // IFELanguage COMインタフェース型取得
            Type langComType = Type.GetTypeFromProgID("MSIME.Japan");
            if (langComType == null)
            {
                throw new NotSupportedException();
            }

            // IFELanguage の Guid 属性から IID 値作成
            var langIfType = typeof(IFELanguage);
            var attrs =
                langIfType.GetCustomAttributes(typeof(GuidAttribute), false)
                    as GuidAttribute[];
            var langIid = new Guid(attrs[0].Value);

            // COMオブジェクト生成
            IntPtr ppv;
            var res =
                CoCreateInstance(
                    langComType.GUID,
                    IntPtr.Zero,
                    CLSCTX_SERVER,
                    langIid,
                    out ppv);
            if (res != S_OK)
            {
                throw new InvalidOperationException();
            }

            // IFELanguage COMインタフェース取得
            var langIfObj = Marshal.GetTypedObjectForIUnknown(ppv, langIfType);
            if (langIfObj == null)
            {
                throw new NotSupportedException();
            }
            var langIf = langIfObj as IFELanguage;

            bool opened = false;
            string dest = null;
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
                // 閉じる
                if (opened)
                {
                    langIf.Close();
                }

                // COMオブジェクトを解放
                Marshal.ReleaseComObject(langIfObj);
            }

            return dest;
        }
    }
}
