Shader "Hidden/Debug/VectorLine"
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
            CGPROGRAM
            //! Sometimes we have to edit this file to reload shader if not loaded correctly
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                //color
                float3 color : COLOR;
            };

            StructuredBuffer<float3> _Positions;
            StructuredBuffer<float3> _Vectors;
            bool _Normalized;
            float _Multiplier;

            v2f vert(uint vertexID: SV_VertexID, uint instanceID: SV_InstanceID)
            {
                v2f o;
                uint baseIndex = vertexID / 2;
                uint offset = vertexID % 2;
                float3 pos = _Positions[baseIndex];
                float3 vec = _Vectors[baseIndex];
                if (_Normalized)
                {
                    vec = normalize(vec);
                }
                if (offset)
                {
                    pos += vec * _Multiplier;
                }

                o.pos = UnityObjectToClipPos(pos);
                o.color = vec * 0.5 + 0.5;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return float4(i.color, 1);
            }
            ENDCG
        }
    }
}