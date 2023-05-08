Shader "Outline/PostprocessOutline"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
	}
	SubShader
	{
		CGINCLUDE
		#pragma multi_compile __ _OCCLUDED
		#pragma multi_compile __ _COLORIZE
		#pragma multi_compile __ _WEBGL
		#include "UnityCG.cginc"
		struct v2f
		{
			float2 uv[9]: TEXCOORD0;
			float4 vertex: SV_POSITION;
		};
		
		sampler2D _MainTex;
		sampler2D _Outline;
		sampler2D _Mask;
		#ifdef _COLORIZE
			sampler2D _CameraDepthTexture;
			float4 _OccludedColor;
		#endif
		float4 _OutlineColor;
		float _OutlineWidth;
		float _OutlineHardness;
		float4 _MainTex_TexelSize;
		
		v2f vert(appdata_img v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			float2 uv = v.texcoord;
			float width = _OutlineWidth;
			o.uv[0] = float2(uv.x - _MainTex_TexelSize.x * width, uv.y + _MainTex_TexelSize.y * width);
			o.uv[1] = float2(uv.x, uv.y + _MainTex_TexelSize.y * width);
			o.uv[2] = float2(uv.x + _MainTex_TexelSize.x * width, uv.y + _MainTex_TexelSize.y * width);
			o.uv[3] = float2(uv.x - _MainTex_TexelSize.x * width, uv.y);
			o.uv[4] = uv;
			o.uv[5] = float2(uv.x + _MainTex_TexelSize.x * width, uv.y);
			o.uv[6] = float2(uv.x - _MainTex_TexelSize.x * width, uv.y - _MainTex_TexelSize.y * width);
			o.uv[7] = float2(uv.x, uv.y - _MainTex_TexelSize.y * width);
			o.uv[8] = float2(uv.x + _MainTex_TexelSize.x * width, uv.y - _MainTex_TexelSize.y * width);
			
			return o;
		}
		
		fixed4 frag1(v2f i): SV_Target //Frag 1, process the original mask, expanding the egdes, get a second mask.
		{
			float4 finalRender;
			float sum = 0;
#ifdef _WEBGL
			float4 mask = tex2D(_Mask, i.uv[0]);
			sum += step(0.001, mask.r);
			mask = tex2D(_Mask, i.uv[1]);
			sum += step(0.001, mask.r);
			mask = tex2D(_Mask, i.uv[2]);
			sum += step(0.001, mask.r);
			mask = tex2D(_Mask, i.uv[3]);
			sum += step(0.001, mask.r);
			mask = tex2D(_Mask, i.uv[4]);
			sum += step(0.001, mask.r);
			mask = tex2D(_Mask, i.uv[5]);
			sum += step(0.001, mask.r);
			mask = tex2D(_Mask, i.uv[6]);
			sum += step(0.001, mask.r);
			mask = tex2D(_Mask, i.uv[7]);
			sum += step(0.001, mask.r);
			mask = tex2D(_Mask, i.uv[8]);
			sum += step(0.001, mask.r);
#else
			for (int it = 0; it < 9; it ++)
			{
				float4 mask = tex2D(_Mask, i.uv[it]);
				sum += step(0.001, mask.r);
			}
#endif
			finalRender = saturate(sum / 9);
			finalRender.a = 1;
			#ifdef _COLORIZE
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv[4]).r;
				depth = LinearEyeDepth(depth) ;
				float maskdepth = SAMPLE_DEPTH_TEXTURE(_Mask, i.uv[4]).r;
				#ifdef _OCCLUDED
					maskdepth = lerp(depth + 0.02, maskdepth, step(0.01, maskdepth));//copy from the scene depth with minor offset to the empty area of the mask, to clear the edge.	
					finalRender *= step(maskdepth, depth + 0.01);  //compare the depth of the mask  with the depth of the scene
				#endif
				#ifndef _OCCLUDED
					finalRender.g *= step(maskdepth, depth + 0.01);  //compare the depth of the mask  with the depth of the scene
				#endif
			#endif
			
			return finalRender;
		}
		
		fixed4 frag2(v2f i): SV_Target //Frag 2, expanding edges again, get a third mask, and lerp with the screen color by the second mask.
		{
			float4 finalRender;
			float sum = 0;
			float sum2 = 0;
#ifdef _WEBGL
			float4 mask = tex2D(_Mask, i.uv[0]);
			sum += step(0.001, mask.r);
			float4 outl = tex2D(_Outline, i.uv[0]);
			sum2 += outl.r;
			mask = tex2D(_Mask, i.uv[1]);
			sum += step(0.001, mask.r);
			outl = tex2D(_Outline, i.uv[1]);
			sum2 += outl.r;
			mask = tex2D(_Mask, i.uv[2]);
			sum += step(0.001, mask.r);
			outl = tex2D(_Outline, i.uv[2]);
			sum2 += outl.r;
			mask = tex2D(_Mask, i.uv[3]);
			sum += step(0.001, mask.r);
			outl = tex2D(_Outline, i.uv[3]);
			sum2 += outl.r;
			mask = tex2D(_Mask, i.uv[4]);
			sum += step(0.001, mask.r);
			outl = tex2D(_Outline, i.uv[4]);
			sum2 += outl.r;
			mask = tex2D(_Mask, i.uv[5]);
			sum += step(0.001, mask.r);
			outl = tex2D(_Outline, i.uv[5]);
			sum2 += outl.r;
			mask = tex2D(_Mask, i.uv[6]);
			sum += step(0.001, mask.r);
			outl = tex2D(_Outline, i.uv[6]);
			sum2 += outl.r;
			mask = tex2D(_Mask, i.uv[7]);
			sum += step(0.001, mask.r);
			outl = tex2D(_Outline, i.uv[7]);
			sum2 += outl.r;
			mask = tex2D(_Mask, i.uv[8]);
			sum += step(0.001, mask.r);
			outl = tex2D(_Outline, i.uv[8]);
			sum2 += outl.r;
#else

			for (int it = 0; it < 9; it ++)
			{
				float4 mask = tex2D(_Mask, i.uv[it]);
				sum += step(0.001, mask.r);
				float4 outl = tex2D(_Outline, i.uv[it]);
				sum2 += outl.r;
			}
			
#endif
			float4 col = tex2D(_MainTex, i.uv[4]);
			#ifdef _COLORIZE
				#ifdef _OCCLUDED
					sum *= step(0.01, sum2);
				#endif
				#ifndef _OCCLUDED
					float outline_g = tex2D(_Outline, i.uv[4]).g;
					float m = tex2D(_Mask, i.uv[4]).r;
					col = lerp(col,_OccludedColor, (1-outline_g)*step(0.01,m));//colorize the occluded parts
				#endif
			#endif
			
			float value=saturate(sum / _OutlineHardness) * (1 - saturate(pow(abs(sum2) / 9, 10)));
			
			finalRender = lerp(col, _OutlineColor,value );
			finalRender.a = 1;
			return finalRender;
		}
		ENDCG
		
		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag1
			#include "UnityCG.cginc"
			ENDCG
			
		}
		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag2
			#include "UnityCG.cginc"
			
			
			ENDCG
			
		}
	}
}
