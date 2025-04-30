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
Texture2D _DayNightRamp;
SamplerState sampler_DayNightRamp;
float4 _DayNightRamp_ST;
float _DNTintStr;
CBUFFER_END

CBUFFER_START (UnityPerMaterial)
float4 _TintColor;
Texture2D _MainTex;
SamplerState sampler_MainTex;
float4 _MainTex_ST;
Texture2D _NormalMap;
SamplerState sampler_NormalMap;
float4 _NormalMap_ST;
float _NormalStrength;
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

    float3 normalSample = normalize(lerp(float3(0, 0, 1),
                                         UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, TRANSFORM_TEX(IN.uv, _NormalMap))),
                                         _NormalStrength));
    float3 normalWS = NormalMapToWorld(normalSample, IN.normal, IN.tangent);

    float4 color = _TintColor * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(IN.uv, _MainTex));

    //---------------------------------------
    // Main Lighting
    //---------------------------------------
    
    //TODO: retrieve gradient and skybox colors
    //TODO: add this to somewhere
    half3 ambientLight = _GlossyEnvironmentColor.rgb;
    float lighting = 1;
    float4 colLighting;
    Light mainLight = GetMainLight(shadowCoord, IN.positionWS, shadowMask);
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
    } else {
        lighting = max(_LightMin, smoothstep(_MidPoint - _LightSmooth, _MidPoint + _LightSmooth, IndirectLighting(normalWS, IN.lmuv)));
        colLighting = lighting;
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
            addLightFinal += HandleAdditionalLight(light.color, light.direction, light.shadowAttenuation, light.distanceAttenuation, normalWS, _LightMin);
        }
    #endif

    
    // ---------------------------------------
    //Color hue shifting based on light/shadow
    // ---------------------------------------

    float4 lightTint, shadowTint;
    lightTint = SAMPLE_TEXTURE2D(_DayNightRamp, sampler_DayNightRamp, TRANSFORM_TEX(float2(_kelvinTemp, 0.75), _DayNightRamp));
    shadowTint = SAMPLE_TEXTURE2D(_DayNightRamp, sampler_DayNightRamp, TRANSFORM_TEX(float2(_kelvinTemp, 0.25), _DayNightRamp));

    colLighting = lerp(colLighting, colLighting * lerp(shadowTint, lightTint, lighting), _DNTintStr);

    return color * (colLighting + float4(ambientLight, 1) + float4(addLightFinal, 1));
}
#endif
