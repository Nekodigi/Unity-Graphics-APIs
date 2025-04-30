using System.Runtime.InteropServices;
using APIs.CSharps;
using UnityEngine;

namespace Examples
{
    public class ParticleGenerator : MonoBehaviour
    {
        private const int THREAD_NUM = 8;
        //     public struct Particle
        // {
        //     public Vector3 position;
        //     public Vector3 velocity;
        //     public float   duration;
        // }

        public ComputeShader computeShader;
        public Mesh mesh;
        public Material material;

        public int maxCount = 100000;
        public int emitCount = 10;
        private RenderParams _renderParams;
        private int kernelIndexEmit;

        private int kernelIndexInitialize;
        private int kernelIndexUpdate;

        private ComputeBuffer particleBuffer;
        private uint[] particleCount;
        private ComputeBuffer particleCountBuffer;

        private ParticleBase[] particles;
        private ComputeBuffer pooledParticleBuffer;

        private void Start()
        {
            maxCount = maxCount / THREAD_NUM * THREAD_NUM;
            emitCount = emitCount / THREAD_NUM * THREAD_NUM;

            kernelIndexInitialize = computeShader.FindKernel("Initialize");
            kernelIndexUpdate = computeShader.FindKernel("Update");
            kernelIndexEmit = computeShader.FindKernel("Emit");

            particleBuffer = new ComputeBuffer(maxCount, Marshal.SizeOf(typeof(ParticleBase)));
            particles = new ParticleBase[maxCount];
            particleBuffer.SetData(particles);

            pooledParticleBuffer = new ComputeBuffer(maxCount, Marshal.SizeOf(typeof(uint)), ComputeBufferType.Append);
            pooledParticleBuffer.SetCounterValue(0);

            particleCountBuffer =
                new ComputeBuffer(1, Marshal.SizeOf(typeof(uint)), ComputeBufferType.IndirectArguments);
            particleCount = new uint[] { 0 };
            particleCountBuffer.SetData(particleCount);

            computeShader.SetBuffer(kernelIndexUpdate, "_ParticleBuffer", particleBuffer);
            computeShader.SetBuffer(kernelIndexEmit, "_ParticleBuffer", particleBuffer);
            computeShader.SetBuffer(kernelIndexInitialize, "_DeadParticleBuffer", pooledParticleBuffer);
            computeShader.SetBuffer(kernelIndexUpdate, "_DeadParticleBuffer", pooledParticleBuffer);
            computeShader.SetBuffer(kernelIndexEmit, "_PooledParticleBuffer", pooledParticleBuffer);

            _renderParams = new RenderParams(material);
            _renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000);
            _renderParams.matProps = new MaterialPropertyBlock();
            _renderParams.matProps.SetBuffer("_ParticleBuffer", particleBuffer);
            material.SetBuffer("_ParticleBuffer", particleBuffer);

            computeShader.Dispatch(kernelIndexInitialize, maxCount / THREAD_NUM, 1, 1);
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
                Emit(Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10));

            computeShader.SetFloat("_DeltaTime", Time.deltaTime);
            computeShader.Dispatch(kernelIndexUpdate, particleBuffer.count / THREAD_NUM, 1, 1);

            //Graphics.DrawMeshInstancedProcedural(mesh, 0, material, new Bounds(Vector3.zero, Vector3.one * 100),
            //maxCount);
            Graphics.RenderMeshPrimitives(_renderParams, mesh, 0, maxCount);
        }

        private void OnDestroy()
        {
            particleBuffer.Release();
            pooledParticleBuffer.Release();
            particleCountBuffer.Release();
        }

        private void OnGUI()
        {
            ComputeBuffer.CopyCount(pooledParticleBuffer, particleCountBuffer, 0);
            particleCountBuffer.GetData(particleCount);
            GUILayout.Label("Pooled(Dead) Particles : " + particleCount[0]);
        }

        private void Emit(Vector3 position)
        {
            ComputeBuffer.CopyCount(pooledParticleBuffer, particleCountBuffer, 0);

            particleCountBuffer.GetData(particleCount);

            if (particleCount[0] < emitCount) return;

            computeShader.SetVector("_ParticleEmitPosition", position);
            computeShader.SetFloat("_ParticleEmitDuration", 3);

            computeShader.Dispatch(kernelIndexEmit, emitCount / THREAD_NUM, 1, 1);
        }
    }
}