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

            uniform float _HalfCellSize;
            uniform float2 _Size;

            // Vertex program.
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            float4 frag(v2f i) : COLOR {
                // Find the velocities of surronding cells.
                float2 velocityUp = tex2D(_VelocityTexture, float2(0, _Size.y) + i.uv).xy;
                float2 velocityDown = tex2D(_VelocityTexture, float2(0, -_Size.y) + i.uv).xy;
                float2 velocityLeft = tex2D(_VelocityTexture, float2(-_Size.x, 0) + i.uv).xy;
                float2 velocityRight = tex2D(_VelocityTexture, float2(_Size.x, 0) + i.uv).xy;

                // Find any surronding solids and set their velocities to zero.
                float solidUp = tex2D(_SolidsTexture, float2(0, _Size.y) + i.uv).x;
                if (solidUp > 0) {
                    velocityUp = 0;
                }

                float solidDown = tex2D(_SolidsTexture, float2(0, -_Size.y) + i.uv).x;
                if (solidDown > 0) {
                    velocityDown = 0;
                }

                float solidLeft = tex2D(_SolidsTexture, float2(-_Size.x, 0) + i.uv).x;
                if (solidLeft > 0) {
                    velocityLeft = 0;
                }

                float solidRight = tex2D(_SolidsTexture, float2(_Size.x, 0) + i.uv).x;
                if (solidRight > 0) {
                    velocityRight = 0;
                }

                float divergenceValue = ((velocityUp.y - velocityDown.y) + (velocityRight.x - velocityLeft.x)) _HalfCellSize;
                return float4(divergenceValue, 0, 0, 1);
            }

            ENDCG
        }
    }
}