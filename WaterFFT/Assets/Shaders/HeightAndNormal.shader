// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/HeightAndNormal"
{
	Properties
	{
		_DisplacementMap("Displacement map", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 globalPos : GLOBAL_POSITION;
			};

			sampler2D _DisplacementMap;
			float4 _DisplacementMap_ST;

			v2f vert(appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal).xyz;
				o.normal = worldNormal;
				o.globalPos = worldPos;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				// sample the texture
				fixed4 waterDisplacement = tex2D(_DisplacementMap, (i.globalPos.xz % 100) * 0.01);

			//store depth in water in red channel and y component of (world) normal in green channel
			return float4(waterDisplacement.g - i.globalPos.y, i.normal.y * 0.5 + 0.5, 0, 0);
	}
	ENDCG
}
	}
}
