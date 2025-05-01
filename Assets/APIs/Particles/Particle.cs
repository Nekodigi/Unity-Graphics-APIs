using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace APIs.Particles
{
    public class ParticleGeneratorBase : MonoBehaviour
    {
        protected const int THREAD_NUM = 8;
        [SerializeField] protected ComputeShader computeShader;
        [SerializeField] protected Material material;

        [SerializeField] protected int maxCount = 100000;
        [SerializeField] protected int emitCount = 10;

        protected ComputeBuffer ParticleBuffer;
        protected uint[] ParticleCount;
        protected ComputeBuffer ParticleCountBuffer;
        protected ParticleBase[] Particles;
        protected ComputeBuffer PooledParticleBuffer;
        protected RenderParams RenderParams;

        protected virtual void Start()
        {
            maxCount = maxCount / THREAD_NUM * THREAD_NUM;
            emitCount = emitCount / THREAD_NUM * THREAD_NUM;

            ParticleBuffer = new ComputeBuffer(maxCount, Marshal.SizeOf(typeof(ParticleBase)));
            Particles = new ParticleBase[maxCount];
            ParticleBuffer.SetData(Particles);

            PooledParticleBuffer = new ComputeBuffer(maxCount, Marshal.SizeOf(typeof(uint)), ComputeBufferType.Append);
            PooledParticleBuffer.SetCounterValue(0);

            ParticleCountBuffer =
                new ComputeBuffer(1, Marshal.SizeOf(typeof(uint)), ComputeBufferType.IndirectArguments);
            ParticleCount = new uint[] { 0 };
            ParticleCountBuffer.SetData(ParticleCount);

            RenderParams = new RenderParams(material);
            RenderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000);
            RenderParams.matProps = new MaterialPropertyBlock();
            RenderParams.matProps.SetBuffer("_ParticleBuffer", ParticleBuffer);
        }

        protected virtual void OnDestroy()
        {
            ParticleBuffer.Release();
            PooledParticleBuffer.Release();
            ParticleCountBuffer.Release();
        }
    }
}