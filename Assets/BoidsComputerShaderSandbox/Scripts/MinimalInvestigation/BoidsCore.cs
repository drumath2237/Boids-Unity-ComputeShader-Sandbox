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

            _positions = new Vector3[options.Count];
            _velocities = new Vector3[options.Count];
            _accelerations = new Vector3[options.Count];
        }

        public void Update(float deltaTime)
        {
            for (var i = 0; i < count; i++)
            {
                // 内部でCalcIndividualAccを使ってacc配列を更新
            }
            
            // 全部のaccが計算できたらaccをもとにvel,posを更新
        }

        /// <summary>
        /// 個別のboidに対してシミュレーションの更新を行い
        /// 次のフレームの加速度を出力する関数
        /// </summary>
        /// <param name="positions">boidsの位置配列</param>
        /// <param name="velocities">boidsの速度配列</param>
        /// <param name="accelerations">boidsの加速度配列</param>
        /// <param name="index">計算対象のboidsのindex</param>
        /// <param name="deltaTime">前回更新からの経過時間</param>
        /// <returns></returns>
        public static Vector3 CalcIndividualAcc(Vector3[] positions, Vector3[] velocities, Vector3[] accelerations,int index, float deltaTime)
        {
            // todo
            return Vector3.Zero();
        }
        

        private static bool WithinRange(Vector3 self, Vector3 target, float range)
            => (target - self).sqrMagnitude <= range * range;
    }
}