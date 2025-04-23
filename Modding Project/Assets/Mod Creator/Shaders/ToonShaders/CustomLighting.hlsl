#ifndef CUSTOM_LIGHTING_HLSL
#define CUSTOM_LIGHTING_HLSL
// Include unity stuff first os that we can overwrite our own functions
#ifndef SHADERGRAPH_PREVIEW
    //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    //#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
#endif

// Includes to make this a single file include, minus the ShaderGraphNodes, avoiding bloat
#include "NdotL.hlsl"
#include "../Utils.hlsl"
#include "Structures.hlsl"
#include "../ColorSpaces.hlsl"
#include "Hair.hlsl"

// Light struct definition for ShaderGraph preview to work.
#ifdef SHADERGRAPH_PREVIEW
struct Light {
    float3 direction;
    float3 color;
    float shadowAttenuation;
    float distanceAttenuation;
};
#endif

//Standard Vertex Input params macro
#define RG_VertIn          \
float3 vertex : POSITION;   \
float3 normal : NORMAL;     \
float4 tangent : TANGENT;   \
float2 uv : TEXCOORD0;      \
float4 lmuv : TEXCOORD1;    \
float4 rtuv : TEXCOORD2

//Standard Fragment Input params macro
#define RG_FragIn               \
float4 position : SV_POSITION;  \
float3 positionWS : TEXCOORD3;  \
float3 normal : NORMAL;         \
float4 tangent : TANGENT;       \
float2 uv : TEXCOORD0;          \
float4 lmuv : TEXCOORD1;        \
float4 rtuv : TEXCOORD2

//Setup all the data and return calculated lighting
#define PE_LIGHTING(uv, posWS, nrm, tng, lmuv) \
    PELighting(\
        SetupFaceData(uv),\
        SetupGeometryData(uv, posWS, nrm, tng, lmuv),\
        SetupDiffuseData(),\
        SetupSpecularData(),\
        SetupMetallicData(uv),\
        SetupRoughnessData(uv),\
        SetupHairData())

Gradient GetMetallicGradient() {
    Gradient o;

    o.type = 1;
                
    o.colorsLength = 6;
    o.colors[0] = float4(0.2075472, 0.2075472, 0.2075472, 0.156);
    o.colors[1] = float4(0.25, 0.25, 0.25, 0.194);
    o.colors[2] = float4(0.7, 0.7, 0.7, 0.506);
    o.colors[3] = float4(1, 1, 1, 0.535);
    o.colors[4] = float4(2.125, 2.125, 2.125, 0.924);
    o.colors[5] = float4(6.0, 6.0, 6.0, 0.941);
    o.colors[6] = float4(6.0, 6.0, 6.0, 0.941);
    o.colors[7] = float4(6.0, 6.0, 6.0, 0.941);
                
    o.alphasLength = 0;
    o.alphas[0] = 0;
    o.alphas[1] = 0;
    o.alphas[2] = 0;
    o.alphas[3] = 0;
    o.alphas[4] = 0;
    o.alphas[5] = 0;
    o.alphas[6] = 0;
    o.alphas[7] = 0;

    return o;
}

