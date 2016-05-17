Shader "Custom/Impulse" {
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
			
            uniform sampler2D _SourceTexture;
            uniform float2 _Location;
            uniform float _Radius;
            uniform float _Fill;

            // Vertex program.
			v2f vert(appdata_base v) {
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy;
				return o;
			}
			
            // Fragment program.
			float4 frag (v2f i) : COLOR {
                float dist = distance(_Location, i.uv);
                float source = tex2D(_SourceTexture, i.uv).x;

                float impulse = 0;
                if (dist < _Radius) {
                    float difference = (_Radius - dist) * 0.5;
                    impulse = min(difference, 1.0);
                }

                return max(0, lerp(source, _Fill, impulse)).xxxx;
			}

			ENDCG
		}
	}
}
