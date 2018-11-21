Shader "Color Space/YCbCrtoRGB Trans" 
{
    Properties 
    {
        _YTex ("Y (RGB)", 2D) = "black" {}
        _CrTex ("Cr (RGB)", 2D) = "gray" {}
        _CbTex ("Cb (RGB)", 2D) = "gray" {}
        _TintColor ("Color", COLOR) = (1,1,1,1)
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

			#include "UnityCG.cginc"

			sampler2D _YTex;
			sampler2D _CbTex;
			sampler2D _CrTex;
			
			#include "YCbCrtoRGB.cginc"
			
			fixed4 _TintColor;
			
			struct v2f 
			{
				float4  pos : SV_POSITION;
				half2  uvY : TEXCOORD0;
				half2  uvCbCr : TEXCOORD1;
			};

			float4 _YTex_ST;
			float4 _CbTex_ST;

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uvY = TRANSFORM_TEX (v.texcoord, _YTex);
				o.uvCbCr = TRANSFORM_TEX (v.texcoord, _CbTex);
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{			
				return YCbCrToRGB(SampleYCbCr( i.uvY, i.uvCbCr))*_TintColor;
			}
			ENDCG
		}
	}
}

