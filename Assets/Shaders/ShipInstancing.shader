Shader "SpaceH/ShipInstancing" {
	Properties {
		_NonInstancedColor ("Non-Instanced Test Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows
		
		#pragma instancing_options assumeuniformscaling procedural:setup

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _NonInstancedColor;
		
		//StructuredBuffer<float4> _test;
		
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		struct InstanceData {
            float4x4 InstanceTransform;
            float4 Color;
        };
		
        StructuredBuffer<InstanceData> _InstanceData;
#endif

        void setup()
        {
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        unity_ObjectToWorld = _InstanceData[unity_InstanceID].InstanceTransform;
        
        float3x3 w2oRotation;
        w2oRotation[0] = unity_ObjectToWorld[1].yzx * unity_ObjectToWorld[2].zxy - unity_ObjectToWorld[1].zxy * unity_ObjectToWorld[2].yzx;
        w2oRotation[1] = unity_ObjectToWorld[0].zxy * unity_ObjectToWorld[2].yzx - unity_ObjectToWorld[0].yzx * unity_ObjectToWorld[2].zxy;
        w2oRotation[2] = unity_ObjectToWorld[0].yzx * unity_ObjectToWorld[1].zxy - unity_ObjectToWorld[0].zxy * unity_ObjectToWorld[1].yzx;

        float det = dot(unity_ObjectToWorld[0], w2oRotation[0]);

        w2oRotation = transpose(w2oRotation);

        w2oRotation *= rcp(det);

        float3 w2oPosition = mul(w2oRotation, -unity_ObjectToWorld._14_24_34);

        unity_WorldToObject._11_21_31_41 = float4(w2oRotation._11_21_31, 0.0f);
        unity_WorldToObject._12_22_32_42 = float4(w2oRotation._12_22_32, 0.0f);
        unity_WorldToObject._13_23_33_43 = float4(w2oRotation._13_23_33, 0.0f);
        unity_WorldToObject._14_24_34_44 = float4(w2oPosition, 1.0f);
#endif
        }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			c *=  _InstanceData[unity_InstanceID].Color;
			#else
			c *= _NonInstancedColor;
			#endif
			
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
