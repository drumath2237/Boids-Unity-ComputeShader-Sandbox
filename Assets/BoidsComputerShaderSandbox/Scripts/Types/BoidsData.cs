using UnityEngine;
using UnityEngine.VFX;

namespace BoidsComputeShaderSandbox.Types
{
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer, "BoidsData")]
    public struct BoidsData
    {
        public Vector3 Position;
        public Vector3 Velocity;

        public BoidsData(Vector3 position, Vector3 velocity)
        {
            Position = position;
            Velocity = velocity;
        }

        public void Deconstruct(
            out Vector3 position,
            out Vector3 velocity
        )
        {
            position = Position;
            velocity = Velocity;
        }
    }
}