#ifndef COMMON_INCLUDE_HLSL
#define COMMON_INCLUDE_HLSL

#include_with_pragmas "../ToonShaders/ToonVariants.hlsl"
#include "../ToonShaders/ToonVariants.hlsl"

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif
#include "../ToonShaders/NdotL.hlsl"
#include "../ToonShaders/CustomLighting.hlsl"
#include "../BlendModes.hlsl"
#include "../Utils.hlsl"
#include "./ShaderFunctions.hlsl"

#endif
