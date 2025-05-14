using System.Linq;
using APIs.Particles;
using APIs.SearchGrids;
using UnityEngine;

namespace Examples.RenderPrimitives.Plexus
{
    public class Plexus : ParticleGeneratorBase
    {
        public Mesh mesh;
        [SerializeField] private float lifeTime = 10;
        [SerializeField] private ComputeShader hashGridCs;
        private int _kernelIndexEmit;
        private int _kernelIndexInitialize;
        private int _kernelIndexUpdate;
        [SerializeField] Material lineMaterial;
        RenderParams _lineRP;

        protected override void Start()
        {
            base.Start();
            
            

            SetKernel();
            
           
        }

        

        private void Update()
        {
            if (Input.GetMouseButton(0))
                Emit(Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10));

            
            computeShader.SetFloat("_DeltaTime", Time.deltaTime);
            computeShader.Dispatch(_kernelIndexUpdate, ParticleBuffer.count / THREAD_NUM, 1, 1);

            //Graphics.RenderMeshPrimitives(RenderParams, mesh, 0, maxCount);
            
            
            _lineRP = new RenderParams(lineMaterial);
            _lineRP.worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000);
            _lineRP.matProps = new MaterialPropertyBlock();
            _lineRP.matProps.SetBuffer("_ParticleBuffer", ParticleBuffer);
            _lineRP.matProps.SetInt("_ParticleCount", ParticleBuffer.count);
            Graphics.RenderPrimitives(_lineRP, MeshTopology.Lines, maxCount * maxCount * 2, 1);
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
            computeShader.SetFloat("_ParticleEmitDuration", lifeTime);

            computeShader.Dispatch(_kernelIndexEmit, emitCount, 1, 1);
        }
        
        
        private void SetKernel()
        {
            _kernelIndexInitialize = computeShader.FindKernel("Initialize");
            _kernelIndexUpdate = computeShader.FindKernel("Update");
            _kernelIndexEmit = computeShader.FindKernel("Emit");

            computeShader.SetBuffer(_kernelIndexInitialize, "_DeadParticleBuffer", PooledParticleBuffer);

            computeShader.SetBuffer(_kernelIndexEmit, "_ParticleBuffer", ParticleBuffer);
            computeShader.SetBuffer(_kernelIndexEmit, "_PooledParticleBuffer", PooledParticleBuffer);

            computeShader.SetBuffer(_kernelIndexUpdate, "_DeadParticleBuffer", PooledParticleBuffer);
            computeShader.SetBuffer(_kernelIndexUpdate, "_ParticleBuffer", ParticleBuffer);
            computeShader.SetInt("_ParticleBufferCount",
                maxCount); //ParticleBuffer.count will be 1 in CS for some reason. It also works when copied count to some var.

            computeShader.Dispatch(_kernelIndexInitialize, maxCount / THREAD_NUM, 1, 1);
        }
    }
}