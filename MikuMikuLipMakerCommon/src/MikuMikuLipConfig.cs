using System;
using System.ComponentModel;
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
            this.AutoNamingDirectoryPath =
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.IsAutoNamingOverwriteConfirmed = true;
            this.DefaultDirectoryPath = "";
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
                        "value",
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
                        "value",
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
        public string AutoNamingDirectoryPath { get; set; }

        /// <summary>
        /// ファイル自動命名時に上書き確認表示を行うか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsAutoNamingOverwriteConfirmed { get; set; }

        /// <summary>
        /// 名前を付けて保存する際の既定のディレクトリパスを取得または設定する。
        /// </summary>
        [DataMember]
        public string DefaultDirectoryPath { get; set; }

        /// <summary>
        /// 入力文テキストファイルを同時に保存するか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsSavingWithText { get; set; }

        #region IExtensibleDataObject の明示的実装

        ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }

        #endregion
    }
}
