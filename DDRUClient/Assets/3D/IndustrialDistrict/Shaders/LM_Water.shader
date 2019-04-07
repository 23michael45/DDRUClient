Shader "IndustrialDistrict/LM_Water" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SpTexture("Specular", 2D) = "black" {}	
		_Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
		_RefIntensity ("Reflection Intensity", Range(0,4)) = 2
		_Opacity("Opacity", Range(0,2)) = 1
		_ScrollSpeed("Scroll_Speed", Range(0,2)) = 0.5		
		
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True"}	
		Lighting Off
		ZTest Less
		LOD 200

		CGPROGRAM
		#pragma surface surf SimpleUnlit alpha		

		sampler2D _MainTex;
		sampler2D _SpTexture;
		samplerCUBE _Cube;
		float _RefIntensity;
		float _ScrollSpeed;
		float _Opacity;		

		struct Input {
			float2 uv_MainTex;			
			float3 worldRefl;
			float3 viewDir;	
		};

		inline float4 LightingSimpleUnlit (SurfaceOutput s, float3 lightDir, float atten)
			{				
				float4 c;
				c.rgb = s.Albedo;
				c.a = s.Alpha;
				return c;
			}

 
		void surf (Input IN, inout SurfaceOutput o) {


			//Scrolling uvs
			float xvalue = _ScrollSpeed * _Time;
			float xvalue2 = _ScrollSpeed * _Time * 0.8;	
			float2 scrollresult = IN.uv_MainTex + float2(1, xvalue);
			float2 scrollresult2 = IN.uv_MainTex + float2(0.4, xvalue2);


			//Textures
			float4 c = tex2D (_MainTex, scrollresult);
			float3 specinput = tex2D(_SpTexture, scrollresult2);
			float3 reflection = texCUBE (_Cube, IN.worldRefl + (specinput.r * 4));


			float3 rim = 1 - saturate(dot(normalize(IN.viewDir), o.Normal));
			float3 rimpow1 = saturate(pow(rim, 0.5));
			reflection = reflection * rimpow1; 
            

			o.Albedo = lerp(c.rgb, reflection.rgb, reflection.r);	

			o.Alpha = saturate(rim + c.r) * _Opacity;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}