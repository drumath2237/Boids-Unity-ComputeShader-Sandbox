using System;
using BoidsComputeShaderSandbox.Types;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BoidsComputeShaderSandbox.MinimalInvestigation
{
    public struct BoidsOptions
    {
        public int Count { get; set; }
        public Vector3 InitPositionRange { get; set; }
        public float InitMaxVelocity { get; set; }
        public float InitMaxAcceleration { get; set; }

        public void Deconstruct(
            out int count,
            out Vector3 positionRange,
            out float maxVelocity,
            out float maxAcceleration
        )
        {
            count = Count;
            positionRange = InitPositionRange;
            maxVelocity = InitMaxVelocity;
            maxAcceleration = InitMaxAcceleration;
        }
    }

    public struct UpdateParams
    {
        public float AlignWeight { get; set; }
        public float SeparationWeight { get; set; }
        public float CohesionWeight { get; set; }
        public Vector3 BoundarySize { get; set; }
        public float MaxVelocity { get; set; }
        public float MaxAcceleration { get; set; }
        public float InsightRange { get; set; }
        public float FleeThreshold { get; set; }

        public void Deconstruct(
            out float alignWeight,
            out float separationWeight,
            out float cohesionWeight,
            out Vector3 boundarySize,
            out float maxVelocity,
            out float maxAcceleration,
            out float insightRange,
            out float fleeThreshold
        )
        {
            alignWeight = AlignWeight;
            separationWeight = SeparationWeight;
            cohesionWeight = CohesionWeight;
            boundarySize = BoundarySize;
            maxVelocity = MaxVelocity;
            maxAcceleration = MaxAcceleration;
            insightRange = InsightRange;
            fleeThreshold = FleeThreshold;
        }
    }

    public class BoidsCore
    {
        private readonly BoidsData[] _boids;
        public BoidsData[] Boids => _boids;


        public int Count { get; }

        public BoidsCore(BoidsOptions options)
        {
            var (count, initPositionRange, maxVelocity, _) = options;
            Count = count;

            _boids = new BoidsData[Count];

            for (var i = 0; i < Count; i++)
            {
                var maxVelocity3 = new Vector3(maxVelocity, maxVelocity, maxVelocity);
                _boids[i] = new BoidsData
                {
                    Position = RandomVector3(-initPositionRange, initPositionRange),
                    Velocity = RandomVector3(-maxVelocity3, maxVelocity3)
                };
            }
        }

        public void Update(float deltaTime, UpdateParams updateParams)
        {
            var (
                _, _, _,
                boundarySize,
                maxVelocity,
                maxAcceleration,
                _, _
                ) = updateParams;
            var accelerations = new Vector3[Count];

            for (var i = 0; i < Count; i++)
            {
                accelerations[i] =
                    CalcIndividualAccChange(_boids.AsSpan(), i, updateParams);
            }

            for (var i = 0; i < Count; i++)
            {
                accelerations[i] = LimitVector(accelerations[i], maxAcceleration);

                var vel = _boids[i].Velocity + accelerations[i] * deltaTime;
                vel = LimitVector(vel, maxVelocity);

                var pos = _boids[i].Position + vel * deltaTime;

                var boidsData = new BoidsData(pos, vel);
                _boids[i] = BorderTreatment(boidsData, boundarySize);
            }
        }

        /// <summary>
        /// 個別のboidに対してシミュレーションの更新を行い
        /// 次のフレームの加速度を出力する関数
        /// </summary>
        /// <param name="boids"></param>
        /// <param name="index">計算対象のboidsのindex</param>
        /// <param name="updateParams"></param>
        /// <returns></returns>
        private static Vector3 CalcIndividualAccChange(
            ReadOnlySpan<BoidsData> boids,
            int index,
            UpdateParams updateParams
        )
        {
            var (
                alignWeight,
                separationWeight,
                cohesionWeight,
                _, _, _,
                insightRange,
                fleeThreshold
                ) = updateParams;

            var alignForce = AlignForce(boids, insightRange, index);
            var separationForce = SeparationForce(boids, insightRange, index, fleeThreshold);
            var cohesionForce = CohesionForce(boids, insightRange, index);

            return
                alignForce * alignWeight
                + separationForce * separationWeight
                + cohesionForce * cohesionWeight;
        }

        /// <summary>
        /// 整列。影響範囲の平均速度ベクトルへの自分の速度ベクトルへの差分を算出する
        /// </summary>
        /// <param name="boids"></param>
        /// <param name="range"></param>
        /// <param name="index"></param>
        /// <returns>整列処理によって算出されたaccの差分</returns>
        private static Vector3 AlignForce(
            ReadOnlySpan<BoidsData> boids,
            float range,
            int index
        )
        {
            var sumOfVelocities = Vector3.zero;
            var insightCount = 0;
            for (var i = 0; i < boids.Length; i++)
            {
                if (i == index || !WithinRange(boids[index], boids[i], range))
                {
                    continue;
                }

                sumOfVelocities += boids[i].Velocity;
                insightCount++;
            }

            if (insightCount == 0)
            {
                return Vector3.zero;
            }

            var averageVelocity = sumOfVelocities / insightCount;
            return averageVelocity - boids[index].Velocity;
        }

        /// <summary>
        /// 分離。
        /// 範囲内の中のさらに分離閾値内にあるboidに対して
        /// 回避するような加速度を算出する。
        /// </summary>
        /// <param name="boids"></param>
        /// <param name="range"></param>
        /// <param name="index"></param>
        /// <param name="separationThreshold"></param>
        /// <returns></returns>
        private static Vector3 SeparationForce(
            ReadOnlySpan<BoidsData> boids,
            float range,
            int index,
            float separationThreshold
        )
        {
            var fleeForce = Vector3.zero;
            for (var i = 0; i < boids.Length; i++)
            {
                if (i == index || !WithinRange(boids[index], boids[i], range))
                {
                    continue;
                }

                var dirPosition = boids[i].Position - boids[index].Position;
                var distance = dirPosition.sqrMagnitude;
                if (distance >= separationThreshold * separationThreshold)
                {
                    continue;
                }

                fleeForce += -(dirPosition - boids[index].Velocity);
            }

            return fleeForce;
        }

        /// <summary>
        /// 結合。
        /// 範囲内の平均Positionに対して近づいていくような
        /// 加速度を算出する。
        /// </summary>
        /// <param name="boids"></param>
        /// <param name="range"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static Vector3 CohesionForce(
            ReadOnlySpan<BoidsData> boids,
            float range,
            int index
        )
        {
            var positionSum = Vector3.zero;
            var sumCount = 0;
            for (var i = 0; i < boids.Length; i++)
            {
                if (i == index || !WithinRange(boids[index], boids[i], range))
                {
                    continue;
                }

                positionSum += boids[i].Position;
                sumCount++;
            }

            if (sumCount == 0)
            {
                return Vector3.zero;
            }

            var averagePosition = positionSum / sumCount;
            var dirPosition = averagePosition - boids[index].Position;
            var seekForce = dirPosition - boids[index].Velocity;

            return seekForce;
        }

        private static BoidsData BorderTreatment(
            BoidsData boids,
            Vector3 boundary
        )
        {
            var (pos, vel) = boids;

            if (pos.x > boundary.x)
            {
                pos = WithX(pos, boundary.x);
                vel = WithX(vel, -vel.x);
            }
            else if (pos.x < -boundary.x)
            {
                pos = WithX(pos, -boundary.x);
                vel = WithX(vel, -vel.x);
            }

            if (pos.y > boundary.y)
            {
                pos = WithY(pos, boundary.y);
                vel = WithY(vel, -vel.y);
            }
            else if (pos.y < -boundary.y)
            {
                pos = WithY(pos, -boundary.y);
                vel = WithY(vel, -vel.y);
            }

            if (pos.z > boundary.z)
            {
                pos = WithZ(pos, boundary.z);
                vel = WithZ(vel, -vel.z);
            }
            else if (pos.z < -boundary.z)
            {
                pos = WithZ(pos, -boundary.z);
                vel = WithZ(vel, -vel.z);
            }

            return new BoidsData(pos, vel);
        }

        private static Vector3 WithX(Vector3 vec, float x)
            => new(x, vec.y, vec.z);

        private static Vector3 WithY(Vector3 vec, float y)
            => new(vec.x, y, vec.z);

        private static Vector3 WithZ(Vector3 vec, float z)
            => new(vec.x, vec.y, z);


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

        private static bool WithinRange(BoidsData self, BoidsData target, float range)
            => WithinRange(self.Position, target.Position, range);

        /// <summary>
        /// ベクトル長がmaxLengthを超えてたら
        /// 長さをmaxLengthに抑える関数
        /// </summary>
        /// <param name="target">処理対象のベクトル</param>
        /// <param name="maxLength">最大長</param>
        /// <returns>cropされたベクトル</returns>
        private static Vector3 LimitVector(Vector3 target, float maxLength)
            => target.sqrMagnitude <= maxLength * maxLength
                ? target
                : target * (maxLength / target.sqrMagnitude);

        private static Vector3 RandomVector3(Vector3 min, Vector3 max)
            => new(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z)
            );
    }
}