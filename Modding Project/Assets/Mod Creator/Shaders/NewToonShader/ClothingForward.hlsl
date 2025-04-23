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

CBUFFER_START(UnityPerMaterial)
float4 _TintColor;
CBufTexture(_MainTex);
CBufTexture(_NormalMap);
float _NormalStrength;
CBufTexture(_ClothFiberMap);
CBufTexture(_ClothFiberNormalMap);
float _FiberStrenght;
float _Sheen;
float _SheenPower;
float4 _SheenColor;

// Fake Camera Light
float _EnableCameraLight;
float4 _CameraLightColor;
float _CameraLightSmooth;
float _CameraLightMidPoint;

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

struct v
{
    RG_VertIn;
};

struct v2f
{
    RG_FragIn;
};

v2f vert(v IN)
{
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

float4 frag(v2f IN) : SV_Target
{    
    #ifdef LOD_FADE_CROSSFADE
        LODFadeCrossFade(IN.position);
    #endif

    half4 shadowMask = SAMPLE_SHADOWMASK(geomData.shadowCoord);

    float4 shadowCoord = 0;
    #ifdef _MAIN_LIGHT_SHADOWS_SCREEN
        shadowCoord = ComputeScreenPos(TransformWorldToHClip(IN.positionWS));
    #else
        shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
    #endif

    float3 normalSample = normalize(
        lerp(float3(0, 0, 1),
        UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, TRANSFORM_TEX(IN.uv, _NormalMap))), _NormalStrength));
    float3 fiberNormal = normalize(
        lerp(float3(0, 0, 1),
        UnpackNormal(SAMPLE_TEXTURE2D(_ClothFiberNormalMap, sampler_ClothFiberNormalMap, TRANSFORM_TEX(IN.uv, _ClothFiberNormalMap))), _FiberStrenght));

    float3 fullNrm = normalize(float3(normalSample.rg + fiberNormal.rg, normalSample.b * fiberNormal.b));
    float3 normalWS = NormalMapToWorld(fullNrm, IN.normal, IN.tangent);

    float4 color = _TintColor *
        SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(IN.uv, _MainTex)) *
        SAMPLE_TEXTURE2D(_ClothFiberMap, sampler_ClothFiberMap, TRANSFORM_TEX(IN.uv, _ClothFiberMap));

    //---------------------------------------
    // Main Lighting
    //---------------------------------------

    Light mainLight = GetMainLight(shadowCoord, IN.positionWS, shadowMask);
    
    //TODO: retrieve gradient and skybox colors
    //TODO: add this to somewhere
    half3 ambientLight = _GlossyEnvironmentColor.rgb;
    float4 colLighting;
    float3 specular = float3(0, 0, 0);
    float lighting;
    if (_EnableCameraLight != 0) {
        ambientLight += _CameraLightColor.rgb;
        // fake camera light
        float3 cameraLightDir = -normalize(GetViewForwardDir());
        float3 shadows         = 1;
        shadows                = saturate(max(shadows, _LightMin));
        shadows = max(_LightMin, max(IndirectLighting(normalWS, IN.lmuv), shadows));
    
        float halfNdotL = max(_LightMin, HalfLambert(normalWS, cameraLightDir) *  mainLight.shadowAttenuation);
        lighting = min(shadows, halfNdotL);
    
        //Stylization
        // Make sure the lighting is between LightMin and 1, smoothed
        lighting = max(_LightMin, smoothstep(_CameraLightMidPoint - _CameraLightSmooth, _CameraLightMidPoint + _CameraLightSmooth, lighting));
    
        colLighting = float4(lighting * _CameraLightColor.rgb, 1);

        specular = Specularity(cameraLightDir, GetWorldSpaceViewDir(IN.positionWS),
            normalWS, _SheenPower, _Sheen, _SheenColor);
    } else {
        //real lighting
        lighting = 1;
        if (mainLight.color.r != 0 || mainLight.color.g != 0 || mainLight.color.b != 0)
        {
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
            specular = Specularity(mainLight.direction, GetWorldSpaceViewDir(IN.positionWS),
                normalWS, _SheenPower, _Sheen, _SheenColor);
        } else {
            lighting = max(_LightMin, smoothstep(_MidPoint - _LightSmooth, _MidPoint + _LightSmooth, IndirectLighting(normalWS, IN.lmuv)));
            colLighting = lighting;
        }
        
    }
    
    //IDEA: have only lighting colored by kelvin then multiplied with surf color

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
            float addShadow = max(0, light.shadowAttenuation * light.distanceAttenuation);
            float addLightHalfNdotL = max(0, HalfLambert(normalWS, light.direction));

            float addLightLighting = min(addShadow, addLightHalfNdotL);
            
            addLightLighting = max(_LightMin, smoothstep(0, _LightSmooth, addLightLighting));
                //max(0, smoothstep(midpoint - _LightSmooth * midpoint, midpoint + _LightSmooth * midpoint, addLightLighting));
            
            addLightFinal += addLightLighting * light.color;
        }
    #endif

    // ---------------------------------------
    // ALPHA
    // ---------------------------------------
    float alpha = color.a;

    if(_AlphaClip)
    {
        clip(alpha - _Cutoff);
        alpha = SharpenAlpha(alpha, _Cutoff);
    }
    
    alpha = OutputAlpha(alpha, _Surface);
    
    // ---------------------------------------
    //Color hue shifting based on light/shadow
    // ---------------------------------------
    //convert lighting to a value between 0 and 1 where 0 is shadow and 1 is light
    float4 lightTint, shadowTint;
    lightTint = SAMPLE_TEXTURE2D(_DayNightRamp, sampler_DayNightRamp, TRANSFORM_TEX(float2(_kelvinTemp, 0.75), _DayNightRamp));
    shadowTint = SAMPLE_TEXTURE2D(_DayNightRamp, sampler_DayNightRamp, TRANSFORM_TEX(float2(_kelvinTemp, 0.25), _DayNightRamp));

    colLighting = lerp(colLighting, colLighting * lerp(shadowTint, lightTint, lighting), _DNTintStr);

    color = color * (colLighting + float4(ambientLight, 1) + float4(addLightFinal, 1)) + float4(specular, 1);
    return float4(color.rgb, alpha);
}
#endif
