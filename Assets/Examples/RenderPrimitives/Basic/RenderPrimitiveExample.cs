    using UnityEngine;
namespace Examples.RenderPrimitives.Example
{

    public class RenderPrimitiveExample : MonoBehaviour
    {
        public Material material;
        public Mesh mesh;

        GraphicsBuffer meshTriangles;
        GraphicsBuffer meshPositions;

        void Start()
        {
            // note: remember to check "Read/Write" on the mesh asset to get access to the geometry data
            int length = (int)(mesh.triangles.Length * 1.5f);
            meshTriangles = new GraphicsBuffer(GraphicsBuffer.Target.Structured, length, sizeof(int));
            int[] edges = new int[length];
            int count = 0;
            for (int i = 0; i < mesh.triangles.Length; i++)
            {
                edges[count++] = mesh.triangles[i];
                if (i % 3 == 1)
                {
                    edges[count++] = mesh.triangles[i];
                }
            }
            meshTriangles.SetData(edges);//just duplicate mid point of triangle.
            meshPositions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertices.Length, 3 * sizeof(float));
            meshPositions.SetData(mesh.vertices);
        }

        void OnDestroy()
        {
            meshTriangles?.Dispose();
            meshTriangles = null;
            meshPositions?.Dispose();
            meshPositions = null;
        }

        void Update()
        {
            RenderParams rp = new RenderParams(material);
            rp.worldBounds = new Bounds(Vector3.zero, 10000*Vector3.one); // use tighter bounds
            rp.matProps = new MaterialPropertyBlock();
            rp.matProps.SetBuffer("_Positions", meshPositions);
            rp.matProps.SetInt("_BaseVertexIndex", (int)mesh.GetBaseVertex(0));
            rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(-4.5f, 0, 0)));
            rp.matProps.SetFloat("_NumInstances", 10.0f);
            Graphics.RenderPrimitivesIndexed(rp, MeshTopology.Lines, meshTriangles, meshTriangles.count, (int)mesh.GetIndexStart(0), 10);
        }
    }
}