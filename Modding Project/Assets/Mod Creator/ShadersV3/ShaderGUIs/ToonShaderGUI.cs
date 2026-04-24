#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEditor.Rendering.Universal;

namespace GameAssets.Shaders.ShaderGUIs {
    public class ToonShaderGUI {
        // Reference: UnityEditor.Rendering.Universal.ShaderGUI.SimpleLitGUI
        // because the shader GUI has a crucial job in setting up keywords and passes for the shader,
        // as such I'm gonna be copying the structure while adapting it to our needs.
        
        public static class Styles {
            public static GUIContent shadingModelText = EditorGUIUtility.TrTextContent("Shading Model", "Select the shading model to use for this material.");
            public static GUIContent addLightMixText = EditorGUIUtility.TrTextContent("Additional Light Mixing", "Select the method to mix additional lights with the main light.");
            public static GUIContent baseMapText = EditorGUIUtility.TrTextContent("Base Map", "Albedo (RGB) and Transparency (A).");
            public static GUIContent normalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map.");
            public static GUIContent normalStrengthText = EditorGUIUtility.TrTextContent("Normal Strength", "Strength of the Normal Map effect.");
            public static GUIContent emissionColorText = EditorGUIUtility.TrTextContent("Emission Color", "Emission color. A black color disables emission.");
            public static GUIContent emissionMapText = EditorGUIUtility.TrTextContent("Emission Map", "Emission (RGB) and Transparency (A).");
        }
        
        public struct ToonShaderProperties {

            public MaterialProperty ShadingModel;
            public MaterialProperty AdditionalLightMixing;
            
            public MaterialProperty BaseColor;
            public MaterialProperty BaseMap;
            
            public MaterialProperty NormalMap;
            public MaterialProperty NormalStrength;
            
            public MaterialProperty EmissionColor;
            public MaterialProperty EmissionMap;
            
            public MaterialProperty SpecularPower;
            public MaterialProperty SpecularAmount;
            public MaterialProperty SpecularColor;
            public MaterialProperty SpecularMap;
            
            public MaterialProperty RimLightColor;
            public MaterialProperty RimLightAmount;
            public MaterialProperty RimLightPower;
            
            public MaterialProperty MetallicStrength;
            public MaterialProperty MetallicColor;
            public MaterialProperty MetallicReflection;
            public MaterialProperty MetallicRoughness;
            
            public MaterialProperty ClothFiberMap;
            public MaterialProperty ClothFiberNormalMap;
            public MaterialProperty FiberStrenght;
            public MaterialProperty Sheen;
            public MaterialProperty SheenPower;
            public MaterialProperty SheenColor;
            
            public MaterialProperty AnisotropyUV;
            public MaterialProperty AnisotropyPower;
            public MaterialProperty PerlinMap;
            public MaterialProperty LowfrequencyScale;
            public MaterialProperty LowfrequencyStrenght;
            public MaterialProperty MidfrequencyScale;
            public MaterialProperty MidfrequencyStrenght;
            public MaterialProperty HighfrequencyScale;
            public MaterialProperty HighfrequencyStrenght;
            public MaterialProperty HighlightContrast;
            public MaterialProperty HighlightColor;

            public MaterialProperty IsFace;
            public MaterialProperty FaceDbg;
            public MaterialProperty SphereCenter;
            public MaterialProperty SphereRadius;

            public MaterialProperty RChannelSmooting;
            public MaterialProperty GChannelSmooting;
            public MaterialProperty BChannelSmooting;

            public MaterialProperty LayerCount;
            public MaterialProperty FurLayerSpacing;
            
            public MaterialProperty ClothingLayersSeparation;
            public MaterialProperty ClothingLayer;

