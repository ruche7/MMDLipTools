﻿using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace ruche.mmd.tools
{
    /// <summary>
    /// 口パクモーフVMDファイル保存設定クラス。
    /// </summary>
    [DataContract(Namespace = "")]
    public class LipVmdConfig : IExtensibleDataObject
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public LipVmdConfig()
        {
            this.ActiveFilePath = "";
            this.BaseDirectoryPath = "";
            this.IsOverwriteConfirmed = true;
        }

        /// <summary>
        /// VMDファイルの保存先ファイルパスリストを取得または設定する。
        /// </summary>
        [DataMember]
        public ObservableCollection<string> FilePathes
        {
            get { return _filePathes; }
            set
            {
                var v = value ?? (new ObservableCollection<string>());
                if (v != _filePathes)
                {
                    // 空文字列アイテムをすべて削除後、先頭に追加
                    while (v.Remove(""))
                    {
                        ;
                    }
                    v.Insert(0, "");

                    _filePathes = v;
                }
            }
        }
        private ObservableCollection<string> _filePathes =
            new ObservableCollection<string>(new[] { "" });

        /// <summary>
        /// アクティブなVMDファイルの保存先ファイルパスを取得または設定する。
        /// </summary>
        [DataMember]
        public string ActiveFilePath { get; set; }

        /// <summary>
        /// VMDファイルの保存先選択時の既定のディレクトリパスを取得または設定する。
        /// </summary>
        [DataMember]
        public string BaseDirectoryPath { get; set; }

        /// <summary>
        /// VMDファイルの上書き確認表示を行うか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsOverwriteConfirmed { get; set; }

        #region IExtensibleDataObject の明示的実装

        ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }

        #endregion
    }
}
