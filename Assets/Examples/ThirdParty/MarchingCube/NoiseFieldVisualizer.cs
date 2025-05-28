using APIs.DebugUtils;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarchingCubes
{
    sealed class NoiseFieldVisualizer : MonoBehaviour
    {
        [SerializeField] float targetValue = 0;
        [SerializeField] Texture3D texture3D = null;

        private (MeshBuilder meshBuilder, GraphicsBuffer voxelBuffer) _drawSDFRequirements;
        private MeshFilter _meshFilter;


        #region MonoBehaviour implementation

        void Start()
        {
            _drawSDFRequirements = RuntimeGizmos.InitDrawSDF();
            _meshFilter = GetComponent<MeshFilter>();
        }

        void Update()
        {
            RuntimeGizmos.DrawSDF(texture3D, _meshFilter, _drawSDFRequirements, targetValue);
        }

        void OnDestroy()
        {
            _drawSDFRequirements.meshBuilder.Dispose();
            _drawSDFRequirements.voxelBuffer.Dispose();
        }

        #endregion
    }
} // namespace MarchingCubes