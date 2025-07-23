#ifndef TOON_VARIANTS
#define TOON_VARIANTS

#pragma target 5.0 //we might want to use 5.0 for better optimized output from compiler.
#pragma multi_compile _ALPHATEST_ON
#pragma multi_compile _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
#pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS //no additional lights disabled option for faster compile.
#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH //no no-shadows option speeding up compile
#pragma multi_compile _ SHADOWS_SHADOWMASK
#pragma multi_compile _ _LIGHT_COOKIES //we might evend disable this if needed
//#pragma multi_compile _ _LIGHT_LAYERS //temporarely disabled, we'll introduce it later
#pragma multi_compile _ DYNAMICLIGHTMAP_ON //realtime GI, probably needed for later
#pragma multi_compile _ LIGHTMAP_ON
#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
#pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
#pragma skip_variants STEREO_CUBEMAP_RENDER_ON STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON UNITY_SINGLE_PASS_STEREO

#endif