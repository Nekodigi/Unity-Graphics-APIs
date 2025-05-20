using System;
using System.Runtime.InteropServices;
using APIs.DebugUtils;
using APIs.Geometry;
using APIs.Shaders;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


namespace Examples.RenderPrimitives.Trail
{
    [ExecuteAlways]
    public class Trail : MonoBehaviour
    {
        [SerializeField] private int particleCount = 2;
        [SerializeField] private float skeletonRadius = 1f;
        [SerializeField] private int skeletonCount = 32;
        [SerializeField] private int vertexCount = 8;
        [SerializeField] private float trailLifeTime = 1f;
        [SerializeField] private float vertexRadius = 0.2f;

        private GraphicsBuffer _particleBuffer;
        private Vector3[] _particleData;


        private GraphicsBuffer _skeletonBuffer, _skeletonTangentBuffer, _skeletonFactorBuffer, _skeletonStartBuffer;
        private Vector3[] _skeletonData, _skeletonTangentData, _skeletonFactorData;

        private GraphicsBuffer _vertexBuffer, _triangleBuffer;

        private Render _render;

        private int _initSkeletonKernelID, _updateSkeletonStartKernelID;
        private int _initVertexKernelID, _initTrianglesKernelID;

        private ComputeShader _trailCs;

        private void OnValidate()
        {
            _trailCs = Resources.Load<ComputeShader>("Trail");
            _render = new Render();
            ResetBuffers();
            FindKernels();
            InitDatas();
            InitBuffers();
            DispatchInit();
        }

        private void FindKernels()
        {
            _initSkeletonKernelID = _trailCs.FindKernel("InitSkeleton");
            _updateSkeletonStartKernelID = _trailCs.FindKernel("UpdateSkeletonStart");
            _initVertexKernelID = _trailCs.FindKernel("InitVertex");
            _initTrianglesKernelID = _trailCs.FindKernel("InitTriangles");
        }

        //skeleton position will be half circle
        private void InitDatas()
        {
            _particleData = new Vector3[particleCount];
            float t = 0;
            for (int i = 0; i < particleCount; i++)
            {
                _particleData[i] = new Vector3(Mathf.Cos(t) * skeletonRadius, Mathf.Sin(t) * skeletonRadius, i);
            }
        }

        private void InitBuffers()
        {
            _particleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, particleCount,
                Marshal.SizeOf(typeof(Vector3)));
            _skeletonBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, particleCount * skeletonCount,
                Marshal.SizeOf(typeof(Vector3)));
            _skeletonTangentBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, particleCount * skeletonCount,
                Marshal.SizeOf(typeof(Vector3)));
            _skeletonFactorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, particleCount * skeletonCount,
                Marshal.SizeOf(typeof(float)));
            _skeletonStartBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, particleCount,
                Marshal.SizeOf(typeof(uint)));
            _vertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured,
                particleCount * skeletonCount * vertexCount,
                Marshal.SizeOf(typeof(Vector3)));
            int triangleCount = particleCount * skeletonCount * vertexCount * 6;
            _triangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured,
                triangleCount,
                Marshal.SizeOf(typeof(int)));


            _centroids = Geometry.CreateBuffersForCentroids(triangleCount);
        }

        private void DispatchInit()
        {
        }

        private void FixedUpdate()
        {
        }

        private GraphicsBuffer _centroids;

        private void Update()
        {
            UpdateData();
            DispatchUpdate();
            RuntimeGizmos.DrawPositions(0.05f, _particleBuffer);
            RuntimeGizmos.DrawTrails(_skeletonBuffer, _skeletonStartBuffer,
                skeletonCount);
            // Geometry.CalcCentroids(
            //     _vertexBuffer, _triangleBuffer, _centroids);
            // RuntimeGizmos.DrawPositions(0.01f, _centroids);

            RuntimeGizmos.DrawVectors(_skeletonBuffer, _skeletonTangentBuffer);
            // //_render.DrawPositions(0.02f, _vertexBuffer);
            // Primitive lines = new Primitive()
            // {
            //     Vertices = _vertexBuffer,
            //     Indices = _triangleBuffer,
            //     Topology = MeshTopology.LineStrip,
            // };
            // RuntimeGizmos.DrawPrimitive(lines, new Color(1, 1, 1, 1f));
            // Primitive triangles = new Primitive()
            // {
            //     Vertices = _vertexBuffer,
            //     Indices = _triangleBuffer,
            //     Topology = MeshTopology.Triangles
            // };
            // RuntimeGizmos.DrawPrimitive(triangles, new Color(1, 1, 1, 1f));
        }

        private void UpdateData()
        {
            float t = Time.time * 2 * Mathf.PI;
            for (int i = 0; i < particleCount; i++)
            {
                _particleData[i] = new Vector3(Mathf.Cos(t) * skeletonRadius, Mathf.Sin(t) * skeletonRadius, i);
            }

            _particleBuffer.SetData(_particleData);
        }

        private void DispatchUpdate()
        {
            _trailCs.SetInt("_ParticleCount", particleCount);
            _trailCs.SetInt("_SkeletonCount", skeletonCount);
            _trailCs.SetInt("_VertexCount", vertexCount);
            _trailCs.SetInt("_SkeletonCount", skeletonCount);
            _trailCs.SetFloat("_VertexRadius", vertexRadius);
            _trailCs.SetVector("_Forward", Vector3.forward);

            _trailCs.AutoDispatch(_initSkeletonKernelID, particleCount,
                new[]
                {
                    ("_SkeletonBuffer", _skeletonBuffer), ("_ParticleBuffer", _particleBuffer),
                    ("_SkeletonStartBuffer", _skeletonStartBuffer),
                    ("_SkeletonTangentBuffer", _skeletonTangentBuffer)
                });

            _trailCs.AutoDispatch(_updateSkeletonStartKernelID, particleCount,
                new[]
                {
                    ("_SkeletonStartBuffer", _skeletonStartBuffer),
                });

            _trailCs.AutoDispatch(_initVertexKernelID, particleCount, skeletonCount, vertexCount,
                new[]
                {
                    ("_SkeletonBuffer", _skeletonBuffer), ("_VertexBuffer", _vertexBuffer),
                    ("_SkeletonTangentBuffer", _skeletonTangentBuffer),
                }
            );

            _trailCs.AutoDispatch(_initTrianglesKernelID, particleCount, skeletonCount, vertexCount * 6,
                new[]
                {
                    ("_TriangleBuffer", _triangleBuffer),
                    ("_SkeletonFactorBuffer", _skeletonFactorBuffer),
                    ("_SkeletonStartBuffer", _skeletonStartBuffer)
                }
            );
        }

        private void ResetBuffers()
        {
            _particleBuffer?.Release();
            _skeletonBuffer?.Release();
            _skeletonTangentBuffer?.Release();
            _skeletonFactorBuffer?.Release();
            _skeletonStartBuffer?.Release();
            _vertexBuffer?.Release();
            _triangleBuffer?.Release();
        }

        private void OnDestroy()
        {
            ResetBuffers();
        }
    }
}