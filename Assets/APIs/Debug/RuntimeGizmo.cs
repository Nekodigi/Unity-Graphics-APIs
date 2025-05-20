using APIs.Geometry;
using JetBrains.Annotations;
using UnityEngine;

namespace APIs.Debug
{
    public class RuntimeGizmo
    {
        private static Material _primitiveMaterial;

        private static Primitive? _cubeWireframeCache;

        public static Primitive CubeWireframe
        {
            get
            {
                _cubeWireframeCache ??= GenAABBWireframe(
                    new Vector3(-0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f, 0.5f, 0.5f));
                return _cubeWireframeCache.Value;
            }
        }

        public static Primitive GenAABBWireframe(Vector3 min, Vector3 max)
        {
            var cubeWireFramePositions = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                cubeWireFramePositions[i] = new Vector3(
                    (i & 1) == 0 ? min.x : max.x,
                    (i & 2) == 0 ? min.y : max.y,
                    (i & 4) == 0 ? min.z : max.z);
            }

            var cubeWireFrameIndices = new uint[]
            {
                0, 1, 1, 3, 3, 2, 2, 0,
                4, 5, 5, 7, 7, 6, 6, 4,
                0, 4, 1, 5, 2, 6, 3, 7
            };
            var vertices = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 8,
                sizeof(float) * 3);
            vertices.SetData(cubeWireFramePositions);
            var indices = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 24,
                sizeof(uint));
            indices.SetData(cubeWireFrameIndices);
            return new Primitive
            {
                Vertices = vertices,
                Indices = indices,
                Topology = MeshTopology.Lines
            };
        }

        private static RenderParams GetBasicRenderParams(Material mat)
        {
            RenderParams renderParams = new RenderParams(mat);
            renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000);
            renderParams.matProps = new MaterialPropertyBlock();
            return renderParams;
        }

        private static void InitPrimitiveInstancedMaterial()
        {
            if (_primitiveMaterial != null) return;
            _primitiveMaterial = new Material(Shader.Find("Hidden/Debug/Primitive"));
            _primitiveMaterial.EnableKeyword("INSTANCED");
        }

        public static void DrawPrimitive(Primitive primitive,
            Vector3 position,
            float size = 1, Color? color = null)
        {
            DrawPrimitivesInternal(primitive, position: position, size: size, color: color);
        }

        public static void DrawPrimitivesInternal(
            Primitive primitive,
            Vector3? position = null, [CanBeNull] GraphicsBuffer positions = null,
            float size = 1, Color? color = null)
        {
            InitPrimitiveInstancedMaterial();
            Material primitiveMaterial = new Material(_primitiveMaterial);
            RenderParams primitiveRP = GetBasicRenderParams(primitiveMaterial);
            primitiveRP.matProps.SetBuffer("_Vertices", primitive.Vertices);
            primitiveRP.matProps.SetColor("_Color", color ?? Color.white);
            primitiveRP.matProps.SetFloat("_Size", size);

            int count = 1;
            if (positions != null)
            {
                count = positions.count;
                primitiveRP.matProps.SetBuffer("_Positions", positions);
                primitiveRP.material.EnableKeyword("INSTANCED");
            }
            else
            {
                primitiveRP.matProps.SetVector("_Position", position ?? Vector3.zero);
                primitiveRP.material.DisableKeyword("INSTANCED");
            }


            Graphics.RenderPrimitivesIndexed(primitiveRP, primitive.Topology, primitive.Indices,
                primitive.Indices.count, 0,
                count);
        }
    }
}