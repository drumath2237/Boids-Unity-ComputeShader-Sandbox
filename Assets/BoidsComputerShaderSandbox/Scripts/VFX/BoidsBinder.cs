using System.Runtime.InteropServices;
using BoidsComputeShaderSandbox.Types;
using Unity.Collections;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
using Random = UnityEngine.Random;

namespace BoidsComputeShaderSandbox.VFX
{
    [VFXBinder("Custom/Boids")]
    public class BoidsBinder : VFXBinderBase
    {
        [VFXPropertyBinding("UnityEngine.GraphicsBuffer")]
        public ExposedProperty boidsBufferProperty;

        [VFXPropertyBinding("System.Int32")]
        public ExposedProperty boidsCountProperty;

        [Header("Init Info")]
        [SerializeField]
        private ComputeShader boidsComputeShader;

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

        [SerializeField]
        private float wallForceWight = 1f;

        [SerializeField]
        private float wallDistanceWeight = 1f;

        private GraphicsBuffer _boidsGraphicsBuffer;
        private int? _csMainKernel;

        private static readonly int Data = Shader.PropertyToID("boidsData");

        private static readonly int BoidsCount = Shader.PropertyToID("boidsCount");
        private static readonly int EffectRange = Shader.PropertyToID("effectRange");
        private static readonly int MaxVelocity = Shader.PropertyToID("maxVelocity");
        private static readonly int MaxAcceleration = Shader.PropertyToID("maxAcceleration");
        private static readonly int Boundary = Shader.PropertyToID("boundary");
        private static readonly int DeltaTime = Shader.PropertyToID("deltaTime");

        private static readonly int FleeThreshold = Shader.PropertyToID("fleeThreshold");
        private static readonly int AlignWeight = Shader.PropertyToID("alignWeight");
        private static readonly int SeparationWeight = Shader.PropertyToID("separationWeight");
        private static readonly int CohesionWeight = Shader.PropertyToID("cohesionWeight");
        private static readonly int WallForceWeight = Shader.PropertyToID("wallForceWeight");
        private static readonly int WallDistanceWeight = Shader.PropertyToID("wallDistanceWeight");

        protected override void OnEnable()
        {
            base.OnEnable();

            Vector3 RandomVector3(Vector3 min, Vector3 max) => new(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z)
            );

            Vector3 CreateVector3(float val) => new(val, val, val);

            var boidsDataArray = new NativeArray<BoidsData>(boidsCount, Allocator.Temp);
            for (var i = 0; i < boidsCount; i++)
            {
                boidsDataArray[i] = new BoidsData
                {
                    Velocity = RandomVector3(CreateVector3(-maxVelocity), CreateVector3(maxVelocity)),
                    Position = RandomVector3(-boundarySize, boundarySize)
                };
            }

            _boidsGraphicsBuffer =
                new GraphicsBuffer(GraphicsBuffer.Target.Structured, boidsCount, Marshal.SizeOf<BoidsData>());
            _boidsGraphicsBuffer.SetData(boidsDataArray);

            _csMainKernel = boidsComputeShader.FindKernel("CSMain");

            boidsComputeShader.SetInt(BoidsCount, boidsCount);
            boidsComputeShader.SetBuffer(_csMainKernel.Value, Data, _boidsGraphicsBuffer);

            boidsDataArray.Dispose();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _boidsGraphicsBuffer?.Release();
            _boidsGraphicsBuffer?.Dispose();
            _boidsGraphicsBuffer = null;
        }

        public override bool IsValid(VisualEffect component)
        {
            var isCountValid = boidsCount > 0;
            var isPropertyExist = component.HasGraphicsBuffer(boidsBufferProperty)
                                  && component.HasInt(boidsCountProperty);
            var isComputeShaderValid = boidsComputeShader != null
                                       && boidsComputeShader.HasKernel("CSMain");

            if (_csMainKernel == null)
            {
                return false;
            }

            boidsComputeShader.GetKernelThreadGroupSizes(_csMainKernel.Value, out var x, out _, out _);
            var isBoidsCountCanDivideWithNumThreads = boidsCount % x == 0;

            return isCountValid
                   && isPropertyExist
                   && isComputeShaderValid
                   && isBoidsCountCanDivideWithNumThreads
                ;
        }

        public override void UpdateBinding(VisualEffect component)
        {
            component.SetInt(boidsCountProperty, boidsCount);

            if (_boidsGraphicsBuffer == null || _csMainKernel == null)
            {
                return;
            }

            boidsComputeShader.SetFloat(EffectRange, insightRange);
            boidsComputeShader.SetFloat(MaxVelocity, maxVelocity);
            boidsComputeShader.SetFloat(MaxAcceleration, maxAcceleration);
            boidsComputeShader.SetVector(Boundary, boundarySize);
            boidsComputeShader.SetFloat(DeltaTime, Time.deltaTime * timeScale);
            boidsComputeShader.SetFloat(FleeThreshold, fleeThreshold);
            boidsComputeShader.SetFloat(AlignWeight, alignWeight);
            boidsComputeShader.SetFloat(SeparationWeight, separationWeight);
            boidsComputeShader.SetFloat(CohesionWeight, cohesionWeight);
            boidsComputeShader.SetFloat(WallForceWeight, wallForceWight);
            boidsComputeShader.SetFloat(WallDistanceWeight, wallDistanceWeight);

            boidsComputeShader.GetKernelThreadGroupSizes(_csMainKernel.Value, out var x, out _, out _);
            boidsComputeShader.Dispatch(_csMainKernel.Value, boidsCount / (int)x, 1, 1);

            component.SetGraphicsBuffer(boidsBufferProperty, _boidsGraphicsBuffer);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(Vector3.zero, boundarySize * 2);
        }
    }
}