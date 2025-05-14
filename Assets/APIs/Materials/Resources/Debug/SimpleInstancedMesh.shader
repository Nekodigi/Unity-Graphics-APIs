Shader "Hidden/Debug/SimpleInstancedMesh"


{
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                uint id : TEXCOORD0;
                float3 color : TEXCOORD1;
            };


            StructuredBuffer<float3> _Positions;
            float _Size;

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                float3 p = _Positions[instanceID];
                v.vertex.xyz *= _Size;
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex + p);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return float4(1, 1, 1, 1);
            }
            ENDCG
        }
    }
}