Shader "Custom/Advect" {
    SubShader {
        Pass {
            CGPROGRAM
            // Compilation directives.
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform sampler2D _VelocityTexture;
            uniform sampler2D _SourceTexture;
            uniform sampler2D _Obstacles;

            uniform float2 _InverseSize;
            uniform float _TimeStep;
            uniform float _Dissipation;
            
            // Vertex to fragment structure.
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Vertex program.
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            // Fragment program.
            float4 frag(v2f i) : COLOR {
                float4 result;
                
                // See if an obstacle is present.
                float solidObstacle = tex2D(_Obstacles, i.uv).x;
                if (solidObstacle > 0.0) {
                    result = float4(0, 0, 0, 0);
                    return result;
                }

                // Input velocity.
                float2 velocity = tex2D(_VelocityTexture, i.uv).xy;

                // Coordinate accounting for the inverse size, the timestep, and the input velocity.
                float2 nextCoord = i.uv - (_InverseSize * _TimeStep * velocity);

                // Account for dissipation and get the result on the source texture.
                result = tex2D(_SourceTexture, nextCoord) * _Dissipation;

                return result;
            }

            ENDCG
        }
    }
}
