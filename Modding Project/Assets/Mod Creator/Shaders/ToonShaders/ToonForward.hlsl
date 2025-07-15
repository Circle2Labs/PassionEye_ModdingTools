#ifndef TOONFORWARD
#define TOONFORWARD

#include_with_pragmas "ToonVariants.hlsl"

#pragma vertex vert
#pragma fragment frag

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

#include "CustomLighting.hlsl"

struct v {
    RG_VertIn;
};

struct v2f {
    RG_FragIn;
};

#include "ToonShaderCBuffer.hlsl"

v2f vert(v IN) {
    v2f OUT;

    if(_ExpandVertices) INFLATE(_ExpandAmount, IN.vertex, IN.normal);
    
    OUT.position = TransformObjectToHClip(IN.vertex);
    OUT.positionWS = TransformObjectToWorld(IN.vertex);
    OUT.normal = TransformObjectToWorldNormal(IN.normal);
    OUT.tangent = float4(TransformObjectToWorldDir(IN.tangent.xyz), IN.tangent.w * GetOddNegativeScale());
    OUT.uv = IN.uv;
    OUT.lmuv = IN.lmuv;
    OUT.rtuv = IN.rtuv;

    //push position of vertex towards camera normal plane based on clothing layer
    float linearZ  = Linear01Depth(OUT.position.z, _ZBufferParams);
    OUT.position.z += _ClothingLayersSeparation * _ClothingLayer * linearZ;
    
    float3 viewDir = GetWorldSpaceViewDir(OUT.positionWS);
    OUT.positionWS += normalize(viewDir) * _ClothingLayer * _ClothingLayersSeparation;
        
    return OUT;
}

float4 frag(v2f IN) : SV_Target {
#ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(IN.position);
#endif

    float4 giuv = float4(IN.lmuv.xy, IN.rtuv.xy);
    return float4(PE_LIGHTING(IN.uv, IN.positionWS, IN.normal, IN.tangent, giuv), CalculateAlpha(IN.uv));
}
#endif