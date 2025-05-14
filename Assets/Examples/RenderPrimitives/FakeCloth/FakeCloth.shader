Shader "Unlit/FakeCloth"
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
            };

            v2f vert(uint vid : SV_VertexID)
            {
                v2f o;
                float3 pos = _VertexBuffer[vid].position;
                float2 uv = float2(pos.x + 0.5, pos.z + 0.5);
                o.vertex = UnityObjectToClipPos(float4(pos, 1));
                o.uv = TRANSFORM_TEX(uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDHLSL
        }
    }
}