            public ToonShaderProperties(MaterialProperty[] properties) {
                ShadingModel = BaseShaderGUI.FindProperty("_ShadingModel", properties, false);
                AdditionalLightMixing = BaseShaderGUI.FindProperty("_AddLightMix", properties, true);
                
                BaseColor = BaseShaderGUI.FindProperty("_BaseColor", properties, true);
                BaseMap = BaseShaderGUI.FindProperty("_BaseMap", properties, true);
                
                NormalMap = BaseShaderGUI.FindProperty("_NormalMap", properties, false);
                NormalStrength = BaseShaderGUI.FindProperty("_NormalStrength", properties, false);
                
                EmissionColor = BaseShaderGUI.FindProperty("_EmissionColor", properties, false);
                EmissionMap = BaseShaderGUI.FindProperty("_EmissionMap", properties, false);
                
                SpecularPower = BaseShaderGUI.FindProperty("_SpecularPower", properties, false);
                SpecularAmount = BaseShaderGUI.FindProperty("_SpecularAmount", properties, false);
                SpecularColor = BaseShaderGUI.FindProperty("_SpecularColor", properties, false);
                SpecularMap = BaseShaderGUI.FindProperty("_SpecularMap", properties, false);
                
                RimLightColor = BaseShaderGUI.FindProperty("_RimLightColor", properties, false);
                RimLightAmount = BaseShaderGUI.FindProperty("_RimLightAmount", properties, false);
                RimLightPower = BaseShaderGUI.FindProperty("_RimLightPower", properties, false);
                
                MetallicStrength = BaseShaderGUI.FindProperty("_MetallicStrength", properties, false);
                MetallicColor = BaseShaderGUI.FindProperty("_MetallicColor", properties, false);
                MetallicReflection = BaseShaderGUI.FindProperty("_MetallicReflection", properties, false);
                MetallicRoughness = BaseShaderGUI.FindProperty("_MetallicRoughness", properties, false);

                ClothFiberMap = BaseShaderGUI.FindProperty("_ClothFiberMap", properties, false);
                ClothFiberNormalMap = BaseShaderGUI.FindProperty("_ClothFiberNormalMap", properties, false);
                FiberStrenght = BaseShaderGUI.FindProperty("_FiberStrenght", properties, false);
                Sheen = BaseShaderGUI.FindProperty("_Sheen", properties, false);
                SheenPower = BaseShaderGUI.FindProperty("_SheenPower", properties, false);
                SheenColor = BaseShaderGUI.FindProperty("_SheenColor", properties, false);
                
                AnisotropyUV = BaseShaderGUI.FindProperty("_AnisoUV", properties, false);
                AnisotropyPower = BaseShaderGUI.FindProperty("_AnisoPower", properties, false);
                PerlinMap = BaseShaderGUI.FindProperty("_PerlinNoise", properties, false);
                LowfrequencyScale = BaseShaderGUI.FindProperty("_LowFreqScale", properties, false);
                LowfrequencyStrenght = BaseShaderGUI.FindProperty("_LowFreqStrenght", properties, false);
                MidfrequencyScale = BaseShaderGUI.FindProperty("_MidFreqScale", properties, false);
                MidfrequencyStrenght = BaseShaderGUI.FindProperty("_MidFreqStrenght", properties, false);
                HighfrequencyScale = BaseShaderGUI.FindProperty("_HighFreqScale", properties, false);
                HighfrequencyStrenght = BaseShaderGUI.FindProperty("_HighFreqStrenght", properties, false);
                HighlightContrast = BaseShaderGUI.FindProperty("_HighlightContrast", properties, false);
                HighlightColor = BaseShaderGUI.FindProperty("_HighlightTint", properties, false);
                
                IsFace = BaseShaderGUI.FindProperty("_IsFace", properties, false);
                FaceDbg = BaseShaderGUI.FindProperty("_FaceDbg", properties, false);
                SphereCenter = BaseShaderGUI.FindProperty("_SphereCenter", properties, false);
                SphereRadius = BaseShaderGUI.FindProperty("_SphereRadius", properties, false);
                
                RChannelSmooting = BaseShaderGUI.FindProperty("_RChSmooth", properties, false);
                GChannelSmooting = BaseShaderGUI.FindProperty("_GChSmooth", properties, false);
                BChannelSmooting = BaseShaderGUI.FindProperty("_BChSmooth", properties, false);
                
                LayerCount = BaseShaderGUI.FindProperty("_LayerCount", properties, false);
                FurLayerSpacing = BaseShaderGUI.FindProperty("_FurLayerSpacing", properties, false);
                
                ClothingLayersSeparation = BaseShaderGUI.FindProperty("_ClothingLayersSeparation", properties, false);
                ClothingLayer = BaseShaderGUI.FindProperty("_ClothingLayer", properties, false);
            }
        }

        public static void Inputs(ToonShaderProperties properties, MaterialEditor materialEditor, Material material) {}

        public static void Advanced(ToonShaderProperties shadingModelProperties) {}
    }

    public class BaseToonShader : BaseShaderGUI {
        private ToonShaderGUI.ToonShaderProperties shaderProperties;
        bool showSpecular = true;
        bool showRimLight = true;
        bool showMetallic = true;
        bool showClothFiber = true;
        bool showAnisotropy = true;
        bool showFaceShadowing = true;
        bool showSkinShading = true;
        
        public override void FindProperties(MaterialProperty[] properties) {
            base.FindProperties(properties);
            shaderProperties = new (properties);
        }
        
