using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BoidsComputeShaderSandbox.MinimalInvestigation
{
    public struct BoidsOptions
    {
        public int Count { get; set; }
        public Vector3 BoundingSize { get; set; }
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

        public Vector3[] Positions => _positions;
        public Vector3[] Velocities => _velocities;

        public int Count => _options.Count;
        public Vector3 BoundingSize => _options.BoundingSize;
        public float InsightRange => _options.InsightRange;
        public float MaxVelocity => _options.MaxVelocity;
        public float MaxAcceleration => _options.MaxAcceleration;

        public BoidsCore(BoidsOptions options)
        {
            _options = options;

            // todo: 初期位置と初期速度はランダムなVec3を生成
            _positions = new Vector3[Count];
            _velocities = new Vector3[Count];
            _accelerations = new Vector3[Count];

            for (var i = 0; i < Count; i++)
            {
                _positions[i] = RandomVector3(-BoundingSize, BoundingSize);
                var maxVelocity3 = new Vector3(MaxVelocity, MaxVelocity, MaxVelocity);
                _velocities[i] = RandomVector3(-maxVelocity3, maxVelocity3);
            }
        }

        public void Update(float deltaTime)
        {
            for (var i = 0; i < Count; i++)
            {
                _accelerations[i] += CalcIndividualAccChange(
                    _positions.AsSpan(),
                    _velocities.AsSpan(),
                    InsightRange,
                    i,
                    deltaTime
                );
            }

            for (var i = 0; i < Count; i++)
            {
                // todo: 速度制限や境界処理の追加
                _accelerations[i] = LimitVector(_accelerations[i], MaxAcceleration);

                _velocities[i] += _accelerations[i];
                _velocities[i] = LimitVector(_velocities[i], MaxVelocity);

                _positions[i] += _velocities[i];

                BorderTreatment(BoundingSize, i);

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
            // todo: 処理を実装
            return Vector3.zero;
        }

        private static Vector3 CohesionForce(
            ReadOnlySpan<Vector3> positions,
            ReadOnlySpan<Vector3> velocities,
            float range,
            int index,
            float deltaTime
        )
        {
            // todo: 処理を実装
            return Vector3.zero;
        }

        private void BorderTreatment(
            Vector3 boundingSize,
            int index
        )
        {
            var pos = _positions[index];
            var vel = _velocities[index];

            if (pos.x > boundingSize.x)
            {
                pos = WithX(pos, boundingSize.x);
                vel = WithX(vel, -vel.x);
            }
            else if (pos.x < -boundingSize.x)
            {
                pos = WithX(pos, -boundingSize.x);
                vel = WithX(vel, -vel.x);
            }

            if (pos.y > boundingSize.y)
            {
                pos = WithY(pos, boundingSize.y);
                vel = WithY(vel, -vel.y);
            }
            else if (pos.y < -boundingSize.y)
            {
                pos = WithY(pos, -boundingSize.y);
                vel = WithY(vel, -vel.y);
            }

            if (pos.z > boundingSize.z)
            {
                pos = WithZ(pos, boundingSize.z);
                vel = WithZ(vel, -vel.z);
            }
            else if (pos.z < -boundingSize.z)
            {
                pos = WithZ(pos, -boundingSize.z);
                vel = WithZ(vel, -vel.z);
            }

            _positions[index] = pos;
            _velocities[index] = vel;
        }

        private static Vector3 WithX(Vector3 vec, float x)
            => new Vector3(x, vec.y, vec.z);

        private static Vector3 WithY(Vector3 vec, float y)
            => new Vector3(vec.x, y, vec.z);

        private static Vector3 WithZ(Vector3 vec, float z)
            => new Vector3(vec.x, vec.y, z);


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

        /// <summary>
        /// ベクトル長がmaxLengthを超えてたら
        /// 長さをmaxLengthに抑える関数
        /// </summary>
        /// <param name="target">処理対象のベクトル</param>
        /// <param name="maxLength">最大長</param>
        /// <returns>cropされたベクトル</returns>
        private static Vector3 LimitVector(Vector3 target, float maxLength)
            => target.sqrMagnitude <= maxLength * maxLength ? target : target * (maxLength / target.sqrMagnitude);

        private static Vector3 RandomVector3(Vector3 min, Vector3 max)
            => new(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z)
            );
    }
}