Shader "Hidden/ToonOutlineBlurBlend"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            Name "Box Blur"
            ZTest Always ZWrite Off Cull Off
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #include "../PostProcessingHelpers.hlsl"
            
            #pragma vertex vert
            #pragma fragment frag
            
            TEXTURE2D(_BlitTexture);
            TEXTURE2D(_OutlineTexture);

			struct appdata
			{
			    uint vertexID : SV_VertexID;
			};

			struct v2f
			{
			    float4 positionCS : SV_POSITION;
			    float2 uv : TEXCOORD0;
			};

			v2f vert(appdata v){
			    v2f f;
			    
			    f.positionCS = GetFullScreenTriangleVertexPosition(v.vertexID);
			    f.uv = GetFullScreenTriangleTexCoord(v.vertexID);
			    
			    return f;
			}

            float4 _OutlineColor;
            float radius;
            float use5x5;
            
            float4 frag(v2f f) : SV_Target
			{
				float4 blitPixel = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, f.uv);
				float4 blurredPixel;
				
				if(use5x5 == 1.0f)
					blurredPixel = GaussianBlur5x5(_OutlineTexture, f.uv);
				else
					blurredPixel = GaussianBlur3x3(_OutlineTexture, f.uv);

				return float4(lerp(blitPixel, _OutlineColor, blurredPixel.r));
            }

            ENDHLSL
        }
    }
}
