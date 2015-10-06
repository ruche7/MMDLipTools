using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;

namespace ruche.mmd.tools
{
    /// <summary>
    /// 口パクモーフモーションデータファイル保存設定クラス。
    /// </summary>
    [DataContract(Namespace = "")]
    public class MikuMikuLipConfig : IExtensibleDataObject
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MikuMikuLipConfig()
        {
        }

        /// <summary>
        /// アクティブなファイルフォーマットを取得または設定する。
        /// </summary>
        /// <remarks>
        /// 自動命名保存時のファイルフォーマットとして使われる。
        /// また、名前を付けて保存する際の既定ファイルフォーマットとしても使われる。
        /// </remarks>
        [DataMember]
        public MotionFileFormat ActiveFileFormat
        {
            get { return _activeFileFormat; }
            set
            {
                if (!Enum.IsDefined(value.GetType(), value))
                {
                    throw new InvalidEnumArgumentException(
                        nameof(value),
                        (int)value,
                        value.GetType());
                }

                _activeFileFormat = value;
            }
        }
        private MotionFileFormat _activeFileFormat = MotionFileFormat.Vmd;

        /// <summary>
        /// ファイル自動命名フォーマットを取得または設定する。
        /// </summary>
        [DataMember]
        public AutoNamingFormat AutoNamingFormat
        {
            get { return _autoNamingFormat; }
            set
            {
                if (!Enum.IsDefined(value.GetType(), value))
                {
                    throw new InvalidEnumArgumentException(
                        nameof(value),
                        (int)value,
                        value.GetType());
                }

                _autoNamingFormat = value;
            }
        }
        private AutoNamingFormat _autoNamingFormat = AutoNamingFormat.Kana;

        /// <summary>
        /// ファイル自動命名時の保存先ディレクトリパスを取得または設定する。
        /// </summary>
        [DataMember]
        public string AutoNamingDirectoryPath { get; set; } =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                @"MMD_Lip");

        /// <summary>
        /// ファイル自動命名時に上書き確認表示を行うか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsAutoNamingOverwriteConfirmed { get; set; } = true;

        /// <summary>
        /// 名前を付けて保存する際の既定のディレクトリパスを取得または設定する。
        /// </summary>
        [DataMember]
        public string DefaultDirectoryPath { get; set; } = "";

        /// <summary>
        /// テキストファイルを同時に保存するか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsSavingWithText { get; set; } = false;

        /// <summary>
        /// クライアント側が対応していれば、キーフレームリスト挿入位置前後の
        /// ウェイト値から自然に繋ぐか否かを取得する。
        /// </summary>
        [DataMember]
        public bool IsNaturalLink { get; set; } = true;

        /// <summary>
        /// クライアント側が対応していれば、キーフレームリスト挿入範囲の
        /// 既存キーフレームを削除して置き換えるか否かを取得する。
        /// </summary>
        [DataMember]
        public bool IsKeyFrameReplacing { get; set; } = true;

        #region IExtensibleDataObject の明示的実装

        ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }

        #endregion
    }
}
