using System.Linq;
using APIs.Particles;
using APIs.SearchGrids;
using UnityEngine;

namespace Examples.RenderPrimitives.Plexus
{
    public class Plexus : ParticleGeneratorBase
    {
        public Mesh mesh;
        [SerializeField] private float hashGridSize = 1;
        [SerializeField] private ComputeShader hashGridCs;
        [SerializeField] private GraphicsBuffer _linesBuffer;
        private HashGrid _hashGrid;
        private int _kernelIndexEmit;
        private int _kernelIndexInitialize;
        private int _kernelIndexUpdate;


        protected override void Start()
        {
            base.Start();

            _hashGrid = new HashGrid(hashGridSize, hashGridCs, ParticleBuffer);
            
            _linesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxCount * maxCount, sizeof(int));


            _kernelIndexInitialize = computeShader.FindKernel("Initialize");
            _kernelIndexUpdate = computeShader.FindKernel("Update");
            _kernelIndexEmit = computeShader.FindKernel("Emit");

            computeShader.SetBuffer(_kernelIndexInitialize, "_DeadParticleBuffer", PooledParticleBuffer);

            computeShader.SetBuffer(_kernelIndexEmit, "_ParticleBuffer", ParticleBuffer);
            computeShader.SetBuffer(_kernelIndexEmit, "_PooledParticleBuffer", PooledParticleBuffer);

            computeShader.SetBuffer(_kernelIndexUpdate, "_DeadParticleBuffer", PooledParticleBuffer);
            computeShader.SetBuffer(_kernelIndexUpdate, "_ParticleBuffer", ParticleBuffer);
            computeShader.SetBuffer(_kernelIndexUpdate, "_CellIndexBuffer", _hashGrid.CellIndexBuffer);
            computeShader.SetBuffer(_kernelIndexUpdate, "_OffsetsBuffer", _hashGrid.CellOffsetBuffer);
            computeShader.SetBuffer(_kernelIndexUpdate, "_IndexBuffer", _hashGrid.IndexBuffer);
            computeShader.SetFloat("_SearchGridCellSize", hashGridSize);
            computeShader.SetInt("_ParticleBufferCount",
                maxCount); //ParticleBuffer.count will be 1 in CS for some reason. It also works when copied count to some var.


            computeShader.Dispatch(_kernelIndexInitialize, maxCount / THREAD_NUM, 1, 1);
        }

        private void Update()
        {
            _hashGrid.Update();

            if (Input.GetMouseButton(0))
                Emit(Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10));

            computeShader.SetFloat("_DeltaTime", Time.deltaTime);
            computeShader.Dispatch(_kernelIndexUpdate, ParticleBuffer.count / THREAD_NUM, 1, 1);

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

            computeShader.Dispatch(_kernelIndexEmit, emitCount / THREAD_NUM, 1, 1);
        }
    }
}