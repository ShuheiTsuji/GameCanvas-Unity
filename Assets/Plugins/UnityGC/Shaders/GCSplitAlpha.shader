Shader "Custom/GCSplitAlpha" {
	Properties {
		[HideInInspector] _MainTex ("元テクスチャ", 2D) = "white" {}
		[Enum(RGB,0,Alpha,1)] _Mode("動作モード", float) = 0
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _Mode;

			fixed4 frag(v2f_img i) : COLOR
			{
				if (_Mode == 1)
				{
					fixed alpha = tex2D(_MainTex, i.uv).a;
					return fixed4(alpha, alpha, alpha, 1);
				}
				else
				{
					return fixed4(tex2D(_MainTex, i.uv).rgb, 1);
				}
			}
			ENDCG
		}
	}
}
