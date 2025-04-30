using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace APIs.CSharps
{
    [Serializable]
    public struct ParticleBase
    {
        //basic simulation
        public Vector3 position;
        public Vector3 velocity;
        public float age;
        public float lifetime;
        public int alive;
        public float size;

        //advanced simulation
        public float mass;
        public Vector3 direction;
        public Vector3 angle;
        public Vector3 angularVelocity;
        public Vector3 oldPosition;
        public Vector3 targetPosition;

        //rendering
        public float alpha;
        public Vector3 color;
        public float renderingSize;
        public Vector3 scale;
        public Vector3 pivot;
        public float texIndex;
        public Vector3 axisX;
        public Vector3 axisY;
        public Vector3 axisZ;

        //system
        public uint particleId;
        public uint seed;
        public float spawnCount;
        public float spawnTime;
        public uint particleIndexInStrip;
    }

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

        private void Start()
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

        private void OnDestroy()
        {
            ParticleBuffer.Release();
            PooledParticleBuffer.Release();
            ParticleCountBuffer.Release();
        }
    }
}