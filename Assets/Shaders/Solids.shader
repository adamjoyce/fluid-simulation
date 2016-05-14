Shader "Unlit/Solids" {
    Properties {
    _MainTex("Texture", 2D) = "white" {}
    }

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
            uniform float2 _Radius;

            // Vertex program.
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            // Fragment program.
            float4 frag(v2f i) : COLOR {
                float4 result = float4(0, 0, 0, 0);

                // Draw bounding edges.
                if (i.uv.x <= _Size.x)
                    result = float4(1, 1, 1, 1);
                else if (i.uv.x >= 1.0 - _Size.x)
                    result = float4(1, 1, 1, 1);

                if (i.uv.y <= _Size.y)
                    result = float4(1, 1, 1, 1);
                else if (i.uv.y >= 1.0 - _Size.y)
                    result = float4(1, 1, 1, 1);

                return result;
            }

            ENDCG
        }
    }
}
