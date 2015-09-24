using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ruche.mmd.morph;
using ruche.mmd.morph.converters;

namespace ruche.mmd.service.lip
{
    /// <summary>
    /// KeyFrames コマンドの情報を保持するクラス。
    /// </summary>
    [DataContract(Namespace = "")]
    [KnownType(typeof(MorphTimelineTable))]
    public class KeyFramesCommandParameter
    {
        /// <summary>
        /// モーフ別タイムラインを取得または設定する。
        /// </summary>
        [DataMember]
        public MorphTimelineTable TimelineTable { get; set; }

        /// <summary>
        /// ユニット基準長(「ア」の長さ)に相当する秒数値を取得または設定する。
        /// </summary>
        [DataMember]
        public decimal UnitSeconds { get; set; }

        /// <summary>
        /// キーフレームリストの先頭と終端で、含まれている全モーフのウェイト値を
        /// ゼロ初期化するか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsEdgeWeightZero { get; set; }

        /// <summary>
        /// クライアント側が対応していれば、キーフレームリスト挿入位置前後の
        /// ウェイト値を保持するか否かを取得または設定する。
        /// </summary>
        [DataMember]
        public bool IsEdgeWeightHeld { get; set; }

        /// <summary>
        /// ユニット基準長(「ア」の長さ)に相当するフレーム長を算出する。
        /// </summary>
        /// <param name="fps">FPS値。</param>
        /// <returns>フレーム値。</returns>
        public decimal CalcUnitFrameLength(decimal fps)
        {
            if (fps <= 0)
            {
                throw new ArgumentOutOfRangeException("fps");
            }

            return (this.UnitSeconds * fps);
        }

        /// <summary>
        /// 設定内容を基にキーフレームリストを作成する。
        /// </summary>
        /// <param name="fps">FPS値。</param>
        /// <param name="beginFrame">開始フレーム位置。</param>
        /// <param name="morphFrameWeightGetter">
        /// 挿入位置前後のウェイト値を保持するために用いる、
        /// 指定モーフ名＆指定フレーム位置のウェイト値提供デリゲート。
        /// 非対応ならば null 。
        /// IsEdgeWeightHeld プロパティ値が false の場合は無視される。
        /// </param>
        /// <returns>キーフレームリスト。</returns>
        public KeyFrameList MakeKeyFrames(
            decimal fps,
            long beginFrame,
            Func<string, long, float> morphFrameWeightGetter)
        {
            // キーフレームリスト作成
            var maker =
                new KeyFrameListMaker
                {
                    UnitFrameLength = this.CalcUnitFrameLength(fps),
                    IsEdgeWeightZero = this.IsEdgeWeightZero,
                };
            var keyFrames = maker.Make(this.TimelineTable, beginFrame);

            // 必要ならウェイト値修正
            if (
                keyFrames.Count > 0 &&
                this.IsEdgeWeightHeld &&
                morphFrameWeightGetter != null)
            {
                ModifyKeyFramesForEdgeWeightHeld(
                    keyFrames,
                    this.TimelineTable.MorphNames,
                    morphFrameWeightGetter);
            }

            return keyFrames;
        }

        /// <summary>
        /// ウェイト値保持のためにキーフレームリストを修正する。
        /// </summary>
        /// <param name="keyFrames">修正対象のキーフレームリスト。</param>
        /// <param name="morphNames">対象モーフ名列挙。</param>
        /// <param name="morphFrameWeightGetter">
        /// 指定モーフ名＆指定フレーム位置のウェイト値提供デリゲート。
        /// </param>
        private static void ModifyKeyFramesForEdgeWeightHeld(
            KeyFrameList keyFrames,
            IEnumerable<string> morphNames,
            Func<string, long, float> morphFrameWeightGetter)
        {
            // 先頭と終端、およびその次のキーフレーム位置取得
            long frameBegin, frameEnd;
            long? frameBeginNext, frameEndNext;
            GetEdgeAndNextFrames(
                keyFrames,
                out frameBegin,
                out frameEnd,
                out frameBeginNext,
                out frameEndNext);

            // 先頭と末尾のキーフレーム追加or修正
            AddOrModifyKeyFrames(
                keyFrames,
                morphNames,
                frameBegin,
                morphFrameWeightGetter);
            if (frameEnd != frameBegin)
            {
                AddOrModifyKeyFrames(
                    keyFrames,
                    morphNames,
                    frameEnd,
                    morphFrameWeightGetter);
            }

            // 先頭および末尾の次のフレームがあるなら
            // キーフレーム登録されていないモーフのウェイト値を 0 にする
            if (frameBeginNext.HasValue)
            {
                AddZeroWeightKeyFrames(keyFrames, morphNames, frameBeginNext.Value);
            }
            if (frameEndNext.HasValue)
            {
                AddZeroWeightKeyFrames(keyFrames, morphNames, frameEndNext.Value);
            }
        }

