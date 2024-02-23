using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace BoidsComputeShaderSandbox.VFX
{
    [VFXBinder("Custom/Boids")]
    public class BoidsBinder : VFXBinderBase
    {
        [VFXPropertyBinding(nameof(UnityEngine.GraphicsBuffer))]
        public ExposedProperty boidsBufferProperty;

        [VFXPropertyBinding(nameof(System.Int32))]
        public ExposedProperty boidsCountProperty;

        [SerializeField]
        private int boidsCount;


        public override bool IsValid(VisualEffect component)
        {
            var isCountValid = boidsCount > 0;
            var isPropertyExist = component.HasGraphicsBuffer(boidsBufferProperty)
                                  && component.HasInt(boidsCountProperty);

            return isCountValid && isPropertyExist;
        }

        public override void UpdateBinding(VisualEffect component)
        {
            throw new System.NotImplementedException();
        }
    }
}