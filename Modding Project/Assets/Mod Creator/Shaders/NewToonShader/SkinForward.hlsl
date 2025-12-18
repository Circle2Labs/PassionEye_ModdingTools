#ifndef BASE_FORWARD
#define BASE_FORWARD

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
CBufTexture(_DayNightRamp);
float _DNTintStr;
CBUFFER_END

CBUFFER_START (UnityPerMaterial)
// SSS
float4 _SSSShadowColor;
float4 _SSSTransitionColor;

// Fake Camera Light
float _EnableCameraLight;
float4 _CameraLightColor;
float _CameraLightSmooth;
float _CameraLightMidPoint;

// Specularity
float _SpecularPower;
float _SpecularAmount;
float4 _SpecularColor;

float4 _TintColor;
CBufTexture(_MainTex);
CBufTexture(_NormalMap);
float _NormalStrength;

CBufTexture(_AlphaMap);
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

    return OUT;
}

Gradient GetSkinGradient() {
    float secondBandSize = 0.1;

    float3 SSSRampShadow = _SSSShadowColor.rgb;
    float3 SSSRampTransition = _SSSTransitionColor.rgb;
    
    
    Gradient gradient;
    gradient.type = 0;
    gradient.colorsLength = 6;
    gradient.alphasLength = 0;
    
    gradient.colors[0] = float4(SSSRampShadow, 0);
    gradient.colors[1] = float4(SSSRampTransition, _CameraLightMidPoint - secondBandSize / 2);
    gradient.colors[2] = float4(SSSRampTransition, _CameraLightMidPoint - secondBandSize / 2 + 0.001);
    gradient.colors[3] = float4(SSSRampTransition, _CameraLightMidPoint + secondBandSize / 2);
    gradient.colors[4] = float4(SSSRampTransition, _CameraLightMidPoint + secondBandSize / 2 + 0.001);
    gradient.colors[5] = float4(1,1,1,1);
    gradient.colors[6] = float4(1,1,1,1);
    gradient.colors[7] = float4(1,1,1,1);

    gradient.alphas[0] = 1;
    gradient.alphas[1] = 1;
    gradient.alphas[2] = 1;
    gradient.alphas[3] = 1;
    gradient.alphas[4] = 1;
    gradient.alphas[5] = 1;
    gradient.alphas[6] = 1;
    gradient.alphas[7] = 1;

    return gradient;
}

float4 Unity_SampleGradient_float(Gradient gradient, float Time) {
    float3 color = gradient.colors[0].rgb;
    [unroll]
    for (int c = 1; c < 8; c++)
    {
        float colorPos = saturate((Time - gradient.colors[c-1].w) / (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, gradient.colorsLength-1);
        color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
    }
    #ifndef UNITY_COLORSPACE_GAMMA
    color = SRGBToLinear(color);
    #endif
    return float4(color, 1);
}


#define RED_HUE 0
#define BLUE_HUE 240

float4 frag(v2f IN) : SV_Target
{    
    #ifdef LOD_FADE_CROSSFADE
        LODFadeCrossFade(IN.position);
    #endif

    //sample alphamap and exit early (for clipping fix)
    float4 alphaTexSample = RG_TEX_SAMPLE(_AlphaMap, IN.uv);
    float alpha = CalculateAlpha(alphaTexSample.r, _AlphaClip, _Cutoff, _Surface);
    
    // as we technically do not need the semi-transparency with clipping fix active,
    // instantly clip out pixels that need to be culled. 
    
    //ShadowMask
    float4 shadowCoord = 0;
    #ifdef _MAIN_LIGHT_SHADOWS_SCREEN
    shadowCoord = ComputeScreenPos(TransformWorldToHClip(IN.positionWS));
    #else
    shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
    #endif
    half4 shadowMask = SAMPLE_SHADOWMASK(shadowCoord);
    
    float3 normalSample = UnpackNormal(RG_TEX_SAMPLE(_NormalMap, IN.uv));
    normalSample = (lerp(float3(0, 0, 1), normalSample, _NormalStrength));
    float3 normalWS = NormalMapToWorld(normalSample, IN.normal, IN.tangent);
    
    float4 color = _TintColor * RG_TEX_SAMPLE(_MainTex,IN.uv);
    // for some semi transparent skin
    color.a = alpha;
    //---------------------------------------
    // Main Lighting
    //---------------------------------------

    Light mainLight = GetMainLight(shadowCoord, IN.positionWS, shadowMask);
    
    //TODO: retrieve gradient and skybox colors
    //TODO: add this to somewhere
    half3 ambientLight = _GlossyEnvironmentColor.rgb;
    
    // fake camera light
    float3 cameraLightDir = -normalize(GetViewForwardDir());
    float3 shadows         = 1;
    shadows                = saturate(max(shadows, _LightMin));
    
    float halfNdotL = max(_LightMin, HalfLambert(normalWS, cameraLightDir));
    float lighting = min(shadows, halfNdotL);
    
    //Stylization
    // Make sure the lighting is between LightMin and 1, smoothed
    lighting = max(_LightMin, smoothstep(_CameraLightMidPoint - _CameraLightSmooth, _CameraLightMidPoint + _CameraLightSmooth, lighting));

    Gradient gradient = GetSkinGradient();
    
    // sample the gradient
    float4 skinCoeff = Unity_SampleGradient_float(gradient, lighting);
    float4 colLighting = lerp(skinCoeff, float4(_CameraLightColor.rgb, 1), lighting);
    
    //IDEA: have only lighting colored by kelvin then multiplied with surf color
    
    //---------------------------------------
    // Additional lights
    //---------------------------------------
    float3 addLightFinal = float3(0, 0, 0);

    // TODO: change additional lights contributions
    #if _ADDITIONAL_LIGHTS
        uint numLights = GetAdditionalLightsCount();
        [unroll]
        for (uint i = 0; i < numLights; i++)
        {
            Light light = GetAdditionalLight(i, IN.positionWS, shadowMask);
            addLightFinal += HandleAdditionalLight(light.color, light.direction, light.shadowAttenuation, light.distanceAttenuation, normalWS, _LightMin);
        }
    
    #endif

    //---------------------------------------
    // Specularity
    //---------------------------------------

    float3 spec = Specularity(cameraLightDir, cameraLightDir, normalWS,
                              _SpecularPower, _SpecularAmount, _SpecularColor.rgb) * pow(lighting, 6);
    
    // ---------------------------------------
    //Color hue shifting based on light/shadow
    // ---------------------------------------

    float4 lightTint = RG_TEX_SAMPLE(_DayNightRamp, float2(_kelvinTemp, 0.75));
    float4 shadowTint = RG_TEX_SAMPLE(_DayNightRamp, float2(_kelvinTemp, 0.25));
    colLighting = lerp(colLighting, colLighting * lerp(shadowTint, lightTint, lighting), _DNTintStr);

    return color * (colLighting + float4(ambientLight, 1) + float4(spec, 1) * float4(addLightFinal, 1));
}
#endif
