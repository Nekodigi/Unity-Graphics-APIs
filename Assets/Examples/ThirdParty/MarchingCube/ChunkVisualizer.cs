using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkVisualizer : MonoBehaviour
{
    private float _isoLevel;
    private int _numPointsPerAxis = 3;
    private const int TriangleBudget = 65536;

    ComputeBuffer _triangleBuffer;
    ComputeBuffer _triCountBuffer;
    ComputeBuffer _vertexBuffer;
    ComputeBuffer _indexBuffer;
    public ComputeBuffer ValuesBuffer { get; private set; }

    private MeshFilter _meshFilter;

    [SerializeField] ComputeShader _marchingCubesCs;
    private Mesh _mesh;
    Vector3[] vertices = new Vector3[TriangleBudget * 3];
    int[] meshTriangles = new int[TriangleBudget * 3];
    ComputeShader _computeShader;

    private void GenerateBuffers(int numPointsPerAxis)
    {
        int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;
        _triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        _triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        _vertexBuffer = new ComputeBuffer(TriangleBudget * 3, sizeof(float) * 3);
        _indexBuffer = new ComputeBuffer(TriangleBudget * 3, sizeof(int));
    }

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _mesh = new Mesh();
    }

    public void Init(int numPointsPerAxis, float isoLevel, ComputeBuffer valuesBuffer)
    {
        GenerateBuffers(numPointsPerAxis);
        ValuesBuffer = valuesBuffer;
        _numPointsPerAxis = numPointsPerAxis;
        _isoLevel = isoLevel;
        float[] values = new float[numPointsPerAxis * numPointsPerAxis * numPointsPerAxis];
        valuesBuffer.GetData(values);
        _computeShader = (ComputeShader)Resources.Load("ChunkVisualizer");
        Debug.Assert(_computeShader != null, "SimpleValues compute shader not found");
        Debug.Assert(_marchingCubesCs != null, "MarchingCubes compute shader not found");
    }

    void LateUpdate()
    {
        _triangleBuffer.SetCounterValue(0);
        _marchingCubesCs.EnableKeyword("TEXTURE3D");
        _marchingCubesCs.SetBuffer(0, "values", ValuesBuffer);
        _marchingCubesCs.SetBuffer(0, "triangles", _triangleBuffer);
        _marchingCubesCs.SetInt("numPointsPerAxis", _numPointsPerAxis);
        _marchingCubesCs.SetFloat("isoLevel", _isoLevel);

        int numVoxelsPerAxis = _numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / 8.0f);
        _marchingCubesCs.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        int numTris = GetCount(_triangleBuffer, _triCountBuffer);
        Triangle[] tris = new Triangle[numTris];
        _triangleBuffer.GetData(tris, 0, 0, numTris);

        // _computeShader.SetBuffer(0, "triangles", _triangleBuffer);
        // _computeShader.SetBuffer(0, "indices", _indexBuffer);
        // _computeShader.SetBuffer(0, "vertices", _vertexBuffer);
        // _computeShader.Dispatch(0, numTris, 1, 1);
        for (int i = 0; i < tris.Length; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }

        // _vertexBuffer.GetData(vertices);
        // _indexBuffer.GetData(meshTriangles);
        // vertices.Log("vertices");
        // meshTriangles.Log("meshTriangles");
        DrawMesh();
    }

    private void DrawMesh()
    {
        _mesh.vertices = vertices;
        _mesh.triangles = meshTriangles;
        _mesh.RecalculateNormals();

        _meshFilter.sharedMesh = _mesh;
    }

    void OnDestroy()
    {
        _triangleBuffer?.Dispose();
        _triCountBuffer?.Dispose();
        ValuesBuffer?.Dispose();
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
    }

    private int GetCount(ComputeBuffer sourceBuffer, ComputeBuffer countBuffer)
    {
        int[] count = new int[1];
        ComputeBuffer.CopyCount(sourceBuffer, countBuffer, 0);
        countBuffer.GetData(count);
        return count[0];
    }

    struct Triangle
    {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }
}