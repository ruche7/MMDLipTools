using System;

namespace ruche.mmd.morph.lip.converters
{
    /// <summary>
    /// モーフ別タイムラインテーブルを作成するクラス。
    /// </summary>
    public class MorphTimelineTableMaker
    {
        /// <summary>
        /// 口形状タイムラインセットとモーフ情報セットから
        /// モーフ別タイムラインテーブルを作成する。
        /// </summary>
        /// <param name="tlSet">口形状タイムラインセット。</param>
        /// <param name="morphSet">モーフ情報セット。</param>
        /// <param name="morphEtoAI">
        /// "え" から "あ","い" へのモーフ変更を行うならば true 。
        /// </param>
        /// <returns>モーフ別タイムラインテーブル。</returns>
        public MorphTimelineTable Make(
            TimelineSet tlSet,
            MorphInfoSet morphSet,
            bool morphEtoAI)
        {
            var table = new MorphTimelineTable();

            foreach (var mt in tlSet)
            {
                var timeline = mt.Value;
                var morphInfo = morphSet[mt.Key];

                timeline.KeyAreas.ForEach(
                    area =>
                    {
                        foreach (var mw in morphInfo.MorphWeights)
                        {
                            AddMorphKeyArea(table, area, mw, morphEtoAI);
                        }
                    });
            }

            return table;
        }

        /// <summary>
        /// 口形状キー領域をモーフウェイト値で変換してタイムラインへ追加する。
        /// </summary>
        /// <param name="table">追加先のタイムラインテーブル。</param>
        /// <param name="lipKeyArea">口形状キー領域。</param>
        /// <param name="morphWeight">モーフ名とそのウェイト値。</param>
        /// <param name="morphEtoAI">
        /// "え" から "あ","い" へのモーフ変更を行うならば true 。
        /// </param>
        private void AddMorphKeyArea(
            MorphTimelineTable table,
            TimelineKeyArea lipKeyArea,
            MorphWeightData morphWeight,
            bool morphEtoAI)
        {
            // "え" から "あ","い" への変換を行うか？
            bool e2ai = (morphEtoAI && morphWeight.MorphName == "え");

            // キー領域作成
            var area = lipKeyArea.Clone();
            foreach (var p in lipKeyArea.Points.Keys)
            {
                area.Points[p] *= morphWeight.Weight * (e2ai ? 0.5f : 1.0f);
            }

            // タイムラインに追加
            // モーフ名が空文字列のものは弾く
            if (e2ai)
            {
                table.GetOrAddNew("あ").KeyAreas.Add(area);
                table.GetOrAddNew("い").KeyAreas.Add(area);
            }
            else if (!string.IsNullOrEmpty(morphWeight.MorphName))
            {
                table.GetOrAddNew(morphWeight.MorphName).KeyAreas.Add(area);
            }
        }
    }
}
