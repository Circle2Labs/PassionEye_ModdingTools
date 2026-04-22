#ifndef HAIR_FORWARD
#define HAIR_FORWARD

#pragma vertex VertexPass
#pragma fragment FragmentPass

Varyings VertexPass(Attributes input) {
    Varyings output = (Varyings)0;
    // DOTS Instancing
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    
    #ifdef _FOG_FRAGMENT
    half fogFactor = 0;
    #else
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    #endif
    
    output.uv = TRANSFORM_TEX(input.texCoords, _BaseMap);
    output.positionWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;
    
    #ifdef _NORMALMAP
    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
    output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
    #else
    output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
    #endif
    
    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
    #ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif
    OUTPUT_SH4(vertexInput.positionWS, output.normalWS.xyz, GetWorldSpaceNormalizeViewDir(vertexInput.positionWS), output.vertexSH, output.probeOcclusion);

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
    #else
    output.fogFactor = fogFactor;
    #endif
    
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
    #endif
    
    return output;
}

void FragmentPass(
    Varyings input,
    out half4 color : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out float4 outRenderingLayers : SV_Target1
#endif
)
{
    // DOTS Instancing
    UNITY_SETUP_INSTANCE_ID(input);

    SurfaceData surfaceData;
    InitializeBaseSurfaceData(input.uv, surfaceData);
    
    StylizationData stylizationData = InitializeBaseStylizationData();
    
    #ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
    #endif

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, UNDO_TRANSFORM_TEX(input.uv, _BaseMap));
    
    #if defined(_DBUFFER)
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
    #endif
    
    InitializeBakedGIData(input, inputData);
    
    float floatSeed1 = GenerateHashedRandomFloat(5000);
    float floatSeed2 = GenerateHashedRandomFloat(5000 + 1);
    float floatSeed3 = GenerateHashedRandomFloat(5000 + 2);
    float highFreqNoise = (RG_TEX_SAMPLE(_PerlinNoise, input.uv * float2(_HighFreqScale, 0.0) + float2(0, floatSeed1)).r ) * _HighFreqStrenght;
    float midFreqNoise = (RG_TEX_SAMPLE(_PerlinNoise, input.uv * float2(_MidFreqScale, 0.0) + float2(0, floatSeed2)).r ) * _MidFreqStrenght;
    float lowFreqNoise = (RG_TEX_SAMPLE(_PerlinNoise, input.uv * float2(_LowFreqScale, 0.0) + float2(0, floatSeed3)).r ) * _LowFreqStrenght;
    float highlightNoise = highFreqNoise + midFreqNoise + lowFreqNoise;

    color = UniversalRGLighting(inputData, surfaceData, stylizationData, highlightNoise);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));
    
    #ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
    #endif
}


#endif
