Shader "Custom/Solids" {
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

            uniform float2 _Size;
            uniform float2 _Location;
            uniform float _Radius;

            // Vertex program.
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            // Fragment program.
            float4 frag(v2f i) : COLOR {
                float4 color = float4(0, 0, 0, 0);

                // Draw bounding edges.
                if (i.uv.x <= _Size.x) {
                    color = float4(1, 1, 1, 1);
                }
                if (i.uv.x >= 1.0 - _Size.x) {
                    color = float4(1, 1, 1, 1);
                }

                if (i.uv.y <= _Size.y) {
                    color = float4(1, 1, 1, 1);
                }
                if (i.uv.y >= 1.0 - _Size.y) {
                    color = float4(1, 1, 1, 1);
                }

                // Draw point in circle.
                float loc = distance(_Location, i.uv);
                if (loc < _Radius) {
                    color = float4(1, 1, 1, 1);
                }

                return color;
            }

            ENDCG
        }
    }
}
