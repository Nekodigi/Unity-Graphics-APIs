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


            #ifdef INSTANCED
            StructuredBuffer<float3> _Positions;
            #else
            float3 _Position;
            #endif
            float _Size;
            float4 _Color;

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                float3 pos = v.vertex.xyz;
                pos *= _Size;
                #ifdef INSTANCED
                pos = _Positions[instanceID];
                #else
                pos = _Position;
                #endif

                v2f o;
                o.vertex = UnityObjectToClipPos(pos);
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