    Shader "IndustrialDistrict/VolumeLight" {
       
        Properties {          
            _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
            _RimPower("Rim Power", float) = 4
            _MainTex ("Base (RGB)", 2D) = "white" {}
        }
       
          SubShader {
            Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }    
            Blend One One Cull Back Lighting Off ZWrite Off ZTest Less Fog { Mode Off }
            LOD 200

            CGPROGRAM
            #pragma target 3.0      
            #pragma surface surf Lambert              

            sampler2D _MainTex;         
          
            float3 _RimColor;
            float _RimPower;
            
            struct Input {
                float2 uv_MainTex;    
                float3 viewDir;         
            };
                    
           
            void surf (Input IN, inout SurfaceOutput o) {                   
             
                //Textures
                float3 tex = tex2D (_MainTex, IN.uv_MainTex);

                tex = tex * _RimColor;

                float3 rim = saturate(dot(normalize(IN.viewDir), o.Normal)); 
                rim = saturate(pow(rim, _RimPower));              

                float3 final = tex * rim;
                  
                o.Albedo = final.rgb;
                o.Alpha = final.r;
                o.Emission = final.rgb;                     
        }

        ENDCG
    } 
    FallBack "VertexLit"
}