        public override void ValidateMaterial(Material material) {
            SetMaterialKeywords(material, SharedToonShaderGUI.SetMaterialKeywords);
        }
        
        public override void DrawSurfaceOptions(Material material) {
            if (material == null)
                throw new ArgumentNullException("material");
            
            DoPopup(ToonShaderGUI.Styles.shadingModelText, shaderProperties.ShadingModel, Enum.GetNames(typeof(SharedToonShaderGUI.LightingStyle)));
            DoPopup(ToonShaderGUI.Styles.addLightMixText, shaderProperties.AdditionalLightMixing, Enum.GetNames(typeof(SharedToonShaderGUI.AdditionalLightMixing)));
            
            EditorGUILayout.Space();
            
            if (material.HasProperty("_LayerCount") && material.HasProperty("_FurLayerSpacing")) {
                EditorGUILayout.LabelField("Fur Settings", EditorStyles.boldLabel);
                materialEditor.RangeProperty(shaderProperties.LayerCount, "Fur Layer Count");
                materialEditor.RangeProperty(shaderProperties.FurLayerSpacing, "Fur Layer Spacing");
                EditorGUILayout.Space();
            }
            
            if (material.HasProperty("_IsFace")) {
                materialEditor.ShaderProperty(shaderProperties.IsFace, "Use Face Shadowing");
                EditorGUILayout.Space();
            }
            
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;
            
            base.DrawSurfaceOptions(material);
        }

