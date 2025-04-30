Shader "Sample/AppendConsumeBuffer"
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
            #include "Assets/APIs/Randoms/Random.cginc"
            #include "Assets/APIs/Particles/DIParticle.cginc"

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


            StructuredBuffer<SParticle> _ParticleBuffer;

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                SParticle p = _ParticleBuffer[instanceID];

                v.vertex.xyz *= 0.1;

                v2f o;
                o.vertex = p.alive == 0
                   ? 0
                   : UnityObjectToClipPos(v.vertex + p.position);
                o.id = instanceID;
                o.color = p.color;

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