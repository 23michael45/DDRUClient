Shader "IndustrialDistrict/LM_Master" {

    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}           
        _BumpMap ("Normalmap", 2D) = "bump" {}       
        _SpMask("Specular Mask", 2D) = "white"{}
        _SpColor("Specular Color", Color) = (1,1,1,1)
        _SpIntensity("Specular Intensity", float) = 1
        _RefCube("Cubemap", CUBE) = ""{}     
        _RimWidth("Fresnel", float) = 3

        _DirtColor("Dirt Color", Color) = (1,1,1,1)
        _DirtMask("Dirt Mask", 2D) = "white" {}
        _DirtTile("Dirt Tiling", float) = 4
        _DirtFade("Dirt Fade", Range(0,1)) = 0.5

        _LMNormal("Normal Highlights", float) = 1 
                 
    } 
     

    SubShader {      

        Tags { "RenderType"="Opaque" "Queue"="Geometry"}         
        LOD 200       

        CGPROGRAM       
        #pragma surface surf Lambert
        #pragma target 3.0


        sampler2D _MainTex;              
        sampler2D _BumpMap;      
        sampler2D _SpMask;

        sampler2D _DirtMask;
        float3 _DirtColor;
        float _DirtTile;        
        float _DirtFade;


        samplerCUBE _RefCube;       
        float3 _SpColor;
        float _SpIntensity;       
        float _RimWidth;

        float _LMNormal;      

        struct Input {
            float2 uv_MainTex;  
           // float2 uv2_DirtMask;          
            float3 viewDir;
            float3 worldNormal;  INTERNAL_DATA
            float3 worldRefl;       
        };


        void surf (Input IN, inout SurfaceOutput o) {                  
                       
            //Textures
            float3 tex = tex2D (_MainTex, IN.uv_MainTex).rgb;
            float3 texNormal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex)).rgb;          
            float specmask = tex2D(_SpMask, IN.uv_MainTex).r;
            float3 dirt = tex2D(_DirtMask, IN.uv_MainTex * _DirtTile).rgb;   

            float3 refl = texCUBE (_RefCube, WorldReflectionVector (IN, texNormal)).rgb;
      
         
            //Calculate Dot
            float3 dotCalc = saturate(dot(normalize(IN.viewDir), texNormal)); 

            //Fresnel from dot
            float3 rim = 1 - dotCalc;
            rim = saturate(pow(rim, _RimWidth) + specmask);
     
            //Calc top edge highlights based on dotCalc and green channel of normalmap
            float3 normalPop = saturate(pow(dotCalc, 5) * texNormal.g) * _SpColor * _LMNormal;
           
            
            //Spec Combined
            float3 spCombined = (refl * rim * _SpIntensity) * specmask * _SpColor;
        
            //Combine texture with spec and edge highlights    
            float3 combined = (tex  + spCombined + normalPop);

            //lerp dirt on top of final result
            float3 final = lerp(_DirtColor, combined, saturate(dirt.r + _DirtFade));        
              
            o.Albedo = final.rgb;
            o.Alpha = final.r;  
            o.Normal = texNormal.rgb;
          }

        ENDCG

    } 

    FallBack "Diffuse"

}