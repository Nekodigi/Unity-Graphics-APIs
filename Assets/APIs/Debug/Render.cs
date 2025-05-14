using UnityEngine;

namespace APIs.Debug
{
    public class Render
    {
        private Material _simpleInstancedMeshMaterial;
        private Material _simplePrimitiveMaterial;
        
        public Render()
        {
            InitRenderParams();
        }
        
        private void InitRenderParams()
        {
            _simpleInstancedMeshMaterial = new Material(Shader.Find("Hidden/Debug/SimpleInstancedMesh"));
            _simplePrimitiveMaterial = new Material(Shader.Find("Hidden/Debug/SimplePrimitive"));
        }
        
        private RenderParams GetBasicRenderParams(Material mat)
        {
            RenderParams renderParams = new RenderParams(mat);
            renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000);
            renderParams.matProps = new MaterialPropertyBlock();
            return renderParams;
        }
        
        public void DrawPositions(float size, GraphicsBuffer buffer, bool withLine = false)
        {
            //use cube mesh
            Mesh cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            RenderParams meshRP = GetBasicRenderParams(_simpleInstancedMeshMaterial);
            meshRP.matProps.SetBuffer("_Positions", buffer);
            meshRP.matProps.SetFloat("_Size", size);
            Graphics.RenderMeshPrimitives(meshRP, cubeMesh, 0, buffer.count);
            if(withLine){
                RenderParams primitiveRP = GetBasicRenderParams(_simplePrimitiveMaterial);
                primitiveRP.matProps.SetBuffer("_Positions", buffer);
                Graphics.RenderPrimitives(primitiveRP, MeshTopology.LineStrip, buffer.count);
            }
        }
    }
}