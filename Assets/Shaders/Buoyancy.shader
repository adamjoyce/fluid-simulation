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

            uniform sampler2D _VelocityTexture;
            uniform sampler2D _DensityTexture;
            uniform sampler2D _TemperatureTexture;

            uniform float _TimeIncrement;
            uniform float _AmbientTemperature;
            uniform float _FluidBuoyancy;
            uniform float _FluidWeight;

            // Vertex program.
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            // Fragment program.
            float4 frag(v2f i) : COLOR {
                float density = tex2D(_DensityTexture, i.uv).x;
                float temperature = tex2D(_TemperatureTexture, i.uv).x;
                float2 velocity = tex2D(_VelocityTexture, i.uv).xy;

                // Apply buoyancy operator where the local temperature is higher than the ambient temperature.
                float2 resultantVelocity = velocity;
                if (temperature > _AmbientTemperature) {
                    float2 direction = float2(0, 1);
                    float temperatureDifference = temperature - _AmbientTemperature;
                    resultantVelocity += (_TimeIncrement * temperatureDifference * _FluidBuoyancy - density * _FluidWeight) * direction;
                }

                return float4(resultantVelocity, 0, 1);
            }

            ENDCG
        }
	}
}
