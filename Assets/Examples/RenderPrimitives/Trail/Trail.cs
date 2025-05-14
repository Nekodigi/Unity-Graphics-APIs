
using System;
using System.Runtime.InteropServices;
using APIs.Debug;
using Unity.VisualScripting;
using UnityEngine;

namespace Examples.RenderPrimitives.Trail
{
    [ExecuteAlways]
    public class Trail : MonoBehaviour
    {
        [SerializeField]private int skeletonCount = 32; 
        [SerializeField]private float skeletonRadius = 1f;
        
        
        private GraphicsBuffer _skeletonBuffer;
        private Vector3[] _skeletonData;
        private Material _material;
        private RenderParams _renderParams;
        
        private void OnEnable()
        {
            _material = new Material(Shader.Find("Hidden/Trail/Skeleton"));
            InitDatas();
            InitBuffers();
        }

        //skeleton position will be half circle
        private void InitDatas()
        {
            _skeletonData = new Vector3[skeletonCount];
            for (int i = 0; i < skeletonCount; i++)
            {
                float angle = Mathf.PI * i / (skeletonCount - 1);
                _skeletonData[i] = new Vector3(Mathf.Cos(angle) * skeletonRadius, Mathf.Sin(angle) * skeletonRadius, 0);
            }
        }
        
        private void InitBuffers()
        {
            _skeletonBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, skeletonCount, Marshal.SizeOf(typeof(Vector3)));
            _skeletonBuffer.SetData(_skeletonData);
        }
        
        private void Update()
        {
            Render render = new Render();
            render.DrawPositions(0.02f, _skeletonBuffer, true);
        }

        private void OnDestroy()
        {
            _skeletonBuffer?.Release();
            _skeletonBuffer = null;
        }
    }
}