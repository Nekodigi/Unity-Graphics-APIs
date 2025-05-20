Shader "Hidden/Debug/InstancedMesh"
{
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #pragma multi_compile __ VECTOR_COLOR

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : TEXCOORD1;
            };


            StructuredBuffer<float3> _Positions;
            float _Size;
            float4 _Color;

            #ifdef VECTOR_COLOR
            StructuredBuffer<float3> _Vectors;
            bool _Normalize1;
            #endif

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                float3 p = _Positions[instanceID];
                v.vertex.xyz *= _Size;
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex + p);
                #ifndef VECTOR_COLOR
                o.color = _Color;
                #endif
                #ifdef VECTOR_COLOR
                o.color = float4(_Vectors[instanceID], _Color.a);
                if (_Normalize1)
                {
                    o.color = o.color * 0.5 + 0.5;
                }
                #endif

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}