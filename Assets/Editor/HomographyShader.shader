// Upgrade NOTE: upgraded instancing buffer 'PerDrawSprite' to new syntax.

Shader "Sprites/Homography"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
		[HideInInspector] _BottomLeftTopLeft("BottomLeftTopLeft", Vector) = (0,0,0,1) // 追加...左下と左上の隅
		[HideInInspector] _TopRightBottomRight("TopRightBottomRight", Vector) = (1,1,1,0) // 追加...右上と右下の隅
		[HideInInspector] _HomographyMatrixR0("HomographyMatrixR0", Vector) = (1,0,0,0) // 追加...UV変換行列0行目
		[HideInInspector] _HomographyMatrixR1("HomographyMatrixR1", Vector) = (0,1,0,0) // 追加...UV変換行列1行目
		[HideInInspector] _HomographyMatrixR2("HomographyMatrixR2", Vector) = (0,0,1,0) // 追加...UV変換行列2行目（実際にはこの行はいつも一定なので、削除してもいいかと思います）
		[HideInInspector] _HomographyMatrixR3("HomographyMatrixR3", Vector) = (0,0,0,1) // 追加...UV変換行列3行目
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

			Pass
			{
			CGPROGRAM
				#pragma vertex SpriteVert
				#pragma fragment SpriteFrag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile _ PIXELSNAP_ON
				#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			// 以下、#include "UnitySprites.cginc"の代わりにUnitySprites.cgincの内容をペーストし、一部修正を加える
			#include "UnityCG.cginc"

			#ifdef UNITY_INSTANCING_ENABLED

				UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
			// SpriteRenderer.Color while Non-Batched/Instanced.
			fixed4 unity_SpriteRendererColorArray[UNITY_INSTANCED_ARRAY_SIZE];
		// this could be smaller but that's how bit each entry is regardless of type
		float4 unity_SpriteFlipArray[UNITY_INSTANCED_ARRAY_SIZE];
	UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

	#define _RendererColor unity_SpriteRendererColorArray[unity_InstanceID]
	#define _Flip unity_SpriteFlipArray[unity_InstanceID]

#endif // instancing

CBUFFER_START(UnityPerDrawSprite)
#ifndef UNITY_INSTANCING_ENABLED
	fixed4 _RendererColor;
	float4 _Flip;
#endif
	float _EnableExternalAlpha;
CBUFFER_END

// Material Color.
fixed4 _Color;

float4 _HomographyMatrixR0; // 追加...UV座標変換用の行列0行目
float4 _HomographyMatrixR1; // 追加...UV座標変換用の行列1行目
float4 _HomographyMatrixR2; // 追加...UV座標変換用の行列2行目（実際にはこの行はいつも一定なので、削除してもいいかと思います）
float4 _HomographyMatrixR3; // 追加...UV座標変換用の行列3行目

struct appdata_t
{
	float4 vertex   : POSITION;
	float4 color    : COLOR;
	float2 texcoord : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	float4 vertex   : SV_POSITION;
	fixed4 color : COLOR;
	float3 texcoord : TEXCOORD0; // 変更...float2をfloat3に
	UNITY_VERTEX_OUTPUT_STEREO
};

v2f SpriteVert(appdata_t IN)
{
	v2f OUT;

	UNITY_SETUP_INSTANCE_ID(IN);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

#ifdef UNITY_INSTANCING_ENABLED
	IN.vertex.xy *= _Flip.xy;
#endif

	OUT.vertex = UnityObjectToClipPos(IN.vertex);
	OUT.texcoord = mul(float4x4(_HomographyMatrixR0, _HomographyMatrixR1, _HomographyMatrixR2, _HomographyMatrixR3), float4(IN.texcoord, 0.0, 1.0)).xyw; // 変更...IN.texcoordを変換してからx、y、wを渡す
	OUT.color = IN.color * _Color * _RendererColor;

	#ifdef PIXELSNAP_ON
	OUT.vertex = UnityPixelSnap(OUT.vertex);
	#endif

	return OUT;
}

sampler2D _MainTex;
sampler2D _AlphaTex;

fixed4 SampleSpriteTexture(float2 uv)
{
	fixed4 color = tex2D(_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
	fixed4 alpha = tex2D(_AlphaTex, uv);
	color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
#endif

	return color;
}

fixed4 SpriteFrag(v2f IN) : SV_Target
{
	fixed4 c = SampleSpriteTexture(IN.texcoord.xy / IN.texcoord.z) * IN.color; // 変更...受け取ったIN.texcoordのx、yをzで割ってサンプリング座標とする
	c.rgb *= c.a;
	return c;
}
ENDCG
}
		}
			CustomEditor "HomographyShaderGUI" // 追加...パラメータ設定用のカスタムUI
}