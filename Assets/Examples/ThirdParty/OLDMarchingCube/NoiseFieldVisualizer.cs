using UnityEngine;

namespace MarchingCubes
{
    sealed class NoiseFieldVisualizer : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] Vector3Int _dimensions = new Vector3Int(64, 32, 64);

        //private Vector3Int _dimensions;
        [SerializeField] float width = 4.0f;
        [SerializeField] int _triangleBudget = 65536;
        [SerializeField] float _targetValue = 0;

        #endregion

        #region Project asset references

        [SerializeField] ComputeShader _volumeCompute = null;
        [SerializeField] ComputeShader _builderCompute = null;
        [SerializeField] Texture3D _texture3D = null;

        #endregion

        #region Private members

        int VoxelCount => _dimensions.x * _dimensions.y * _dimensions.z;

        ComputeBuffer _voxelBuffer;
        MeshBuilder _builder;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            //get size of texture3D
            //_dimensions = new Vector3Int(_texture3D.width, _texture3D.height, _texture3D.depth);
            _builderCompute = Resources.Load<ComputeShader>("ThirdParty/MarchingCubes/MarchingCubes");
            _voxelBuffer = new ComputeBuffer(VoxelCount, sizeof(float));
            _builder = new MeshBuilder(_dimensions, _triangleBudget, _builderCompute);
            Debug.Log(_dimensions);
        }

        void OnDestroy()
        {
            _voxelBuffer.Dispose();
            _builder.Dispose();
        }

        void Update()
        {
            // Noise field update
            _volumeCompute.SetInts("_Dims", _dimensions);
            //_volumeCompute.SetFloat("_Scale", 1f / Mathf.Max(_dimensions.x, _dimensions.y, _dimensions.z));

            _volumeCompute.SetFloat("_Time", Time.time);
            _volumeCompute.SetTexture(0, "_Texture3D", _texture3D);
            _volumeCompute.SetBuffer(0, "_Voxels", _voxelBuffer);
            _volumeCompute.SetVector("_Texture3DResolution",
                new Vector3(_texture3D.width, _texture3D.height, _texture3D.depth));
            _volumeCompute.DispatchThreads(0, _dimensions);

            _builder.BuildIsosurface(_voxelBuffer, _targetValue,
                width / Mathf.Max(_dimensions.x, _dimensions.y, _dimensions.z));
            GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;
        }

        #endregion
    }
} // namespace MarchingCubes