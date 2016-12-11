Shader "Custom/GameCanvas/DrawImage" {
	Properties{
		_MainTex ("キャンバス", 2D) = "white" {}
		_ImageTex("画像データ(RGB)", 2D) = "white" {}
		_AlphaTex("画像データ(Alpha)", 2D) = "white" {}
		_Clip("切り抜き範囲 (left, top, right, bottom)", Vector) = (0, 0, 0, 0)
		[Toggle] _EnableAlphaSplit("アルファ分割", float) = 0
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex, _ImageTex, _AlphaTex;
			half4 _MainTex_TexelSize, _ImageTex_TexelSize;
			float4 _Clip;
			float4x4 _Matrix;
			float _EnableAlphaSplit;

			fixed4 frag(v2f_img i) : COLOR
			{
				float2 pm = i.uv * _MainTex_TexelSize.zw;                			// 処理するピクセル座標
				float2 pi = mul(_Matrix, float4(pm, 0, 1));							// 対応する画像データのピクセル座標
				float2 si = _ImageTex_TexelSize.zw - _Clip.zw - _Clip.xy;			// 右下の画像ピクセル座標

				if (pi.x >= 0 && pi.y >= 0 && pi.x < si.x && pi.y < si.y)
				{
					float2 uv = float2((pi.x+_Clip.x) * _ImageTex_TexelSize.x, 1 - (pi.y+_Clip.y) * _ImageTex_TexelSize.y);
					fixed4 c = tex2D(_ImageTex, uv);
					fixed4 a = _EnableAlphaSplit == 0 ? c.a : tex2D(_AlphaTex, uv);
					if (a.r != 1)
						return fixed4(lerp(tex2D(_MainTex, i.uv).rgb, c.rgb, a.r), 1);
					else
						return c;
				}
				else
				{
					return tex2D(_MainTex, i.uv);
				}
			}
			ENDCG
		}
	}
}
