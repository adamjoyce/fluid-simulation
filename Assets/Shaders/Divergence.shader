Shader "Custom/Divergence" {
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

            uniform sampler2D _VelocityTexture;
            uniform sampler2D _SolidsTexture;

            uniform float _HalfInverseCellSize;
            uniform float2 _InverseSize;

            // Vertex program.
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            float4 frag(v2f IN) : COLOR {
                // Find the veolicties of surronding cells.

                // Find any ssurronding solids and set their velocities to zero.
            }

            ENDCG
        }
    }
}