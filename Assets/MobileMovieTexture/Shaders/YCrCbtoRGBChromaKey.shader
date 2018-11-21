Shader "Color Space/YCbCrtoRGB Chroma Key" 
{
   Properties 
   {
        _YTex ("Y (RGB)", 2D) = "black" {}
        _CrTex ("Cr (RGB)", 2D) = "gray" {}
        _CbTex ("Cb (RGB)", 2D) = "gray" {}

       [YCbCr] _KeyYCbCr ("Chroma Key Color", Vector) = (0,0,0,-0.6) 
       [YCbCrPriority] _YCbCRDeltaScale ("YCbCr priority", Vector) = (0.1,1,1)  //Different CbCr means a more different color than a different Y
	   _LowThreshold ("Low threashold", Range(0.0, 1.0)) = 0.2
	   _HighThreshold ("High threashold", Range(0.0, 1.0)) = 0.25
   }
   SubShader 
   {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Pass 
		{
			Lighting Off Fog { Color (0,0,0,0) }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _YTex;
			sampler2D _CbTex;
			sampler2D _CrTex;

			#include "YCbCrtoRGB.cginc"

			half3 _KeyYCbCr;
			half4 _YCbCRDeltaScale;
			half _LowThreshold;
			half _HighThreshold;

			struct v2f 
			{
				float4  pos : SV_POSITION;
				half2  uvY : TEXCOORD0;
				half4  uvCbCr : TEXCOORD1; // u,v,offset,normalise
			};

			float4 _YTex_ST;
			float4 _CbTex_ST;
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uvY = TRANSFORM_TEX (v.texcoord, _YTex);
				o.uvCbCr.xy = TRANSFORM_TEX (v.texcoord, _CbTex);

				//Work out the threasholds in the vertex shader
				//float scaleLength = length(_YCbCRDeltaScale);
				float scaleLength = _YCbCRDeltaScale.w;

				float bottom = _LowThreshold * scaleLength;
				float top = _HighThreshold * scaleLength;

				float range = top - bottom;
				float offset = -bottom;

				float normalise = 1.0/range;

				o.uvCbCr.z = offset;
				o.uvCbCr.w = normalise;

				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 YCbCr = SampleYCbCr( i.uvY, i.uvCbCr);				
				fixed4 rgbVec = YCbCrToRGB(YCbCr);
								
				half3 deltaVec = (YCbCr.xyz - _KeyYCbCr.xyz) * _YCbCRDeltaScale.xyz;

				rgbVec.w = (length(deltaVec) +  i.uvCbCr.z)* i.uvCbCr.w;
			
				return rgbVec;
			}
			ENDCG
		}
	}
}

