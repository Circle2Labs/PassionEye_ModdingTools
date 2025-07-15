#ifndef COMMON_INCLUDE_HLSL
#define COMMON_INCLUDE_HLSL

#include_with_pragmas "../ToonShaders/ToonVariants.hlsl"

//This is stupid AF but rider won't find references unless we also add this. Oh and have fun everytime it gets updated.
#include "Library/PackageCache/com.unity.render-pipelines.core@14.0.11/ShaderLibrary/Common.hlsl"
#include "Library/PackageCache/com.unity.render-pipelines.universal@14.0.11/ShaderLibrary/Core.hlsl"
#include "Library/PackageCache/com.unity.render-pipelines.universal@14.0.11/ShaderLibrary/Input.hlsl"
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
