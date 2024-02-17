using System;
using UnityEngine;

namespace BoidsComputeShaderSandbox.MinimalInvestigation
{
    public struct BoidsOptions
    {
        public int Count { get; set; }
        public float BoundingWidth { get; set; }
        public float BoundingHeight { get; set; }
        public float MaxVelocity { get; set; }
        public float MaxAcceleration { get; set; }
        public float InsightRange { get; set; }
    }

    public class BoidsCore
    {
        private readonly BoidsOptions _options;
        private readonly Vector3[] _positions;
        private readonly Vector3[] _velocities;
        private readonly Vector3[] _accelerations;

        public BoidsCore(BoidsOptions options)
        {
            _options = options;

            // todo: 初期位置と初期速度はランダムなVec3を生成
            _positions = new Vector3[options.Count];
            _velocities = new Vector3[options.Count];
            _accelerations = new Vector3[options.Count];
        }

        public void Update(float deltaTime)
        {
            for (var i = 0; i < _options.Count; i++)
            {
                _accelerations[i] += CalcIndividualAccChange(
                    _positions.AsSpan(),
                    _velocities.AsSpan(),
                    _options.InsightRange,
                    i,
                    deltaTime
                );
            }

            for (var i = 0; i < _options.Count; i++)
            {
                // todo: 速度制限や境界処理の追加
                _velocities[i] += _accelerations[i];
                _positions[i] += _velocities[i];
                _accelerations[i] = Vector3.zero;
            }
        }

        /// <summary>
        /// 個別のboidに対してシミュレーションの更新を行い
        /// 次のフレームの加速度を出力する関数
        /// </summary>
        /// <param name="positions">boidsの位置配列</param>
        /// <param name="velocities">boidsの速度配列</param>
        /// <param name="range">影響を与えるboidの範囲</param>
        /// <param name="index">計算対象のboidsのindex</param>
        /// <param name="deltaTime">前回更新からの経過時間</param>
        /// <returns></returns>
        private static Vector3 CalcIndividualAccChange(
            ReadOnlySpan<Vector3> positions,
            ReadOnlySpan<Vector3> velocities,
            float range,
            int index,
            float deltaTime
        )
        {
            var alignForce = AlignForce(positions, velocities, range, index, deltaTime);
            var separationForce = SeparationForce(positions, velocities, range, index, deltaTime);
            var cohesionForce = CohesionForce(positions, velocities, range, index, deltaTime);

            return alignForce + separationForce + cohesionForce;
        }

        /// <summary>
        /// 整列。影響範囲の平均速度ベクトルへの自分の速度ベクトルへの差分を算出する
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="velocities"></param>
        /// <param name="range"></param>
        /// <param name="index"></param>
        /// <param name="deltaTime"></param>
        /// <returns>整列処理によって算出されたaccの差分</returns>
        private static Vector3 AlignForce(
            ReadOnlySpan<Vector3> positions,
            ReadOnlySpan<Vector3> velocities,
            float range,
            int index,
            float deltaTime
        )
        {
            if (positions.Length != velocities.Length)
            {
                return Vector3.zero;
            }

            var sumOfVelocities = new Vector3();
            var insightCount = 0;
            for (var i = 0; i < velocities.Length; i++)
            {
                if (i == index || !WithinRange(positions[index], positions[i], range))
                {
                    continue;
                }

                sumOfVelocities += velocities[i];
                insightCount++;
            }

            if (insightCount == 0)
            {
                return Vector3.zero;
            }

            var averageVelocity = sumOfVelocities / insightCount;
            return averageVelocity - velocities[index];
        }

        private static Vector3 SeparationForce(
            ReadOnlySpan<Vector3> positions,
            ReadOnlySpan<Vector3> velocities,
            float range,
            int index,
            float deltaTime
        )
        {
            throw new NotImplementedException();
        }

        private static Vector3 CohesionForce(
            ReadOnlySpan<Vector3> positions,
            ReadOnlySpan<Vector3> velocities,
            float range,
            int index,
            float deltaTime
        )
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 対象のベクトルの指定範囲内に
        /// ベクトルが入っているかの判定
        /// </summary>
        /// <param name="self">対象のベクトル</param>
        /// <param name="target">検査対象のベクトル</param>
        /// <param name="range">指定範囲</param>
        /// <returns>判定結果</returns>
        private static bool WithinRange(Vector3 self, Vector3 target, float range)
            => (target - self).sqrMagnitude <= range * range;
    }
}