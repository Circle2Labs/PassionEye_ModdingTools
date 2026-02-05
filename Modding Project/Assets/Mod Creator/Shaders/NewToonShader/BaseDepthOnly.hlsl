#ifndef BASE_DEPTH_ONLY
#define BASE_DEPTH_ONLY

#include_with_pragmas "./CommonInclude.hlsl"

CBUFFER_START(UnityPerMaterial)
CBufTexture(_MainTex);
float _UseAlphaForTransparency;
float _AlphaClip;
float _Cutoff;
float _Surface;
CBUFFER_END
            
#pragma vertex vert
#pragma fragment frag

struct v {
    float3 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f {
    float4 position : SV_POSITION;
    float3 positionWS : TEXCOORD3;
    float2 uv : TEXCOORD0;
};

v2f vert(v IN) {
    v2f OUT;
    OUT.position   = TransformObjectToHClip(IN.vertex);
    OUT.positionWS = TransformObjectToWorld(IN.vertex);
    OUT.uv         = IN.uv;
    return OUT;
}

float frag(v2f IN) : SV_Depth
{
    //sample alphamap and exit early (for clipping fix)
    float4 alphaTexSample = RG_TEX_SAMPLE(_MainTex, IN.uv);
    return CalculateAlpha(alphaTexSample.a, _AlphaClip, _Cutoff, _Surface);
}
#endif
