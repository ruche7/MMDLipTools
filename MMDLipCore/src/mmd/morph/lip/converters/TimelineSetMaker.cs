﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ruche.mmd.morph.lip.converters
{
    /// <summary>
    /// 同じ口形状かつ LinkType.Normal のユニットが連続する場合の
    /// 接続ウェイト値を決定するデリゲート。
    /// </summary>
    /// <param name="before">先行ユニット。</param>
    /// <param name="after">後続ユニット。</param>
    /// <returns>接続ウェイト値。</returns>
    public delegate float SameLipLinkWeightDecider(
        LipSyncUnit before,
        LipSyncUnit after);

    /// <summary>
    /// 口形状種別ごとのタイムラインを作成するクラス。
    /// </summary>
    /// <remarks>
    /// ユニット基準長(「ア」の長さ)を 1.0 とする時間単位のタイムラインを作成する。
    /// </remarks>
    public class TimelineSetMaker
    {
        /// <summary>
        /// 前後のユニットと重なる長さの既定パーセント値。
        /// 対象ユニットの長さに対するパーセント値で示す。
        /// </summary>
        public static readonly decimal DefaultLinkLengthPercent = 50;

        /// <summary>
        /// 前後のユニットと重なる長さの最小パーセント値。
        /// 対象ユニットの長さに対するパーセント値で示す。
        /// </summary>
        public static readonly decimal MinLinkLengthPercent = 0.01m;

        /// <summary>
        /// 前後のユニットと重なる長さの最大パーセント値。
        /// 対象ユニットの長さに対するパーセント値で示す。
        /// </summary>
        public static readonly decimal MaxLinkLengthPercent = 100;

        /// <summary>
        /// 長音の最大開口終端位置におけるウェイト値の既定値。
        /// </summary>
        public static readonly float DefaultLongSoundLastWeight = 1.0f;

        /// <summary>
        /// 同じ口形状かつ LinkType.Normal のユニットが連続する場合の
        /// 接続ウェイト値の既定値を決定する。
        /// </summary>
        /// <param name="before">先行ユニット。</param>
        /// <param name="after">後続ユニット。</param>
        /// <returns>接続ウェイト値の既定値。</returns>
        private static float DecideDefaultSameLipLinkWeight(
            LipSyncUnit before,
            LipSyncUnit after)
        {
            switch (after.LipId)
            {
            case LipId.I:
            case LipId.Closed:
                return 1.0f;

            case LipId.U:
                return 0.9f;
            }

            return 0.7f;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public TimelineSetMaker()
        {
        }

        /// <summary>
        /// 前後のユニットと重なる長さ。対象ユニットの長さに対するパーセント値で示す。
        /// </summary>
        /// <remarks>
        /// <par>
        /// 開口開始からこの長さで最大開口に達し、ユニット本来の長さ分まで開口を続け、
        /// この長さで閉口する。
        /// つまり各ユニットの実動作時間はこの長さ分だけ増えることになる。
        /// </par>
        /// <par>
        /// 例えばあるユニットの本来の長さが 200 ミリ秒で、この値が 25 である場合、
        /// 50 ミリ秒かけて開口し、その後 150 ミリ秒間開口を続け、
        /// 最後に 50 ミリ秒かけて閉口する動作となる。
        /// </par>
        /// </remarks>
        public decimal LinkLengthPercent
        {
            get { return _linkLengthPercent; }
            set
            {
                _linkLengthPercent =
                    Math.Min(
                        Math.Max(MinLinkLengthPercent, value),
                        MaxLinkLengthPercent);
            }
        }
        private decimal _linkLengthPercent = DefaultLinkLengthPercent;

        /// <summary>
        /// 長音の最大開口終端位置におけるウェイト値を取得または設定する。
        /// </summary>
        public float LongSoundLastWeight { get; set; } = DefaultLongSoundLastWeight;

        /// <summary>
        /// タイムラインの先頭と終端を閉口状態とするか否かを取得または設定する。
        /// </summary>
        /// <remarks>
        /// true の場合、タイムライン全体の先頭と終端における閉口タイムラインの
        /// ウェイト値が 1 になります。
        /// false の場合は 0 になります。
        /// </remarks>
        public bool IsEdgeClosed { get; set; } = true;

        /// <summary>
        /// 同じ口形状かつ LinkType.Normal のユニットが連続する場合の
        /// 接続ウェイト値を決定するデリゲートを取得または設定する。
        /// </summary>
        public SameLipLinkWeightDecider SameLipLinkWeightDecider { get; set; } = null;

        /// <summary>
        /// リップシンクユニット列挙から口形状種別ごとのタイムラインを作成する。
        /// </summary>
        /// <param name="units">リップシンクユニット列挙。</param>
        /// <returns>口形状種別ごとのタイムライン。</returns>
        public TimelineSet Make(IEnumerable<LipSyncUnit> units)
        {
            var tlSet = new TimelineSet();

            // タイムラインセット作成
            this.AddAreaUnitLists(tlSet, units);

            // 閉口タイムラインを修正
            this.FixClosedTimeline(tlSet);

            return tlSet;
        }

        /// <summary>
        /// リップシンクユニット列挙を各々キー領域に変換してタイムラインに追加する。
        /// </summary>
        /// <param name="tlSet">追加先のタイムラインセット。</param>
        /// <param name="units">リップシンクユニット列挙。</param>
        private void AddAreaUnitLists(
            TimelineSet tlSet,
            IEnumerable<LipSyncUnit> units)
        {
            var areaUnits = new List<LipSyncUnit>();
            LipSyncUnit prevUnit = null;
            decimal place = 0;

            foreach (var unit in units)
            {
                // 次の音に入ったか？
                if (unit.LinkType.HasSingleSound())
                {
                    // ここまでの音をタイムラインに追加
                    place = AddAreaUnitList(tlSet, place, areaUnits, prevUnit, unit);
                    prevUnit = areaUnits.FirstOrDefault();
                    areaUnits.Clear();
                }

                // キー領域ユニット追加
                areaUnits.Add(unit);
            }

            // 最後の音をタイムラインに追加
            AddAreaUnitList(tlSet, place, areaUnits, prevUnit, null);
        }

        /// <summary>
        /// ユニットリストをキー領域に変換してタイムラインに追加する。
        /// </summary>
        /// <param name="tlSet">追加先のタイムラインセット。</param>
        /// <param name="beginPlace">開始キー位置。</param>
        /// <param name="areaUnits">ユニットリスト。</param>
        /// <param name="prevUnit">先行発声ユニット。無いならば null 。</param>
        /// <param name="nextUnit">後続発声ユニット。無いならば null 。</param>
        /// <returns>後続のキー領域を開始すべきキー位置。</returns>
        private decimal AddAreaUnitList(
            TimelineSet tlSet,
            decimal beginPlace,
            IList<LipSyncUnit> areaUnits,
            LipSyncUnit prevUnit,
            LipSyncUnit nextUnit)
        {
            if (areaUnits.Count <= 0)
            {
                return beginPlace;
            }

            // 最初のユニットと口形状種別IDを取得
            var unit = areaUnits[0];
            var id = unit.LipId;

            var area = new TimelineKeyArea();

            // 開口および閉口の長さを算出
            var openCloseLen = CalcOpenCloseLength(unit);

            // 開始キーを追加
            var openHalfPos = beginPlace + openCloseLen / 2;
            if (prevUnit != null && unit.LinkType != LinkType.Normal)
            {
                // 一旦口を(半分)閉じるならば開口時間の1/2まで経過してから開口開始
                area.SetWeight(openHalfPos, 0);
            }
            else if (prevUnit != null && prevUnit.LipId == id)
            {
                // 直前と同じ口形状ならば開口時間の1/2まで経過してから開口開始
                // 接続ウェイト値は直前の口形状の終端部で設定済み
                area.SetWeight(openHalfPos, 0);
            }
            else
            {
                area.SetWeight(beginPlace, 0);
            }

            // 最大開口開始キーを追加
            var maxBeginPos = beginPlace + openCloseLen;
            area.SetWeight(maxBeginPos, 1);

            // 次の音開始位置(＝最大開口終了位置)を算出
            var linkPos = maxBeginPos + CalcMaxOpenLength(unit);

            // 最初のユニットをカット
            var units = areaUnits.Skip(1);

            // 次のユニットを取得
            unit = units.FirstOrDefault();

            // 最大開口終端位置追加済みフラグ
            bool maxEndAdded = false;

            // 長音部分を計算
            if (unit != null && unit.LinkType == LinkType.LongSound)
            {
                // 次の音開始位置をシフト
                foreach (
                    var u in units.TakeWhile(u => u.LinkType == LinkType.LongSound))
                {
                    linkPos += CalcUnitLength(u);
                }

                // 長音終端キーを追加
                area.SetWeight(linkPos, this.LongSoundLastWeight);
                maxEndAdded = true;

                // 長音部分をカット
                units = units.SkipWhile(u => u.LinkType == LinkType.LongSound);

                // 次のユニットを取得
                unit = units.FirstOrDefault();
            }

            // 促音部分を計算
            if (unit != null && unit.LinkType == LinkType.Tsu)
            {
                // 次の音開始位置をシフト
                foreach (
                    var u in units.TakeWhile(u => u.LinkType == LinkType.Tsu))
                {
                    linkPos += CalcUnitLength(u);
                }

                // 促音終端キーを追加
                area.SetWeight(linkPos, area.Points.Last().Value);
                maxEndAdded = true;

                // 長音部分をカット
                units = units.SkipWhile(u => u.LinkType == LinkType.Tsu);

                // 次のユニットを取得
                unit = units.FirstOrDefault();
            }

            // 最大開口終端キーが追加されていなければ追加
            if (!maxEndAdded)
            {
                area.SetWeight(linkPos, area.Points.Last().Value);
            }

            // 継続部分を計算
            // 継続より後ろの長音や促音も継続扱い
            if (unit != null)
            {
                // 次の音開始位置をシフト
                foreach (var u in units)
                {
                    linkPos += CalcUnitLength(u);
                }
            }

            // 終端キーを追加
            var endPos = linkPos + openCloseLen;
            if (nextUnit != null && nextUnit.LinkType != LinkType.Normal)
            {
                // 一旦口を閉じるならば次の音の開口時間の1/2だけ終端を早める
                var closeHalfPos = endPos - CalcOpenCloseLength(nextUnit) / 2;
                closeHalfPos = Math.Max(closeHalfPos, linkPos);
                area.SetWeight(closeHalfPos, 0);
            }
            else if (
                nextUnit != null &&
                nextUnit.LipId == id &&
                nextUnit.LinkType == LinkType.Normal)
            {
                // 後続と同じ口形状の場合

                // 次の音の開口時間の1/2だけ終端を早める
                var closeHalfPos = endPos - CalcOpenCloseLength(nextUnit) / 2;
                closeHalfPos = Math.Max(closeHalfPos, linkPos);

                // 接続ウェイト値を終端値とする
                // 最大開口終端キーのウェイト値に比例させる(長音対応)
                var weight = DecideSameLipLinkWeight(areaUnits[0], nextUnit);
                weight *= area.Points.Last().Value;

                area.SetWeight(closeHalfPos, weight);
            }
            else
            {
                area.SetWeight(endPos, 0);
            }

            // タイムラインに追加
            tlSet[id].KeyAreas.Add(area);

            // 次の音の開始位置を返す
            return linkPos;
        }

        /// <summary>
        /// 同じ口形状かつ LinkType.Normal のユニットが連続する場合の
        /// 接続ウェイト値を決定する。
        /// </summary>
        /// <param name="before">先行ユニット。</param>
        /// <param name="after">後続ユニット。</param>
        /// <returns>接続ウェイト値。</returns>
        private float DecideSameLipLinkWeight(
            LipSyncUnit before,
            LipSyncUnit after)
        {
            return (this.SameLipLinkWeightDecider == null) ?
                DecideDefaultSameLipLinkWeight(before, after) :
                this.SameLipLinkWeightDecider(before, after);
        }

        /// <summary>
        /// 開口および閉口の長さを算出する。
        /// </summary>
        /// <param name="unit">ユニット。</param>
        /// <returns>開口および閉口の長さ。</returns>
        private decimal CalcOpenCloseLength(LipSyncUnit unit) =>
            (unit.LengthPercent * this.LinkLengthPercent / 10000);

        /// <summary>
        /// ユニットの開口開始位置から最大開口終了位置までの長さを算出する。
        /// </summary>
        /// <param name="unit">ユニット。</param>
        /// <returns>開口開始位置から最大開口終了位置までの長さ。</returns>
        private decimal CalcUnitLength(LipSyncUnit unit) =>
            (unit.LengthPercent / 100m);

        /// <summary>
        /// ユニットの最大開口の長さを算出する。
        /// </summary>
        /// <param name="unit">ユニット。</param>
        /// <returns>最大開口の長さ。</returns>
        private decimal CalcMaxOpenLength(LipSyncUnit unit) =>
            (CalcUnitLength(unit) - CalcOpenCloseLength(unit));

        /// <summary>
        /// 閉口タイムラインを修正する。
        /// </summary>
        /// <param name="tlSet">修正対象のタイムラインセット。</param>
        private void FixClosedTimeline(TimelineSet tlSet)
        {
            // 閉口抜きのタイムライン配列作成
            var tlAiueo = (
                from it in tlSet
                where it.Key != LipId.Closed
                select it.Value)
                .ToArray();

            // 全キー位置と閉口以外のウェイト値合計のテーブル作成
            var points =
                new SortedList<decimal, float>(
                    tlSet.GetAllPlaces().ToDictionary(
                        p => p,
                        p => tlAiueo.Sum(tl => tl.GetWeight(p))));

            // 閉口タイムラインをクリア
            tlSet.Closed.KeyAreas.Clear();

            // 閉口タイムラインにキー領域を追加するデリゲート作成
            Action<TimelineKeyArea> areaAdder =
                a =>
                {
                    // キーが足りない or ウェイト値がすべて 0 なら追加しない
                    if (a.Points.Count < 2 || a.Points.All(pw => pw.Value <= 0))
                    {
                        return;
                    }

                    // エッジを閉口状態としないなら先頭と終端のウェイト値を 0 にする
                    if (!this.IsEdgeClosed)
                    {
                        a.Points[a.BeginPlace] = 0;
                        a.Points[a.EndPlace] = 0;
                    }

                    // 追加
                    tlSet.Closed.KeyAreas.Add(a);
                };

            var area = new TimelineKeyArea();

            // 閉口タイムラインを作成
            foreach (var pw in points)
            {
                // (1 - 閉口以外のウェイト値合計) を閉口ウェイト値とする
                var weight = Math.Max(0, 1 - pw.Value);

                if (weight > 0)
                {
                    // キー追加
                    area.SetWeight(pw.Key, weight);
                }
                else
                {
                    // 終端キー追加
                    area.SetWeight(pw.Key, 0);

                    // 閉口タイムラインにキー領域を追加
                    areaAdder(area);

                    // 新しいキー領域を作成開始
                    area = new TimelineKeyArea();
                    area.SetWeight(pw.Key, 0);
                }
            }

            // 閉口タイムラインに最後のキー領域を追加
            areaAdder(area);
        }
    }
}
