          Shader "Plexus/Line"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma enable_d3d11_debug_symbols
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/APIs/Particles/DIParticle.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
            };

            StructuredBuffer<SParticle> _ParticleBuffer;
            uint _ParticleCount;

            v2f vert(uint vertexID: SV_VertexID, uint instanceID : SV_InstanceID)
            {
                v2f o;
                uint whichSide = vertexID % 2;
                uint thisID = (vertexID / 2) % _ParticleCount;
                uint otherID = (vertexID / 2) / _ParticleCount;
                float3 pos = float3(0,0,0);
                SParticle thisP = _ParticleBuffer[thisID];
                SParticle otherP = _ParticleBuffer[otherID];
                if (thisP.alive && otherP.alive && distance(thisP.position, otherP.position) < 1)
                {
                    if (whichSide)
                    {
                        pos = _ParticleBuffer[thisID].position;
                    }else
                    {
                        pos = _ParticleBuffer[otherID].position;
                    }
                }
                o.pos = UnityObjectToClipPos(pos) ;
                o.color = float4(1,1,1,1);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}