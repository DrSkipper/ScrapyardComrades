Shader "Custom/OverlayAlphaColor"
{
    Properties {
        _MainTex ("Texture", 2D) = "" {}
        _Color ("Blend Color", Color) = (0.2, 0.3, 1, 1)
		_LerpAmount ("Lerp Amount", float) = 0.5
    }
    
    SubShader {
    
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        
        Cull Off
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Fog { Mode Off }
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;

            uniform float4 _MainTex_ST;
            uniform float4 _Color;
			uniform float _LerpAmount;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
				// Lerp between texture color and overlay color
                float4 texColor = tex2D(_MainTex, i.texcoord);
                fixed4 output = lerp(texColor, _Color, _LerpAmount);
				output.a = texColor.a * _Color.a;
				return output;
            }
            ENDCG
        }
    }  
    
    Fallback off
}
