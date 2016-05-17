Shader "Custom/Buoyancy" {
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

            uniform sampler2D _Velocity;
            uniform sampler2D _Density;
            uniform sampler2D _Temperature;

            uniform float _TimeIncrement;
            uniform float _AmbientTemperature;
            uniform float _ScaleFactor;
            uniform float _Direction;

            // Vertex program.
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            // Fragment program.
            float4 frag(v2f i) : COLOR {
                float temperature = tex2D(_Temperature, i.uv).x;
                float density = tex2D(_Density, i.uv).x;
                float2 velocity = tex2D(_Velocity, i.uv).xy;

                // Apply buoyancy operator where the local temperature is higher than the ambient temperature.
                float2 resultantVelocity = velocity;
                if (temperature > _AmbientTemperature) {
                    float temperatureDifference = temperature - _AmbientTemperature;
                    resultantVelocity += _TimeIncrement * (temperatureDifference * (_ScaleFactor - density) * _Direction) * float2(0, 1);
                }

                return float4(resultantVelocity, 0, 1);

            }

            ENDCG
        }
	}
}
