#ifndef BASE_FORWARD
#define BASE_FORWARD

#include_with_pragmas "./CommonInclude.hlsl"

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
float4 _TintColor;
CBufTexture(_MainTex);
CBufTexture(_NormalMap);
float _NormalStrength;

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

float4 frag(v2f IN, float facing : VFACE) : SV_Target {
    //LOD
    #ifdef LOD_FADE_CROSSFADE
        LODFadeCrossFade(IN.position);
    #endif

    //ColorMap
    float4 color = _TintColor * RG_TEX_SAMPLE(_MainTex, IN.uv);

    // Alpha
    float alpha = CalculateAlpha(color.a, _AlphaClip, _Cutoff, _Surface);

    //ShadowMask
    float4 shadowCoord = 0;
    #ifdef _MAIN_LIGHT_SHADOWS_SCREEN
        shadowCoord = ComputeScreenPos(TransformWorldToHClip(IN.positionWS));
    #else
        shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
    #endif
    half4 shadowMask = SAMPLE_SHADOWMASK(shadowCoord);

    //Normal Map
    float3 normalSample = UnpackNormal(RG_TEX_SAMPLE(_NormalMap,IN.uv));
    normalSample = normalize(lerp(float3(0, 0, 1), normalSample, _NormalStrength));
    float3 normalWS = NormalMapToWorld(normalSample, IN.normal, IN.tangent);
    if (facing < 0) normalWS = -normalWS;
    
    //---------------------------------------
    // Main Lighting
    //---------------------------------------
    
    //TODO: retrieve gradient and skybox colors
    //TODO: add this to somewhere
    half3 ambientLight = _GlossyEnvironmentColor.rgb;
    float lighting = 1;
    float4 colLighting = float4(0, 0, 0, 1);
    Light mainLight = GetMainLight(shadowCoord, IN.positionWS, shadowMask);
    #ifdef _CHARACTER_LIGHTING //customized lighting for characters, prioritizes main light
    if (mainLight.color.r != 0 || mainLight.color.g != 0 || mainLight.color.b != 0) {
        //Attenuate shadows using the same formula as the half lambert 
        float3 shadows         = LinearToGamma22(mainLight.shadowAttenuation * mainLight.distanceAttenuation);
        shadows                = saturate(max(shadows, _LightMin));
    
        shadows = max(_LightMin, max(IndirectLighting(normalWS, IN.lmuv), shadows));
    
        float halfNdotL = max(_LightMin, HalfLambert(normalWS, mainLight.direction));

        lighting = min(shadows, halfNdotL);

        float midpoint = _MidPoint;
        //Stylization
        // Make sure the lighting is between LightMin and 1, smoothed
        lighting = max(_LightMin, smoothstep(midpoint - _LightSmooth, midpoint + _LightSmooth, lighting));
        colLighting = float4(lighting * mainLight.color, 1);
    } else {
        lighting = max(_LightMin, smoothstep(_MidPoint - _LightSmooth, _MidPoint + _LightSmooth, IndirectLighting(normalWS, IN.lmuv)));
        colLighting = lighting;
    }
    //IDEA: have only lighting colored by kelvin then multiplied with surf color
    #else //normal lighting, GI friendly
        float halfNdotL = max(_LightMin, HalfLambert(normalWS, mainLight.direction));
        half realtimeShadows = max(_LightMin, MainLightRealtimeShadow(shadowCoord));
        half rtShadowFade = GetMainLightShadowFade(IN.positionWS);
    
        float3 indLighting = IndirectLighting(normalWS, float4(IN.lmuv, IN.rtuv));
    
        colLighting = float4((halfNdotL * mainLight.distanceAttenuation * mainLight.color), alpha);
        colLighting *= color;
        colLighting += float4(indLighting, 0);
    #endif

    //---------------------------------------
    // Additional lights
    //---------------------------------------
    float3 addLightFinal = float3(0, 0, 0);
    
    #if _ADDITIONAL_LIGHTS
        uint numLights = GetAdditionalLightsCount();
        [unroll]
        for (uint i = 0; i < numLights; i++)
        {
            Light light = GetAdditionalLight(i, IN.positionWS, shadowMask);
            addLightFinal += HandleAdditionalLight(light.color, light.direction, light.shadowAttenuation, light.distanceAttenuation, normalWS, _LightMin);
        }
    #endif
    
    // ---------------------------------------
    //Color hue shifting based on light/shadow
    // ---------------------------------------
    float4 lightTint = RG_TEX_SAMPLE(_DayNightRamp, float2(_kelvinTemp, 0.75));
    float4 shadowTint = RG_TEX_SAMPLE(_DayNightRamp, float2(_kelvinTemp, 0.25));

    colLighting = lerp(colLighting, colLighting * lerp(shadowTint, lightTint, lighting), _DNTintStr);
    float4 finalColor = color * (colLighting + float4(ambientLight, 1) + float4(addLightFinal, 1));
    finalColor.a = alpha;

    return finalColor;
}
#endif
