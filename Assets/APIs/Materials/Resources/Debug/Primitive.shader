Shader "Hidden/Debug/Primitive"
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
        }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile __ INSTANCED

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : TEXCOORD0;
            };

            StructuredBuffer<float3> _Vertices;
            float4 _Color;
            float _Size;
            #ifdef INSTANCED
            StructuredBuffer<float3> _Positions;
            #else

            float3 _Position;
            #endif

            v2f vert(uint vertexID: SV_VertexID, uint instanceID: SV_InstanceID)
            {
                v2f o;
                float3 pos = _Vertices[vertexID];
                float size = _Size;
                pos *= size;
                #ifdef INSTANCED
                pos += _Positions[instanceID];
                #else
                pos += _Position;
                #endif

                o.pos = UnityObjectToClipPos(pos);
                o.color = _Color;
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