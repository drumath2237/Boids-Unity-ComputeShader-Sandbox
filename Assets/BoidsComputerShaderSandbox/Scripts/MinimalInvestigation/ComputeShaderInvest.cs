using System;
using System.Runtime.InteropServices;
using BoidsComputeShaderSandbox.Types;
using Unity.Collections;
using UnityEngine;

namespace BoidsComputeShaderSandbox.MinimalInvestigation
{
    public class ComputeShaderInvest : MonoBehaviour
    {
        [SerializeField]
        private int boidsCount;

        [SerializeField]
        private ComputeShader boidsComputeShader;

        private static readonly int FillVec = Shader.PropertyToID("fillVec");
        private static readonly int BoidsDataId = Shader.PropertyToID("boidsData");

        private void Start()
        {
            if (boidsComputeShader == null || boidsCount <= 0)
            {
                return;
            }

            if (!boidsComputeShader.HasKernel("CSMain"))
            {
                return;
            }

            using var boidsArray = new NativeArray<BoidsData>(boidsCount, Allocator.Temp);
            using var boidsBuffer =
                new ComputeBuffer(boidsCount, Marshal.SizeOf<BoidsData>(), ComputeBufferType.Structured);
            boidsBuffer.SetData(boidsArray);

            var kernel = boidsComputeShader.FindKernel("CSMain");

            boidsComputeShader.SetVector(FillVec, new Vector3(1, 2, 3));
            boidsComputeShader.SetBuffer(kernel, BoidsDataId, boidsBuffer);

            boidsComputeShader.GetKernelThreadGroupSizes(kernel, out var x, out _, out _);
            boidsComputeShader.Dispatch(kernel, boidsCount / (int)x, 1, 1);

            var result = new BoidsData[boidsBuffer.count];
            boidsBuffer.GetData(result);

            Debug.Log("test");
        }
    }
}