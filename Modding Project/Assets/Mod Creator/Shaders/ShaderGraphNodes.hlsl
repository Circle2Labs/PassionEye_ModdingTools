#ifndef SHADERGRAPH_NODES_HLSL
#define SHADERGRAPH_NODES_HLSL

#ifndef SHADERGRAPH_PREVIEW
    //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
#endif

#include "ToonShaders/AnisotropicHighlights.hlsl"
#include "ToonShaders/NdotL.hlsl"
#include "ToonShaders/FaceSdfShadow.hlsl"
#include "ToonShaders/SphereNormals.hlsl"
#include "ToonShaders/Structures.hlsl"
#include "ToonShaders/CustomLighting.hlsl"
#include "Utils.hlsl"
#include "ColorSpaces.hlsl"

//====================================================================
//                  ShaderGraph Wrappers
//====================================================================

//--------------------------------------------------------------------
//                  Anisotropic Highlight
//--------------------------------------------------------------------

void AnisotropicHighlight_float(float3 mask, float lightDot, float3 normalWS, float3 ViewDir, float fresnelPow, float fresnelIntensity, out float3 anisotropicHighlight) {
    anisotropicHighlight = AnisotropicHighlight(mask, lightDot, normalWS, ViewDir, fresnelPow, fresnelIntensity);
}

void AnisotropicHighlight_half(half3 mask, half lightDot, half3 normalWS, half3 ViewDir, half fresnelPow, half fresnelIntensity, out half3 anisotropicHighlight) {
    anisotropicHighlight = AnisotropicHighlight(mask, lightDot, normalWS, ViewDir, fresnelPow, fresnelIntensity);
}

//--------------------------------------------------------------------
//                  NdotL
//--------------------------------------------------------------------

void NdotL_float(float3 normalWS, float3 lightDir, out float lambLight) {
    lambLight = NdotL(normalWS, lightDir);
}

void NdotL_half(half3 normalWS, half3 lightDir, out half lambLight) {
    lambLight = NdotL(normalWS, lightDir);
}

//--------------------------------------------------------------------
//                  Face SDF Shadow
//--------------------------------------------------------------------

void FaceSdfShadow_float(float smoothing, float3 FwdVec, float3 RgtVec, float3 LightDir, float2 shadowTex, out float shadowInt) {
    shadowInt = FaceSdfShadow(smoothing, FwdVec, RgtVec, LightDir, shadowTex);
}

void FaceSdfShadow_half(half smoothing, half3 FwdVec, half3 RgtVec, half3 LightDir, half2 shadowTex, out half shadowInt) {
    shadowInt = FaceSdfShadow(smoothing, FwdVec, RgtVec, LightDir, shadowTex);
}

//--------------------------------------------------------------------
//                  Sphere Normals
//--------------------------------------------------------------------

void SphereNrm_float(float3 positionWS, float3 center, out float3 normal) {
    normal = SphereNrm(positionWS, center);
}

void SphereNrm_half(half3 positionWS, half3 center, out half3 normal) {
    normal = SphereNrm(positionWS, center);
}

//--------------------------------------------------------------------
//                  Sample Shadow Coord
//--------------------------------------------------------------------

void SampleShadowCoord_float(float posWS, out float4 ShadowCoord) {
    ShadowCoord = SampleShadowCoord(posWS);
}

void SampleShadowCoord_half(half posWS, out half4 ShadowCoord) {
    ShadowCoord = SampleShadowCoord(posWS);
}

//--------------------------------------------------------------------
//                  HSV Conversions
//--------------------------------------------------------------------

void RGBAtoHSVA_float(float4 rgb, out float4 hsv, out float hue, out float saturation, out float value, out float alpha) {
    hsv = RGBAtoHSVA(rgb);
    hue = hsv.x;
    saturation = hsv.y;
    value = hsv.z;
    alpha = hsv.w;
}

