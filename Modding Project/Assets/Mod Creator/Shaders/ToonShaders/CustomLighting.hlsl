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

// Light struct definition for ShaderGraph preview to work.
#ifdef SHADERGRAPH_PREVIEW
struct Light {
    float3 direction;
    float3 color;
    float shadowAttenuation;
    float distanceAttenuation;
};
#endif


float3 TwoBandStylize(float smooth, float secondBandOffset, float NdotL, float3 lightColor, float3 firstBandColor, float3 secondBandColor) {
    float main = smoothstep(0, smooth, NdotL);
    float secondary = smoothstep(0, smooth + secondBandOffset, NdotL);

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
        diffData.secBndCol
    );
}

float3 DirectDiffuse(DiffuseData diffData, float3 mainLightColor) {
    return TwoBandStylize(
        diffData.smooth,
        diffData.secBndOffset,
        diffData.NdotL,
        diffData.lightTint,
        diffData.firstBndCol,
        diffData.secBndCol
    ) * mainLightColor;
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

half3 IndirectLighting(GeometryData geomData) {
    #ifdef SHADERGRAPH_PREVIEW
        return 0;
    #else
        float3 sh;
        OUTPUT_SH(geomData.nrmWs, sh);
        return SAMPLE_GI(geomData.lightmapUV, sh, geomData.nrmWs);
    #endif
}

half3 ProbeReflection(GeometryData geomData, RoughnessData roughData){
    #ifndef SHADERGRAPH_PREVIEW
        float3 reflectVector = reflect(-geomData.viewDir, geomData.nrmWs);
        //TODO: add fresnel proper.
        float fresnel = Fresnel(geomData.nrmWs, geomData.viewDir, roughData.fresnelAmt, 1);
        return GlossyEnvironmentReflection(reflectVector, RoughnessToPerceptualRoughness(roughData.roughness), 1) * fresnel;
    #else
        return 0;
    #endif
}

float3 PELighting(FaceData faceData, GeometryData geomData, DiffuseData diffData, SpecularData specData, MetallicData metalData, RoughnessData roughData) {
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
    float3 indirectColor;
    /*
    UNITY_BRANCH if(!faceData.isFace) {
        indirectColor = TwoBandStylize(diffData.smooth, diffData.secBndOffset, IndirectLighting(geomData)) * metalData.baseCol;
    } else {
        indirectColor = TwoBandStylize(diffData.smooth, diffData.secBndOffset, IndirectLighting(geomData)) * metalData.baseCol;
    }*/
    
    indirectColor = IndirectLighting(geomData) * metalData.baseCol;
    return (totalColor + indirectColor);
}

#endif
