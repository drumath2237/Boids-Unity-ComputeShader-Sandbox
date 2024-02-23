using System.Runtime.InteropServices;
using BoidsComputeShaderSandbox.MinimalInvestigation;
using BoidsComputeShaderSandbox.Types;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace BoidsComputeShaderSandbox.VFX
{
    [VFXBinder("Custom/Boids")]
    public class BoidsBinder : VFXBinderBase
    {
        [VFXPropertyBinding("UnityEngine.GraphicsBuffer")]
        public ExposedProperty boidsBufferProperty;

        [VFXPropertyBinding("System.Int32")]
        public ExposedProperty boidsCountProperty;

        [SerializeField]
        private int boidsCount;

        [Space, Header("Update Info")]
        [SerializeField]
        private float insightRange = 3f;

        [SerializeField]
        private float maxVelocity = 0.1f;

        [SerializeField]
        private float maxAcceleration = 0.1f;

        [SerializeField]
        private Vector3 boundarySize;

        [SerializeField]
        private float timeScale = 1f;

        [SerializeField]
        private float fleeThreshold = 1f;

        [Space, Header("Force Weights")]
        [SerializeField]
        private float alignWeight = 1f;

        [SerializeField]
        private float separationWeight = 1f;

        [SerializeField]
        private float cohesionWeight = 1f;

        private BoidsCore _boidsCore;

        private GraphicsBuffer _boidsGraphicsBuffer;

        protected override void OnEnable()
        {
            base.OnEnable();

            _boidsCore = new BoidsCore(new BoidsOptions
            {
                Count = boidsCount,
                InitPositionRange = boundarySize,
                InitMaxAcceleration = maxAcceleration,
                InitMaxVelocity = maxVelocity
            });

            // var count = Marshal.SizeOf<BoidsData>() * boidsCount;
            _boidsGraphicsBuffer =
                new GraphicsBuffer(GraphicsBuffer.Target.Structured, boidsCount, Marshal.SizeOf<BoidsData>());
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _boidsCore = null;
            _boidsGraphicsBuffer?.Release();
            _boidsGraphicsBuffer?.Dispose();
            _boidsGraphicsBuffer = null;
        }

        public override bool IsValid(VisualEffect component)
        {
            var isCountValid = boidsCount > 0;
            var isPropertyExist = component.HasGraphicsBuffer(boidsBufferProperty)
                                  && component.HasInt(boidsCountProperty);

            return isCountValid && isPropertyExist;
        }

        public override void UpdateBinding(VisualEffect component)
        {
            component.SetInt(boidsCountProperty, boidsCount);

            _boidsCore.Update(Time.deltaTime * timeScale, new UpdateParams
            {
                InsightRange = insightRange,
                MaxVelocity = maxVelocity,
                MaxAcceleration = maxAcceleration,
                BoundarySize = boundarySize,
                FleeThreshold = fleeThreshold,
                AlignWeight = alignWeight,
                SeparationWeight = separationWeight,
                CohesionWeight = cohesionWeight
            });

            if (_boidsGraphicsBuffer == null)
            {
                return;
            }

            _boidsGraphicsBuffer.SetData(_boidsCore.Boids);
            component.SetGraphicsBuffer(boidsBufferProperty, _boidsGraphicsBuffer);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(Vector3.zero, boundarySize * 2);
        }
    }
}