float4 GetShadowPositionHClip(float3 positionOS, float3 normalOS, float3 LightDirWS, float3 LightPosWS) {
    float3 positionWS = TransformObjectToWorld(positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(normalOS);
                
    #if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(LightDirWS);
    #else
    float3 lightDirectionWS = LightDirWS;
    #endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

    #if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #else
    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #endif

    return positionCS;
}

float3 TwoBandStylize(float smooth, float secondBandOffset, float NdotL, float3 lightColor, float3 firstBandColor,
                      float3 secondBandColor, float3 SSSPower, float3 SSSOffset) {

    float3 NdotLVec = clamp(pow(NdotL, SSSPower) - SSSOffset, 0.0f, 1.0f);
    
    float3 main = smoothstep(0, smooth, NdotLVec);
    float3 secondary = smoothstep(0, smooth + secondBandOffset, NdotLVec);

    //limit bands to light color
    float mainLightPercLightness = LinearToPerceivedLightness(lightColor);

    firstBandColor = min(firstBandColor*mainLightPercLightness, firstBandColor);
    secondBandColor = min(secondBandColor*mainLightPercLightness, secondBandColor);

    float3 mainCol = lerp(firstBandColor, lightColor, main);
    float3 secondaryCol = lerp(secondBandColor, lightColor, secondary);
    
    return lerp(mainCol, secondaryCol, main);
}

float3 TwoBandStylize(float smooth, float secondBandOffset, float3 value) {
    float3 valueHSL = RGBtoHSL(value);
    
    float main = smoothstep(0, smooth, value.z);
    float secondary = smoothstep(0, smooth + secondBandOffset, value.z);

    //float3 mainCol = lerp(firstBandColor, value.gba, main);
    //float3 secondaryCol = lerp(secondBandColor, value.gba, secondary);

    return HSLtoRGB(float3(valueHSL.xy, lerp(main, secondary, main)));
} 

float3 DirectDiffuse(DiffuseData diffData) {
    return TwoBandStylize(
        diffData.smooth,
        diffData.secBndOffset,
        diffData.NdotL,
        diffData.lightTint,
        diffData.firstBndCol,
        diffData.secBndCol,
        diffData.SSSPower,
        diffData.SSSOffset
    );
}

float3 DirectDiffuse(DiffuseData diffData, float3 mainLightColor) {
    return TwoBandStylize(
        diffData.smooth,
        diffData.secBndOffset,
        diffData.NdotL,
        diffData.lightTint,
        diffData.firstBndCol,
        diffData.secBndCol,
        diffData.SSSPower,
        diffData.SSSOffset
    ) * mainLightColor;
}

float3 Specularity(float3 lightDir, float3 viewDir, float3 normal, float power, float amount, float3 color){
    float3 halfVec = normalize(lightDir + viewDir);
    return pow(max(saturate(dot(normal, halfVec)), 0),power) * amount * color;
}

float3 Specularity(SpecularData specData, GeometryData geomData) {
    float3 halfVec = normalize(geomData.viewDir + geomData.lgtDir);

    float spec = pow(max(dot(geomData.nrmWs, halfVec), 0.0), specData.specPow);

    spec = spec * specData.specAmt * specData.specCol * NdotL(geomData.nrmWs, geomData.lgtDir);
    //TODO: check why specularity skyrockets sometimes
    return clamp(spec, 0, 2);
}

float3 Metalness(GeometryData geomData, MetallicData metalData) {
    float uv = dot(geomData.nrmWs, normalize(geomData.viewDir + geomData.lgtDir));

    float4 gradientSample = UnitySampleGradient(metalData.gradient, uv);

    gradientSample = LinearToSRGB(gradientSample);
    
    float3 baseColorHSL = RGBtoHSL(metalData.baseCol);
    if(baseColorHSL.y != 0) {
        baseColorHSL.z = 0.5;
        baseColorHSL.y = metalData.metalness;
    } else {
        //should look like silver
        baseColorHSL.z = 0.5;
    }
    float3 baseColor = HSLtoRGB(baseColorHSL);
    
    return baseColor.rgb * gradientSample.rgb;
}

void ShadeMainLight(FaceData faceData, GeometryData geomData, DiffuseData diffData, SpecularData specData, MetallicData metalData,
    out float3 diffuse, out float3 specular, out float3 metallic) {
    diffData.NdotL = NdotL(faceData, geomData, diffData) * diffData.shadowAttn;
    #ifndef SHADERGRAPH_PREVIEW
        Light mainLight = GetMainLight(geomData.shadowCoord, geomData.posWs, geomData.shadowMask);
    #else
        Light mainLight;
        mainLight.color = float3(1,1,1);
        mainLight.direction = float3(0.5,0.5,0);
        mainLight.shadowAttenuation = 0;
        mainLight.distanceAttenuation = 0;
    #endif
    diffuse = DirectDiffuse(diffData, mainLight.color);
    specular = Specularity(specData, geomData) * diffData.NdotL;
    metallic = Metalness(geomData, metalData);
}

void ShadeAdditionalLights(FaceData faceData, GeometryData geomData, DiffuseData diffData, SpecularData specData, MetallicData metalData,
    out float3 diffuse, out float3 specular, out float3 metallic) {

    diffuse = 0;
    specular = 0;
    metallic = 0;
    
    #ifndef SHADERGRAPH_PREVIEW
        //#define _ADDITIONAL_LIGHTS 1
        #if _ADDITIONAL_LIGHTS
            uint numLights = GetAdditionalLightsCount();

            diffData.firstBndCol = 0;
            diffData.secBndCol = 0;
            
            faceData.isSdf = false;
            [unroll]
            for(uint i = 0; i < numLights; ++i) {
                half4 shadowMask = SAMPLE_SHADOWMASK(geomData.lightmapUV);
                Light light = GetAdditionalLight(i, geomData.posWs, shadowMask);
                geomData.lgtDir = light.direction;

                float _NdotL = NdotL(faceData, geomData, diffData) * light.shadowAttenuation * light.distanceAttenuation;
                diffData.NdotL = _NdotL;
                diffData.lightTint = light.color;

                diffuse += DirectDiffuse(diffData).rgb * light.color;
                //diffuse += light.distanceAttenuation * light.color;
                specular += Specularity(specData, geomData) * _NdotL * light.color;

                //calculate light direction based on position for metalness
                float3 lightDir = normalize(light.direction - geomData.posWs);
                geomData.lgtDir = lightDir;
                metallic += Metalness(geomData, metalData);
            }
        #endif
    #endif
}

half3 IndirectLighting(float3 normal, float4 lightmapUV) {
    float3 sh;
    OUTPUT_SH(normal, sh);
    #if defined(LIGHTMAP_ON) && defined(DYNAMICLIGHTMAP_ON)
    return SAMPLE_GI(lightmapUV.xy, lightmapUV.zw, sh, normal);
    #elif defined(DYNAMICLIGHTMAP_ON)
    return SAMPLE_GI(lightmapUV.xy, lightmapUV.zw, sh, normal);
    #elif defined(LIGHTMAP_ON)
    return SAMPLE_GI(lightmapUV.xy, sh, normal);
    #else //probes
    return SAMPLE_GI(lightmapUV.xyz, sh, normal);
    #endif
}

half3 IndirectLighting(GeometryData geomData) {
    #ifdef SHADERGRAPH_PREVIEW
        return 0;
    #else
        float3 sh;
        OUTPUT_SH(geomData.nrmWs, sh);
        return sh;//SAMPLE_GI(geomData.lightmapUV, sh, geomData.nrmWs);
    #endif
}

half3 ProbeReflection(GeometryData geomData, RoughnessData roughData){
    #ifndef SHADERGRAPH_PREVIEW
        UNITY_BRANCH if(roughData.fresnelAmt == 0) return 0;
        float3 reflectVector = reflect(-geomData.viewDir, geomData.nrmWs);
        //TODO: add fresnel proper.
        float fresnel = Fresnel(geomData.nrmWs, geomData.viewDir, roughData.fresnelAmt, 1);
        return GlossyEnvironmentReflection(reflectVector, RoughnessToPerceptualRoughness(roughData.roughness), 1) * fresnel;
    #else
        return 0;
    #endif
}

float3 PELighting(FaceData faceData, GeometryData geomData, DiffuseData diffData, SpecularData specData, MetallicData metalData, RoughnessData roughData, HairData hairData) {
    UNITY_BRANCH if(diffData.auto2ndBndCol) {
        diffData.secBndCol = AutoShadowColor(diffData.firstBndCol, 0.8);
    }

    UNITY_BRANCH if(specData.autoCol) {
        specData.specCol = AutoSpecularColor(metalData.baseCol);
    }
    
    //get main light
    #ifndef SHADERGRAPH_PREVIEW
        half4 shadowMask = SAMPLE_SHADOWMASK(geomData.shadowCoord);
        Light mainLight = GetMainLight(geomData.shadowCoord, geomData.posWs, shadowMask);
        
        geomData.lgtDir = mainLight.direction;
        geomData.mainLgtDir = mainLight.direction;
        diffData.shadowAttn = mainLight.shadowAttenuation * mainLight.distanceAttenuation;
    #else
        geomData.lgtDir = float3(0.0,0.5,0);
        geomData.mainLgtDir = float3(0.5,0.5,0);
        diffData.shadowAttn = 1;
    #endif

    diffData.NdotL = saturate(NdotL(faceData, geomData, diffData) * diffData.shadowAttn);

    //main light
    float3 mainLightDiffuse;
    float3 mainLightSpecular;
    float3 mainLightMetallic;
    ShadeMainLight(faceData, geomData, diffData, specData, metalData, mainLightDiffuse, mainLightSpecular, mainLightMetallic);

    //additional lights
    float3 addLightsDiffuse;
    float3 addLightsSpecular;
    float3 addLightsMetallic;
    ShadeAdditionalLights(faceData, geomData, diffData, specData, metalData, addLightsDiffuse, addLightsSpecular, addLightsMetallic);

    float3 color = 0;
    float3 addColor = 0;
    
    UNITY_BRANCH if(metalData.useMetallic) {
        color = lerp(mainLightDiffuse, mainLightMetallic, lerp(min(metalData.metalness, 0.075), metalData.metalness, diffData.NdotL));
        addColor = lerp(addLightsDiffuse, addLightsMetallic, lerp(min(metalData.metalness, 0.033), metalData.metalness, diffData.NdotL));
    } else {
        color = mainLightDiffuse;
        addColor = addLightsDiffuse;
    }
    
    float3 totalColor = (color + addColor) * metalData.baseCol;

    //Additive Specularity
    UNITY_BRANCH if (roughData.useRoughness) {
        totalColor += mainLightSpecular + addLightsSpecular + (ProbeReflection(geomData, roughData) * (1 - roughData.roughness));
    } else {
        totalColor += mainLightSpecular + addLightsSpecular;
    }
    
    //Indirect lighting
    float3 indirectColor = IndirectLighting(geomData) * metalData.baseCol;
    /*
    UNITY_BRANCH if(!faceData.isFace) {
        indirectColor = TwoBandStylize(diffData.smooth, diffData.secBndOffset, IndirectLighting(geomData)) * metalData.baseCol;
    } else {
        indirectColor = TwoBandStylize(diffData.smooth, diffData.secBndOffset, IndirectLighting(geomData)) * metalData.baseCol;
    }*/

    if (hairData.enableHighlight)
    {
        float4 hairHighlight = HairHighlight(geomData.posWs, geomData.nrmWs, geomData.uv, hairData);

        //apply ndotl to hair highlight
        hairHighlight.a *= diffData.NdotL * diffData.shadowAttn;
        return lerp(totalColor + indirectColor, hairHighlight.rgb, hairHighlight.a);
    } else {
        return totalColor + indirectColor;
    }
}

#endif
