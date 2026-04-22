#ifndef COMMON_VARIANTS_HLSL
#define COMMON_VARIANTS_HLSL

#if defined(RG_CLOTHING) || defined(RG_HAIR) || defined(RG_FUR)
#define RG_STYLIZED_LAMBERT // chara related use stylized
#elif defined(RG_SKINDEF) || defined(RG_EYE)
#define RG_SKIN // skin and eyes use skin
#elif defined(RG_BASE) || defined(RG_METAL) || defined(RG_PLASTIC) || defined(RG_WATER)
#define RG_LAMBERT // everything else use lambert
#else
#pragma multi_compile_local_fragment RG_LAMBERT RG_HALF_LAMBERT RG_STYLIZED_LAMBERT RG_SKIN // unused path
#endif

#if defined(RG_BASE) || defined(RG_METAL) || defined(RG_PLASTIC) || defined(RG_WATER) || defined(RG_SKINDEF) || defined(RG_EYE) || defined(RG_CLOTHING) || defined(RG_HAIR) || defined(RG_FUR)
#define RG_MAX_MIX // everything uses replace lighting
#else
#pragma multi_compile_local_fragment RG_ADDITIVE_MIX RG_MAX_MIX // unused path
#endif

// -------------------------------------
// Material keywords
#if !defined(RG_EYE)
#pragma shader_feature_local _NORMALMAP
#pragma shader_feature_local_fragment _EMISSION
#endif
#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
#pragma shader_feature_local_fragment _ALPHATEST_ON
#pragma shader_feature_local_fragment _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON

// -------------------------------------
// Universal Pipeline keywords
#define _MAIN_LIGHT_SHADOWS_CASCADE 1 // _MAIN_LIGHT_SHADOWS dont use it (cascade is better) _MAIN_LIGHT_SHADOWS_SCREEN dont use it (we dont use screenspace shadows)
#define _ADDITIONAL_LIGHTS 1 // _ADDITIONAL_LIGHTS_VERTEX dont use it (we dont use vertex lighting)
//#pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX dont use it (we dont use apv)
//#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING dont use it (might need if lightmap shadows weird)
#if !defined(RG_HAIR) && !defined(RG_EYE)
#pragma multi_compile _ SHADOWS_SHADOWMASK
#endif
#define _LIGHT_LAYERS 1
#define _FORWARD_PLUS 1
#define _ADDITIONAL_LIGHT_SHADOWS 1
#define _SHADOWS_SOFT 1
#pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
#define _SCREEN_SPACE_OCCLUSION 1
#pragma multi_compile _ _DBUFFER_MRT3 // _DBUFFER_MRT1 _DBUFFER_MRT2 (other two are lower quality)
//#pragma multi_compile_fragment _ _LIGHT_COOKIES dont use it (havent found use yet)
//#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
//#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl"
#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

// -------------------------------------
// Unity defined keywords
#if !defined(RG_HAIR) && !defined(RG_EYE)
#pragma multi_compile _ DIRLIGHTMAP_COMBINED
#pragma multi_compile _ LIGHTMAP_ON
#endif
//#pragma multi_compile _ DYNAMICLIGHTMAP_ON dont use it (we dont use realtime gi)
//#pragma multi_compile _ USE_LEGACY_LIGHTMAPS dont use it (we dont use instancing)
//#pragma multi_compile_fog dont use it (havent found use yet)
//#pragma multi_compile_fragment _ DEBUG_DISPLAY dont use it (??)
//#pragma multi_compile _ LOD_FADE_CROSSFADE dont use it (we dont use lods)

//--------------------------------------
// GPU Instancing
//#pragma multi_compile_instancing dont use it (havent found use yet)
//#pragma instancing_options renderinglayer dont use it (havent found use yet)
//#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl" dont use it (havent found use yet)

#endif
