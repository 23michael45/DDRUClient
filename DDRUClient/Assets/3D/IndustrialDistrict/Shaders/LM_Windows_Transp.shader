Shader "IndustrialDistrict/LM_Windows_Transp" {

    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}           
        _BumpMap ("Normalmap", 2D) = "bump" {}  
        _SpecMask("Specular Mask", 2D) = "black"{}    
        _RefCube("Cubemap", CUBE) = ""{}
        _RefIntensity("Reflection Intensity", Range(0,10)) = 1        

        _Opacity("Opacity", Range(0,1)) = 0.5  
         
    }
     

    SubShader {

        Tags { "RenderType"="Transparent" "Queue"="Transparent"}        
        LOD 200       

        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf Lambert alpha       

        sampler2D _MainTex;
        sampler2D _SpecMask;
        sampler2D _BumpMap;        
            
        sampler2D _MaskTex;
        samplerCUBE _RefCube;
        float _RefIntensity;

        float _Opacity;
      
        struct Input {
            float2 uv_MainTex;                 
            float3 viewDir;
            float3 worldNormal;  INTERNAL_DATA
            float3 worldRefl;          
        };

          

        void surf (Input IN, inout SurfaceOutput o) {                    

            //Textures
            float4 tex = tex2D (_MainTex, IN.uv_MainTex);
            float3 specMask = tex2D(_SpecMask, IN.uv_MainTex);
            float3 texNormal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

            //Reflection
            float3 refl = texCUBE (_RefCube, WorldReflectionVector (IN, texNormal)).rgb * _RefIntensity * specMask.r;
           
           //calculate fresnel
            float3 rim = 1 - saturate(dot(normalize(IN.viewDir), texNormal)); 
            rim = saturate(pow(rim, 2) + 0.1); 
            float3 combined = refl * rim;    
  
            //Assemble final Output
            float3 final = tex + combined;     
              
            o.Albedo = final.rgb;
            o.Alpha = saturate(tex.a + _Opacity + rim);  
            o.Normal = texNormal;             
        }

        ENDCG
    } 
    FallBack "Diffuse"
}