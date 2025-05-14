using System.Runtime.InteropServices;
using UnityEngine;

namespace Examples.RenderPrimitives.ClothTrail
{
    [ExecuteAlways]
    public class ClothTrail : MonoBehaviour
    {
        public int segmentCount = 32; // 骨組みの数
        public int radialResolution = 16; // 円周分割数
        public float radius = 0.1f;
        public Material material;
        public ComputeShader computeShader;

        GraphicsBuffer skeletonBuffer;
        GraphicsBuffer vertexBuffer;         // ベース形状
        GraphicsBuffer displayVertexBuffer;  // ノイズ適用後の表示用
        int vertexCount;
        Bounds bounds;
        int kernelCSInit, kernelCSMain;
        bool initialized = false;

        struct Particle
        {
            public int id;
            public Vector3 position;
            public Vector3 axisX;
            public Vector3 axisY;
            public Vector3 axisZ;
        }

        struct Vertex
        {
            public Vector3 position;
            public Vector3 normal;
        }

        void OnEnable()
        {
            CreateMaterialIfNeeded();
            CreateSkeletonBuffer();
            CreateVertexBuffers();
            bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
            if (computeShader != null)
            {
                kernelCSInit = computeShader.FindKernel("CSInit");
                kernelCSMain = computeShader.FindKernel("CSMain");
            }
            initialized = false;
        }

        void OnDisable()
        {
            ReleaseBuffers();
        }

        void CreateMaterialIfNeeded()
        {
            if (material == null)
            {
                Shader shader = Shader.Find("Unlit/FakeCloth");
                if (shader == null)
                {
                    Debug.LogError("Shader 'Unlit/FakeCloth' not found. Please ensure it exists.");
                    return;
                }
                material = new Material(shader);
            }
        }

        void CreateSkeletonBuffer()
        {
            ReleaseBuffers();

            skeletonBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, segmentCount, Marshal.SizeOf(typeof(Particle)));
            Particle[] particles = new Particle[segmentCount];
            float angleStep = Mathf.PI / (segmentCount - 1);
            for (int i = 0; i < segmentCount; i++)
            {
                float angle = angleStep * i;
                Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), i * 0.1f);
                Vector3 forward = Vector3.forward;
                Vector3 up = Vector3.up;
                Vector3 right = Vector3.Cross(up, forward).normalized;
                particles[i].id = i;
                particles[i].position = pos;
                particles[i].axisZ = forward;
                particles[i].axisY = up;
                particles[i].axisX = right;
            }
            skeletonBuffer.SetData(particles);
        }

        void CreateVertexBuffers()
        {
            int seg = segmentCount - 1;
            vertexCount = seg * radialResolution * 6;
            vertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexCount, Marshal.SizeOf(typeof(Vertex)));
            displayVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexCount, Marshal.SizeOf(typeof(Vertex)));
        }

        void ReleaseBuffers()
        {
            if (skeletonBuffer != null) { skeletonBuffer.Release(); skeletonBuffer = null; }
            if (vertexBuffer != null) { vertexBuffer.Release(); vertexBuffer = null; }
            if (displayVertexBuffer != null) { displayVertexBuffer.Release(); displayVertexBuffer = null; }
        }

        void Update()
        {
            if (vertexBuffer == null || displayVertexBuffer == null || skeletonBuffer == null || material == null || computeShader == null) return;

            // 初回のみ: ベース形状生成
            if (!initialized)
            {
                computeShader.SetInt("_SegmentCount", segmentCount);
                computeShader.SetInt("_RadialResolution", radialResolution);
                computeShader.SetFloat("_Radius", radius);
                computeShader.SetBuffer(kernelCSInit, "_SkeletonBuffer", skeletonBuffer);
                computeShader.SetBuffer(kernelCSInit, "_VertexBuffer", vertexBuffer);
                computeShader.Dispatch(kernelCSInit, Mathf.CeilToInt(vertexCount / 64f), 1, 1);
                initialized = true;
            }

            // 表示用バッファ生成（ノイズ適用）
            computeShader.SetFloat("_Time", Time.time);
            computeShader.SetInt("_SegmentCount", segmentCount);
            computeShader.SetInt("_RadialResolution", radialResolution);
            computeShader.SetFloat("_Radius", radius);
            computeShader.SetBuffer(kernelCSMain, "_VertexBuffer", vertexBuffer);
            computeShader.SetBuffer(kernelCSMain, "_DisplayVertexBuffer", displayVertexBuffer);
            computeShader.Dispatch(kernelCSMain, Mathf.CeilToInt(vertexCount / 64f), 1, 1);

            material.SetBuffer("_VertexBuffer", displayVertexBuffer);

            var rp = new RenderParams(material)
            {
                worldBounds = bounds
            };
            Graphics.RenderPrimitives(rp, MeshTopology.Triangles, vertexCount, 1);
        }
    }
}
