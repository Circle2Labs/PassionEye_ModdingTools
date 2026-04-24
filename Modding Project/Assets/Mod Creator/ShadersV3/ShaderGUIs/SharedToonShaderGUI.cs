using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameAssets.Shaders.ShaderGUIs
{
	public class SharedToonShaderGUI
	{
        public enum LightingStyle {
            Lambert,
            StylizedLambert,
            HalfLambert
        }

        public enum AdditionalLightMixing {
            Additive,
            Replace
        }
        
        public static void SetMaterialKeywords(Material material) {
            if (material == null)
                throw new ArgumentNullException("material");
            
            if(material.HasProperty("_ShadingModel")) {
                // Shading Model
                int shadingModel = (int) material.GetFloat("_ShadingModel");
                CoreUtils.SetKeyword(material, "RG_LAMBERT", shadingModel == (int) LightingStyle.Lambert);
                CoreUtils.SetKeyword(material, "RG_STYLIZED_LAMBERT", shadingModel == (int) LightingStyle.StylizedLambert);
                CoreUtils.SetKeyword(material, "RG_HALF_LAMBERT", shadingModel == (int) LightingStyle.HalfLambert);
            } else { // it's skin
                CoreUtils.SetKeyword(material, "RG_LAMBERT", false);
                CoreUtils.SetKeyword(material, "RG_STYLIZED_LAMBERT", false);
                CoreUtils.SetKeyword(material, "RG_HALF_LAMBERT", false);
                CoreUtils.SetKeyword(material, "RG_SKIN", true);
            }
            
            if(material.HasProperty("_AddLightMix")) {
                // Additional Light Mixing
                int addLightMix = (int) material.GetFloat("_AddLightMix");
                CoreUtils.SetKeyword(material, "RG_ADDITIVE_MIX", addLightMix == (int) AdditionalLightMixing.Additive);
                CoreUtils.SetKeyword(material, "RG_MAX_MIX", addLightMix == (int) AdditionalLightMixing.Replace);
            }
            
            if (material.HasProperty("_NormalMap")) {
                // Normal Map
                bool hasNormalMap = material.GetTexture("_NormalMap") != null;
                CoreUtils.SetKeyword(material, "_NORMALMAP", hasNormalMap);
            }

            if (material.HasProperty("_EmissionColor") && material.HasProperty("_EmissionMap")) {
                // Emission
                bool hasEmissionColor = material.GetColor("_EmissionColor") != Color.black;
                bool hasEmissionMap = material.GetTexture("_EmissionMap") != null;
                CoreUtils.SetKeyword(material, "_EMISSION", hasEmissionColor || hasEmissionMap);
            }
            
            if (material.HasProperty("_SpecularAmount") && material.HasProperty("_SpecularColor")) {
                // Specular
                bool hasSpecular = material.GetFloat("_SpecularAmount") > 0f;
                CoreUtils.SetKeyword(material, "RG_SPECULAR", hasSpecular);
            }

            if (material.HasProperty("_MetallicStrength") && material.HasProperty("_MetallicColor")) {
                // Metallic
                bool hasMetallic = material.GetFloat("_MetallicStrength") > 0f;
                CoreUtils.SetKeyword(material, "RG_METALLIC", hasMetallic);
            }
            
            if (material.HasProperty("_Sheen")) {
                // Cloth Fiber
                bool hasClothFiber = material.GetFloat("_Sheen") > 0f;
                CoreUtils.SetKeyword(material, "RG_SHEEN", hasClothFiber);
            }
            
            if (material.HasProperty("_AnisoUV")){
                // Anisotropy
                CoreUtils.SetKeyword(material, "RG_ANISO", true);
            }
        }
    }
}