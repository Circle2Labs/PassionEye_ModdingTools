﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GameAssets.Shaders.ShaderGUIs
{
    public class ToonShaderGUI : BaseShaderGUI
    {
        // It's crucial to ensure these are correctly populated before use.
        MaterialProperty[] properties;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material mat = materialEditor.target as Material;
            
            FindProperties(properties);

            if (m_FirstTimeApply)
            {
                OnOpenGUI(mat, materialEditor);
                this.materialEditor = materialEditor;
                this.properties = properties;
                m_FirstTimeApply = false;
            }

            ShaderPropertiesGUI(mat);
        }

        public override void DrawSurfaceInputs(Material material)
        {
            foreach (var property in properties)
            {
                if ((property.flags & MaterialProperty.PropFlags.HideInInspector) == 0)
                {
                    materialEditor.ShaderProperty(property, property.displayName);
                }
            }
        }
    }
}
#endif