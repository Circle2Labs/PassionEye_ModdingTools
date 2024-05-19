Shader "Toon/ToonShader" {
    Properties {
        [Space]
        _ShadowSharpness ("Light Smooth", Range(0, 1)) = 0.1
        _SecondBandOffset ("Second Band Offset", Range(0, 1)) = 0.1
        _NdotLBias ("NdotL Bias", Range(-1, 1)) = 0
        [Space]
        _LightTint ("Light Tint", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 1)
        [Toggle] _Shadow2ColorAuto ("Automatic Penumbra Color", float) = 0
        _Shadow2Color ("Penumbra Color", Color) = (0.25, 0.25, 0.25, 1)
        [Space]
        [MainColor] _BaseColor ("Tint Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap ("Main Color (RGB)", 2D) = "white" {}
        [Normal] _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0, 1)) = 0.5
        [Space]
        _MRFATex ("Metalness, Roughness, Fresnel, Alpha", 2D) = "white" {}
        [Toggle] _Use_Metalness ("Use Metalness", float) = 0
        _Metalness ("Metalness", Range(0, 1)) = 0 //texture
        [Space]
        [Toggle] _Use_Roughness ("Use Roughness", float) = 0
        _Roughness ("Roughness", Range(0, 1)) = 0.5 //texture
        _Fresnel ("Fresnel Amount", Range(0, 1)) = 0
        [Space]
        _SpecularAmount ("Specular Amount", Range(0, 1)) = 0
        [PowerSlider(1.5)] _SpecularPower ("Specular Power", Range(0.01, 100)) = 0.1
        [Toggle] _AutoSpecularColor ("Automatic Specular Color", float) = 0
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        [Space]
        [Toggle] _Face_Rendering ("Face Rendering Fix", float) = 0
        [Toggle] _IsSdf ("Use SDF for Face Rendering", float) = 0
        _SDFTex ("SDF Texture", 2D) = "white" {}
        _FaceCenter ("Face Center", Vector) = (0, 0, 0, 0)
        _FaceFwdVec ("Face Forward Vector", Vector) = (0, 0, 0, 0)
        _FaceRightVec ("Face Right Vector", Vector) = (0, 0, 0, 0)
        [Space]
        _Transparency ("Alpha Intensity", Range(0, 1)) = 1
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Space]
        _CensorPartTex ("Censor Texture", 2D) = "black" {}
        
        // Blending state
        [HideInInspector]_Surface("__surface", Float) = 0.0
        [HideInInspector]_Blend("__blend", Float) = 0.0
        [HideInInspector]_Cull("__cull", Float) = 2.0
        [HideInInspector][Toggle(_ALPHATEST_ON)] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite("__zwrite", Float) = 1.0
        [HideInInspector] _ZTest("__ztest", Float) = 4.0
        [HideInInspector] _BlendModePreserveSpecular("_BlendModePreserveSpecular", Float) = 1.0
        // Editmode props
        _QueueOffset("Queue offset", Float) = 0.0
    }
    SubShader {

        Pass {
            Name "ToonForward"
            
            Tags { 
                "RenderPipeline"="UniversalPipeline"
            }
            
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
            Cull [_Cull]
                        
            HLSLPROGRAM
  #pragma target 5.0 //we might want to use 5.0 for better optimized output from compiler.
            //#pragma dynamic_branch _ALPHATEST_ON //we might rather want to just do it anyways.
            #pragma multi_compile _ALPHATEST_ON
            #pragma multi_compile _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS //no additional lights disabled option for faster compile.
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH //no no-shadows option speeding up compile
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _LIGHT_COOKIES //we might evend disable this if needed
            //#pragma multi_compile _ _LIGHT_LAYERS //temporarely disabled, we'll introduce it later
            //#pragma multi_compile _ DYNAMICLIGHTMAP_ON //realtime GI, probably needed for later
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //#pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING //minor quality hit frankly
            //#pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION //might not be needed until later in development
            //#pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX //pixel only if commented
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
            //this is VR stuff. Not needed and safe to skip.
            #pragma skip_variants STEREO_CUBEMAP_RENDER_ON STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON UNITY_SINGLE_PASS_STEREO

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #if defined(LOD_FADE_CROSSFADE)
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif
            #include "CustomLighting.hlsl"
            
            struct attributes {
                float3 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
                float4 lmuv : TEXCOORD1;
                float4 rtuv : TEXCOORD2;
            };

            struct varyings {
                float4 position : SV_POSITION;
                float3 positionWS : TEXCOORD3;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
                float4 lmuv : TEXCOORD1;
                float4 rtuv : TEXCOORD2;
            };

            #include "ToonShaderCBuffer.hlsl"
            
            varyings vert(attributes IN) {
                varyings OUT;
                
                OUT.position = TransformObjectToHClip(IN.vertex);
                OUT.positionWS = TransformObjectToWorld(IN.vertex);
                OUT.normal = TransformObjectToWorldNormal(IN.normal);
                OUT.tangent = float4(TransformObjectToWorldDir(IN.tangent.xyz), IN.tangent.w * GetOddNegativeScale());
                OUT.uv = IN.uv;
                OUT.lmuv = IN.lmuv;
                OUT.rtuv = IN.rtuv;
                
                return OUT;
            }

            float4 frag(varyings IN) : SV_Target {
                // Input base color
                float2 baseColUV = TRANSFORM_TEX(IN.uv, _BaseMap);
                float3 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, baseColUV).rgb * _BaseColor.rgb;

                float2 MRFAUV = TRANSFORM_TEX(IN.uv, _MRFATex);
                float alpha = SAMPLE_TEXTURE2D(_MRFATex, sampler_MRFATex, MRFAUV).a * _Transparency;
                
                AlphaDiscard(alpha, _Cutoff);
                
                alpha = OutputAlpha(alpha, _Surface);
                baseCol = AlphaModulate(baseCol, alpha);

                #ifdef LOD_FADE_CROSSFADE
                    LODFadeCrossFade(IN.position);
                #endif
                
                FaceData fdata;
                fdata.isFace = _Face_Rendering != 0;
                fdata.isSdf = _IsSdf != 0;
                #ifdef _SDF_SHADING
                    fdata.sdfSample = SAMPLE_TEXTURE2D(_SDFTex, sampler_LinearClamp, IN.uv);
                #else
                    fdata.sdfSample = 0;
                #endif
                fdata.faceCenter = _FaceCenter;
                fdata.faceFwdVec = _FaceFwdVec;
                fdata.faceRgtVec = _FaceRightVec;

                float2 normalMapUV = TRANSFORM_TEX(IN.uv, _NormalMap);
                float4 normalSample = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, normalMapUV);
                float3 unpackedNormal = normalize(lerp(float3(0,0,1), UnpackNormal(normalSample), _NormalStrength));
                
                GeometryData gdata;
                gdata.lgtDir = 0;
                gdata.posWs = IN.positionWS;
                gdata.nrmWs = NormalMapToWorld(unpackedNormal, IN.normal, IN.tangent);
                gdata.viewDir = GetWorldSpaceViewDir(IN.positionWS.xyz);
                gdata.mainLgtDir = 0;
                gdata.shadowMask = 0;
                gdata.lightmapUV = 0;
                OUTPUT_LIGHTMAP_UV(IN.lmuv, unity_LightmapST, gdata.lightmapUV);

                #ifdef _MAIN_LIGHT_SHADOWS_SCREEN
                    gdata.shadowCoord = ComputeScreenPos(TransformWorldToHClip(IN.positionWS.xyz));
                #else
                    gdata.shadowCoord = TransformWorldToShadowCoord(IN.positionWS.xyz);
                #endif
                
                DiffuseData ddata;
                ddata.smooth = _ShadowSharpness;
                ddata.secBndOffset = _SecondBandOffset;
                ddata.NdotL = 0;
                ddata.NdotLBias = _NdotLBias;
                ddata.lightTint = _LightTint.rgb;
                ddata.auto2ndBndCol = _Shadow2ColorAuto != 0;
                ddata.firstBndCol = _ShadowColor.rgb;
                ddata.secBndCol = _Shadow2Color.rgb;
                ddata.auto2ndBndCol = _Shadow2ColorAuto != 0;
                ddata.shadowAttn = 0;
                
                SpecularData sdata;
                sdata.specAmt = _SpecularAmount;
                sdata.specPow = _SpecularPower;
                sdata.specCol = _SpecularColor.xyz;
                sdata.autoCol = _AutoSpecularColor != 0;

                MetallicData mdata;
                mdata.useMetallic = _Use_Metalness != 0;
                mdata.metalness = SAMPLE_TEXTURE2D(_MRFATex, sampler_MRFATex, MRFAUV).r * _Metalness;
                mdata.baseCol = baseCol;

                Gradient _metallicGradient;

                _metallicGradient.type = 1;
                
                _metallicGradient.colorsLength = 6;
                _metallicGradient.colors[0] = float4(0.2075472, 0.2075472, 0.2075472, 0.156);
                _metallicGradient.colors[1] = float4(0.25, 0.25, 0.25, 0.194);
                _metallicGradient.colors[2] = float4(0.7, 0.7, 0.7, 0.506);
                _metallicGradient.colors[3] = float4(1, 1, 1, 0.535);
                _metallicGradient.colors[4] = float4(2.125, 2.125, 2.125, 0.924);
                _metallicGradient.colors[5] = float4(6.0, 6.0, 6.0, 0.941);
                _metallicGradient.colors[6] = float4(6.0, 6.0, 6.0, 0.941);
                _metallicGradient.colors[7] = float4(6.0, 6.0, 6.0, 0.941);
                
                _metallicGradient.alphasLength = 0;
                _metallicGradient.alphas[0] = 0;
                _metallicGradient.alphas[1] = 0;
                _metallicGradient.alphas[2] = 0;
                _metallicGradient.alphas[3] = 0;
                _metallicGradient.alphas[4] = 0;
                _metallicGradient.alphas[5] = 0;
                _metallicGradient.alphas[6] = 0;
                _metallicGradient.alphas[7] = 0;
                
                mdata.gradient = _metallicGradient;
                
                RoughnessData rdata;
                rdata.useRoughness = _Use_Roughness != 0;
                rdata.roughness = SAMPLE_TEXTURE2D(_MRFATex, sampler_MRFATex, MRFAUV).g * _Roughness;
                rdata.fresnelAmt = SAMPLE_TEXTURE2D(_MRFATex, sampler_MRFATex, MRFAUV).b * _Fresnel;
                
                float3 color = PELighting(fdata, gdata, ddata, sdata, mdata, rdata);

                return float4(color, alpha);
            }
            ENDHLSL
        }
        
        Pass {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            
            ZWrite [_ZWrite]
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            
            #pragma vertex vertShadow
            #pragma fragment fragShadow

            #pragma multi_compile _ _ALPHATEST_ON
            #pragma multi_compile _ _SURFACE_TYPE_TRANSPARENT

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #if defined(LOD_FADE_CROSSFADE)
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif
            #include "CustomLighting.hlsl"

            //biases
            float3 _LightDirection;
            float3 _LightPosition;
            
            // Define input structure for vertex shader
            struct VertexInput {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            // Define output structure from vertex shader, input to fragment shader
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            #include "ToonShaderCBuffer.hlsl"

            float4 GetShadowPositionHClip(VertexInput input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif

                return positionCS;
            }

            
            // Vertex shader
            VertexOutput vertShadow(VertexInput input) {
                VertexOutput output;
                output.pos = GetShadowPositionHClip(input);
                output.uv = input.uv;
                
                return output;
            }

            // Fragment shader
            float4 fragShadow(VertexOutput input) : SV_TARGET {
                // Sample the alpha mask texture
                float2 MRFAUV = TRANSFORM_TEX(input.uv, _MRFATex);
                float alpha = SAMPLE_TEXTURE2D(_MRFATex, sampler_MRFATex, MRFAUV).a * _Transparency;
                
                #if defined(_ALPHATEST_ON) || defined(_SURFACE_TYPE_TRANSPARENT)
                    clip(alpha - _Cutoff);
                #endif

                #ifdef LOD_FADE_CROSSFADE
                    LODFadeCrossFade(input.positionCS);
                #endif

                return 0;
            }
            ENDHLSL
        }
        
        Pass {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite [_ZWrite]
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #if defined(LOD_FADE_CROSSFADE)
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif
            #include "CustomLighting.hlsl"

            #if defined(LOD_FADE_CROSSFADE)
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

            // -------------------------------------
            // Shader Stages
            #pragma vertex vert
            #pragma fragment frag

            // -------------------------------------
            // Material Keywords
            #pragma multi_compile _ _ALPHATEST_ON
            #pragma multi_compile _ _SURFACE_TYPE_TRANSPARENT
            
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            //#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // Define input structure for vertex shader
            struct VertexInput {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            // Define output structure from vertex shader, input to fragment shader
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            #include "ToonShaderCBuffer.hlsl"

            VertexOutput vert(VertexInput input) {
                VertexOutput output;
                output.pos = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                return output;
            }

            float frag(VertexOutput input) : SV_Target {
                float2 MRFAUV = TRANSFORM_TEX(input.uv, _MRFATex);
                float alpha = SAMPLE_TEXTURE2D(_MRFATex, sampler_MRFATex, MRFAUV).a * _Transparency;
                AlphaDiscard(alpha, _Cutoff);
                return 1;
            }
            
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }
            
            ZWrite [_ZWrite]
            Cull[_Cull]
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #if defined(LOD_FADE_CROSSFADE)
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

            #include "../Utils.hlsl"
            
            #pragma vertex vert
            #pragma fragment frag

            #pragma dynamic_branch _ _ALPHATEST_ON
            #pragma dynamic_branch _ LOD_FADE_CROSSFADE

            struct VertexInput {
                float3 positionOS : POSITION;
                float4 tangentOS : TANGENT;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float4 tangentWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
            };

            #include "ToonShaderCBuffer.hlsl"

            VertexOutput vert(VertexInput input) {
                VertexOutput output;
                output.pos = TransformObjectToHClip(input.positionOS);
                output.normal = input.normalOS;
                output.uv = input.uv;
                output.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w * GetOddNegativeScale());
                output.viewDirWS = GetWorldSpaceViewDir(input.positionOS.xyz);
                return output;
            }

            void frag(VertexOutput input,out half4 outNormalWS : SV_Target0
                #ifdef _WRITE_RENDERING_LAYERS
                    , out float4 outRenderingLayers : SV_Target1
                #endif
                ){
                
                float2 uv = TRANSFORM_TEX(input.uv, _MRFATex);
                float alpha = SAMPLE_TEXTURE2D(_MRFATex, sampler_MRFATex, uv).a * _Transparency;
                AlphaDiscard(alpha, _Cutoff);

                #ifdef LOD_FADE_CROSSFADE
                    LODFadeCrossFade(input.pos);
                #endif

                float3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv), _NormalStrength);
                float3 normalWS = NormalMapToWorld(normalTS, input.normal, input.tangentWS);

                outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);

                #ifdef _WRITE_RENDERING_LAYERS
                    uint renderingLayers = GetMeshRenderingLayer();
                    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
                #endif
            }
            
            ENDHLSL
        }
        //UsePass "Universal Render Pipeline/Lit/META"
    }
    //FallBack "Hidden/Core/FallbackError"
    CustomEditor "GameAssets.Shaders.ShaderGUIs.ToonShaderGUI"
}
