Shader "Hidden/Debug/Render"
{
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile __ INSTANCED
            #pragma multi_compile __ MESH
            #pragma multi_compile __ SHADED//must be used with indexed
            #pragma multi_compile __ INDEXED
            #pragma multi_compile __ VECTOR

            #include "UnityCG.cginc"

            #ifdef MESH
            struct appdata
            {
                float4 vertex : POSITION;
            #ifdef SHADED
                float3 normal : NORMAL;
            #endif
            };
            #else
            StructuredBuffer<float3> _Vertices;
            #ifdef INDEXED
            StructuredBuffer<uint> _Indices;
            #endif
            #ifdef VECTOR
            StructuredBuffer<float3> _Vectors;
            #endif
            #endif

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : TEXCOORD0;
                #ifdef SHADED
                float3 normal : NORMAL;
                #endif
            };

            float4 _Color;
            float _Size;

            #ifdef INSTANCED
            StructuredBuffer<float3> _Positions;
            #else
            float3 _Position;
            #endif

            #ifdef MESH
            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
            #else
            v2f vert(uint vertexID: SV_VertexID, uint instanceID: SV_InstanceID)
            {
                #endif
                float3 pos;
                #ifdef MESH
                pos = v.vertex.xyz;
                #else
                uint index = vertexID;
                #ifdef VECTOR
                index /= 2;
                uint indexInLine = vertexID % 2;
                #endif
                #ifdef INDEXED
                    index = _Indices[vertexID];
                #endif
                pos = _Vertices[index];
                #ifdef VECTOR
                if (indexInLine == 1)pos += _Vectors[index];
                #endif
                #endif

                pos *= _Size;

                #ifdef INSTANCED
                pos += _Positions[instanceID];
                #else
                pos += _Position;
                #endif


                v2f o;
                o.pos = UnityObjectToClipPos(pos);
                o.color = _Color;
                #ifdef SHADED
                #ifdef MESH
                    o.normal = v.normal;
                #else
                uint baseIndex = vertexID / 3 * 3;
                o.normal = normalize(cross(_Vertices[_Indices[baseIndex + 1]] - _Vertices[_Indices[baseIndex]], _Vertices[_Indices[baseIndex + 2]] - _Vertices[_Indices[baseIndex]]));
                #endif
                #endif
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                #ifdef SHADED
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float intensity = max(0, dot(i.normal, lightDir));
                float3 ambient = 0.1 * i.color;
                float3 diffuse = 0.9 * intensity * i.color;
                return fixed4(ambient + diffuse, _Color.a);
                #else
                return i.color;
                #endif
            }
            ENDCG
        }
    }
}