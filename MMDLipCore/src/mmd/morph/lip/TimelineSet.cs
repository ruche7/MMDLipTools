using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace ruche.mmd.morph.lip
{
    /// <summary>
    /// 口形状種別ごとのタイムラインを保持するクラス。
    /// </summary>
    public class TimelineSet : TimelineTableBase<LipId>
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public TimelineSet()
        {
            var ids = (LipId[])Enum.GetValues(typeof(LipId));
            this.Table = new InnerTable(ids.Length);
            Array.ForEach(ids, id => this.Table.Add(id, new Timeline()));
        }

        /// <summary>
        /// タイムラインを取得または設定するインデクサ。
        /// </summary>
        /// <param name="id">口形状種別ID。</param>
        /// <returns>タイムライン。</returns>
        public override Timeline this[LipId id]
        {
            get
            {
                ValidateLipId(id);
                return this.Table[id];
            }
            set
            {
                ValidateLipId(id);
                this.Table[id] = value ?? (new Timeline());
            }
        }

        /// <summary>
        /// 閉口のタイムラインを取得または設定する。
        /// </summary>
        public Timeline Closed
        {
            get { return this[LipId.Closed]; }
            set { this[LipId.Closed] = value; }
        }

        /// <summary>
        /// 「あ」のタイムラインを取得または設定する。
        /// </summary>
        public Timeline A
        {
            get { return this[LipId.A]; }
            set { this[LipId.A] = value; }
        }

        /// <summary>
        /// 「い」のタイムラインを取得または設定する。
        /// </summary>
        public Timeline I
        {
            get { return this[LipId.I]; }
            set { this[LipId.I] = value; }
        }

        /// <summary>
        /// 「う」のタイムラインを取得または設定する。
        /// </summary>
        public Timeline U
        {
            get { return this[LipId.U]; }
            set { this[LipId.U] = value; }
        }

        /// <summary>
        /// 「え」のタイムラインを取得または設定する。
        /// </summary>
        public Timeline E
        {
            get { return this[LipId.E]; }
            set { this[LipId.E] = value; }
        }

        /// <summary>
        /// 「お」のタイムラインを取得または設定する。
        /// </summary>
        public Timeline O
        {
            get { return this[LipId.O]; }
            set { this[LipId.O] = value; }
        }

        /// <summary>
        /// 口形状種別IDが有効な値か検証する。
        /// </summary>
        /// <param name="id">口形状種別ID。</param>
        /// <exception cref="InvalidEnumArgumentException">
        /// id が有効な値ではない場合。
        /// </exception>
        private void ValidateLipId(LipId id)
        {
            if (!this.Table.ContainsKey(id))
            {
                throw new InvalidEnumArgumentException(
                    nameof(id),
                    (int)id,
                    id.GetType());
            }
        }
    }
}
