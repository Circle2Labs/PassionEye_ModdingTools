#ifndef RailShaderFunc
#define RailShaderFunc

#include "../NdotL.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../BlendModes.hlsl"
#include "../Utils.hlsl"

#ifdef RG_METALLIC
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

float3 CalculateMetalness(float3 baseColor, float metallicStrength, float gradientSample) {
    
    float3 baseColorHSL = RGBtoHSL(baseColor);
    if(baseColorHSL.y != 0) {
        baseColorHSL.z = 0.5;
        baseColorHSL.y = metallicStrength;
    } else {
        //should look like silver
        baseColorHSL.z = 0.5;
    }

    baseColor = HSLtoRGB(baseColorHSL);
    
    return baseColor.rgb * gradientSample;
}

float3 CalculateReflections(float3 viewDir, float3 normal, float roughness){
    float3 reflectVector = reflect(-viewDir, normal);
    //TODO: fresnel maybe? or do we want to keep this more simple?
    return GlossyEnvironmentReflection(reflectVector, roughness, 1 );

}

#endif

#if USE_FORWARD_PLUS
    #define RG_LIGHT_LOOP_BEGIN(lightCount) { \
    uint lightIndex; \
    ClusterIterator _urp_internal_clusterIterator = ClusterInit(normalizedScreenSpaceUV, input.positionWS, 0); \
    [loop] while (ClusterNext(_urp_internal_clusterIterator, lightIndex)) { \
    lightIndex += URP_FP_DIRECTIONAL_LIGHTS_COUNT; \
    FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
    #define RG_LIGHT_LOOP_END } }
#else
    #define RG_LIGHT_LOOP_BEGIN(lightCount) \
    for (uint lightIndex = 0u; lightIndex < lightCount; ++lightIndex) {
    #define RG_LIGHT_LOOP_END }
#endif

struct StylizationData {
    float lightMin;
    float midpoint;
    float lightSmooth;
    half3 lightTint;
    half3 shadowTint;
    float tintStrength;
};

float LuminanceFormula(float3 color) {
    return 0.21223*color.r + 0.7152*color.g + 0.0722*color.b;
}

float3 HandleAdditionalLight(float3 lightColor, float3 lightDir, float shadowAttenuation, float distanceAttenuation,
                             float3 normalWS, float lightMin, float lightSmooth=1) {
    float addShadow = shadowAttenuation * distanceAttenuation;
    float addLightLighting = min(addShadow, HalfLambert(normalWS, lightDir));
    addLightLighting = max(lightMin, smoothstep(0, lightSmooth, addLightLighting));
    
    return addLightLighting * lightColor;
}

float CalculateAlpha(float alpha, float alphaClip, float cutoff, float surface) {
    if (alphaClip) {
        clip(alpha - cutoff);
        alpha = SharpenAlpha(alpha, cutoff);
    }
    return OutputAlpha(alpha, surface);
}

float CharlieD(float roughness, float ndoth)
{
    float invR = 1. / roughness;
    float cos2h = ndoth * ndoth;
    float sin2h = 1. - cos2h;
    return (2. + invR) * pow(sin2h, invR * .5) / (2. * PI);
}
 
float AshikhminV(float ndotv, float ndotl)
{
    return 1. / (4. * (ndotl + ndotv - ndotl * ndotv));
}

float PhongAnisotropic(float3 normal, float3 lightDir, float3 viewDir, float3 tangent, float3 bitangent, float anisoU, float anisoV)
{
    float3 h = normalize(lightDir + viewDir);
    float hDotT = dot(h, tangent);
    float hDotB = dot(h, bitangent);
    float nDotH = max(dot(normal, h), 0.001);
    float expo = anisoU * pow(hDotT, 2.0) + anisoV * pow(hDotB, 2.0);
    return pow(nDotH, expo);
}

struct SkinData {
    float3 ChSmooth;
    half  SkinAmt;
    bool IsFace;
    float4 Center;
};

/*
 * Updated version of CalculateLighting that takes into account the surface data and stylization parameters.
 * This function is designed to be more flexible and adaptable to different shading models and stylization techniques.
 * Parameters:
 * - light: The light source for which to calculate the lighting contribution.
 * - inputData: A struct containing various input data such as normal, view direction, position, etc.
 * - surfaceData: A struct containing surface properties such as albedo, metallic, smoothness, etc.
 * - stylizationData: A struct containing stylization parameters such as light tint, shadow tint, midpoint, light smoothness, etc.
 * Returns: The calculated lighting contribution as a half3 color value.
 */
half3 CalculateLighting(Light light, InputData inputData, SurfaceData surfaceData, StylizationData stylizationData
    #ifdef RG_SKIN
    , SkinData skinData
    #endif
    #ifdef RG_ANISO
    , float highlightNoise = 0
    #endif
    #ifdef RG_HALF_LAMBERT
    , bool applyStylization = true
    #endif
    ) {
    
    float diffuseFactor = 1;
    half3 lightDiffuse = half3(0,0,0);
    float3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
    
    #ifdef RG_LAMBERT
    float ndotl = saturate(dot(inputData.normalWS, light.direction));
    lightDiffuse = ndotl * attenuatedLightColor;
    #endif
    
    #ifdef RG_STYLIZED_LAMBERT
    float ndotl = saturate(dot(inputData.normalWS, light.direction)); //normal NdotL
    lightDiffuse = smoothstep(0, stylizationData.lightSmooth, ndotl) * attenuatedLightColor;
    #endif
    
    #ifdef RG_HALF_LAMBERT
    diffuseFactor = saturate(HalfLambertCurve(dot(inputData.normalWS, light.direction)));
    diffuseFactor = Remap(0.25, 1, 0, 1, diffuseFactor); //remaps half lambert term to 0-1 range
    if (applyStylization){
        lightDiffuse = max(stylizationData.lightMin, attenuatedLightColor * diffuseFactor);
        lightDiffuse = smoothstep(saturate(stylizationData.midpoint - stylizationData.lightSmooth), saturate(stylizationData.midpoint + stylizationData.lightSmooth), lightDiffuse);
        lightDiffuse *= (dot(inputData.normalWS, light.direction) * attenuatedLightColor);
    } else {
        lightDiffuse = attenuatedLightColor * diffuseFactor;
    }
    #endif
    
    #ifdef RG_SKIN
    float ndotl = saturate(dot(inputData.normalWS, light.direction));
    if (skinData.IsFace) { //fake sphere normals
        float3 sphereNormal = SphereNrm(inputData.positionWS, skinData.Center.xyz);
        ndotl = saturate(dot(sphereNormal, light.direction));
        if (_FaceDbg > 0) {
            // Visualize the spherical normals for debugging
            return half4(sphereNormal, 1.0);
        }
        //return ndotl;
    }
    
    half3 lambert = ndotl * light.distanceAttenuation;
    
    float R = smoothstep(0, skinData.ChSmooth.r, ndotl);
    float G = smoothstep(0, skinData.ChSmooth.g, ndotl);
    float B = smoothstep(0, skinData.ChSmooth.b, ndotl);
    half3 skin = half3(R, G, B) * attenuatedLightColor;
    //return skin;
    lightDiffuse = lerp(skin, lambert * light.color, skinData.SkinAmt);
    #endif
    
    // Specular
    half3 lightSpecular = half3(0,0,0);
    
    #if defined(RG_SPECULAR) || defined(RG_SHEEN)
    float3 halfVec = normalize(float3(light.direction) + float3(inputData.viewDirectionWS));
    float NdotH = float(saturate(dot(normalize(inputData.normalWS), halfVec)));
    #endif
    
    #ifdef RG_SPECULAR
    // Half produces banding, need full precision
    //NOTE: we are on NPR, smoothness is used for spec power.
    float smoothness = exp2(10 * surfaceData.smoothness + 1);
    float modifier = pow(float(NdotH), float(smoothness)); 
    float3 specularReflection = surfaceData.specular * modifier * light.distanceAttenuation;
    lightSpecular = specularReflection * attenuatedLightColor;
    #endif
    
    #ifdef RG_SHEEN
    float f = Fresnel(inputData.normalWS, inputData.viewDirectionWS, _SheenPower, _Sheen);
    float d = CharlieD(1, NdotH);
    float v = AshikhminV(dot(inputData.normalWS, inputData.viewDirectionWS), dot(inputData.normalWS, light.direction));
    float3 sheen = light.color * f * (d * v * PI * NdotL(inputData.normalWS, light.direction)) * light.distanceAttenuation;
    lightSpecular += saturate(sheen) * _SheenColor;
    #endif
    
    half3 metallicFinal = half3(0,0,0);
    
    #ifdef RG_METALLIC    
    float metalUv = dot(inputData.normalWS, normalize(inputData.viewDirectionWS + normalize(light.direction)));
    
    float3 metalRampSample = UnitySampleGradient(GetMetallicGradient(), metalUv) * surfaceData.metallic;
    float3 metallicColor = CalculateMetalness(surfaceData.albedo, surfaceData.metallic, metalRampSample);

    float3 reflection = CalculateReflections(inputData.viewDirectionWS, inputData.normalWS, _MetallicRoughness);
    reflection = lerp(float3(1,1,1), reflection, _MetallicReflection);
    float3 reflectionColor = metallicColor * reflection * light.color * light.distanceAttenuation;
    
    //we also want to fade the metallic based on NdotL
    metallicFinal = reflectionColor * surfaceData.specular;
    #endif
    
    #ifdef RG_ANISO
    float3 tangent = cross(inputData.normalWS, float3(1, 0, 0));
    float3 bitangent = cross(inputData.normalWS, float3(0, 0, 1));
    float aniso = PhongAnisotropic(inputData.normalWS, light.direction, inputData.viewDirectionWS, tangent, bitangent, abs(_AnisoUV.x), abs(_AnisoUV.y));
    aniso = saturate(pow(aniso, _AnisoPower));
    half3 finalAniso = Contrast(highlightNoise.xxx * aniso, _HighlightContrast) * _HighlightTint;
    lightSpecular += finalAniso * attenuatedLightColor * light.distanceAttenuation;
    #endif
    
    half3 finalColor;
    #ifdef RG_SKIN
        finalColor = lerp(
        #if _ALPHAPREMULTIPLY_ON
            lightDiffuse * surfaceData.albedo * surfaceData.alpha,
        #else
            lightDiffuse * surfaceData.albedo,
        #endif
        #if _ALPHAPREMULTIPLY_ON
            (surfaceData.albedo * surfaceData.albedo) * surfaceData.alpha,
        #else
            surfaceData.albedo * surfaceData.albedo,
        #endif
            clamp(lambert, light.distanceAttenuation, 0.8)
        ) + lightSpecular;
    #else
        #if _ALPHAPREMULTIPLY_ON
        finalColor = surfaceData.albedo * surfaceData.alpha;
        #else
        finalColor = surfaceData.albedo;
        #endif
        finalColor = lightDiffuse * finalColor + lightSpecular + metallicFinal;
    #endif
    
    return finalColor;
}

half3 CalculateFinalLighting(LightingData lightingData, StylizationData stylizationData){
    if (IsOnlyAOLightingFeatureEnabled()) return lightingData.giColor; // Contains white + AO
    
    half3 finalColor = 0;
           
    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_GLOBAL_ILLUMINATION))
        finalColor += lightingData.giColor;
    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_EMISSION))
        finalColor += lightingData.emissionColor;
    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_VERTEX_LIGHTING))
        finalColor += lightingData.vertexLightingColor;
    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_MAIN_LIGHT))
        finalColor += lightingData.mainLightColor;
    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_ADDITIONAL_LIGHTS))
        finalColor += lightingData.additionalLightsColor;
    // We finished accumulating lighting
    
    // Tint color based on luma
    half3 dayNightTintColor = lerp(stylizationData.shadowTint, stylizationData.lightTint, LuminanceFormula(finalColor));
    finalColor = lerp(finalColor, finalColor * dayNightTintColor, stylizationData.tintStrength);
    return finalColor;
}

