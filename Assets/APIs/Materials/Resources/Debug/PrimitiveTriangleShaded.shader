Shader "Hidden/Debug/PrimitiveTriangleShaded"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            //"LightMode"="ForwardBase"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile __ VECTOR_COLOR

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float3 color : TEXCOORD1;
            };

            StructuredBuffer<float3> _Positions;
            StructuredBuffer<uint> _Triangles;
            float4 _Color;

            #ifdef VECTOR_COLOR
            StructuredBuffer<float3> _Vectors;
            bool _Normalize1;
            #endif

            v2f vert(uint vertexID: SV_VertexID, uint instanceID: SV_InstanceID)
            {
                uint baseID = vertexID / 3;
                float3 v0 = _Positions[_Triangles[baseID * 3]];
                float3 v1 = _Positions[_Triangles[baseID * 3 + 1]];
                float3 v2 = _Positions[_Triangles[baseID * 3 + 2]];
                float3 normal = normalize(cross(v1 - v0, v2 - v0));

                v2f o;
                float3 pos = _Positions[_Triangles[vertexID]];
                o.pos = UnityObjectToClipPos(pos);
                o.normal = normal;
                #ifndef VECTOR_COLOR
                o.color = _Color.rgb;
                #endif
                #ifdef VECTOR_COLOR
                o.color = float4(_Vectors[baseID], _Color.a);
                if (_Normalize1)
                {
                    o.color = o.color * 0.5 + 0.5;
                }
                #endif
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float intensity = max(0, dot(i.normal, lightDir));
                float3 ambient = 0.1 * i.color;
                float3 diffuse = 0.9 * intensity * i.color;
                return fixed4(ambient + diffuse, _Color.a);
            }
            ENDCG
        }
    }
}