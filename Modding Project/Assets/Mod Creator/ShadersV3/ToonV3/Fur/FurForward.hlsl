#ifndef FUR_FORWARD
#define FUR_FORWARD

#pragma vertex VertexPass
#pragma geometry GeometryPass
#pragma fragment FragmentPass

GeomData VertexPass(Attributes input) {
    GeomData output = (GeomData)0;
    // DOTS Instancing
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    
    output.vertexOS = input.positionOS;
    output.normalOS = input.normalOS;
    output.tangentOS = input.tangentOS;
    output.texCoords = input.texCoords;
    output.staticLightmapUV = input.staticLightmapUV;
    output.dynamicLightmapUV = input.dynamicLightmapUV;
    return output;
}


[maxvertexcount(3*16)]
void GeometryPass(triangle GeomData input[3], inout TriangleStream<Varyings> triStream) {
    float layerOffset = _FurLayerSpacing;
    for (int layer = 0; layer <= _LayerCount; layer++) {
        for (int x = 0; x < 3; x++) {
            Varyings output = (Varyings)0;
            VertexPositionInputs vertexInput = GetVertexPositionInputs(input[x].vertexOS.xyz);
            VertexNormalInputs normalInput = GetVertexNormalInputs(input[x].normalOS, input[x].tangentOS);
    
            #ifdef _FOG_FRAGMENT
            half fogFactor = 0;
            #else
            half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
            #endif
    
            output.uv = TRANSFORM_TEX(input[x].texCoords, _BaseMap);
            output.positionWS.xyz = vertexInput.positionWS + (normalInput.normalWS * layer * layerOffset);
            output.positionCS = TransformWorldToHClip(output.positionWS.xyz);
            output.layerDepth = saturate((float(layer) / _LayerCount)+0.1);
    
            #ifdef _NORMALMAP
            half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
            output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
            output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
            output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
            #else
            output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
            #endif
    
            //OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV); //todo: this errors out
            #ifdef DYNAMICLIGHTMAP_ON
            output.dynamicLightmapUV = input[x].dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
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
            
            triStream.Append(output);
        }
        triStream.RestartStrip();
    }
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
    
    color = UniversalRGLighting(inputData, surfaceData, stylizationData) * input.layerDepth;
    //color.rgb= input.layerDepth;
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(saturate(color.a+1-input.layerDepth), IsSurfaceTypeTransparent(_Surface));
    
    #ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
    #endif
}

#endif
