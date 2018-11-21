Shader "Color Space/YCrCbtoRGB Split Alpha" 
{
    Properties 
    {
        _YTex ("Y (RGB)", 2D) = "black" {}
        _CrTex ("Cr (RGB)", 2D) = "gray" {}
        _CbTex ("Cb (RGB)", 2D) = "gray" {}

		[KeywordEnum(Vertical, Horizontal)] Mode ("Alpha Mode", Float) = 0
    }
    SubShader 
    {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Pass 
        {
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Lighting Off Fog { Color (0,0,0,0) }
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile MODE_VERTICAL MODE_HORIZONTAL

			#include "UnityCG.cginc"

			sampler2D _YTex;
			sampler2D _CbTex;
			sampler2D _CrTex;
			
			#include "YCbCrtoRGB.cginc"
			
			struct v2f 
			{
				float4 pos : SV_POSITION;
				half2 uvY : TEXCOORD0;
				half2 uvAlpha : TEXCOORD1;
				half2 uvCbCr : TEXCOORD2;
			};

			float4 _YTex_ST;
			float4 _CbTex_ST;

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				
				float4 texcoordBottom = v.texcoord;
				float4 texcoordTop = v.texcoord;
#if MODE_VERTICAL
				texcoordBottom.y = ( v.texcoord.y / 2.0f );
				texcoordTop.y = texcoordBottom.y + 0.5f;
#else
				texcoordBottom.x = ( v.texcoord.x / 2.0f );				
				texcoordTop.x = texcoordBottom.x + 0.5f;
#endif
				
				o.uvY = TRANSFORM_TEX (texcoordTop, _YTex);
				o.uvAlpha = TRANSFORM_TEX (texcoordBottom, _YTex);
				o.uvCbCr = TRANSFORM_TEX (texcoordTop, _CbTex);
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 rgbVec = YCbCrToRGB(SampleYCbCr( i.uvY, i.uvCbCr));
				
				rgbVec.w = ((tex2D(_YTex, i.uvAlpha).a - (16.0/255.0)) * (255.0/219.0));
			
				return rgbVec;
			}
			ENDCG
		}
	}
}