half4 UniversalRGLighting(InputData inputData, SurfaceData surfaceData, StylizationData stylizationData
#ifdef RG_SKIN
, SkinData skinData
#endif
#ifdef RG_ANISO
, float highlightNoise = 0
#endif
){
    #if defined(DEBUG_DISPLAY)
    half4 debugColor;
    if (CanDebugOverrideOutputColor(inputData, surfaceData, debugColor)) {
        return debugColor;
    }
    #endif

    uint meshRenderingLayers = GetMeshRenderingLayer();
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);
    
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, aoFactor);

    inputData.bakedGI *= surfaceData.albedo;

    LightingData lightingData = CreateLightingData(inputData, surfaceData);
    #ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
    #endif
    {
        lightingData.mainLightColor += CalculateLighting(mainLight, inputData, surfaceData, stylizationData
        #ifdef RG_SKIN
        , skinData
        #endif
        #ifdef RG_ANISO
        , highlightNoise
        #endif
        );
    }
    
    #ifdef _ADDITIONAL_LIGHTS
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_FORWARD_PLUS
    [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
        #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
        {
            #ifdef RG_ADDITIVE_MIX
            lightingData.additionalLightsColor += CalculateLighting(light, inputData, surfaceData, stylizationData
                #ifdef RG_SKIN
                , skinData
                #endif
                #ifdef RG_ANISO
                    , highlightNoise
                #endif
                #ifdef RG_HALF_LAMBERT    
                    , false
                #endif
            );
            
            #elif defined(RG_MAX_MIX)
            half3 thisLightColor = CalculateLighting(light, inputData, surfaceData, stylizationData
                #ifdef RG_SKIN
                , skinData
                #endif    
                #ifdef RG_ANISO
                    , highlightNoise
                #endif
                #ifdef RG_HALF_LAMBERT
                    , false
                #endif
            );
            
            if (LuminanceFormula(thisLightColor) > LuminanceFormula(lightingData.additionalLightsColor))
                lightingData.additionalLightsColor = max(thisLightColor, lightingData.additionalLightsColor);
            #endif
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
    #ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
    {
        #ifdef RG_ADDITIVE_MIX
        lightingData.additionalLightsColor += CalculateLighting(light, inputData, surfaceData, stylizationData
            #ifdef RG_SKIN
            , skinData
            #endif
            #ifdef RG_ANISO
                , highlightNoise
            #endif
            #ifdef RG_HALF_LAMBERT    
                , false
            #endif
        );
            
        #elif defined(RG_MAX_MIX)
        half3 thisLightColor = CalculateLighting(light, inputData, surfaceData, stylizationData
            #ifdef RG_SKIN
            , skinData
            #endif
            #ifdef RG_ANISO
                , highlightNoise
            #endif
            #ifdef RG_HALF_LAMBERT
                , false
            #endif
        );
            
        if (LuminanceFormula(thisLightColor) > LuminanceFormula(lightingData.additionalLightsColor))
            lightingData.additionalLightsColor = max(thisLightColor, lightingData.additionalLightsColor);
        #endif
    }
    LIGHT_LOOP_END
    #endif

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
    lightingData.vertexLightingColor += inputData.vertexLighting * surfaceData.albedo;
    #endif

    return half4(CalculateFinalLighting(lightingData, stylizationData), surfaceData.alpha);
}

#endif