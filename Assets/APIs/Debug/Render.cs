using APIs.Graphic.Buffer;
using UnityEngine;

namespace APIs.Debug
{
    public class Render
    {
        private Material _instancedMeshMaterial;
        private Material _instancedMeshVectorColorMaterial;
        private Material _instancedLineStripMaterial;
        private Material _primitiveMaterial;
        private Material _primitiveMaterialIndexed;
        private Material _primitiveTriangleShadedMaterial;
        private Material _primitiveTriangleShadedVectorColorMaterial;
        private Material _vectorLineMaterial;
        private ComputeShader _calcCentroidsCs;
        private Buffer _bufferUtil;

        public Render()
        {
            InitMaterials();
            _bufferUtil = new Buffer();
        }

        private void InitMaterials()
        {
            _instancedMeshMaterial = new Material(Shader.Find("Hidden/Debug/InstancedMesh"));
            _instancedMeshVectorColorMaterial = new Material(_instancedMeshMaterial);
            _instancedMeshVectorColorMaterial.EnableKeyword("VECTOR_COLOR");
            _instancedLineStripMaterial = new Material(Shader.Find("Hidden/Debug/InstancedLineStrip"));
            _primitiveMaterial = new Material(Shader.Find("Hidden/Debug/Primitive"));
            _primitiveMaterialIndexed = new Material(_primitiveMaterial);
            _primitiveMaterialIndexed.EnableKeyword("INDEXED");
            _primitiveTriangleShadedMaterial =
                new Material(Shader.Find("Hidden/Debug/PrimitiveTriangleShaded"));
            _primitiveTriangleShadedVectorColorMaterial =
                new Material(_primitiveTriangleShadedMaterial);
            _primitiveTriangleShadedVectorColorMaterial.EnableKeyword("VECTOR_COLOR");
            _vectorLineMaterial = new Material(Shader.Find("Hidden/Debug/VectorLine"));
            _calcCentroidsCs = Resources.Load<ComputeShader>("CalcCentroids");
        }

        private RenderParams GetBasicRenderParams(Material mat)
        {
            RenderParams renderParams = new RenderParams(mat);
            renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000);
            renderParams.matProps = new MaterialPropertyBlock();
            return renderParams;
        }

        public void DrawVectors(float multiplier, GraphicsBuffer positions, GraphicsBuffer vectors,
            bool normalized = true, bool drawOrigin = true)
        {
            if (drawOrigin) DrawPositions(multiplier * 0.1f, positions);
            RenderParams primitiveRP = GetBasicRenderParams(_vectorLineMaterial);
            primitiveRP.matProps.SetBuffer("_Positions", positions);
            primitiveRP.matProps.SetBuffer("_Vectors", vectors);
            primitiveRP.matProps.SetInt("_Normalized", normalized ? 1 : 0);
            primitiveRP.matProps.SetFloat("_Multiplier", multiplier);
            Graphics.RenderPrimitives(primitiveRP, MeshTopology.Lines, positions.count * 2);
        }


        //visualize index.
        public void DrawPositions(float size, GraphicsBuffer positions, bool drawLine = false, Color? color = null,
            GraphicsBuffer data = null, int dataDim = 1, bool dataNormalized1 = false)
        {
            //use cube mesh
            Mesh cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            RenderParams meshRP;
            if (data != null)
                meshRP = GetBasicRenderParams(_instancedMeshVectorColorMaterial);
            else
                meshRP =
                    GetBasicRenderParams(_instancedMeshMaterial);


            meshRP.matProps.SetBuffer("_Positions", positions);
            meshRP.matProps.SetFloat("_Size", size);
            color = color ?? Color.white;
            meshRP.matProps.SetColor("_Color", (Color)color);

            GraphicsBuffer convertedVectors = null;
            if (data != null)
            {
                convertedVectors =
                    _bufferUtil.ConvertVectorGraphicsBuffer(data, dataDim, 3); // float4
                meshRP.matProps.SetBuffer("_Vectors", convertedVectors);
                meshRP.matProps.SetInt("_Normalize1", dataNormalized1 ? 1 : 0);
            }

            Graphics.RenderMeshPrimitives(meshRP, cubeMesh, 0, positions.count);
            convertedVectors?.Release();

            if (drawLine)
            {
                RenderParams primitiveRP = GetBasicRenderParams(_primitiveMaterial);
                primitiveRP.matProps.SetBuffer("_Positions", positions);
                primitiveRP.matProps.SetColor("_Color", (Color)color);
                Graphics.RenderPrimitives(primitiveRP, MeshTopology.LineStrip, positions.count);
            }
        }

