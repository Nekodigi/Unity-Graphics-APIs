Shader "Unlit/ClothTrail"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"

            struct Vertex
            {
                float3 position;
                float3 normal;
            };
            StructuredBuffer<Vertex> _VertexBuffer;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert(uint vid : SV_VertexID)
            {
                v2f o;
                float3 pos = _VertexBuffer[vid].position;
                float3 normal = _VertexBuffer[vid].normal;
                float2 uv = float2(pos.x + 0.5, pos.z + 0.5);
                o.vertex = UnityObjectToClipPos(float4(pos, 1));
                o.uv = TRANSFORM_TEX(uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(normal);
                o.worldPos = mul(unity_ObjectToWorld, float4(pos, 1)).xyz;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Use Unity's built-in light direction (_WorldSpaceLightPos0)
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 normal = normalize(i.normal);
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);

                // Ambient
                float3 ambient = float3(0.18, 0.18, 0.18);

                // Diffuse
                float diff = saturate(dot(normal, lightDir));

                // Specular (Blinn-Phong)
                float3 halfDir = normalize(lightDir + viewDir);
                float spec = pow(saturate(dot(normal, halfDir)), 32.0);

                float3 color = tex2D(_MainTex, i.uv).rgb;
                float3 finalColor = color * (ambient + diff * 0.85) + float3(1,1,1) * spec * 0.25;
                return float4(finalColor, 1);
            }
            ENDHLSL
        }
    }
}
