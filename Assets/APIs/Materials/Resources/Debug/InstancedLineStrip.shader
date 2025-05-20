Shader "Hidden/Debug/InstancedLineStrip"
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

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            StructuredBuffer<float3> _Positions;
            StructuredBuffer<uint> _Starts;
            float4 _Color;
            uint _SkeletonCount;

            v2f vert(uint vertexID: SV_VertexID, uint instanceID: SV_InstanceID)
            {
                uint index = instanceID * _SkeletonCount + (vertexID + _Starts[instanceID]) % _SkeletonCount;
                v2f o;
                float3 pos = _Positions[index];
                o.pos = UnityObjectToClipPos(pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}