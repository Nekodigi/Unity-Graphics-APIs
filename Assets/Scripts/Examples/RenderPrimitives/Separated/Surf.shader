Shader "RenderPrimitives/SeparatedSurface"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,0,0,1)
        _Displacement ("Displacement", Float) = 0.1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200
        Cull Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert

        #include "UnityCG.cginc"

        struct Input
        {
            float3 worldPos;
        };

        float _Displacement;
        float4 _Color;

        void vert(inout appdata_full v)
        {
            float3 normalDir = normalize(v.normal);
            float offset = sin(_Time.y + v.vertex.x * 0.1) * _Displacement + _Displacement;
            v.vertex.xyz += normalDir * offset;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}