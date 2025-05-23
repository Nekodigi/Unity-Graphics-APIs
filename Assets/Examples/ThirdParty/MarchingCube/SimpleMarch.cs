using System;
using UnityEngine;

[ExecuteInEditMode]
public class SimpleMarch : MonoBehaviour
{
    public Material material;
    public bool generateCollider = true;

    [Header("Voxel Settings")] public float isoLevel;
    [Range(2, 100)] public int numPointsPerAxis = 3;

    // Buffers
    ComputeBuffer triangleBuffer;
    ComputeBuffer pointsBuffer;
    ComputeBuffer triCountBuffer;

    ComputeBuffer valuesBuffer;
    // ComputeBuffer vertexBuffer;
    // ComputeBuffer indexBuffer;

    [SerializeField] ComputeShader shader;


    void OnValidate()
    {
        int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;
        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        pointsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        valuesBuffer = new ComputeBuffer(numPoints, sizeof(float));
        // vertexBuffer = new ComputeBuffer(maxTriangleCount * 3, sizeof(float) * 3, ComputeBufferType.Append);
        // indexBuffer = new ComputeBuffer(maxTriangleCount * 3, sizeof(int), ComputeBufferType.Append);

        Vector4[] points = new Vector4[numPoints];
        float[] values = new float[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 pos = new Vector3(i % numPointsPerAxis, (i / numPointsPerAxis) % numPointsPerAxis,
                i / (numPointsPerAxis * numPointsPerAxis));
            float value = Mathf.Sin(pos.x * 0.5f + Time.time) * Mathf.Sin(pos.y * 0.5f) * Mathf.Sin(pos.z * 0.5f) -
                          0.2f;
            points[i] = new Vector4(pos.x, pos.y, pos.z,
                value);
            values[i] = value;
        }

        pointsBuffer.SetData(points);
        valuesBuffer.SetData(values);
    }

    void OnDestroy()
    {
        pointsBuffer.Dispose();
        triangleBuffer.Dispose();
        triCountBuffer.Dispose();
        valuesBuffer.Dispose();
    }

    void Update()
    {
        triangleBuffer.SetCounterValue(0);
        shader.EnableKeyword("TEXTURE3D");
        shader.SetBuffer(0, "values", valuesBuffer);
        shader.SetBuffer(0, "triangles", triangleBuffer);
        shader.SetInt("numPointsPerAxis", numPointsPerAxis);
        shader.SetFloat("isoLevel", isoLevel);

        int numVoxelsPerAxis = numPointsPerAxis - 1;

        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / 8.0f);

        shader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);


        var vertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];
        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }

        int chunkSize = 60000;
        int meshObjectCount = Mathf.CeilToInt(vertices.Length / (float)chunkSize);
        //Instantiate Child Game Object, and make sure old one is destroyed

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < meshObjectCount; i++)
        {
            GameObject childMesh = new GameObject("ChildMesh" + i);
            childMesh.transform.parent = transform;
            MeshFilter childMeshFilter = childMesh.AddComponent<MeshFilter>();
            MeshRenderer childMeshRenderer = childMesh.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            var subVertices = new Vector3[chunkSize];
            var subTriangles = new int[chunkSize];
            for (int j = 0; j < chunkSize; j++)
            {
                int index = i * chunkSize + j;
                if (index >= vertices.Length)
                    break;

                subTriangles[j] = j;
                subVertices[j] = vertices[index];
            }

            mesh.vertices = subVertices;
            mesh.triangles = subTriangles;
            mesh.RecalculateNormals();

            childMeshFilter.sharedMesh = mesh;
            childMeshRenderer.sharedMaterial = material;
        }
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