        public void DrawSkeletons(float size, GraphicsBuffer positions, GraphicsBuffer starts,
            Color? color = null)
        {
            color = color ?? Color.white;
            DrawPositions(size, positions, color: color);
            //We can use lineStrip primitives instanced
            RenderParams skeletonsRP = GetBasicRenderParams(_instancedLineStripMaterial);
            skeletonsRP.matProps.SetBuffer("_Positions", positions);
            skeletonsRP.matProps.SetBuffer("_Starts", starts);
            skeletonsRP.matProps.SetColor("_Color", (Color)color);
            int skeletonCount = positions.count / starts.count;
            skeletonsRP.matProps.SetInt("_SkeletonCount", skeletonCount);
            Graphics.RenderPrimitives(skeletonsRP, MeshTopology.LineStrip, skeletonCount, starts.count);
        }

        public void DrawTriangles(float size, GraphicsBuffer positions, GraphicsBuffer triangles, Color? color = null,
            bool drawCentroid = false, float lineAlpha = 1f, float triangleAlpha = 0.9f, GraphicsBuffer data = null,
            int dataDim = 1, bool dataNormalized1 = false)
        {
            color = color ?? Color.white;

            if (drawCentroid)
            {
                _calcCentroidsCs.SetBuffer(0, "_VertexBuffer", positions);
                _calcCentroidsCs.SetBuffer(0, "_TriangleBuffer", triangles);
                GraphicsBuffer centroids = new GraphicsBuffer(GraphicsBuffer.Target.Structured, triangles.count / 3,
                    sizeof(float) * 3);
                _calcCentroidsCs.SetBuffer(0, "_CentroidBuffer", centroids);
                _calcCentroidsCs.Dispatch(0, triangles.count / 3, 1, 1);
                DrawPositions(size, centroids, false, color: color);
            }

            if (lineAlpha > 0)
            {
                color = new Color(color.Value.r, color.Value.g, color.Value.b, lineAlpha);
                RenderParams primitiveRP = GetBasicRenderParams(_primitiveMaterialIndexed);
                var lines = _bufferUtil.ConvertMeshTopology(triangles, MeshTopology.Triangles, MeshTopology.Lines);
                primitiveRP.matProps.SetBuffer("_Positions", positions);
                primitiveRP.matProps.SetBuffer("_Indexes", lines);
                primitiveRP.matProps.SetColor("_Color", (Color)color);
                Graphics.RenderPrimitives(primitiveRP, MeshTopology.Lines, lines.count);
                lines.Release();
            }

            RenderParams triangleRP;
            if (data != null)
                triangleRP = GetBasicRenderParams(_primitiveTriangleShadedVectorColorMaterial);
            else
                triangleRP = GetBasicRenderParams(_primitiveTriangleShadedMaterial);
            triangleRP.matProps.SetBuffer("_Positions", positions);
            triangleRP.matProps.SetBuffer("_Triangles", triangles);
            color = new Color(color.Value.r, color.Value.g, color.Value.b, triangleAlpha);
            triangleRP.matProps.SetColor("_Color", (Color)color);


            if (triangleAlpha > 0)
            {
                GraphicsBuffer convertedVectors = null;
                if (data != null)
                {
                    convertedVectors =
                        _bufferUtil.ConvertVectorGraphicsBuffer(data, dataDim, 3); // float4
                    triangleRP.matProps.SetBuffer("_Vectors", convertedVectors);
                    triangleRP.matProps.SetInt("_Normalize1", dataNormalized1 ? 1 : 0);
                }

                Graphics.RenderPrimitives(triangleRP, MeshTopology.Triangles, triangles.count);
                convertedVectors?.Release();
            }
        }
    }
}