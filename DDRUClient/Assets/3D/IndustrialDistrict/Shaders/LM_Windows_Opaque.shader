Shader "IndustrialDistrict/LM_Windows_Opaque" {

    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}           
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _SpecMask("Specular Mask", 2D) = "black"{} 
        _EmiMask("Emission Mask", 2D) = "black" {}         
        _RefCube("Cubemap", CUBE) = ""{}
        _RefIntensity("Reflection Intensity", Range(0,10)) = 1 
        _EmiTint("Emission Tint", Color) = (1,1,1,1)    
         
    }
     

    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}        
        LOD 200       

        CGPROGRAM
        #pragma target 3.0      
        #pragma surface surf Lambert              

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _SpecMask;
        sampler2D _EmiMask;  
        samplerCUBE _RefCube;
        float _RefIntensity;
        float _Opacity;
        float3 _EmiTint;
        
        struct Input {
            float2 uv_MainTex;    
            float3 viewDir;
            float3 worldNormal;  INTERNAL_DATA
            float3 worldRefl;
              
        };
                    
           
        void surf (Input IN, inout SurfaceOutput o) {                   
         
            //Textures
            float3 tex = tex2D (_MainTex, IN.uv_MainTex);
            float3 specMask = tex2D(_SpecMask, IN.uv_MainTex);
            float3 emiMask = tex2D(_EmiMask, IN.uv_MainTex);
            float3 texNormal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

            //Reflection
            float3 refl = texCUBE (_RefCube, WorldReflectionVector (IN, texNormal)).rgb * _RefIntensity * specMask.r;;
           
           //Fresnel
            float3 rim = 1 - saturate(dot(normalize(IN.viewDir), texNormal)); 
            rim = saturate(pow(rim, 2) + 0.1); 
            float3 combined = refl * rim;    
                                  
            //Assemble final Output
            float3 final = tex + combined;     
              
            o.Albedo = final.rgb;
            o.Alpha = 1;
            o.Normal = texNormal;
            o.Emission = (_EmiTint + combined * 0.4) * emiMask.r;            
        }

        ENDCG
    } 
    FallBack "Diffuse"
}