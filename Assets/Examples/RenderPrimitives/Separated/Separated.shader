Shader "RenderPrimitives/Separated"
{
    SubShader
    {
        Pass
        {
            Cull Off


            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
                float3 normal : TEXCOORD0;
            };

            StructuredBuffer<float3> _Positions;
            uniform uint _BaseVertexIndex;
            uniform float4x4 _ObjectToWorld;
            uniform float _NumInstances;

            v2f vert(uint vertexID: SV_VertexID, uint instanceID : SV_InstanceID)
            {
                v2f o;
                int idx = vertexID + _BaseVertexIndex;
                int triBaseIdx = ((int)idx / 3) * 3;
                float3 pos = _Positions[idx];
                float3 triA = _Positions[triBaseIdx + 0];
                float3 triB = _Positions[triBaseIdx + 1];
                float3 triC = _Positions[triBaseIdx + 2];
                float3 mid = (triA + triB + triC) / 3;
                //pos.x += sin(_Time.y) * 1.0f;
                float3 outwardVec = normalize(mid);
                pos += outwardVec * (sin(vertexID * 0.01 + _Time.y) * 0.3f + 0.3f);


                float4 wpos = mul(_ObjectToWorld, float4(pos + float3(instanceID * 20.0f, 0, 0), 1.0f));
                o.pos = mul(UNITY_MATRIX_VP, wpos);
                //o.color = float4(instanceID / _NumInstances, 0.0f, 0.0f, 0.0f);
                o.color = float4(sin(vertexID / 100.0f) / 2 + 0.5, 0, 0, 1);

                float3 worldA = mul(_ObjectToWorld, float4(triA, 1.0f)).xyz;
                float3 worldB = mul(_ObjectToWorld, float4(triB, 1.0f)).xyz;
                float3 worldC = mul(_ObjectToWorld, float4(triC, 1.0f)).xyz;
                float3 normal = normalize(cross(worldB - worldA, worldC - worldA));
                o.normal = normal;

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = max(0, dot(i.normal, lightDir));
                float3 baseColor = float3(1.0, 0.4, 0.2);
                float3 color = baseColor * NdotL;
                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}