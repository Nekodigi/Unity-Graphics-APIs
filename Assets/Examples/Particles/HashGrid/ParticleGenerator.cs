using System;
using System.Runtime.InteropServices;
using APIs.Particles;
using APIs.SearchGrids;
using UnityEngine;
using UnityEngine.Serialization;

namespace Examples.Particles.RadiusQuery
{
    public class ParticleGenerator : ParticleGeneratorBase
    {
        public Mesh mesh;
        private int kernelIndexEmit;
        private int kernelIndexInitialize;
        private int kernelIndexUpdate;
        private HashGrid hashGrid;
        [SerializeField] private float hashGridSize = 1;
        [SerializeField] private ComputeShader hashGridCs;

        protected override void Start()
        {
            base.Start();

            hashGrid = new HashGrid(hashGridSize, hashGridCs, ParticleBuffer);
            
            kernelIndexInitialize = computeShader.FindKernel("Initialize");
            kernelIndexUpdate = computeShader.FindKernel("Update");
            kernelIndexEmit = computeShader.FindKernel("Emit");

            computeShader.SetBuffer(kernelIndexInitialize, "_DeadParticleBuffer", PooledParticleBuffer);
            
            computeShader.SetBuffer(kernelIndexEmit, "_ParticleBuffer", ParticleBuffer);
            computeShader.SetBuffer(kernelIndexEmit, "_PooledParticleBuffer", PooledParticleBuffer);
            
            computeShader.SetBuffer(kernelIndexUpdate, "_DeadParticleBuffer", PooledParticleBuffer);
            computeShader.SetBuffer(kernelIndexUpdate, "_ParticleBuffer", ParticleBuffer);
            computeShader.SetBuffer(kernelIndexUpdate, "_CellIndexBuffer", hashGrid.CellIndexBuffer);
            computeShader.SetBuffer(kernelIndexUpdate, "_CellOffsetBuffer", hashGrid.CellOffsetBuffer);
            computeShader.SetBuffer(kernelIndexUpdate, "_IndexBuffer", hashGrid.IndexBuffer);
            computeShader.SetFloat("_SearchGridCellSize", hashGridSize);
            computeShader.SetInt("_ParticleBufferCount", maxCount);//ParticleBuffer.count will be 1 in CS for some reason. It also works when copied count to some var.
            
            
            

            computeShader.Dispatch(kernelIndexInitialize, maxCount / THREAD_NUM, 1, 1);
        }

        private void Update()
        {
            hashGrid.Update();
            
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

            computeShader.Dispatch(kernelIndexEmit, emitCount / THREAD_NUM, 1, 1);
        }
    }
}