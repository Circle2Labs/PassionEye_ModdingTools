#ifndef FUR_DEPTHONLY
#define FUR_DEPTHONLY

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

struct Attributes
{
    float4 position     : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct GeomData
{
    float4 vertexOS     : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    #if defined(_ALPHATEST_ON)
    float2 uv       : TEXCOORD0;
    #endif
    float4 positionCS   : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    float layerDepth : ATTRIB0;
};

GeomData DepthOnlyVertex(Attributes input)
{
    GeomData output = (GeomData)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    
    output.vertexOS = input.position;
    output.normalOS = input.normalOS;
    output.tangentOS = input.tangentOS;
    
    return output;
}

[maxvertexcount(3*16)]
void DepthOnlyGeometry(triangle GeomData input[3], inout TriangleStream<Varyings> triStream)
{
    float layerOffset = _FurLayerSpacing;
    for (int layer = 0; layer <= _LayerCount; layer++) {
        for (int x = 0; x < 3; x++) {
            Varyings output = (Varyings)0;
            VertexPositionInputs vertexInput = GetVertexPositionInputs(input[x].vertexOS.xyz);
            VertexNormalInputs normalInput = GetVertexNormalInputs(input[x].normalOS, input[x].tangentOS);
    
            #if defined(_ALPHATEST_ON)
            output.uv = TRANSFORM_TEX(input[x].texcoord, _BaseMap);
            #endif
            float3 positionWS = vertexInput.positionWS + (normalInput.normalWS * layer * layerOffset);
            output.positionCS = TransformWorldToHClip(positionWS.xyz);
            triStream.Append(output);
        }
        triStream.RestartStrip();
    }
}

half DepthOnlyFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #if defined(_ALPHATEST_ON)
    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
    #endif

    #if defined(LOD_FADE_CROSSFADE)
    LODFadeCrossFade(input.positionCS);
    #endif

    return input.positionCS.z;
}
#endif
