Shader "Unlit/Display" {
	Properties {
		_MainTex ("Texture", 2D) = "black" {}
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

			sampler2D _MainTex;
            uniform sampler2D _Solids;
			
            // Vertex program.
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }
			
            // Fragment program.
			float4 frag (v2f i) : COLOR {
				// Sample the textures.
				float main = tex2D(_MainTex, i.uv).x;
                float solid = tex2D(_Solids, i.uv).x;

                // Determine the colour 
                float3 color = float3(solid, solid, solid);
                color.x += main.x;        
				return float4(color, 1);
			}
			ENDCG
		}
	}
}
