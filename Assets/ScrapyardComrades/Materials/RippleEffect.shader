Shader "Custom/RippleEffect"
{
    Properties {
        _MainTex ("Texture", 2D) = "" {}
		_T ("Time", float) = 0.0
		_Intensity ("Intensity", float) = 0.02
		_Bounds ("Location Bounds", Vector) = (0.0, 0.0, 0.0, 0.0)
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
			uniform float _T;
			uniform float _Intensity;
			uniform float4 _Bounds;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex) - _Bounds.xy;
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
				float2 p = i.texcoord; //2.0 * i.texcoord - 1.0;
				float len = length(p);
				float2 uv = i.texcoord + (p / len) * cos(len * 12.0 - _T * 4.0) * _Intensity;
				uv = lerp(uv, p, min(1, length(p / _Bounds.zw))) + _Bounds.xy;
				return tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(uv, _MainTex_ST));
            }
            ENDCG
        }
    }
    
    Fallback off
}
