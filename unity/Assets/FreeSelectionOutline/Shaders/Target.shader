Shader "Outline/Target"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
	}
	SubShader
	{
		Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" }
		LOD 100
		
		Pass
		{
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ _OCCLUDED
			#pragma multi_compile __ _COLORIZE

			#include "UnityCG.cginc"
			
			struct appdata
			{
				float4 vertex: POSITION;
				float2 uv: TEXCOORD0;
			};
			
			struct v2f
			{
				float4 vertex: SV_POSITION;
				float2 uv: TEXCOORD0;
				#ifdef _COLORIZE
                float4 ObjPos:TEXCOORD1;
				#endif
			};
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert(appdata v)
			{
				v2f o;
				#ifdef _COLORIZE
				o.ObjPos=v.vertex;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			float4 frag(v2f i): SV_Target
			{
				float4 final=float4(1, 1, 1, 1);
				#ifdef _COLORIZE
					final*=-UnityObjectToViewPos(i.ObjPos).z; //return eye depth of the mask
				#endif
				fixed4 col = tex2D(_MainTex, i.uv);
				clip(col.a - 0.3);
				return final;
			}
			ENDCG
			
		}
	}
}
