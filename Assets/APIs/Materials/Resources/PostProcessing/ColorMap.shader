Shader "Hidden/ColorMap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma multi_compile JET VIRIDIS INFERNO MAGMA PLASMA SDF GRADIENT

            #include "UnityCG.cginc"
            #include "Assets/APIs/Shaders/Resources/Utils/ColorMap.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Offset;
            float _Scale;
            #ifdef GRADIENT
            sampler2D _ColorMap; // Add this line to receive the texture from C# via SetTexture
            #endif

            fixed4 frag(v2f_img i) : SV_Target
            {
                float3 texCol = tex2D(_MainTex, i.uv);
                float value = texCol.r; //TODO multicompile if we need
                float d = (value + _Offset) * _Scale;
                float3 col;
                #if defined(JET)
                col = jet(d);
                #elif defined(VIRIDIS)
                col = viridis(d);
                #elif defined(INFERNO)
                col = inferno(d);
                #elif defined(MAGMA)
                col = magma(d);
                #elif defined(PLASMA)
                col = plasma(d);
                #elif defined(SDF)
                col = sdf(d);
                #elif defined(GRADIENT)
                col = tex2D(_ColorMap, float2(d, 0.0)).rgb;
                #else
                col = vec3(d);
                #endif
                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
}