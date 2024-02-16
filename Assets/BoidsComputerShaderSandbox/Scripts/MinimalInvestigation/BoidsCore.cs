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

        private static bool WithinRange(Vector3 self, Vector3 target, float range)
            => (target - self).sqrMagnitude <= range * range;
    }
}