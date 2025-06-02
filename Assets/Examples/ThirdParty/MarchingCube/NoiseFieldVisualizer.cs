using APIs.DebugUtils;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarchingCubes
{
    sealed class NoiseFieldVisualizer : MonoBehaviour
    {
        [SerializeField] float targetValue = 0;
        [SerializeField] float size = 1;
        [SerializeField] Texture3D texture3D = null;
        private RenderTexture _renderTexture;

        private (MeshBuilder meshBuilder, GraphicsBuffer voxelBuffer, MeshFilter) _drawSDFRequirements;


        #region MonoBehaviour implementation

        void Start()
        {
            _drawSDFRequirements =
                RuntimeGizmos.InitSDF(new Vector3Int(texture3D.width, texture3D.height, texture3D.depth),
                    gameObject);
            _renderTexture = new RenderTexture(texture3D.width, texture3D.height, 0, RenderTextureFormat.ARGBFloat);
            _renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            _renderTexture.volumeDepth = texture3D.depth;
            _renderTexture.enableRandomWrite = true;
            _renderTexture.Create();
            var cs = RuntimeGizmos.InitCopyTexture3D();
            RuntimeGizmos.CopyTexture3D(texture3D, _renderTexture, cs);
        }

        void Update()
        {
            RuntimeGizmos.DrawSDF(_renderTexture, _drawSDFRequirements, targetValue, size);
        }

        void OnDestroy()
        {
            _drawSDFRequirements.meshBuilder.Dispose();
            _drawSDFRequirements.voxelBuffer.Dispose();
        }

        #endregion
    }
} // namespace MarchingCubes