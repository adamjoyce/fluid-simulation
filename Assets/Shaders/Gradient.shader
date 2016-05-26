﻿Shader "Custom/Gradient" {
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
            uniform sampler2D _PressureTexture;
            uniform sampler2D _SolidsTexture;

            uniform float2 _Size;
            uniform float _GradientScale;

            // Vertex program.
            v2f vert(appdata_base v) {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }

            // Fragment program.
            float4 frag(v2f i) : COLOR {
                // Pressure at the centre.
                float pressureCentre = tex2D(_PressureTexture, i.uv).x;

                // Find the pressure of surronding cells.
                float pressureUp = tex2D(_PressureTexture, float2(0, _Size.y) + i.uv).x;
                float pressureDown = tex2D(_PressureTexture, float2(0, -_Size.y) + i.uv).x;
                float pressureLeft = tex2D(_PressureTexture, float2(-_Size.x, 0) + i.uv).x;
                float pressureRight = tex2D(_PressureTexture, float2(_Size.x, 0) + i.uv).x;

                // Find any surronding solids and set their pressure to the central pressure.
                float solidUp = tex2D(_SolidsTexture, float2(0, _Size.y) + i.uv).x;
                if (solidUp > 0) {
                    pressureUp = pressureCentre;
                }

                float solidDown = tex2D(_SolidsTexture, float2(0, -_Size.y) + i.uv).x;
                if (solidDown > 0) {
                    pressureDown = pressureCentre;
                }

                float solidLeft = tex2D(_SolidsTexture, float2(-_Size.x, 0) + i.uv).x;
                if (solidLeft > 0) {
                    pressureLeft = pressureCentre;
                }

                float solidRight = tex2D(_SolidsTexture, float2(_Size.x, 0) + i.uv).x;
                if (solidRight > 0) {
                    pressureRight = pressureCentre;
                }

                // Free slip boundary condition.
                float2 gradient = _GradientScale * float2(pressureLeft - pressureRight, pressureUp - pressureDown);
                float2 velocity = tex2D(_VelocityTexture, i.uv).xy;
                float2 newVelocity = velocity - gradient;

                return float4(newVelocity, 0, 1);
            }

            ENDCG
        }
    }
}
