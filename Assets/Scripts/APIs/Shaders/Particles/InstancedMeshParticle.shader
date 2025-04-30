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
            #include "Assets/Scripts/APIs/HLSLs/Random.cginc"
            #include "Assets/Scripts/APIs/HLSLs/Particle.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                uint id : TEXCOORD0;
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

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return float4(random3(i.id), 1);
            }
            ENDCG
        }
    }
}