Shader "SpaceH/SpawnPoint" {
	Properties {
		[PerRendererData] _Color ("Color", Color) = (1,1,1,1)
		[PerRendererData] _Emmision ("Emmision", float) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
		};

		fixed4 _Color;
		half _Emmision;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = _Color;
			// Metallic and smoothness come from slider variables
			o.Metallic = 0.5;
			o.Smoothness = 0.5;
			o.Emission = _Emmision;
			o.Alpha = 1.0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
