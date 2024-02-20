using UnityEngine;
using UnityEngine.VFX;

namespace BoidsComputeShaderSandbox.Types
{
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer, "BoidsData")]
    public struct BoidsData
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 Acceleration;

        public BoidsData(Vector3 position, Vector3 velocity, Vector3 acceleration)
        {
            Position = position;
            Velocity = velocity;
            Acceleration = acceleration;
        }

        public void Deconstruct(
            out Vector3 position,
            out Vector3 velocity,
            out Vector3 acceleration
        )
        {
            position = Position;
            velocity = Velocity;
            acceleration = Acceleration;
        }
    }
}