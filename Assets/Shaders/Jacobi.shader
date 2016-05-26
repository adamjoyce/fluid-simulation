Shader "Custom/Jacobi" {
	SubShader {
        Pass {
            CGPROGRAM
            // Compilation directives.
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Vertex to fragment structure.
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            uniform sampler2D _DivergenceTexture;
            uniform sampler2D _PressureTexture;
            uniform sampler2D _SolidsTexture;



            ENDCG
        }
	}
}
