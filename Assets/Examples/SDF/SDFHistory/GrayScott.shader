Shader "Hidden/GrayScottEffect"
{
    Properties
    {
        _MainTex ("Texture (UV Data - Prev State)", 2D) = "white" {} // Stores U in R, V in G
        _FeedRate ("Feed Rate (F)", Float) = 0.055
        _KillRate ("Kill Rate (k)", Float) = 0.062
        _DiffusionU ("Diffusion U (Du)", Float) = 1.0
        _DiffusionV ("Diffusion V (Dv)", Float) = 0.5
        _DeltaTime ("Delta Time", Float) = 1.0
        // _TexelSize is automatically provided by Unity when using Graphics.Blit
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

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // (1/width, 1/height, width, height)

            float _FeedRate;
            float _KillRate;
            float _DiffusionU;
            float _DiffusionV;
            float _DeltaTime;

            // Function to sample UV values, simple clamp to edge
            // For toroidal (wrap-around), you'd use frac(uv)
            float2 Sample(float2 uv)
            {
                return tex2D(_MainTex, uv).rg; // Assuming U in R, V in G
            }

            float2 Laplacian(float2 uv_center)
            {
                float2 sum = float2(0, 0);
                float dx = _MainTex_TexelSize.x;
                float dy = _MainTex_TexelSize.y;

                // Direct neighbors (Von Neumann)
                sum += Sample(uv_center + float2(0, dy)) * 0.2;
                sum += Sample(uv_center + float2(0, -dy)) * 0.2;
                sum += Sample(uv_center + float2(dx, 0)) * 0.2;
                sum += Sample(uv_center + float2(-dx, 0)) * 0.2;

                // Diagonal neighbors (Moore)
                sum += Sample(uv_center + float2(dx, dy)) * 0.05;
                sum += Sample(uv_center + float2(dx, -dy)) * 0.05;
                sum += Sample(uv_center + float2(-dx, dy)) * 0.05;
                sum += Sample(uv_center + float2(-dx, -dy)) * 0.05;

                sum -= Sample(uv_center); // Subtract center pixel contribution from the weighted sum

                return sum;
            }

            float4 frag(v2f_img i) : SV_Target
            {
                float2 uv_coords = i.uv;
                float2 chemical_uv = tex2D(_MainTex, uv_coords).rg; // R = U, G = V

                float u = chemical_uv.x;
                float v = chemical_uv.y;

                float2 laplace = Laplacian(uv_coords);

                float reactionRate = u * v * v;

                float newU = u + (_DiffusionU * laplace.x - reactionRate + _FeedRate * (1.0 - u)) * _DeltaTime;
                float newV = v + (_DiffusionV * laplace.y + reactionRate - (_KillRate + _FeedRate) * v) * _DeltaTime;

                // Clamp values
                newU = clamp(newU, 0.0, 1.0);
                newV = clamp(newV, 0.0, 1.0);

                // Output new U in R, new V in G.
                // For visualization directly, you could do: return float4(newU, newU, newU, 1.0);
                // Or map newV to color, or a mix. Here we store data for further processing or specific visualization.
                return float4(newU, newV, 0.0, 1.0);
            }
            ENDCG
        }
    }
}