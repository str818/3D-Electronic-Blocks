Shader "Custom/Rim Light" {  
       Properties {  
              _MainTex("Base (RGB)", 2D) = "white" {}  
              _RimColor("_RimColor", Color) =(0.17,0.36,0.81,0.0)  
              _RimWidth("_RimWidth", Range(0.6,9.0)) = 0.9  
       }  
   
       SubShader {  
              Tags{ "RenderType" = "Opaque"}  
              LOD 150  
  
              CGPROGRAM  
   
              #pragma surface surf Lambert  
               
              struct Input {  
                     float2 uv_MainTex;  
                     float3 viewDir;  
              };  
   
              sampler2D _MainTex;  
              fixed4 _RimColor;  
              fixed _RimWidth;  
   
              void surf (Input IN, inout SurfaceOutput o) {  
                     fixed4 t = tex2D (_MainTex, IN.uv_MainTex);  
                     o.Albedo = t.rgb;  
                     o.Alpha = t.a;  
                     half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));  
                     o.Emission= _RimColor.rgb * pow (rim, _RimWidth);  
              }  
  
              ENDCG  
       }  
   
       Fallback "Diffuse"  
}