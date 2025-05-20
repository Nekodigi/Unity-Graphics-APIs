Shader "Hidden/Debug/Trail"
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

            #include "UnityCG.cginc"

            StructuredBuffer<float3> _Vertices;
            StructuredBuffer<uint> _Starts;
            uint _VerticesPerTrail;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : TEXCOORD0;
            };

            float4 _Color;

            v2f vert(uint vertexID: SV_VertexID, uint instanceID: SV_InstanceID)
            {
                uint adjustedVertexID = (vertexID + _Starts[instanceID]) % _VerticesPerTrail;
                uint flatIndex = instanceID * _VerticesPerTrail + adjustedVertexID;
                float3 pos = _Vertices[flatIndex];

                v2f o;
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