        /// <summary>
        /// キーフレームリストから、末端および末端の次のキーフレーム位置を取得する。
        /// </summary>
        /// <param name="keyFrames">キーフレームリスト。</param>
        /// <param name="frameBegin">先頭のキーフレーム位置の設定先。</param>
        /// <param name="frameEnd">終端のキーフレーム位置の設定先。</param>
        /// <param name="frameBeginNext">
        /// 先頭の次のキーフレーム位置の設定先。存在しなければ null が設定される。
        /// </param>
        /// <param name="frameEndNext">
        /// 終端の手前のキーフレーム位置の設定先。存在しなければ null が設定される。
        /// </param>
        private static void GetEdgeAndNextFrames(
            KeyFrameList keyFrames,
            out long frameBegin,
            out long frameEnd,
            out long? frameBeginNext,
            out long? frameEndNext)
        {
            frameBeginNext = null;
            frameEndNext = null;

            // 先頭と終端のキーフレーム位置設定
            var begin = frameBegin = keyFrames.Min(f => f.Frame);
            var end = frameEnd = keyFrames.Max(f => f.Frame);

            // 先頭の次のキーフレーム位置があれば設定
            var frames = keyFrames.Where(f => f.Frame > begin);
            if (frames.Any())
            {
                var beginNext = frames.Min(f => f.Frame);
                frameBeginNext = (beginNext < end) ? (long?)beginNext : null;
            }

            // 終端の手前のキーフレーム位置があれば設定
            frames = keyFrames.Where(f => f.Frame < end);
            if (frames.Any())
            {
                var endNext = frames.Max(f => f.Frame);
                frameEndNext = (endNext > begin) ? (long?)endNext : null;
            }
        }

        /// <summary>
        /// キーフレームをまとめて追加またはウェイト値修正する。
        /// </summary>
        /// <param name="keyFrames">対象キーフレームリスト。</param>
        /// <param name="morphNames">対象モーフ名列挙。</param>
        /// <param name="frame">フレーム位置。</param>
        /// <param name="morphFrameWeightGetter">
        /// 指定モーフ名＆指定フレーム位置のウェイト値提供デリゲート。
        /// </param>
        private static void AddOrModifyKeyFrames(
            KeyFrameList keyFrames,
            IEnumerable<string> morphNames,
            long frame,
            Func<string, long, float> morphFrameWeightGetter)
        {
            foreach (var name in morphNames)
            {
                // ウェイト値取得
                var weight = morphFrameWeightGetter(name, frame);

                // 指定フレーム位置のキーフレーム取得
                var frames =
                    keyFrames.Where(f => f.MorphName == name && f.Frame == frame);
                if (frames.Any())
                {
                    // ウェイト値書き換え
                    foreach (var f in frames)
                    {
                        f.Weight = weight;
                    }
                }
                else
                {
                    // 存在しないので追加
                    keyFrames.Add(new KeyFrame(name, frame, weight));
                }
            }
        }

        /// <summary>
        /// ウェイト値が 0 のキーフレームをまとめて追加する。
        /// </summary>
        /// <param name="keyFrames">追加先のキーフレームリスト。</param>
        /// <param name="morphNames">対象モーフ名列挙。</param>
        /// <param name="frame">フレーム位置。</param>
        /// <remarks>
        /// 既に同位置にキーフレームが存在する場合は追加しない。
        /// </remarks>
        private static void AddZeroWeightKeyFrames(
            KeyFrameList keyFrames,
            IEnumerable<string> morphNames,
            long frame)
        {
            keyFrames.AddRange(
                from name in morphNames
                where !keyFrames.Any(f => f.MorphName == name && f.Frame == frame)
                select new KeyFrame(name, frame, 0));
        }
    }
}
