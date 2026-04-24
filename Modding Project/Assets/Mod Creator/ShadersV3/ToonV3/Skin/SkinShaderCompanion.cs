#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace GameAssets.Shaders.ToonV3.Skin {
    
    [CustomEditor(typeof(SkinShaderCompanion))]
    public class SkinShaderCompanionEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Save Gradient Textures")) {
                ((SkinShaderCompanion)target).ExportGradientTextures();
            }
            if (GUILayout.Button("Reset Gradient Textures")) {
                ((SkinShaderCompanion)target).ResetTextures();
            }
        }
    }
    
    [ExecuteAlways]
    public class SkinShaderCompanion : MonoBehaviour {
        private List<Material> materials;
        [SerializeField] private Gradient EnviromentalReflection;
        [SerializeField] private Gradient RimLight;
        [SerializeField] private Vector2 TexRes = new Vector2(256, 32);
        private Texture2D envReflTex;
        private Texture2D rimLightTex;
        private int envReflID = Shader.PropertyToID("_DiffuseRamp"); 
        private int rimLightID = Shader.PropertyToID("_RimLightRamp");
        
        Func<int, int, Texture2D> TextureInit = (resX, resY) => new(resX, resY, TextureFormat.RGBA32, false);

        public void Awake() {
            //Start();
        }

        public void Start() {
            materials = GetComponent<Renderer>().sharedMaterials.Where(m => m.shader.name == "Toon/Skin").ToList();
            
            if (materials.Count == 0) Debug.LogWarning("No materials with the Toon/Skin shader found on this object. Was a skin shader removed?");
            
            // Initialize textures
            envReflTex = TextureInit((int)TexRes.x, (int)TexRes.y);
            rimLightTex = TextureInit((int)TexRes.x, (int)TexRes.y);
            
            foreach (var mat in materials){
                RelinkIfNeeded(envReflTex, mat, envReflID);
                RelinkIfNeeded(rimLightTex, mat, rimLightID);
            }
        }

        public void OnValidate() {
            if (envReflTex == null || rimLightTex == null) {
                Debug.Log("Textures not initialized. Initializing now.");
                Start();
            }
            
            for (int x = 0; x < envReflTex.width; x++) {
                for (int y = 0; y < envReflTex.height; y++) {
                    envReflTex.SetPixel(x, y, EnviromentalReflection.Evaluate(x / (float)envReflTex.width));
                }
            }
            for (int x = 0; x < rimLightTex.width; x++) {
                for (int y = 0; y < rimLightTex.height; y++) {
                    rimLightTex.SetPixel(x, y, RimLight.Evaluate(x / (float)rimLightTex.width));
                }
            }
            envReflTex.Apply();
            Debug.Log($"Updated envReflTex with gradient data. Texture size: {envReflTex.width}x{envReflTex.height}");
            rimLightTex.Apply();
            Debug.Log($"Updated rimLightTex with gradient data. Texture size: {envReflTex.width}x{envReflTex.height}");
            
            foreach (var mat in materials){
                RelinkIfNeeded(envReflTex, mat, envReflID);
                RelinkIfNeeded(rimLightTex, mat, rimLightID);
            }
        }
        
        public void ResetTextures() {
            Debug.Log($"Resetting texture: envRefl");
            ResetTexture(ref envReflTex);
            Debug.Log($"Resetting texture: rimLight");
            ResetTexture(ref rimLightTex);
            OnValidate();
        }
        
        public void ExportGradientTextures() {
            string path = EditorUtility.SaveFilePanel("Save Gradient Texture", "", "GradientTexture.png", "png");
            if (string.IsNullOrEmpty(path)) {
                return;
            }
            System.IO.File.WriteAllBytes(path, envReflTex.EncodeToPNG());
            path = EditorUtility.SaveFilePanel("Save Rim Light Gradient Texture", "", "RimLightGradientTexture.png", "png");
            if (string.IsNullOrEmpty(path)) {
                AssetDatabase.Refresh();
                return;
            }
            System.IO.File.WriteAllBytes(path, rimLightTex.EncodeToPNG());
            AssetDatabase.Refresh();
        }

        private void RelinkIfNeeded(Texture2D tex, Material mat, int propertyId) {
            mat.SetTexture(propertyId, tex);
        }
        
        private void ResetTexture(ref Texture2D tex) {
            Debug.Log($"texture data: {tex.width}x{tex.height}, format: {tex.format} isReadable: {tex.isReadable}");
            if (tex is null) tex = TextureInit((int)TexRes.x, (int)TexRes.y);
            else tex.Reinitialize((int)TexRes.x, (int)TexRes.y);
            tex.Apply();
        }
    }
    
    public class GradientToTexture {
        public static void Convert(ref Texture2D texture, Gradient gradient) {
            for (int x = 0; x < texture.width; x++) {
                for (int y = 0; y < texture.height; y++) {
                    texture.SetPixel(x, y, gradient.Evaluate(x / (float)texture.width));
                }
            }
            texture.Apply();
        }
    }
}
#endif