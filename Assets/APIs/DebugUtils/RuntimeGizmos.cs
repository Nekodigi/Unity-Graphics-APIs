using System;
using APIs.Geometry;
using APIs.Shaders;
using Common.ShaderUtils;
using JetBrains.Annotations;
using MarchingCubes;
using UnityEngine;


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

        public static ((MeshBuilder, GraphicsBuffer, MeshFilter), RenderTexture, ComputeShader) InitSDFHistory(
            Vector3Int resolution,
            GameObject parent,
            int triangleBudget = 655360)
        {
            var renderTexture = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.RGFloat,
                RenderTextureReadWrite.Linear)
            {
                volumeDepth = resolution.z,
                dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
                enableRandomWrite = true
            };
            renderTexture.Create();
            var sdfCache = InitSDF(resolution, parent, triangleBudget);
            var computeShader = Resources.Load<ComputeShader>("Algorithms/SDFHistory");
            return (sdfCache, renderTexture, computeShader);
        }

        public static void UpdateSDFHistory(RenderTexture sourceTexture,
            ((MeshBuilder, GraphicsBuffer, MeshFilter), RenderTexture renderTex3D, ComputeShader computeShader) cache)
        {
            cache.computeShader.SetTexture(0, "_InTex", sourceTexture);
            var rt3D = cache.renderTex3D;
            cache.computeShader.SetTexture(0, "_PrevHistoryTex", rt3D);
            cache.computeShader.SetTexture(0, "_OutHistoryTex", rt3D);
            cache.computeShader.SetVector("_Texture3DResolution",
                new Vector3(rt3D.width, rt3D.height, rt3D.volumeDepth));
            cache.computeShader.AutoDispatch(0, rt3D.width, rt3D.height, rt3D.volumeDepth);
        }

        public static void DrawSDFHistory(
            ((MeshBuilder, GraphicsBuffer, MeshFilter) sdfCache, RenderTexture renderTex3D, ComputeShader) cache,
            float targetValue = 0,
            float size = 1)

        {
            DrawSDF(cache.renderTex3D, cache.sdfCache, targetValue, size);
        }

        public static (MeshBuilder, GraphicsBuffer, MeshFilter) InitSDF(Vector3Int resolution, GameObject parent,
            int triangleBudget = 655360)
        {
            //Create primitive
            var gameObject = new GameObject("SDF");
            var meshFilter = gameObject.AddComponent<MeshFilter>();
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            var builderCompute = Resources.Load<ComputeShader>("ThirdParty/MarchingCubes/MarchingCubes");
            var meshBuilder = new MeshBuilder(resolution, triangleBudget,
                builderCompute);
            var voxelBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured,
                resolution.x * resolution.y * resolution.z
                , sizeof(float));
            return (meshBuilder, voxelBuffer, meshFilter);
        }

        public static void DrawSDF(RenderTexture texture,
            (MeshBuilder meshBuilder, GraphicsBuffer voxelBuffer, MeshFilter meshFilter) cache, float targetValue = 0,
            float size = 1)
        {
            var volumeCompute = Resources.Load<ComputeShader>("ThirdParty/MarchingCubes/TextureToVoxel");
            volumeCompute.SetInts("_Dims", new Vector3Int(texture.width, texture.height, texture.volumeDepth));
            volumeCompute.SetFloat("_Time", Time.time);
            volumeCompute.SetTexture(0, "_Texture3D", texture);
            volumeCompute.SetBuffer(0, "_Voxels", cache.voxelBuffer);
            volumeCompute.SetVector("_VoxelResolution",
                new Vector3(texture.width, texture.height, texture.volumeDepth));
            volumeCompute.DispatchThreads(0, new Vector3Int(texture.width, texture.height, texture.volumeDepth));
            cache.meshBuilder.BuildIsosurface(cache.voxelBuffer, targetValue,
                size / Mathf.Max(texture.width, texture.height, texture.volumeDepth));
            cache.meshFilter.sharedMesh = cache.meshBuilder.Mesh;
        }

        public static ComputeShader InitCopyTexture3D()
        {
            return Resources.Load<ComputeShader>("Utils/CopyTexture3D");
        }

        public static void CopyTexture3D(Texture3D source, RenderTexture destination, ComputeShader cs)
        {
            if (source == null || destination == null || cs == null) return;
            if (!destination.enableRandomWrite)
            {
                Debug.LogError("Destination RenderTexture must have enableRandomWrite set to true for compute shader.",
                    destination);
                return;
            }

            cs.SetTexture(0, "_SourceTex", source);
            cs.SetTexture(0, "_DestTex", destination);

            cs.AutoDispatch(0, destination.width, destination.height,
                destination.volumeDepth);
        }

        public static ((RenderTexture, Material, Material)[], RenderTexture[], GameObject[]) InitSDFSegment(
            Texture3D texture, Transform parent)
        {
            var caches = new (RenderTexture, Material, Material)[3];
            var sliceRTs = new RenderTexture[3];
            var quads = new GameObject[3];
            Debug.Log(texture.depth);
            for (int i = 0; i < 3; i++)
            {
                //create quad
                quads[i] = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quads[i].transform.SetParent(parent);
                //rotation Y90 = X, X90=Y 0 = Z
                switch (i)
                {
                    case 0:
                        quads[i].transform.rotation = Quaternion.Euler(0, 90, 0);
                        break;
                    case 1:
                        quads[i].transform.rotation = Quaternion.Euler(90, 0, 0);
                        break;
                    default:
                        break;
                }

                RenderTexture sliceRT;
                switch (i)
                {
                    case 0:
                        sliceRT = new RenderTexture(texture.depth, texture.height, 0, RenderTextureFormat.RFloat);
                        break;
                    case 1:
                        sliceRT = new RenderTexture(texture.width, texture.depth, 0, RenderTextureFormat.RFloat);
                        break;
                    default:
                        sliceRT = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.RFloat);
                        break;
                }

                sliceRT.enableRandomWrite = true;
                sliceRT.Create();
                sliceRTs[i] = sliceRT;
                var cache = RuntimeGizmos.InitTexture(sliceRT, quads[i].GetComponent<Renderer>());
                caches[i] = cache;
            }

            return (caches, sliceRTs, quads);
        }


        public static void DrawSDFSegment(Texture3D texture, Vector3Int slicePosition,
            ((RenderTexture dest, Material colorMapMat, Material renderMat)[] dTcaches, RenderTexture[] sliceRTs,
                GameObject[] quads) caches, float displayScale = 1f,
            ColorMapType colorMapType = ColorMapType.JET, float scale = 1f, float offset = 0f, Gradient gradient = null)
        {
            var keywords = new[]
            {
                "X_PLANE", "Y_PLANE", "Z_PLANE"
            };
            for (int i = 0; i < 3; i++)
            {
                var sliceCs = Resources.Load<ComputeShader>("SliceVolume");
                for (int j = 0; j < keywords.Length; j++)
                {
                    if (i == j)
                    {
                        sliceCs.EnableKeyword(keywords[j]);
                    }
                    else
                    {
                        sliceCs.DisableKeyword(keywords[j]);
                    }
                }

                sliceCs.SetTexture(0, "_Texture3D", texture);
                sliceCs.SetTexture(0, "_OutputTexture", caches.sliceRTs[i]);
                sliceCs.SetInt("_SlicePosition", slicePosition[i]);

                int highestEdgeRes = Mathf.Max(texture.width, texture.height, texture.depth);
                Vector3 rawQuadScale = new Vector3((float)texture.width / highestEdgeRes,
                    (float)texture.height / highestEdgeRes,
                    (float)texture.depth / highestEdgeRes);
                Vector3 quadScale = rawQuadScale * displayScale;
                switch (i)
                {
                    case 0:
                        float facX = ((float)slicePosition.x / texture.width) - 0.5f;
                        facX *= displayScale;
                        caches.quads[i].transform.position = new Vector3(facX * rawQuadScale.x, 0, 0);
                        caches.quads[i].transform.localScale = new Vector3(quadScale.z, quadScale.y, 1);
                        sliceCs.AutoDispatch(0, texture.depth, texture.height, 1);
                        break;
                    case 1:
                        float facY = ((float)slicePosition.y / texture.height) - 0.5f;
                        facY *= displayScale;
                        caches.quads[i].transform.position = new Vector3(0, facY * rawQuadScale.y, 0);
                        caches.quads[i].transform.localScale = new Vector3(quadScale.x, quadScale.z, 1);
                        sliceCs.AutoDispatch(0, texture.width, texture.depth, 1);
                        break;
                    default:
                        float facZ = ((float)slicePosition.z / texture.depth) - 0.5f;
                        facZ *= displayScale;
                        caches.quads[i].transform.position = new Vector3(0, 0, facZ * rawQuadScale.z);
                        caches.quads[i].transform.localScale = new Vector3(quadScale.x, quadScale.y, 1);
                        sliceCs.AutoDispatch(0, texture.width, texture.height, 1);
                        break;
                }


                RuntimeGizmos.DrawTexture(
                    caches.sliceRTs[i],
                    caches.dTcaches[i],
                    colorMapType,
                    scale,
                    offset,
                    gradient
                );
            }
        }

        public static (RenderTexture, Material, Material) InitTexture(Texture texture, Renderer renderer)
        {
            RenderTexture renderTexture =
                new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            Material colorMapMat = new Material(Shader.Find("Hidden/ColorMap"));
            Material renderMat = new Material(Shader.Find("Unlit/DoubleSide"));
            renderer.sharedMaterial = renderMat;
            return (renderTexture, colorMapMat, renderMat);
        }

        public static void DrawTexture(Texture source,
            (RenderTexture dest, Material colorMapMat, Material renderMat) cache,
            ColorMapType colorMap = ColorMapType.JET,
            float scale = 1f, float offset = 0f, Gradient gradient = null)
        {
            cache.colorMapMat.SetFloat("_Scale", scale);
            cache.colorMapMat.SetFloat("_Offset", offset);
            foreach (ColorMapType type in Enum.GetValues(typeof(ColorMapType)))
            {
                cache.colorMapMat.DisableKeyword(type.ToString());
            }

            cache.colorMapMat.EnableKeyword(colorMap.ToString());
            if (gradient != null)
            {
                cache.colorMapMat.SetTexture("_ColorMap", gradient.ToTexture(128));
            }

            Graphics.Blit(source, cache.dest, cache.colorMapMat);
            cache.renderMat.mainTexture = cache.dest;
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