﻿using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace ruche.mmd.morph
{
    /// <summary>
    /// モーフ名とそのウェイト値を保持する構造体。
    /// </summary>
    [DataContract(Namespace = "")]
    public class MorphWeightData : INotifyPropertyChanged, ICloneable
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MorphWeightData()
        {
        }

        /// <summary>
        /// モーフ名を取得または設定する。
        /// </summary>
        [DataMember]
        public string MorphName
        {
            get { return _morphName; }
            set
            {
                var v = value ?? "";
                if (v != _morphName)
                {
                    _morphName = v;
                    this.OnPropertyChanged("MorphName");
                }
            }
        }
        private string _morphName = "";

        /// <summary>
        /// ウェイト値を取得または設定する。
        /// </summary>
        [DataMember]
        public float Weight
        {
            get { return _weight; }
            set
            {
                var v = Math.Min(Math.Max(0.0f, value), 1.0f);
                if (v != _weight)
                {
                    _weight = v;
                    this.OnPropertyChanged("Weight");
                }
            }
        }
        private float _weight = 1.0f;

        /// <summary>
        /// プロパティの変更時に呼び出されるイベント。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 自身のクローンを作成する。
        /// </summary>
        /// <returns>自身のクローン。</returns>
        public MorphWeightData Clone()
        {
            return
                new MorphWeightData
                {
                    MorphName = this.MorphName,
                    Weight = this.Weight,
                };
        }

        /// <summary>
        /// プロパティの変更時に呼び出される。
        /// </summary>
        /// <param name="propertyName">プロパティ名。</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region ICloneable の明示的実装

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
    }
}
