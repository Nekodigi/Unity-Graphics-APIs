using APIs.DebugUtils;
using APIs.Shaders;
using MarchingCubes;
using UnityEngine;
using UnityEngine.UI;

namespace Examples.SDF.SDFHistory
{
    public class SDFHistory : MonoBehaviour
    {
        [SerializeField] float targetValue = 0;

        private Material _effectMaterial;
        private ComputeShader _sdfHistoryCs;

        public int textureWidth = 256;
        public int textureHeight = 256;
        public int history = 256;

        [Header("Gray-Scott Parameters")] public float feedRate = 0.055f;
        public float killRate = 0.062f;
        public float diffusionU = 1.0f;
        public float diffusionV = 0.5f;
        public float deltaTime = 1.0f;

        [Header("Initialization")] public Vector2Int seedCenter = new Vector2Int(256, 256);
        public int seedRadius = 20;
        public bool initializeOnStart = true;

        public int iterationsPerFrame = 10;

        // Ping-pong textures
        private RenderTexture sourceTexture;
        private RenderTexture destinationTexture;
        private RenderTexture _historyTexture;
        private (RenderTexture, Material, Material) _caches;
        private ((MeshBuilder, GraphicsBuffer, MeshFilter), RenderTexture, ComputeShader) _sdfHistoryCaches;
        private MeshFilter _meshFilter;

        void Start()
        {
            _effectMaterial = new Material(Shader.Find("Hidden/GrayScottEffect"));
            sourceTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.RGFloat,
                RenderTextureReadWrite.Linear);
            sourceTexture.enableRandomWrite = true;
            sourceTexture.filterMode = FilterMode.Bilinear;
            sourceTexture.wrapMode = TextureWrapMode.Repeat;
            sourceTexture.Create();

            destinationTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.RGFloat,
                RenderTextureReadWrite.Linear);
            destinationTexture.enableRandomWrite = true;
            destinationTexture.filterMode = sourceTexture.filterMode;
            destinationTexture.wrapMode = sourceTexture.wrapMode;
            destinationTexture.Create();
            _sdfHistoryCaches = RuntimeGizmos.InitSDFHistory(new Vector3Int(textureWidth, textureHeight, history),
                gameObject, 6000000);


            if (initializeOnStart)
            {
                InitializeTextures();
            }
        }

        void InitializeTextures()
        {
            // Create a temporary texture to set initial values
            Texture2D initialData = new Texture2D(textureWidth, textureHeight, TextureFormat.RGFloat, false, true);
            Color[] pixels = new Color[textureWidth * textureHeight];

            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    float u = 1.0f; // U = 1 everywhere
                    float v = 0.0f; // V = 0 everywhere

                    // Seed a square/circle of V
                    float distSq = (x - seedCenter.x) * (x - seedCenter.x) + (y - seedCenter.y) * (y - seedCenter.y);
                    if (distSq < seedRadius * seedRadius)
                    {
                        v = 0.75f + Random.Range(-0.1f, 0.1f); // Some V in the center
                        u = 0.25f + Random.Range(-0.1f, 0.1f); // Less U in the center
                    }

                    pixels[y * textureWidth + x] = new Color(u, v, 0, 1);
                }
            }

            initialData.SetPixels(pixels);
            initialData.Apply();

            // Blit the initial data to both textures (or just source, then one sim step to dest)
            Graphics.Blit(initialData, sourceTexture);
            Graphics.Blit(initialData, destinationTexture); // Start them identical

            Destroy(initialData); // Clean up temporary texture

            Debug.Log("Textures Initialized. Source: " + sourceTexture.name + " Dest: " + destinationTexture.name);
        }

        void Update()
        {
            if (_effectMaterial == null || sourceTexture == null || destinationTexture == null) return;

            // Set shader parameters
            _effectMaterial.SetFloat("_FeedRate", feedRate);
            _effectMaterial.SetFloat("_KillRate", killRate);
            _effectMaterial.SetFloat("_DiffusionU", diffusionU);
            _effectMaterial.SetFloat("_DiffusionV", diffusionV);
            _effectMaterial.SetFloat("_DeltaTime", deltaTime);
            // _MainTex_TexelSize is set automatically by Graphics.Blit

            for (int i = 0; i < iterationsPerFrame; i++)
            {
                // The sourceTexture for this pass is the output of the previous pass
                Graphics.Blit(sourceTexture, destinationTexture, _effectMaterial);

                // Ping-pong: the destination becomes the source for the next iteration
                RenderTexture temp = sourceTexture;
                sourceTexture = destinationTexture;
                destinationTexture = temp;
            }

            if (Time.frameCount % 5 == 0)
            {
                RuntimeGizmos.UpdateSDFHistory(sourceTexture, _sdfHistoryCaches);
            }

            RuntimeGizmos.DrawSDFHistory(_sdfHistoryCaches, targetValue);
        }

        void OnDestroy()
        {
            if (_effectMaterial != null) Destroy(_effectMaterial);
            if (sourceTexture != null) sourceTexture.Release();
            if (destinationTexture != null) destinationTexture.Release();
        }
    }
}