void RGBAtoHSVA_half(half4 rgb, out half4 hsv, out half hue, out half saturation, out half value, out half alpha) {
    hsv = RGBAtoHSVA(rgb);
    hue = hsv.x;
    saturation = hsv.y;
    value = hsv.z;
    alpha = hsv.w;
}

void HSVAtoRGBA_float(float hue, float saturation, float value, float alpha, out float4 rgba) {
    rgba = HSVAtoRGBA(float4(hue, saturation, value, alpha));
}

void HSVAtoRGBA_half(half hue, half saturation, half value, half alpha, out half4 rgba) {
    rgba = HSVAtoRGBA(half4(hue, saturation, value, alpha));
}

//--------------------------------------------------------------------
//                  HSL Conversions
//--------------------------------------------------------------------

void RGBAtoHSLA_float(float4 rgb, out float4 hsl, out float hue, out float saturation, out float lightness, out float alpha) {
    hsl = RGBAtoHSLA(rgb);
    hue = hsl.x;
    saturation = hsl.y;
    lightness = hsl.z;
    alpha = hsl.w;
}

void RGBAtoHSLA_half(half4 rgb, out half4 hsl, out half hue, out half saturation, out half lightness, out half alpha) {
    hsl = RGBAtoHSLA(rgb);
    hue = hsl.x;
    saturation = hsl.y;
    lightness = hsl.z;
    alpha = hsl.w;
}

void HSLAtoRGBA_float(float hue, float saturation, float lightness, float alpha, out float4 rgba) {
    rgba = HSLAtoRGBA(float4(hue, saturation, lightness, alpha));
}

void HSLAtoRGBA_half(half hue, half saturation, half lightness, half alpha, out half4 rgba) {
    rgba = HSLAtoRGBA(half4(hue, saturation, lightness, alpha));
}

//--------------------------------------------------------------------
//                  OkLab Conversions
//--------------------------------------------------------------------

void RGBAtoOkLab_float(float4 rgb, out float4 oklab, out float l, out float a, out float b, out float alpha) {
    oklab = float4(LinearSRGBtoOkLAB(rgb), rgb.a);
    l = oklab.x;
    a = oklab.y;
    b = oklab.z;
    alpha = oklab.w;
}

void RGBAtoOkLab_half(half4 rgb, out half4 oklab, out half l, out half a, out half b, out half alpha) {
    oklab = half4(LinearSRGBtoOkLAB(rgb), rgb.a);
    l = oklab.x;
    a = oklab.y;
    b = oklab.z;
    alpha = oklab.a;
}

void OkLabtoRGBA_float(float4 labA, out float4 rgba) {
    rgba = float4(OkLABtoLinearSRGB(labA), labA.a);
}

void OkLabtoRGBA_half(half4 labA, out half4 rgba) {
    rgba = half4(OkLABtoLinearSRGB(labA), labA.a);
}

//====================================================================
//                  Custom Lighting Nodes
//====================================================================

//--------------------------------------------------------------------
//                  Main Light Direction
//--------------------------------------------------------------------

void MainLightDir_float(out float3 result) {
    #ifdef SHADERGRAPH_PREVIEW //bogus lighting for preview
        result = float3(0.5, 0.5, 0);
    #else
        Light mainLight = GetMainLight(0);
        // while this doesn't make sense at first... unity sends you a wrong direction if no directional light is active.
        // multiplying it by shadowAttenuation and distanceAttenuation fixes it, making it result 0. Which is not a direction,
        // but at least it will make sure whatever calculation is done with it will result 0.
        result = normalize(mainLight.direction) * mainLight.distanceAttenuation;
    #endif
}

void MainLightDir_half(out half3 result) {
    #ifdef SHADERGRAPH_PREVIEW //bogus lighting for preview
    result = half3(0.5, 0.5, 0);
    #else
    Light mainLight = GetMainLight(0);
    // while this doesn't make sense at first... unity sends you a wrong direction if no directional light is active.
    // multiplying it by shadowAttenuation and distanceAttenuation fixes it, making it result 0. Which is not a direction,
    // but at least it will make sure whatever calculation is done with it will result 0.
    result = normalize(mainLight.direction) * mainLight.distanceAttenuation;
    #endif
}

