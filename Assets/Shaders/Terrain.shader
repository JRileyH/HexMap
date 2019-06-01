Shader "Custom/Terrain" {
	Properties {
		
		_MainTex ("Terrain", 2DArray) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _MinDepth ("MinDepth", int) = -100
        _MaxDepth ("MaxDepth", int) = 100
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.5

		UNITY_DECLARE_TEX2DARRAY(_MainTex);

		struct Input {
			float4 color : COLOR;
            float3 worldPos;
            float3 terrain;
		};

        void vert (inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);
			data.terrain = v.texcoord2.xyz;
		}

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
        int _MinDepth, _MaxDepth;

        float GetDepthValue (Input IN) {
            return ((IN.worldPos.y - _MinDepth) / (_MaxDepth - _MinDepth) + 0.1);
        }

        float4 GetTerrainColor (Input IN, int index) {
			float3 uvw = float3(IN.worldPos.xz * 0.02, IN.terrain[index]);
			float4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uvw);
			return c * IN.color[index];
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = GetTerrainColor(IN, 0) + GetTerrainColor(IN, 1) + GetTerrainColor(IN, 2);
			o.Albedo = c.rgb * _Color * GetDepthValue(IN);
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}