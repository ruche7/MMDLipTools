﻿using System;
using System.Runtime.Serialization;

namespace ruche.mmd.gui.lip
{
    /// <summary>
    /// 口パクモーフプリセット編集設定クラス。
    /// </summary>
    [DataContract(Namespace = "")]
    [KnownType(typeof(MorphPresetList))]
    public class MorphPresetConfig : IExtensibleDataObject
    {
        /// <summary>
        /// 既定の口パクモーフプリセットリスト。
        /// </summary>
        private static readonly MorphPresetList DefaultPresets =
            new MorphPresetList(new[] { new MorphPreset() });

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MorphPresetConfig()
        {
            this.ActivePresetName = this.Presets[0].Name;
        }

        /// <summary>
        /// 口パクモーフプリセットリストを取得または設定する。
        /// </summary>
        /// <remarks>
        /// 要素数 1 未満にはならない。
        /// </remarks>
        [DataMember]
        public MorphPresetList Presets
        {
            get { return _presets; }
            set
            {
                var v = value ?? DefaultPresets.Clone();
                if (v != _presets)
                {
                    // 空っぽならデフォルト値追加
                    if (v.Count == 0)
                    {
                        v.Add(new MorphPreset());
                    }

                    _presets = v;
                }
            }
        }
        private MorphPresetList _presets = DefaultPresets.Clone();

        /// <summary>
        /// アクティブな口パクモーフプリセット名を取得または設定する。
        /// </summary>
        [DataMember]
        public string ActivePresetName { get; set; }

        #region IExtensibleDataObject の明示的実装

        ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }

        #endregion
    }
}
