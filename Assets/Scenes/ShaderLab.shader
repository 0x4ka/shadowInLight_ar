Shader "Hidden/Silhouette"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (0, 0, 0, 1)
		_SaturationThreshold("Saturation Threshold", Range(0.0, 1.0)) = 0.25
		_ValueThreshold("Value Threshold", Range(0.0, 1.0)) = 0.25
	}
		SubShader
		{
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					// UV座標をfloat4型に変更
					float4 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
				};

				// UV変換用行列をセットするための変数を追加
				float4x4 _HomographyMatrix;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);

					// 元のUV座標をUV変換用行列で変換してからセットする
					o.uv = mul(_HomographyMatrix, float4(v.uv, 0.0, 1.0));

					return o;
				}

				sampler2D _MainTex;
				float4 _Color;
				float _SaturationThreshold;
				float _ValueThreshold;

				fixed4 frag(v2f i) : SV_Target
				{
					// Webカメラ上の色をtex2Dに代わってtex2Dprojでサンプリングする
					float3 rawColor = tex2Dproj(_MainTex, i.uv).rgb;

					float value = max(max(rawColor.r, rawColor.g), rawColor.b);
					float lowestValue = min(min(rawColor.r, rawColor.g), rawColor.b);
					float difference = value - lowestValue;
					float saturation = (value > 0.01) ? (difference / value) : 0.0;
					return fixed4(_Color.rgb, step(value, _ValueThreshold) * step(saturation, _SaturationThreshold));
				}
				ENDCG
			}

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
				};

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					return o;
				}

				float4 _Color;

				fixed4 frag(v2f i) : SV_Target
				{
					return fixed4(_Color.rgb, 1.0);
				}
				ENDCG
			}
		}
}