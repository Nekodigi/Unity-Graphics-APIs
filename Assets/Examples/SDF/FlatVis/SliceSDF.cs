using System;
using APIs.DebugUtils;
using APIs.Shaders;
using Common.ShaderUtils;
using UnityEngine;

namespace Examples.SDF.FlatVis
{
    public class SliceSDF : MonoBehaviour
    {
        [SerializeField] private Texture2D texture2D;
        [SerializeField] private Texture3D texture3D;
        [SerializeField] private Vector3Int slicePosition = new Vector3Int(128, 128, 128);
        [SerializeField] private float scale = 1f;
        [SerializeField] private float offset = 0f;
        [SerializeField] private Gradient gradient;
        [SerializeField] private ColorMapType colorMapType = ColorMapType.JET;
        [SerializeField] private float displayScale = 10f;


        //private (RenderTexture, RenderTexture, RenderTexture, ComputeShader, Material, Material) _cache;
        private ((RenderTexture dest, Material colorMapMat, Material renderMat)[], RenderTexture[], GameObject[])
            _caches;


        private void Start()
        {
            _caches = RuntimeGizmos.InitSDFSegment(texture3D, transform);
        }

        private void Update()
        {
            RuntimeGizmos.DrawSDFSegment(
                texture3D,
                slicePosition,
                _caches,
                displayScale,
                colorMapType,
                scale,
                offset,
                gradient
            );
        }
    }
}