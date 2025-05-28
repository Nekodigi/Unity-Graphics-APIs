using UnityEngine;

namespace MarchingCubes
{
    sealed class ChunkedNoiseFieldVisualizer : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] Vector3Int _dimensions = new Vector3Int(64, 32, 64);
        [SerializeField] Vector3Int _chunkDimensions = new Vector3Int(4, 1, 4);

        //private Vector3Int _dimensions;
        [SerializeField] float _gridScale = 4.0f / 64;
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

        //MeshBuilder _builder;
        private MeshBuilder[,,] _builders;
        GameObject[,,] _chunks;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _voxelBuffer = new ComputeBuffer(VoxelCount, sizeof(float));
            //_builder = new MeshBuilder(_dimensions, _triangleBudget, _builderCompute);
            _builders = new MeshBuilder[_chunkDimensions.x, _chunkDimensions.y, _chunkDimensions.z];

            for (int i = 0; i < _chunkDimensions.x; i++)
            {
                for (int j = 0; j < _chunkDimensions.y; j++)
                {
                    for (int k = 0; k < _chunkDimensions.z; k++)
                    {
                        _builders[i, j, k] = new MeshBuilder(_dimensions, _triangleBudget, _builderCompute);
                    }
                }
            }

            DestroyChildren();
            _chunks = new GameObject[_chunkDimensions.x, _chunkDimensions.y, _chunkDimensions.z];
            for (int i = 0; i < _chunkDimensions.x; i++)
            {
                for (int j = 0; j < _chunkDimensions.y; j++)
                {
                    for (int k = 0; k < _chunkDimensions.z; k++)
                    {
                        GameObject child = new GameObject($"Chunk_{i}_{j}_{k}");
                        child.transform.SetParent(transform);
                        child.AddComponent<MeshFilter>();
                        child.AddComponent<MeshRenderer>();
                        child.GetComponent<MeshRenderer>().material =
                            new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        child.GetComponent<MeshRenderer>().material.color = Color.white;
                        //translate
                        child.transform.localPosition =
                            new Vector3(i * _dimensions.x * _gridScale,
                                j * _dimensions.y * _gridScale,
                                k * _dimensions.z * _gridScale);
                        _chunks[i, j, k] = child;
                    }
                }
            }
        }

        private void DestroyChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject); //TODO Immidiate is not best way.
            }
        }

        void OnDestroy()
        {
            _voxelBuffer.Dispose();
            for (int i = 0; i < _chunkDimensions.x; i++)
            {
                for (int j = 0; j < _chunkDimensions.y; j++)
                {
                    for (int k = 0; k < _chunkDimensions.z; k++)
                    {
                        _builders[i, j, k].Dispose();
                    }
                }
            }
        }

        void Update()
        {
            for (int i = 0; i < _chunkDimensions.x; i++)
            {
                for (int j = 0; j < _chunkDimensions.y; j++)
                {
                    for (int k = 0; k < _chunkDimensions.z; k++)
                    {
                        _volumeCompute.SetInts("_Dims", _dimensions);
                        _volumeCompute.SetFloat("_Scale", _gridScale);
                        _volumeCompute.SetFloat("_Time", Time.time);
                        _volumeCompute.SetVector("_Offset",
                            new Vector3(i * _dimensions.x / 1f, j * _dimensions.y / 1f, k * _dimensions.z / 1f));
                        _volumeCompute.SetTexture(0, "_Texture3D", _texture3D);
                        _volumeCompute.SetBuffer(0, "_Voxels", _voxelBuffer);
                        _volumeCompute.DispatchThreads(0, _dimensions);

                        var index = i * _chunkDimensions.y * _chunkDimensions.z + j * _chunkDimensions.z + k;
                        _builders[i, j, k].BuildIsosurface(_voxelBuffer, _targetValue, _gridScale);
                        //GetComponent<MeshFilter>().sharedMesh = _builder.Mesh;
                        MeshFilter meshFilter = _chunks[i, j, k].GetComponent<MeshFilter>();
                        meshFilter.sharedMesh = _builders[i, j, k].Mesh;
                    }
                }
            }
        }

        #endregion
    }
} // namespace MarchingCubes