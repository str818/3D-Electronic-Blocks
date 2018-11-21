Shader "Custom/LogoFlash" 
{
Properties     
{            
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _FlashColor ("Flash Color", Color) = (1,1,1,1)
        _Angle ("Flash Angle", Range(0, 180)) = 45
        _Width ("Flash Width", Range(0, 1)) = 0.2
        _LoopTime ("Loop Time", Float) = 1
        _Interval ("Time Interval", Float) = 3
       // _BeginTime ("Begin Time", Float) = 2
}        
SubShader     
  {     
        Tags     
        {      
            "Queue"="Transparent"      
            "IgnoreProjector"="True"      
            "RenderType"="Transparent"      
            "PreviewType"="Plane"     
            "CanUseSpriteAtlas"="True"     
        }     
        // 源rgba*源a + 背景rgba*(1-源A值)   
        Blend SrcAlpha OneMinusSrcAlpha     
        Pass     
        {     
            CGPROGRAM     
            #pragma vertex vert     
            #pragma fragment frag     
            #include "UnityCG.cginc"     
                 
            struct appdata_t     
            {     
                float4 vertex   : POSITION;     
                float4 color    : COLOR;     
                float2 texcoord : TEXCOORD0;     
            };     
     
            struct v2f     
            {     
                float4 vertex   : SV_POSITION;     
                fixed4 color    : COLOR;     
                half2 texcoord  : TEXCOORD0;     
            };     
               
            sampler2D _MainTex;       
          
           
         float4 _FlashColor;  
         float _Angle;  
         
float _Width;  
         float _LoopTime;  
         float _Interval;  
//        float _BeginTime;    
      float inFlash(half2 uv)
         {
             float brightness = 0;
            
             float angleInRad = 0.0174444 * _Angle;
             float tanInverseInRad = 1.0 / tan(angleInRad);
            
//            float currentTime = _Time.y - _BeginTime;
      float currentTime = _Time.y;
      float totalTime = _Interval + _LoopTime;
             float currentTurnStartTime = (int)((currentTime / totalTime)) * totalTime;
             float currentTurnTimePassed = currentTime - currentTurnStartTime - _Interval;
            
             bool onLeft = (tanInverseInRad > 0);
      float xBottomFarLeft = onLeft? 0.0 : tanInverseInRad;
      float xBottomFarRight = onLeft? (1.0 + tanInverseInRad) : 1.0;
      float percent = currentTurnTimePassed / _LoopTime;
             float xBottomRightBound = xBottomFarLeft + percent * (xBottomFarRight - xBottomFarLeft);
             float xBottomLeftBound = xBottomRightBound - _Width;
            
             float xProj = uv.x + uv.y * tanInverseInRad;
            
             if(xProj > xBottomLeftBound && xProj < xBottomRightBound)
             {
               brightness = 1.0 - abs(2.0 * xProj - (xBottomLeftBound + xBottomRightBound)) / _Width;
             }

             return brightness;
         }
            v2f vert(appdata_t IN)     
            {     
                v2f OUT;     
                OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);     
                OUT.texcoord = IN.texcoord;     
#ifdef UNITY_HALF_TEXEL_OFFSET     
                OUT.vertex.xy -= (_ScreenParams.zw-1.0);     
#endif     
                OUT.color = IN.color * _FlashColor;     
                return OUT;  
            }  
     
            fixed4 frag(v2f IN) : SV_Target     
            {     
                half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;     
                //float grey = dot(color.rgb, fixed3(0.22, 0.707, 0.071));   
                //return half4(grey,grey,grey,color.a);    
                
                 ///////////////
                 half4 texCol = tex2D(_MainTex, IN.texcoord);
              float brightness = inFlash(IN.texcoord);
        
             color.rgb = texCol.rgb + _FlashColor.rgb * brightness;
              color.a = texCol.a; 
              return color;
            }                       
            ENDCG     
        }     
    }     
   FallBack "Unlit/Transparent"  
}