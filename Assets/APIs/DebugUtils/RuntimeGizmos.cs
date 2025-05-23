using APIs.Geometry;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace APIs.DebugUtils
{
    public static class RuntimeGizmos
    {
        private static RenderParams GetBasicRenderParams(Material mat)
        {
            RenderParams renderParams = new RenderParams(mat);
            renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000);
            renderParams.matProps = new MaterialPropertyBlock();
            return renderParams;
        }

        private static Material InitRenderMaterial(bool isInstanced, bool isMesh, bool isShaded, bool isIndexed,
            bool isVector)
        {
            var material = new Material(Shader.Find("Hidden/Debug/Render"));
            if (isInstanced) material.EnableKeyword("INSTANCED");
            if (isMesh) material.EnableKeyword("MESH");
            if (isShaded) material.EnableKeyword("SHADED");
            if (isIndexed) material.EnableKeyword("INDEXED");
            if (isVector) material.EnableKeyword("VECTOR");
            return material;
        }

        public static void DrawPositions(float size, GraphicsBuffer positions)
        {
            DrawMeshes(Primitives.CubeMesh, positions, size);
        }

        private static void DrawMeshes(Mesh mesh,
            GraphicsBuffer positions,
            float size = 1, Color? color = null)
        {
            DrawInternal(null, mesh, positions: positions, size: size, color: color);
        }

        public static void DrawSDF(Texture3D texture)
        {
        }

        public static void DrawPrimitive(Primitive primitive, Color? color = null)
        {
            DrawInternal(primitive, null, color: color);
        }

        public static void DrawTrails(GraphicsBuffer vertices, GraphicsBuffer starts, int verticesPerTrail,
            Color? color = null)
        {
            var material = new Material(Shader.Find("Hidden/Debug/Trail"));
            RenderParams rp = GetBasicRenderParams(material);
            rp.matProps.SetBuffer("_Vertices", vertices);
            rp.matProps.SetBuffer("_Starts", starts);
            rp.matProps.SetInt("_VerticesPerTrail", verticesPerTrail);
            rp.matProps.SetColor("_Color", color ?? Color.white);
            Graphics.RenderPrimitives(rp, MeshTopology.LineStrip, verticesPerTrail, vertices.count / verticesPerTrail);
        }

        public static void DrawVectors(GraphicsBuffer vertices, GraphicsBuffer vectors, Color? color = null)
        {
            Primitive primitive = new Primitive()
            {
                Vertices = vertices,
                Topology = MeshTopology.Lines
            };
            DrawInternal(primitive, vectors: vectors, color: color);
        }

        private static void DrawInternal(
            Primitive? primitive, [CanBeNull] Mesh mesh = null, [CanBeNull] GraphicsBuffer vectors = null,
            Vector3? position = null, [CanBeNull] GraphicsBuffer positions = null,
            float size = 1, Color? color = null)
        {
            bool isInstanced = positions != null;
            bool isMesh = mesh != null;
            bool isShaded = primitive?.Topology == MeshTopology.Triangles;
            bool isIndexed = primitive?.Indices != null;
            bool isVector = vectors != null;
            Primitive primitiveValue = primitive ?? default;
            if (primitive == null && mesh == null)
                UnityEngine.Debug.LogError("Either primitive or mesh must be provided.");
            if (isShaded && !isIndexed)
                UnityEngine.Debug.LogError("Shaded primitives require indexed data.");
            if (isVector && (primitiveValue.Topology != MeshTopology.Lines || primitiveValue.Indices != null))
            {
                Debug.LogError("Vector primitives require line topology and no indices.");
            }

            Material material = InitRenderMaterial(isInstanced, isMesh, isShaded, isIndexed, isVector);
            RenderParams rp = GetBasicRenderParams(material);
            rp.matProps.SetBuffer("_Vertices", primitiveValue.Vertices);
            rp.matProps.SetColor("_Color", color ?? Color.white);
            rp.matProps.SetFloat("_Size", size);

            int count = 1;
            if (positions != null)
            {
                count = positions.count;
                rp.matProps.SetBuffer("_Positions", positions);
                rp.material.EnableKeyword("INSTANCED");
            }
            else
            {
                rp.matProps.SetVector("_Position", position ?? Vector3.zero);
                rp.material.DisableKeyword("INSTANCED");
            }


            if (isMesh)
            {
                Graphics.RenderMeshPrimitives(rp, mesh, 0, count);
            }
            else
            {
                if (primitiveValue.Indices == null)
                {
                    int verticesCount = primitiveValue.Vertices.count;
                    if (isVector)
                    {
                        rp.matProps.SetBuffer("_Vectors", vectors);
                        verticesCount *= 2;
                    }

                    Graphics.RenderPrimitives(rp, primitiveValue.Topology, verticesCount,
                        count);
                }
                else
                {
                    rp.matProps.SetBuffer("_Indices", primitiveValue.Indices);
                    Graphics.RenderPrimitives(rp, primitiveValue.Topology, primitiveValue.Indices.count,
                        count);
                }
            }
        }
    }
}