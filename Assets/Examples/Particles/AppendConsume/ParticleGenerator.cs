using System.Runtime.InteropServices;
using APIs.Particles;
using UnityEngine;

namespace Examples.Particles.AppendConsume
{
    public class ParticleGenerator : ParticleGeneratorBase
    {
        public Mesh mesh;
        private int kernelIndexEmit;
        private int kernelIndexInitialize;
        private int kernelIndexUpdate;

        protected override void Start()
        {
            base.Start();

            kernelIndexInitialize = computeShader.FindKernel("Initialize");
            kernelIndexUpdate = computeShader.FindKernel("Update");
            kernelIndexEmit = computeShader.FindKernel("Emit");

            computeShader.SetBuffer(kernelIndexUpdate, "_ParticleBuffer", ParticleBuffer);
            computeShader.SetBuffer(kernelIndexEmit, "_ParticleBuffer", ParticleBuffer);
            computeShader.SetBuffer(kernelIndexInitialize, "_DeadParticleBuffer", PooledParticleBuffer);
            computeShader.SetBuffer(kernelIndexUpdate, "_DeadParticleBuffer", PooledParticleBuffer);
            computeShader.SetBuffer(kernelIndexEmit, "_PooledParticleBuffer", PooledParticleBuffer);

            computeShader.Dispatch(kernelIndexInitialize, maxCount / THREAD_NUM, 1, 1);
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
                Emit(Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10));

            computeShader.SetFloat("_DeltaTime", Time.deltaTime);
            computeShader.Dispatch(kernelIndexUpdate, ParticleBuffer.count / THREAD_NUM, 1, 1);

            Graphics.RenderMeshPrimitives(RenderParams, mesh, 0, maxCount);
        }
        private void OnGUI()
        {
            ComputeBuffer.CopyCount(PooledParticleBuffer, ParticleCountBuffer, 0);
            ParticleCountBuffer.GetData(ParticleCount);
            GUILayout.Label("Pooled(Dead) Particles : " + ParticleCount[0]);
        }

        private void Emit(Vector3 position)
        {
            ComputeBuffer.CopyCount(PooledParticleBuffer, ParticleCountBuffer, 0);

            ParticleCountBuffer.GetData(ParticleCount);

            if (ParticleCount[0] < emitCount) return;

            computeShader.SetVector("_ParticleEmitPosition", position);
            computeShader.SetFloat("_ParticleEmitDuration", 3);

            computeShader.Dispatch(kernelIndexEmit, emitCount, 1, 1);
        }
    }
}