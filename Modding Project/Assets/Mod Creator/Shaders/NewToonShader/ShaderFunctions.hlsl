#ifndef RailShaderFunc
#define RailShaderFunc

#include "../../Shaders/ToonShaders/NdotL.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

float LuminanceFormula(float3 color)
{
    return 0.21223*color.r + 0.7152*color.g + 0.0722*color.b;
}

float3 HandleAdditionalLight(float3 lightColor, float3 lightDir, float shadowAttenuation, float distanceAttenuation,
                             float3 normalWS, float lightMin, float lightSmooth=1)
{
    float addShadow = LinearToGamma22(shadowAttenuation * distanceAttenuation);
    float addLightLighting = min(addShadow, HalfLambert(normalWS, lightDir));
    addLightLighting = max(lightMin, smoothstep(0, lightSmooth, addLightLighting));
    
    return addLightLighting * lightColor;
}

float CalculateAlpha(float alpha, float cutoff, float surface) {
    clip(alpha - cutoff);
    alpha = SharpenAlpha(alpha, cutoff);
    
    return OutputAlpha(alpha, surface);
}
#endif