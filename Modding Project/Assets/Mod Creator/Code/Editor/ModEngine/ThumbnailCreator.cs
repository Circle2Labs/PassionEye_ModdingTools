using System.IO;
using System.Reflection;
using Code.Managers;
using Code.Tools;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace Code.Editor.ModEngine
{
	public class ThumbnailCreator : EditorWindow
	{
		public const string Version = "v0.1.0.0";

		[SerializeField] 
		public string[] AvailableLanguages = {"English"};
		
		[SerializeField]
		public int Language;
	
		[SerializeField]
		public int FieldOfView = 26;
		
		[SerializeField]
		public Color BackgroundColor = Color.black;
		
		[SerializeField]
		public Vector3 FocusAngle = new (10, 170, 0);
		
		[SerializeField]
		public Vector3 FocusPosition = new (-0.02f, 0, 0);

		[SerializeField]
		public float FocusZoom = 0.5f;

		[SerializeField]
		public string ExportPath = "Assets/";
		
		private RenderTexture renderTexture;

		[MenuItem("Mod Engine/Thumbnail Creator")]
		public static void Initialize()
		{
			var window = (ThumbnailCreator)GetWindow(typeof(ThumbnailCreator), true, $"Thumbnail Creator {Version}");
			window.minSize = new Vector2(516, 775);
			window.maxSize = window.minSize;
			window.Show();
			window.InitializeLanguages();
		}

		public void OnGUI()
		{
			var canFocus = Selection.activeGameObject != null;
			var canExport = canFocus && Directory.Exists(ExportPath);
			var shouldFocus = false;

			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_LANGUAGES"), EditorStyles.boldLabel);

			var prevLanguage = Language;
			Language = EditorGUILayout.Popup($"{GetLocalizedString("MODCREATOR_PRESETSLANGUAGE_CURRENTLANG")}*", Language, AvailableLanguages);

			if (prevLanguage != Language)
				SetLanguage(AvailableLanguages[Language]);
			
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			validateTexture();
			
			GUILayout.BeginHorizontal();
			GUILayout.Space(2);
			GUILayout.Label(renderTexture, GUIStyle.none, GUILayout.Width(512), GUILayout.Height(512));
			GUILayout.EndHorizontal();
			
			FieldOfView = EditorGUILayout.IntSlider(GetLocalizedString("MODCREATOR_THUMBNAIL_FOV"), FieldOfView, 6, 46);
			BackgroundColor = EditorGUILayout.ColorField(GetLocalizedString("MODCREATOR_THUMBNAIL_BGCOLOR"), BackgroundColor);
			
			GUILayout.Space(5);

			EditorGUI.BeginChangeCheck();
			
			FocusAngle = EditorGUILayout.Vector3Field(GetLocalizedString("MODCREATOR_THUMBNAIL_FOCUSANG"), FocusAngle);
			FocusPosition = EditorGUILayout.Vector3Field(GetLocalizedString("MODCREATOR_THUMBNAIL_FOCUSPOS"), FocusPosition);
			FocusZoom = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_THUMBNAIL_FOCUSZOOM"), FocusZoom, 0.0001f, 3f);

			if (EditorGUI.EndChangeCheck())
				shouldFocus = true;
			
			GUILayout.FlexibleSpace();

			ExportPath = EditorGUILayout.TextField(GetLocalizedString("MODCREATOR_THUMBNAIL_EXPORTPATH"), ExportPath);
			
			GUILayout.BeginHorizontal();
			
			if (!canFocus)
				GUI.enabled = false;

			if (GUILayout.Button(GetLocalizedString("MODCREATOR_THUMBNAIL_FOCUS")) || (canFocus && shouldFocus))
				focus();
			
			GUI.enabled = true;
			
			if (!canExport)
				GUI.enabled = false; 

			if (GUILayout.Button(GetLocalizedString("MODCREATOR_THUMBNAIL_CAPTURE")))
				captureThumbnail();
			
			GUI.enabled = true;

			GUILayout.EndHorizontal();
			
			Repaint();
		}
		
		public void InitializeLanguages()
		{
			LocalizationManager.Instance.LoadAllLanguages();

			AvailableLanguages = LocalizationManager.Instance.GetAvailableLanguages().ToArray();
			SetLanguage("English");
		}
		
		public void SetLanguage(string language)
		{
			for (var i = 0; i < AvailableLanguages.Length; i++)
			{
				if (AvailableLanguages[i] != language) 
					continue;
				
				Language = i;
			}
		}
		
		public string GetLocalizedString(string key)
		{
			return LocalizationManager.Instance.GetLocalizedString(AvailableLanguages[Language], key).Value;
		}

		private void captureThumbnail()
		{
			if (BackgroundColor.a < 1f)
			{
				// TODO: URP 17 allows post processing with transparency, warn until upgraded
				Debug.LogWarning("Taking transparent screenshots is not supported, remove transparency manually until implemented");
			}
			
			var sceneCamera = SceneView.lastActiveSceneView.camera;
			sceneCamera.targetTexture = renderTexture;
			sceneCamera.Render();
			
			RenderTexture.active = renderTexture;
			
			var tex = new Texture2D(512, 512, TextureFormat.RGBA32, true);
			tex.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
			tex.Apply();
			
			var bytes = tex.EncodeToPNG();
			DestroyImmediate(tex);
			
			var fileName = "thumbnail_" + Selection.activeGameObject.name;
			fileName = fileName.ReplaceInvalidFileNameCharacters("") + ".png";

			if (FileTools.WriteAllBytesSafe(Path.Combine(ExportPath, fileName), bytes))
				Debug.Log($"Saved thumbnail {fileName}");
		}
		
		private void validateTexture()
		{
			if (renderTexture == null)
				renderTexture = new RenderTexture(512, 512, 32);
			
			var sceneCamera = SceneView.lastActiveSceneView.camera;
			sceneCamera.fieldOfView = FieldOfView;
			sceneCamera.allowHDR = false;
			sceneCamera.clearFlags = CameraClearFlags.SolidColor;
			sceneCamera.targetTexture = renderTexture;
			sceneCamera.backgroundColor = BackgroundColor;

			sceneCamera.Render();
			renderTexture.Create();
		}

		private void focus()
		{
			var sceneView = SceneView.lastActiveSceneView;
				
			var focusedField = sceneView.GetType().GetField("m_WasFocused", BindingFlags.NonPublic | BindingFlags.Instance);
			focusedField.SetValue(sceneView, false);
				
			sceneView.FrameSelected(false, true);

			var previousPosition = sceneView.pivot;
			previousPosition += FocusPosition;
				
			sceneView.pivot = previousPosition;
				
			var previousAngles = Selection.activeGameObject.transform.rotation.eulerAngles;
			previousAngles += FocusAngle;

			sceneView.rotation = Quaternion.Euler(previousAngles);

			var previousSize = sceneView.size;
			previousSize += FocusZoom;

			sceneView.size = previousSize;
		}
	}
}