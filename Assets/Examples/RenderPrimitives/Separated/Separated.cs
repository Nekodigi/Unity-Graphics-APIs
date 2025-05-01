using System.Linq;
using UnityEngine;

namespace Examples.RenderPrimitives.Separated
{
    public class Separated : MonoBehaviour
    {
        public Material material;
        public Mesh mesh;
        private GraphicsBuffer meshPositions;

        private GraphicsBuffer meshTriangles;

        private void Update()
        {
            var rp = new RenderParams(material);
            rp.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one); // use tighter bounds
            rp.matProps = new MaterialPropertyBlock();
            rp.matProps.SetBuffer("_Positions", meshPositions);
            rp.matProps.SetInt("_BaseVertexIndex", (int)mesh.GetBaseVertex(0));
            rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(-4.5f, 0, 0)));
            rp.matProps.SetFloat("_NumInstances", 10.0f);
            Graphics.RenderPrimitivesIndexed(rp, MeshTopology.Triangles, meshTriangles, meshTriangles.count,
                (int)mesh.GetIndexStart(0), 10);
        }

        private void OnDestroy()
        {
            meshTriangles?.Dispose();
            meshTriangles = null;
            meshPositions?.Dispose();
            meshPositions = null;
        }

        private void OnValidate()
        {
            // note: remember to check "Read/Write" on the mesh asset to get access to the geometry data
            meshTriangles = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.triangles.Length, sizeof(int));
            meshTriangles.SetData(mesh.triangles);
            //separate all triangle so that none of them share vertices
            var vertices = new Vector3[mesh.triangles.Length];
            for (var i = 0; i < mesh.triangles.Length; i++)
                vertices[i] = mesh.vertices[mesh.triangles[i]];
            mesh.vertices = vertices;
            mesh.triangles = Enumerable.Range(0, vertices.Length).ToArray();

            meshPositions =
                new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertices.Length, 3 * sizeof(float));
            meshPositions.SetData(mesh.vertices);
        }
    }
}