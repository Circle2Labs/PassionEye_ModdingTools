#ifndef METAL_FORWARD
#define METAL_FORWARD

#include_with_pragmas "./CommonInclude.hlsl"
#include "./CommonInclude.hlsl"

GLOBAL_CBUFFER_START(ToonGlobalBuffer, b0)
// light smoothing
float _LightSmooth;
//Magic number for the minimum value of the light
float _LightMin;
float _MidPoint;

// hue shifting
float _ShiftAmount;
float _kelvinTemp; //0-20000
Texture2D _DayNightRamp;
SamplerState sampler_DayNightRamp;
float4 _DayNightRamp_ST;
float _DNTintStr;
CBUFFER_END

CBUFFER_START (UnityPerMaterial)
float4 _TintColor;
CBufTexture(_MainTex);
CBufTexture(_NormalMap);
float _NormalStrength;

float _MetallicStrength;
float3 _MetallicColor;
float _MetallicReflection;
float _MetallicRoughness;

// Clothing layer
float _ClothingLayer;
float _ClothingLayersSeparation;

// ALPHA Values
float _UseAlphaForTransparency;
float _AlphaClip;
float _Cutoff;
float _Surface;
CBUFFER_END

#pragma vertex vert
#pragma fragment frag

struct v {
    RG_VertIn;
};

struct v2f {
    RG_FragIn;
};

v2f vert(v IN) {
    v2f OUT;

    OUT.position   = TransformObjectToHClip(IN.vertex);
    OUT.positionWS = TransformObjectToWorld(IN.vertex);
    OUT.normal     = TransformObjectToWorldNormal(IN.normal);
    OUT.tangent    = float4(TransformObjectToWorldDir(IN.tangent.xyz), IN.tangent.w * GetOddNegativeScale());
    OUT.uv         = IN.uv;
    OUT.lmuv       = IN.lmuv;
    OUT.rtuv       = IN.rtuv;

    //push position of vertex towards camera normal plane based on clothing layer
    float linearZ  = Linear01Depth(OUT.position.z, _ZBufferParams);
    OUT.position.z += _ClothingLayersSeparation * _ClothingLayer * linearZ;
    
    float3 viewDir = GetWorldSpaceViewDir(OUT.positionWS);
    OUT.positionWS += normalize(viewDir) * _ClothingLayer * _ClothingLayersSeparation;

    return OUT;
}

#define RED_HUE 0
#define BLUE_HUE 240

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

float4 frag(v2f IN, float facing : VFACE) : SV_Target {    
    #ifdef LOD_FADE_CROSSFADE
        LODFadeCrossFade(IN.position);
    #endif
    
    float4 shadowCoord = 0;
    #ifdef _MAIN_LIGHT_SHADOWS_SCREEN
        shadowCoord = ComputeScreenPos(TransformWorldToHClip(IN.positionWS));
    #else
        shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
    #endif
    half4 shadowMask = SAMPLE_SHADOWMASK(shadowCoord);

    float3 normalSample = UnpackNormal(RG_TEX_SAMPLE(_NormalMap, IN.uv));
    normalSample = normalize(lerp(float3(0, 0, 1), normalSample, _NormalStrength));
    float3 normalWS = NormalMapToWorld(normalSample, IN.normal, IN.tangent);
    if (facing < 0) normalWS = -normalWS;
    
    float4 color = _TintColor * RG_TEX_SAMPLE(_MainTex, IN.uv);
    float alpha = CalculateAlpha(color.a, _AlphaClip, _Cutoff, _Surface);

    //---------------------------------------
    // Additional lights
    //---------------------------------------

    // In the case of metallic, we just use the additional lights to decide where
    // the metallic reflections should be.
    float3 addLightFinal = float3(0, 0, 0);
    float addLighting = 0;
    
    #if _ADDITIONAL_LIGHTS
    uint numLights = GetAdditionalLightsCount();
    [unroll]
    for (uint i = 0; i < numLights; i++)
    {
        Light light = GetAdditionalLight(i, IN.positionWS, shadowMask);
        addLightFinal += HandleAdditionalLight(light.color, light.direction, light.shadowAttenuation, light.distanceAttenuation, normalWS, _LightMin);

        float luminance = LuminanceFormula(light.color);
        float halfNdotL = HalfLambert(normalWS, light.direction) * light.shadowAttenuation * light.distanceAttenuation;
        addLighting += luminance * halfNdotL;
        
    }
    #endif
    
    //---------------------------------------
    // Main Lighting
    //---------------------------------------
    
    //TODO: retrieve gradient and skybox colors
    //TODO: add this to somewhere
    half3 ambientLight = _GlossyEnvironmentColor.rgb;
    float lighting = 1;
    float4 colLighting;
    Light mainLight = GetMainLight(shadowCoord, IN.positionWS, shadowMask);
    if (mainLight.color.r != 0 || mainLight.color.g != 0 || mainLight.color.b != 0) {
        //Attenuate shadows using the same formula as the half lambert 
        float3 shadows = mainLight.shadowAttenuation * mainLight.distanceAttenuation;
        shadows = saturate(max(shadows, _LightMin));
    
        shadows = max(_LightMin, max(IndirectLighting(normalWS, float4(IN.lmuv, IN.rtuv)), shadows));
    
        float halfNdotL = max(_LightMin, HalfLambert(normalWS, mainLight.direction));

        lighting = min(shadows, halfNdotL);

        float midpoint = _MidPoint;
        //Stylization
        // Make sure the lighting is between LightMin and 1, smoothed
        lighting = max(_LightMin, smoothstep(midpoint - _LightSmooth, midpoint + _LightSmooth, lighting));
        colLighting = float4(lighting * mainLight.color, 1);
    } else {
        lighting = max(_LightMin, smoothstep(_MidPoint - _LightSmooth, _MidPoint + _LightSmooth, IndirectLighting(normalWS, float4(IN.lmuv, IN.rtuv))));
        colLighting = lighting;
    }

    //---------------------------------------
    // Metallic
    //---------------------------------------
    float3 viewDir = GetWorldSpaceNormalizeViewDir(IN.positionWS);
    float3 lightDir = normalize(mainLight.direction);
    
    float metalUv = dot(normalWS, normalize(viewDir + lightDir));
    
    float3 metalRampSample = UnitySampleGradient(GetMetallicGradient(), metalUv.x) * _MetallicStrength;
    float3 metallicColor = CalculateMetalness(color, _MetallicStrength, metalRampSample);

    float3 reflection = CalculateReflections(viewDir, normalWS, _MetallicRoughness);
    reflection = lerp(float3(1,1,1), reflection, _MetallicReflection);
    float3 reflectionColor = metallicColor * reflection * mainLight.color;
    
    //we also want to fade the metallic based on NdotL
    float3 metallicFinal = reflectionColor * (lighting + addLighting);

    // ---------------------------------------
    //Color hue shifting based on light/shadow
    // ---------------------------------------

    float4 lightTint = RG_TEX_SAMPLE(_DayNightRamp, float2(_kelvinTemp, 0.75));
    float4 shadowTint = RG_TEX_SAMPLE(_DayNightRamp, float2(_kelvinTemp, 0.25));
    colLighting = lerp(colLighting, colLighting * lerp(shadowTint, lightTint, lighting), _DNTintStr);

    float4 finalColor = color * (colLighting + float4(ambientLight, 1) + float4(addLightFinal, 1)) + float4(metallicFinal, 1);
    finalColor.a = alpha;

    return finalColor;
}
#endif
