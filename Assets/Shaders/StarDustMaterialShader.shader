Shader "Custom/StarDustMaterialShader"
{
	Properties
	{
		_SlowColor("Slow Color", Color) = (.92, .34, .85, 1)
		_FastColor("Fast Color", Color) = (.34, .85, .92, 1)
		_FastThreshold("Fast Threshold", Range(0.1, 100)) = 20
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct Particle {
				float3 position;
				float3 velocity;
			};

			StructuredBuffer<Particle> particleBuffer;

			struct v2f
			{
				float4 color : COLOR;
				float4 position : SV_POSITION;
			};

			float4 _SlowColor;
			float4 _FastColor;
			float _FastThreshold;
			
			v2f vert (uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
			{
				v2f o;
				float speed = length(particleBuffer[instance_id].velocity);
				float lerpVal = clamp((speed * speed) / _FastThreshold, 0.0f, 1.0f);
				o.color = lerp(_SlowColor, _FastColor, lerpVal);
				o.position = UnityObjectToClipPos(particleBuffer[instance_id].position);
				return o;
			}
			
			float4 frag (v2f i) : COLOR
			{
				return i.color;
			}
			ENDCG
		}
	}
}
