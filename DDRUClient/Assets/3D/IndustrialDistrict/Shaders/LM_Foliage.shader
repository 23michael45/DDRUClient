Shader "IndustrialDistrict/LM_Foliage" {

    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}        
        _BumpMap ("Normalmap", 2D) = "bump" {}       
        _SelfIllum("Self Illumination", float) = .05
                
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5       
        
    }    

    SubShader {

        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest"  }        
        LOD 200 
        CGPROGRAM
        #pragma target 3.0

        #pragma surface surf Lambert alphatest:_Cutoff        

        sampler2D _MainTex;              
        sampler2D _BumpMap;
        half _SelfIllum;
        half3 unity_FogColor;
         
        struct Input {
            half2 uv_MainTex;
            half3 viewDir;       
                    
        };

        void surf (Input IN, inout SurfaceOutput o) {                   
           
           //Textures                     
            half4 tex = tex2D (_MainTex, IN.uv_MainTex);            
            half3 texNormal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
                  
            o.Albedo = tex.rgb;           
            o.Normal = texNormal;
            o.Alpha = tex.a;
            o.Emission = ( _SelfIllum * tex.rgb * 0.2);            
        }
        ENDCG
    } 

    FallBack "VertexLit"

}