        public override void DrawSurfaceInputs(Material material) {
            
            materialEditor.TexturePropertySingleLine(ToonShaderGUI.Styles.baseMapText, shaderProperties.BaseMap, shaderProperties.BaseColor);
            DrawTileOffset(materialEditor, baseMapProp);
            EditorGUILayout.Space();
            
            if (shaderProperties.NormalMap != null)
            {
                DrawNormalArea(materialEditor, shaderProperties.NormalMap, shaderProperties.NormalStrength);
                DrawTileOffset(materialEditor, shaderProperties.NormalMap);
            }

            if (shaderProperties.EmissionColor != null && shaderProperties.EmissionMap != null) {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Emission", EditorStyles.boldLabel);
                DrawEmissionProperties(material, true);
            }
            
            if (shaderProperties.SpecularPower != null && shaderProperties.SpecularAmount != null && shaderProperties.SpecularColor != null){
                EditorGUILayout.Space();
                showSpecular = EditorGUILayout.BeginFoldoutHeaderGroup(showSpecular, "Specular", EditorStyles.foldoutHeader);
                if (showSpecular) {
                    materialEditor.RangeProperty(shaderProperties.SpecularPower, "Power");
                    materialEditor.RangeProperty(shaderProperties.SpecularAmount, "Amount");
                    materialEditor.TexturePropertySingleLine(new("Map"), shaderProperties.SpecularMap,
                        shaderProperties.SpecularColor);
                    DrawTileOffset(materialEditor, shaderProperties.SpecularMap);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            
            if (shaderProperties.Sheen != null && shaderProperties.SheenPower != null && shaderProperties.SheenColor != null) {
                EditorGUILayout.Space();
                showClothFiber = EditorGUILayout.BeginFoldoutHeaderGroup(showClothFiber, "Cloth Fiber", EditorStyles.foldoutHeader);
                if (showClothFiber) {
                    materialEditor.RangeProperty(shaderProperties.Sheen, "Sheen");
                    materialEditor.RangeProperty(shaderProperties.SheenPower, "Sheen Power");
                    materialEditor.ColorProperty(shaderProperties.SheenColor, "Sheen Color");
                    materialEditor.TexturePropertySingleLine(new("Fiber Map"), shaderProperties.ClothFiberMap);
                    DrawTileOffset(materialEditor, shaderProperties.ClothFiberMap);
                    DrawNormalArea(materialEditor, shaderProperties.ClothFiberNormalMap, shaderProperties.FiberStrenght);
                    DrawTileOffset(materialEditor, shaderProperties.ClothFiberNormalMap);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            if (shaderProperties.RimLightColor != null && shaderProperties.RimLightAmount != null && shaderProperties.RimLightPower != null) {
                EditorGUILayout.Space();
                showRimLight = EditorGUILayout.BeginFoldoutHeaderGroup(showRimLight, "Rim Light", EditorStyles.foldoutHeader);
                if (showRimLight) {
                    materialEditor.ColorProperty(shaderProperties.RimLightColor, "Color");
                    materialEditor.RangeProperty(shaderProperties.RimLightAmount, "Amount");
                    materialEditor.RangeProperty(shaderProperties.RimLightPower, "Power");
                }
            }
            
            if (shaderProperties.MetallicStrength != null && shaderProperties.MetallicColor != null && shaderProperties.MetallicReflection != null && shaderProperties.MetallicRoughness != null) {
                EditorGUILayout.Space();
                showMetallic = EditorGUILayout.BeginFoldoutHeaderGroup(showMetallic, "Metallic", EditorStyles.foldoutHeader);
                if (showMetallic) {
                    materialEditor.RangeProperty(shaderProperties.MetallicStrength, "Strength");
                    materialEditor.ColorProperty(shaderProperties.MetallicColor, "Color");
                    materialEditor.RangeProperty(shaderProperties.MetallicReflection, "Reflection");
                    materialEditor.RangeProperty(shaderProperties.MetallicRoughness, "Roughness");
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            if (shaderProperties.AnisotropyUV != null) {
                EditorGUILayout.Space();
                showAnisotropy = EditorGUILayout.BeginFoldoutHeaderGroup(showAnisotropy, "Anisotropic Reflections", EditorStyles.foldoutHeader);
                if (showAnisotropy) {
                    EditorGUILayout.LabelField("Anisotropy", EditorStyles.boldLabel);
                    materialEditor.VectorProperty(shaderProperties.AnisotropyUV, "Anisotropy UV");
                    materialEditor.RangeProperty(shaderProperties.AnisotropyPower, "Anisotropy Power");
                    EditorGUILayout.Space();
                    materialEditor.ColorProperty(shaderProperties.HighlightColor, "Highlight Tint");
                    materialEditor.RangeProperty(shaderProperties.HighlightContrast, "Highlight Contrast");
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Noise", EditorStyles.boldLabel);
                    materialEditor.TexturePropertySingleLine(new("Perlin Noise"), shaderProperties.PerlinMap);
                    EditorGUILayout.Space();
                    materialEditor.RangeProperty(shaderProperties.LowfrequencyScale, "Low Frequency Scale");
                    materialEditor.RangeProperty(shaderProperties.LowfrequencyStrenght, "Low Frequency Strength");
                    EditorGUILayout.Space();
                    materialEditor.RangeProperty(shaderProperties.MidfrequencyScale, "Mid Frequency Scale");
                    materialEditor.RangeProperty(shaderProperties.MidfrequencyStrenght, "Mid Frequency Strength");
                    EditorGUILayout.Space();
                    materialEditor.RangeProperty(shaderProperties.HighfrequencyScale, "High Frequency Scale");
                    materialEditor.RangeProperty(shaderProperties.HighfrequencyStrenght, "High Frequency Strength");
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            
            if (shaderProperties.IsFace != null) {
                EditorGUILayout.Space();
                showFaceShadowing = EditorGUILayout.BeginFoldoutHeaderGroup(showFaceShadowing, "Face Shading", EditorStyles.foldoutHeader);
                if (showFaceShadowing) {
                    materialEditor.ShaderProperty(shaderProperties.FaceDbg, "Show Face Normals");
                    materialEditor.VectorProperty(shaderProperties.SphereCenter, "Sphere Center");
                    materialEditor.FloatProperty(shaderProperties.SphereRadius, "Sphere Radius");
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            
            if(shaderProperties.RChannelSmooting != null && shaderProperties.GChannelSmooting != null && shaderProperties.BChannelSmooting != null) {
                EditorGUILayout.Space();
                showSkinShading = EditorGUILayout.BeginFoldoutHeaderGroup(showSkinShading, "Skin Shading", EditorStyles.foldoutHeader);
                if (showSkinShading) {
                    materialEditor.RangeProperty(shaderProperties.RChannelSmooting, "R Channel Smoothing");
                    materialEditor.RangeProperty(shaderProperties.GChannelSmooting, "G Channel Smoothing");
                    materialEditor.RangeProperty(shaderProperties.BChannelSmooting, "B Channel Smoothing");
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
        
        public override void DrawAdvancedOptions(Material material) {
            ToonShaderGUI.Advanced(shaderProperties);
            if (shaderProperties.ClothingLayersSeparation != null && shaderProperties.ClothingLayer != null) {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Clothing Layers", EditorStyles.boldLabel);
                materialEditor.RangeProperty(shaderProperties.ClothingLayersSeparation, "Clothing Layers Separation");
                materialEditor.FloatProperty(shaderProperties.ClothingLayer, "Clothing Layer");
            }
            base.DrawAdvancedOptions(material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader) {
            material.shaderKeywords = null;
            material.enabledKeywords = null;
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
        }
    }
}
#endif