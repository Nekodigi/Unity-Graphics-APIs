using System.Runtime.InteropServices;
using UnityEngine;

namespace Examples.RenderPrimitives.FakeCloth
{
    [ExecuteAlways]
    public class FakeCloth : MonoBehaviour
    {
        public int resolution = 32;
        public Material material;
        public ComputeShader computeShader;

        GraphicsBuffer vertexBuffer;
        int vertexCount;
        Bounds bounds;
        int kernelCSInit, kernelCSMain;
        bool initialized = false;

        struct Vertex
        {
            public Vector3 position;
            public Vector3 normal;
        }

        void OnEnable()
        {
            CreateMaterialIfNeeded();
            CreateVertexBuffer();
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

        void CreateVertexBuffer()
        {
            ReleaseBuffers();

            int quadCount = resolution * resolution;
            vertexCount = quadCount * 6;
            vertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertexCount, Marshal.SizeOf(typeof(Vertex)));
        }

        void ReleaseBuffers()
        {
            if (vertexBuffer != null) { vertexBuffer.Release(); vertexBuffer = null; }
        }

        void Update()
        {
            if (vertexBuffer == null || material == null || computeShader == null) return;

            // 初回のみ: 頂点バッファ初期化
            if (!initialized)
            {
                computeShader.SetInt("_Resolution", resolution);
                computeShader.SetBuffer(kernelCSInit, "_VertexBuffer", vertexBuffer);
                computeShader.Dispatch(kernelCSInit, Mathf.CeilToInt(vertexCount / 64f), 1, 1);
                initialized = true;
            }

            // 頂点アニメーション
            computeShader.SetFloat("_Time", Time.time);
            computeShader.SetInt("_Resolution", resolution);
            computeShader.SetBuffer(kernelCSMain, "_VertexBuffer", vertexBuffer);
            computeShader.Dispatch(kernelCSMain, Mathf.CeilToInt(vertexCount / 64f), 1, 1);

            material.SetBuffer("_VertexBuffer", vertexBuffer);

            var rp = new RenderParams(material)
            {
                worldBounds = bounds
            };
            Graphics.RenderPrimitives(rp, MeshTopology.Triangles, vertexCount, 1);
        }
    }
}
