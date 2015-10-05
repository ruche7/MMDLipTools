using System;
using System.ComponentModel;

namespace ruche.mmd.morph.lip
{
    /// <summary>
    /// リップシンクユニット情報を保持するクラス。
    /// </summary>
    public class LipSyncUnit : ICloneable
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public LipSyncUnit() : this(LipId.Closed, LinkType.Normal, 0)
        {
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="lipId">口形状種別ID。</param>
        /// <param name="linkType">前の音からの繋ぎ方を表す列挙値。</param>
        /// <param name="lengthPercent">
        /// フレーム長の基準値に対するパーセント値。
        /// </param>
        public LipSyncUnit(
            LipId lipId,
            LinkType linkType,
            int lengthPercent)
        {
            this.LipId = lipId;
            this.LinkType = linkType;
            this.LengthPercent = lengthPercent;
        }

        /// <summary>
        /// 口形状種別IDを取得または設定する。
        /// </summary>
        public LipId LipId
        {
            get { return _lipId; }
            set
            {
                if (!Enum.IsDefined(typeof(LipId), value))
                {
                    throw new InvalidEnumArgumentException(
                        nameof(value),
                        (int)value,
                        value.GetType());
                }
                _lipId = value;
            }
        }
        private LipId _lipId = LipId.Closed;

        /// <summary>
        /// 前の音からの繋ぎ方を表す列挙値を取得または設定する。
        /// </summary>
        public LinkType LinkType
        {
            get { return _linkType; }
            set
            {
                if (!Enum.IsDefined(typeof(LinkType), value))
                {
                    throw new InvalidEnumArgumentException(
                        nameof(value),
                        (int)value,
                        value.GetType());
                }
                _linkType = value;
            }
        }
        private LinkType _linkType = LinkType.Normal;

        /// <summary>
        /// フレーム長の基準値に対するパーセント値を取得または設定する。
        /// 前後の音などにより実際の長さは増減する場合がある。
        /// </summary>
        public int LengthPercent { get; set; }

        /// <summary>
        /// このオブジェクトの文字列表現を作成する。
        /// </summary>
        /// <returns>文字列表現。</returns>
        public override string ToString()
        {
            // 口を閉じている場合は長さに応じた '＿', '_', '.' の列挙を返す
            if (this.LipId == LipId.Closed)
            {
                string result = string.Empty;
                var len = this.LengthPercent;
                result += new String('＿', len / 100);
                len %= 100;
                result += new String('_', len / 50);
                len %= 50;
                if (len >= 25 || result.Length <= 0)
                {
                    result += ".";
                }
                return result;
            }

            // 繋ぎ方による処理分け
            switch (this.LinkType)
            {
            case LinkType.Tsu:
                // 促音
                return KanaDefine.GetSmallTsuLetters()[0].ToString();

            case LinkType.LongSound:
                // 長音
                return KanaDefine.GetLongSoundLetters()[0].ToString();

            case LinkType.Keep:
                // 持続
                return (this.LengthPercent < 100) ? "・" : "…";

            default:
                // アイウエオ
                bool preClose = (this.LinkType == LinkType.PreClose);
                bool preHalf = (this.LinkType == LinkType.PreHalfClose);
                if (this.LipId == LipId.A)
                {
                    return preClose ? "マ" : preHalf ? "ワ" : "ア";
                }
                if (this.LipId == LipId.I)
                {
                    return preClose ? "ミ" : preHalf ? "ヰ" : "イ";
                }
                if (this.LipId == LipId.U)
                {
                    return preClose ? "ム" : "ウ";
                }
                if (this.LipId == LipId.E)
                {
                    return preClose ? "メ" : preHalf ? "ヱ" : "エ";
                }
                if (this.LipId == LipId.O)
                {
                    return preClose ? "モ" : preHalf ? "ヲ" : "オ";
                }
                break;
            }

            return " ";
        }

        /// <summary>
        /// 自身の内容を別のオブジェクトにコピーする。
        /// </summary>
        /// <param name="dest">コピー先。</param>
        public void CopyTo(LipSyncUnit dest)
        {
            if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }
            dest.CloneCore(this);
        }

        /// <summary>
        /// 自身のクローンを生成する。
        /// </summary>
        /// <returns>自身のクローン。</returns>
        public LipSyncUnit Clone()
        {
            var obj = CreateInstance();
            obj.CloneCore(this);
            return obj;
        }

        /// <summary>
        /// 自身の新しいインスタンスを生成する。
        /// </summary>
        /// <returns>インスタンス。</returns>
        /// <remarks>
        /// 派生先のクラスが引数なしの公開コンストラクタを持たない場合、
        /// このメソッドをオーバライドする必要がある。
        /// </remarks>
        protected virtual LipSyncUnit CreateInstance() =>
            (LipSyncUnit)Activator.CreateInstance(GetType());

        /// <summary>
        /// クローン元から内容を自身にコピーする。
        /// </summary>
        /// <param name="src">クローン元。</param>
        /// <remarks>
        /// 派生先のクラスでコピーすべきメンバが増えた場合、
        /// このメソッドをオーバライドする必要がある。
        /// </remarks>
        protected virtual void CloneCore(LipSyncUnit src)
        {
            this.LipId = src.LipId;
            this.LinkType = src.LinkType;
            this.LengthPercent = src.LengthPercent;
        }

        #region ICloneable の明示的実装

        object ICloneable.Clone() => this.Clone();

        #endregion
    }
}
