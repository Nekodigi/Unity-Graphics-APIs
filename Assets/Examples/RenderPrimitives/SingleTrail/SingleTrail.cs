using System;
using System.Runtime.InteropServices;
using APIs.Debug;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


namespace Examples.RenderPrimitives.Trail
{
    [ExecuteAlways]
    public class SingleTrail : MonoBehaviour
    {
        [SerializeField] private int skeletonCount = 16;
        [SerializeField] private int vertexCount = 8;
        [SerializeField] private float skeletonRadius = 1f;
        [SerializeField] private float vertexRadius = 0.2f;
        [SerializeField] [Range(-1, 1)] private float startPosition = 0f;
        [SerializeField] [Range(-1, 1)] private float factorOffset = 0f;
        [SerializeField] private Vector2 forwardRotation = new Vector2(0, 0);
        private int _startIndex;

        private GraphicsBuffer _skeletonBuffer;
        private GraphicsBuffer _skeletonTangentsBuffer;
        private GraphicsBuffer _skeletonFactorBuffer;
        private Vector3[] _skeletonData;
        private Vector3[] _skeletonTangentsData;
        private float[] _skeletonFactorData;
        private GraphicsBuffer _vertexBuffer;
        private GraphicsBuffer _triangleBuffer;
        private GraphicsBuffer _triangleInfoBuffer;
        private RenderParams _renderParams;
        private ComputeShader _trailShader;
        private Render _render;

        private int _initVertexKernelID;
        private int _initTrianglesKernelID;

        private int PositiveMod(int x, int y)
        {
            int result = x % y;
            if (result < 0)
            {
                result += y;
            }

            return result;
        }

        private void OnValidate()
        {
            ResetBuffers();
            _trailShader = Resources.Load<ComputeShader>("SingleTrail");
            _startIndex = PositiveMod((int)(startPosition * skeletonCount), skeletonCount);
            _render = new Render();
            FindKernels();
            InitDatas();
            InitBuffers();
            DispatchInit();
        }

        private void FindKernels()
        {
            _initVertexKernelID = _trailShader.FindKernel("InitVertex");
            _initTrianglesKernelID = _trailShader.FindKernel("InitTriangles");
        }

        //skeleton position will be half circle
        private void InitDatas()
        {
            _skeletonData = new Vector3[skeletonCount];
            _skeletonTangentsData = new Vector3[skeletonCount];
            _skeletonFactorData = new float[skeletonCount];
            for (int i = 0; i < skeletonCount; i++)
            {
                float angle = Mathf.PI * i / skeletonCount * 2;
                _skeletonData[i] = new Vector3(Mathf.Cos(angle) * skeletonRadius, Mathf.Sin(angle) * skeletonRadius, 0);
                _skeletonTangentsData[i] =
                    new Vector3(Mathf.Cos(angle + Mathf.PI / 2), Mathf.Sin(angle + Mathf.PI / 2), 0);
                //must replace this with safe mod;
                _skeletonFactorData[i] =
                    (float)PositiveMod(i - _startIndex, skeletonCount) / (skeletonCount - 1) - factorOffset;
                _skeletonData[i] += Vector3.forward * _skeletonFactorData[i];
            }
        }

        private void InitBuffers()
        {
            _skeletonBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, skeletonCount,
                Marshal.SizeOf(typeof(Vector3)));
            _skeletonBuffer.SetData(_skeletonData);
            _skeletonTangentsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, skeletonCount,
                Marshal.SizeOf(typeof(Vector3)));
            _skeletonTangentsBuffer.SetData(_skeletonTangentsData);
            _vertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, skeletonCount * vertexCount,
                Marshal.SizeOf(typeof(Vector3)));
            int trianglesCount = skeletonCount * vertexCount * 6;
            _triangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, trianglesCount,
                Marshal.SizeOf(typeof(int)));
            _skeletonFactorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, skeletonCount,
                Marshal.SizeOf(typeof(float)));
            _skeletonFactorBuffer.SetData(_skeletonFactorData);
            _triangleInfoBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, trianglesCount,
                Marshal.SizeOf(typeof(Vector3)));
        }

        private void DispatchInit()
        {
            _trailShader.SetInt("_VertexCount", vertexCount);
            _trailShader.SetInt("_SkeletonCount", skeletonCount);
            _trailShader.SetFloat("_VertexRadius", vertexRadius);
            _trailShader.SetInt("_StartIndex", _startIndex);
            //spherical coord to cartesian coord
            float sy = Mathf.Sin(forwardRotation.y);
            Vector3 forward = new Vector3(Mathf.Cos(forwardRotation.x) * sy, Mathf.Sin(forwardRotation.x) * sy,
                Mathf.Cos(forwardRotation.y));

            _trailShader.SetVector("_Forward", forward);

            _trailShader.SetBuffer(_initVertexKernelID, "_SkeletonBuffer", _skeletonBuffer);
            _trailShader.SetBuffer(_initVertexKernelID, "_VertexBuffer", _vertexBuffer);
            _trailShader.SetBuffer(_initVertexKernelID, "_SkeletonTangentsBuffer", _skeletonTangentsBuffer);
            _trailShader.SetBuffer(_initVertexKernelID, "_SkeletonFactorBuffer", _skeletonFactorBuffer);
            _trailShader.Dispatch(_initVertexKernelID, skeletonCount, vertexCount, 1);


            _trailShader.SetBuffer(_initTrianglesKernelID, "_TriangleBuffer", _triangleBuffer);
            _trailShader.SetBuffer(_initTrianglesKernelID, "_SkeletonFactorBuffer", _skeletonFactorBuffer);
            _trailShader.SetBuffer(_initTrianglesKernelID, "_TriangleInfoBuffer", _triangleInfoBuffer);
            _trailShader.Dispatch(_initTrianglesKernelID, skeletonCount, vertexCount, 1);
        }

        private void Update()
        {
            _render.DrawVectors(0.2f, _skeletonBuffer, _skeletonTangentsBuffer);
            _render.DrawTriangles(0.01f, _vertexBuffer, _triangleBuffer, new Color(1, 1, 1, 0.1f), drawCentroid: true,
                lineAlpha: 0.1f,
                triangleAlpha: 0.1f, data: _triangleInfoBuffer, dataDim: 3, dataNormalized1: false);

            _render.DrawPositions(0.05f, _skeletonBuffer, data: _skeletonFactorBuffer, dataDim: 1);
        }

        private void ResetBuffers()
        {
            _skeletonBuffer?.Release();
            _skeletonBuffer = null;
        }

        private void OnDestroy()
        {
            ResetBuffers();
        }
    }
}