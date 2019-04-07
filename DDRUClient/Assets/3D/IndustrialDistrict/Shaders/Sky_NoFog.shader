Shader "IndustrialDistrict/Sky_NoFog" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
    Tags { "RenderType"="Background" "Queue" = "Background" "IgnoreProjector" = "True"}
    LOD 100
    Fog {Mode Off}
    Lighting Off
    
    Pass {
    	Tags { "LightMode" = "Vertex" }
    	Fog {Mode Off}      
        SetTexture [_MainTex] { combine texture } 
    }
}
}