//--------------------------------------------------------------------
//                  Custom Lighting
//--------------------------------------------------------------------
void PELighting_float(float lightSmooth, bool isFace, bool isSdf, float2 sdfSample, float3 faceCenter, float3 faceFwdVec, float3 faceRightVec, float secondBandOffset, float NdotLBias, float4 SSSOffset, float4 SSSPower, float3 positionWS, float3 NormVecWS, float3 viewDir, float4 lightTint, float4 shadowColor,
    float4 shadow2Color, bool shadow2ColorAuto, float4 baseTex, bool useMetalness, float metalness, Gradient metallicGradient, bool useRoughness, float roughness, float fresnelAmount,
    float specularAmount, float specularPow, float3 specularColor, bool autoSpecularColor, half4 lightmapUv, out float3 color)
{
    FaceData faceData;
    faceData.isFace = isFace;
    faceData.isSdf = isSdf;
    faceData.sdfSample = sdfSample;
    faceData.faceCenter = faceCenter;
    faceData.faceFwdVec = faceFwdVec;
    faceData.faceRgtVec = faceRightVec;

    GeometryData geomData;
    geomData.lgtDir = 0;
    geomData.posWs = positionWS;
    geomData.nrmWs = NormVecWS;
    geomData.viewDir = viewDir;
    geomData.mainLgtDir = 0;
    geomData.shadowMask = 0;
    #ifndef SHADERGRAPH_PREVIEW
        geomData.lightmapUV = lightmapUv;
        OUTPUT_LIGHTMAP_UV(lightmapUv, unity_LightmapST, geomData.lightmapUV);
        #ifdef _MAIN_LIGHT_SHADOWS_SCREEN
            geomData.shadowCoord = ComputeScreenPos(TransformWorldToHClip(positionWS));
        #else
            geomData.shadowCoord = TransformWorldToShadowCoord(positionWS);
        #endif
    #else
        geomData.shadowCoord = 0;
        geomData.lightmapUV = 0;
    #endif
    
    
    DiffuseData diffData;
    diffData.smooth = lightSmooth;
    diffData.secBndOffset = secondBandOffset;
    diffData.NdotL = 0;
    diffData.NdotLBias = NdotLBias;
    diffData.SSSPower = SSSOffset.rgb; //TODO: should we allow doing this to transparency too?
    diffData.SSSPower = SSSPower.rgb;
    diffData.lightTint = lightTint.rgb;
    diffData.auto2ndBndCol = shadow2ColorAuto;
    diffData.firstBndCol = shadowColor.rgb;
    diffData.secBndCol = shadow2Color.rgb;
    #ifndef SHADERGRAPH_PREVIEW
        #ifdef SHADOWS_SCREEN
            diffData.shadowAttn = SampleScreenSpaceShadowmap(geomData.shadowCoord);
        #else
            ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
            half shadowStrength = GetMainLightShadowStrength();
            diffData.shadowAttn = SampleShadowmap(
                geomData.shadowCoord,
                TEXTURE2D_ARGS(_MainLightShadowmapTexture,sampler_MainLightShadowmapTexture),
                shadowSamplingData,
                shadowStrength,
                false
            );
        #endif
    #else
        diffData.shadowAttn = 1;
    #endif

    SpecularData specData;
    specData.specAmt = specularAmount;
    specData.specPow = specularPow;
    specData.autoCol = autoSpecularColor;
    specData.specCol = specularColor;

    MetallicData metalData;
    metalData.useMetallic = useMetalness;
    metalData.metalness = metalness;
    metalData.baseCol = baseTex.rgb;
    metalData.gradient = metallicGradient;

    RoughnessData roughData;
    roughData.useRoughness = useRoughness;
    roughData.roughness = roughness;
    roughData.fresnelAmt = fresnelAmount;

    color = PELighting(faceData, geomData, diffData, specData, metalData, roughData);
}
#endif