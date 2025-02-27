using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Assets.Code.Frameworks.Animation.Enums;
using Code.Components;
using Code.EditorScripts.ModCreator;
using Code.Frameworks.Animation;
using Code.Frameworks.Animation.Enums;
using Code.Frameworks.Animation.Interfaces;
using Code.Frameworks.Character.CharacterObjects;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.ModdedScenes;
using Code.Frameworks.RayTracing;
using Code.Frameworks.Studio.Interfaces;
using Code.Frameworks.Studio.StudioObjects;
using Code.Tools;
using Packages.SFB;
using Railgun.AssetPipeline.Enums;
using Railgun.AssetPipeline.Interfaces;
using Railgun.AssetPipeline.Models;
using Railgun.AssetPipeline.Services;
using Railgun.AssetPipeline.Types;
using Railgun.ModEngine.Providers;
using Railgun.ModEngine.Services;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Tuple = Code.Tools.Tuple;
using Vector2 = UnityEngine.Vector2;

namespace Code.Editor.ModEngine
{
	public partial class ModCreator 
	{
		[SerializeField]
		public bool DebugMode;
		
		[SerializeField]
		public ECompression DebugCompressionType = ECompression.None;

		private Vector2 builderScrollPosition;
		
		private Regex lodRegex = new ("^LOD\\d$");
		
		private const string uniqueIDFormat = "_#";

		public void DrawBuilder()
		{
			builderScrollPosition = GUILayout.BeginScrollView(builderScrollPosition);

			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BUILDER_MODINFO"), EditorStyles.boldLabel);
			Manifest.Name = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BUILDER_NAME")}*", Manifest.Name);
			if (Manifest.Name != null)
				Manifest.Name = Regex.Replace(Manifest.Name, nameTypeFilter, "");
			
			Manifest.Description = EditorGUILayout.TextField(GetLocalizedString("MODCREATOR_BUILDER_DESC"), Manifest.Description);

			Manifest.Author = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BUILDER_AUTHOR")}*", Manifest.Author);
			if (Manifest.Author != null)
				Manifest.Author = Regex.Replace(Manifest.Author, nameTypeFilter, "");
			
			Manifest.Version = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BUILDER_VERSION")}*", Manifest.Version);

			GUILayout.Space(10);

			{
				var list = Manifest.CompatibleVersions != null ? Manifest.CompatibleVersions.ToList() : new List<string>();
				verticalList(list, GetLocalizedString("MODCREATOR_BUILDER_COMPATVER"));
				Manifest.CompatibleVersions = list.ToArray();
			}
			
			GUILayout.Space(10);

			{
				var list = Manifest.Dependencies != null ? Manifest.Dependencies.ToList() : new List<string>();
				verticalList(list, GetLocalizedString("MODCREATOR_BUILDER_DEPS"));
				Manifest.Dependencies = list.ToArray();
			}
			
			GUILayout.Space(10);
			
			BasicFolds[0] = EditorGUILayout.Foldout(BasicFolds[0], GetLocalizedString("MODCREATOR_BUILDER_CHARAOBJ"), true);
			if (BasicFolds[0])
			{
				GUILayout.BeginVertical();
				verticalList(Prefabs, ETemplateType.CharacterObject,GetLocalizedString("MODCREATOR_BUILDER_LIST"));
				GUILayout.EndVertical();
			}
			
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			
			BasicFolds[1] = EditorGUILayout.Foldout(BasicFolds[1], GetLocalizedString("MODCREATOR_BUILDER_STUDIOOBJ"), true);
			if (BasicFolds[1])
			{
				GUILayout.BeginVertical();
				verticalList(Prefabs, ETemplateType.StudioObject, GetLocalizedString("MODCREATOR_BUILDER_LIST"));
				GUILayout.EndVertical();

			}
			
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			
			BasicFolds[2] = EditorGUILayout.Foldout(BasicFolds[2], GetLocalizedString("MODCREATOR_BUILDER_MODSCENE"), true);
			if (BasicFolds[2])
			{
				GUILayout.BeginVertical();
				verticalList(Prefabs, ETemplateType.ModdedScene, GetLocalizedString("MODCREATOR_BUILDER_LIST"));
				GUILayout.EndVertical();
			}
			
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			
			BasicFolds[3] = EditorGUILayout.Foldout(BasicFolds[3], GetLocalizedString("MODCREATOR_BUILDER_ANIM"), true);
			if (BasicFolds[3])
			{
				GUILayout.BeginVertical();
				verticalList(Prefabs, ETemplateType.Animation, GetLocalizedString("MODCREATOR_BUILDER_LIST"));
				GUILayout.EndVertical();
			}
			
			GUILayout.Space(10);
			
			if (!string.IsNullOrEmpty(Assembly.Item1))
			{
				EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BUILDER_ASMINFO"), EditorStyles.boldLabel);
				
				EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BUILDER_NAME"), Assembly.Item1, "TextField");
				EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BUILDER_SIZE"), Assembly.Item2.Length / 1024 + " kB", "TextField");
			}

			GUILayout.EndScrollView();
			
			GUILayout.FlexibleSpace();

			DebugMode = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BUILDER_DEBUG"), DebugMode);

			if (DebugMode)
			{
				GUILayout.BeginHorizontal();

				DebugCompressionType = (ECompression)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BUILDER_COMPTYPE"), DebugCompressionType, "MODCREATOR_BUILDER_COMPTYPE_", GUILayout.Width(275));
                
				var actionText = DebugCompressionType == ECompression.None ? GetLocalizedString("MODCREATOR_BUILDER_DECOMPRESS") : $"{GetLocalizedString("MODCREATOR_BUILDER_COMPRESS")} ({(DebugCompressionType == ECompression.NullValue ? GetLocalizedString("MODCREATOR_BUILDER_DEFAULT") : DebugCompressionType)})";
				if (GUILayout.Button(actionText, GUILayout.Width(175)))
				{
					StandaloneFileBrowser.OpenFilePanelAsync(GetLocalizedString("MODCREATOR_BUILDER_LOAD"), "", "rgm", false,delegate(string[] files)
					{
						if (files.Length == 0 || files[0] == "")
							return;
                        
						var affService = Railgun.AssetPipeline.Providers.ServiceProvider.Obtain<IAFFService>();
						if (affService.TryCompressModFile(files[0], out var decompressedPath, DebugCompressionType))
							Debug.Log($"Sucessfully {(DebugCompressionType == ECompression.None ? "Decompress" : "Compress")}ed mod to {decompressedPath}");
						else
							Debug.LogError($"Failed to {(DebugCompressionType == ECompression.None ? "Decompress" : "Compress")} mod {files[0]}");
					});
				}

				GUILayout.FlexibleSpace();

				if (GUILayout.Button(GetLocalizedString("MODCREATOR_BUILDER_LOAD"), GUILayout.Width(125)))
					Load();
				
				GUILayout.EndHorizontal();
			}
			else
			{
				GUILayout.BeginHorizontal();

				if (GUILayout.Button(GetLocalizedString("MODCREATOR_BUILDER_ASSIGN"), GUILayout.Width(175)))
					AssignComponents();

				if (GUILayout.Button(GetLocalizedString("MODCREATOR_BUILDER_BUILD"), GUILayout.Width(125)))
				{
					try
					{
						Build();
					}
					catch(Exception e)
					{
						Debug.LogError($"Error when building mod {e}");
					}
					
					EditorUtility.ClearProgressBar();
				}
                
				GUILayout.FlexibleSpace();

				if (GUILayout.Button(GetLocalizedString("MODCREATOR_BUILDER_RESET"), GUILayout.Width(75)))
				{
					ResetState();
				}

				GUILayout.EndHorizontal();
			}
		}

		public void ResetState()
		{
			Manifest = new Manifest { Name = "", Author = "", Version = "1.0.0" };

			Prefabs.Clear();
			Templates.Clear();

			Assembly = new Tuple.SerializableTuple<string, byte[]>(null, null);

			copyBuffer = null;
			tempAssembly = null;
		}
		
		public void Load()
		{
			StandaloneFileBrowser.OpenFilePanelAsync("Load Mod", "", "rgm", false, delegate(string[] files)
			{
				if (files.Length == 0 || files[0] == "")
					return;

				var modLoader = ServiceProvider.Obtain<IModLoader>();
                var mod = modLoader.LoadMod(files[0]).Result;
				if (mod == null)
				{
					Debug.LogError("Failed loading mod " + files[0]);
					return;
				}
				
				Templates.Clear();
				Prefabs.Clear();
				
				if (mod.TryGetManifest(out Manifest man))
					Manifest = man;

				var adv = false;
				
				foreach (var tuple in mod.GetModObjects())
				{
					var go = Railgun.ModEngine.Conversion.Converter.From(tuple.Item2).To<GameObject>().Convert();
					
					foreach (var component in tuple.Item2.Components)
					{
						var type = Type.GetType($"{component.Type.Item1}, {component.Type.Item2}");
						if (type == null || (!type.GetInterfaces().Contains(typeof(IStudioObject)) && 
						                     !type.GetInterfaces().Contains(typeof(ICharacterObject)) && 
						                     type != typeof(ModdedScene) && 
						                     !type.GetInterfaces().Contains(typeof(IAnimation))))
							continue;

						if (type.Assembly != typeof(BaseClothing).Assembly)
							adv = true;

						var template = new Template();

						var comp = go.GetComponent(type);
						if (comp is IStudioObject studioObject)
						{
							template.TemplateType = ETemplateType.StudioObject;
							template.StudioObjectType = studioObject.StudioObjectType;
							template.Name = studioObject.Name;
							template.Tags = studioObject.Tags;
							template.Icon = studioObject.Icon;
							template.Description = studioObject.Description;
							template.IsNSFW = studioObject.IsNSFW;
							template.FKData = studioObject.FKData;
						}
						else if (comp is ICharacterObject characterObject)
						{
							template.TemplateType = ETemplateType.CharacterObject;
							template.Name = characterObject.Name;
							template.Tags = characterObject.Tags;
							template.Icon = characterObject.Icon;
							template.Description = characterObject.Description;
							template.IsNSFW = characterObject.IsNSFW;
							template.SupportedGendersFlags = characterObject.SupportedGendersFlags;
							template.Simulation = characterObject.Simulation;
							
							switch (comp)
							{
								case IClothing cloth:
									template.ClothingType = cloth.ClothingType;
									template.CharacterObjectType = ECharacterObjectType.Clothing;
									template.ClothingStates = new Transform[Enum.GetNames(typeof(EClothingState)).Length];
									template.BlendshapeOffsets = cloth.BlendshapeOffsets;
					
									for (var k = 0; k < cloth.ClothingStates.Length; k++)
										template.ClothingStates[k] = cloth.ClothingStates[k];
									
									template.ClippingDistance = cloth.ClippingDistance;
									break;
								case IHair hair:
									template.HairType = hair.HairType;
									template.CharacterObjectType = ECharacterObjectType.Hair;
									break;
								case IAccessory acc:
									template.AccessoryType = acc.AccessoryType;
									template.CharacterObjectType = ECharacterObjectType.Accessory;
									template.DefaultParent = acc.DefaultParent;
									template.Reparentable = acc.Reparentable;

									for (var i = 0; i < defaultParents.Length; i++)
									{
										if (defaultParents[i] != acc.DefaultParent) 
											continue;
										
										template.DefaultParentIdx = i;
										break;
									}
									
									break;
								case ITexture tex:
									template.TextureType = tex.TextureType;
									template.Texture = tex.Texture;
									template.IsOverlay = tex.IsOverlay;
									template.OverlayTarget = tex.OverlayTarget;
									template.OverlayMode = tex.OverlayMode;
									template.OverlayColor = tex.OverlayColor;
									break;
							}
						}
						else if (comp is ModdedScene moddedScene)
						{
							template.TemplateType = ETemplateType.ModdedScene;
							template.Name = moddedScene.Name;
							template.Tags = moddedScene.Tags;
							template.Icon = moddedScene.Icon;
							template.Description = moddedScene.Description;
							template.IsNSFW = moddedScene.IsNSFW;
							template.ModdedSceneUsageFlags = moddedScene.UsageFlags;
							template.LargeBackground = moddedScene.LargeBackground;

							var parentObject = new GameObject(template.Name);
							go.transform.SetParent(parentObject.transform);
							
							removeUniqueID(go);
						}
						else if (comp is IAnimation animation)
						{
							template.TemplateType = ETemplateType.Animation;
							template.Name = animation.Name;
							template.Tags = animation.Tags;
							template.Icon = animation.Icon;
							template.Description = animation.Description;
							template.IsNSFW = animation.IsNSFW;
							template.AnimationUsageFlags = animation.UsageFlags;
							template.AnimationFadeDuration = animation.FadeDuration;
							template.AnimationClipContainers = animation.ClipContainers;
						}

						template.Tags ??= Array.Empty<string>();
						
						Templates.Add(template);
						Prefabs.Add(comp.GetType() == typeof(ModdedScene) ? null : go);
						
						break;
					}
				}

				removeUniqueIDs(Prefabs);

				FixAccessoryParents();
				
				for (var i = 0; i < BasicFolds.Length; i++)
					BasicFolds[i] = false;

				foreach (var template in Templates.Where(template => !BasicFolds[(int)template.TemplateType]))
					BasicFolds[(int)template.TemplateType] = true;

				Debug.Log("Loaded mod " + files[0]);

				if (adv)
				{
					// todo: retrieving type, includes, source code from Advanced mods
					Debug.LogWarning("Advanced tab of components that use it were not autofilled due to current limitations.");
				}

				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
			});
		}
		
		public void Build()
		{
			if (!CanBuild())
			{
				Debug.LogError("Failed build checks");
				return;
			}

			Thread.Sleep(100);
			EditorUtility.DisplayProgressBar("Building Mod..", "Cleaning up data", 0.1f);

			try
			{
				RemoveBrokenComponents();
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed removing broken components {e}");
			}
			
			Thread.Sleep(100);
			EditorUtility.DisplayProgressBar("Building Mod..", "Assigning Renderers", 0.3f);
			
			try
			{
				AssignRenderers();
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed assigning renderers {e}");
			}
			
			Thread.Sleep(100);
			EditorUtility.DisplayProgressBar("Building Mod..", "Setting up BVHlist", 0.4f);
			
			var bvhList = new List<int>();
			for (var i = 0; i < Templates.Count; i++)
			{
				var template = Templates[i];
				if (template.TemplateType != ETemplateType.CharacterObject || template.CharacterObjectType != ECharacterObjectType.Clothing || !template.UseClippingFix)
					continue;
				
				bvhList.Add(i);
			}

			for (var i = 0; i < bvhList.Count; i++)
			{
				var template = Templates[bvhList[i]];
				
				Thread.Sleep(100);
				EditorUtility.DisplayProgressBar("Building Mod..", $"Creating BVHs for {template.Name}", (float)((i + 1) / (float)bvhList.Count));

				try
				{
					var gameObject = (GameObject)Prefabs[bvhList[i]];
					var clothing = gameObject.GetComponent<IClothing>();

					var bvhDatas = new BVHDataGPU[template.ClothingStates.Length];
					for (var k = 0; k < bvhDatas.Length; k++)
					{
						var clothingState = template.ClothingStates[k];
						if (clothingState == null)
							continue;
					
						var renderers = clothingState.GetComponentsInChildren<SkinnedMeshRenderer>(true);
						if (renderers.Length == 0)
							continue;
					
						var raycaster = new Raycaster();

						foreach (var renderer in renderers)
							raycaster.AddMesh(renderer.sharedMesh, renderer.transform);

						raycaster.BuildBVH();

						var tuple = raycaster.BvhRoot.ToGPU();
						var bvhData = new BVHDataGPU
						{
							Nodes = tuple.Item1,
							Triangles = tuple.Item2
						};
					
						bvhDatas[k] = bvhData;
					}

					clothing.BVHData = bvhDatas.ToBytes();
				}
				catch (Exception e)
				{
					Debug.LogError($"Failed creating bvhs for {template.Name} {e}");
				}
			}
			
			var buildList = new List<Object>();
			foreach (var obj in Prefabs)
				buildList.AddUnique(obj);

			var currentScene = SceneManager.GetActiveScene();
			var scenes = new List<Scene>();

			Thread.Sleep(100);
			EditorUtility.DisplayProgressBar("Building Mod..", "Setting up Buildlist", 0.5f);

			try
			{
				for (var i = 0; i < buildList.Count; i++)
				{
					if (buildList[i] is not SceneAsset sceneAsset)
						continue;
				
					var path = AssetDatabase.GetAssetOrScenePath(sceneAsset);
					var moddedScene = currentScene;
					
					if (currentScene.path != path)
					{
						moddedScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
						scenes.Add(moddedScene);
					}

					var gameObject = moddedScene.GetRootGameObjects()[0];
					buildList[i] = gameObject;
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed setting up buildlist {e}");
			}
			
			Railgun.ModEngine.Conversion.Converter.OnContentDescriptorCreate += OnContentDescriptorCreate;

			Thread.Sleep(100);
			EditorUtility.DisplayProgressBar("Building Mod..", "Adding unique IDs", 0.75f);

			try
			{
				addUniqueIDs(buildList);
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed adding unique IDs {e}");
			}

			Thread.Sleep(100);
			EditorUtility.DisplayProgressBar("Building Mod..", "Setting up Mod", 0.9f);

			var modBuilder = ServiceProvider.Obtain<IModBuilderService>();
			modBuilder.Reset();
			
			modBuilder.SetManifest(Manifest);
			modBuilder.SetAssembly(!string.IsNullOrEmpty(Assembly.Item1) && Assembly.Item2 != null ? new Tuple<string, byte[]>(Assembly.Item1, Assembly.Item2) : null);
			modBuilder.SetSettings(null);
			
			Thread.Sleep(100);
			EditorUtility.DisplayProgressBar("Building Mod..", $"Building objects", 0f);

			for (var i = 0; i < buildList.Count; i++)
			{
				Thread.Sleep(100);
				EditorUtility.DisplayProgressBar("Building Mod..", $"Building object {buildList[i].name}", (float)((i + 1) / (float)buildList.Count));

				try
				{
					modBuilder.AddObject(buildList[i]);
				}
				catch (Exception e)
				{
					Debug.LogError($"Failed building object {buildList[i].name} {e}");
				}
			}

			Mod mod = null;
			
			Thread.Sleep(100);
			EditorUtility.DisplayProgressBar("Building Mod..", "Getting Mod Result", 0.1f);

			try
			{
				mod = modBuilder.GetMod();
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed getting mod result {e}");
			}
			
			Thread.Sleep(100);
			EditorUtility.DisplayProgressBar("Building Mod..", "Removing unique IDs", 0.3f);

			try
			{
				removeUniqueIDs(buildList);
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed removing unique IDs {e}");
			}

			Thread.Sleep(100);
			EditorUtility.DisplayProgressBar("Building Mod..", "Closing Scenes", 0.7f);

			try
			{
				foreach (var scene in scenes)
					EditorSceneManager.CloseScene(scene, true);

				SceneManager.SetActiveScene(currentScene);
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed closing scenes {e}");
			}
			
			Railgun.ModEngine.Conversion.Converter.OnContentDescriptorCreate -= OnContentDescriptorCreate;
			
			if (mod == null)
			{
				EditorUtility.ClearProgressBar();
				Debug.LogError("Failed building Mod");
				return;
			}
			
            Debug.Log($"Built mod {mod} with {mod.ModObjectsCount()} root mod objects and {mod.AssetsCount()} loose assets.");

            if (IsVerbose)
	            foreach(var tuple in mod.GetModObjects())
		            logBuildModObject(tuple.Item2);
			
			Thread.Sleep(100);
			EditorUtility.DisplayProgressBar("Building Mod..", "Writing Mod", 0.95f);

            StandaloneFileBrowser.SaveFilePanelAsync("Save Mod", "", $"pr.{Manifest.Author}.{Manifest.Name}.rgm", "rgm", delegate(string file)
			{
				if (string.IsNullOrEmpty(file))
				{
					EditorUtility.ClearProgressBar();
					return;
				}

				if (File.Exists(file))
				{
					Debug.Log($"Deleting existing file {file}");
					File.Delete(file);
				}
				
				try
				{
					if (ServiceProvider.Obtain<IModWriter>().WriteMod(file, mod))
						Debug.Log("Saved mod " + file);
					else
						Debug.LogError("Failed saving mod " + file);
				}
				catch (Exception e)
				{
					Debug.LogError("Failed saving mod " + file + "\n" + e);
				}
				
				EditorUtility.ClearProgressBar();
			});
		}
		
		public void AssignComponents()
		{
			if (!CanAssignComponents())
			{
				Debug.LogError("Failed component checks");
				return;
			}
			
			var asmName = $"pr.{Manifest.Author}.{Manifest.Name}.dll";
			var code = GetCompleteSource();

			// build assembly if there's any custom components
			BuildAssembly(asmName, code);

			// assign components to gameobjects
			for (var i = 0; i < Prefabs.Count; i++)
			{
				GameObject gameObject = null;

				var template = Templates[i];
				if (template.TemplateType != ETemplateType.ModdedScene)
					gameObject = (GameObject)Prefabs[i];
                
				if (template.TemplateType == ETemplateType.CharacterObject)
				{
					var existing = gameObject.GetComponents<ICharacterObject>();
					for (var k = existing.Length - 1; k >= 0; k--)
						DestroyImmediate((Component)existing[k]);
					
					Component comp;
					if (template.Advanced)
					{
						comp = AssignCustomComponent(gameObject, template.Type);
						if (comp == null)
						{
							Debug.LogError($"Type not found in generated assembly for {template.TemplateType.ToString()} {template.Name}");
							continue;
						}
					}
					else
					{
						switch (template.CharacterObjectType)
						{
							case ECharacterObjectType.Clothing:
								comp = gameObject.AddComponent<BaseClothing>();
								break;
							case ECharacterObjectType.Accessory:
								comp = gameObject.AddComponent<BaseAccessory>();
								break;
							case ECharacterObjectType.Hair:
								comp = gameObject.AddComponent<BaseHair>();
								break;
							case ECharacterObjectType.Texture:
								comp = gameObject.AddComponent<BaseTexture>();
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}

					if (comp is ICharacterObject charaObject)
					{
						charaObject.Name = template.Name;
						charaObject.Description = template.Description;
						charaObject.IsNSFW = template.IsNSFW;
						charaObject.SupportedGendersFlags = template.SupportedGendersFlags;
						charaObject.Icon = template.Icon;
						charaObject.Tags = template.Tags;
						charaObject.Simulation = template.Simulation;
					}
					
					switch (comp)
					{
						case IClothing cloth:
							cloth.ClothingType = template.ClothingType;
							cloth.ObjectType = ECharacterObjectType.Clothing;
							cloth.ClothingStates = new Transform[Enum.GetNames(typeof(EClothingState)).Length];
							cloth.BlendshapeOffsets = template.BlendshapeOffsets;

							for (var k = 0; k < template.ClothingStates.Length; k++)
								cloth.ClothingStates[k] = template.ClothingStates[k];

							cloth.ClippingDistance = template.ClippingDistance;
							break;
						case IHair hair:
							hair.HairType = template.HairType;
							hair.ObjectType = ECharacterObjectType.Hair;
							break;
						case IAccessory acc:
							acc.AccessoryType = template.AccessoryType;
							acc.ObjectType = ECharacterObjectType.Accessory;
							acc.DefaultParent = template.DefaultParent;
							acc.Reparentable = template.Reparentable;
							break;
						case ITexture tex:
							tex.TextureType = template.TextureType;
							tex.ObjectType = ECharacterObjectType.Texture;
							tex.Texture = template.Texture;
							tex.IsOverlay = template.IsOverlay;
							tex.OverlayTarget = template.OverlayTarget;
							tex.OverlayMode = template.OverlayMode;
							tex.OverlayColor = template.OverlayColor;
							break;
					}
				}
				else if (template.TemplateType == ETemplateType.StudioObject)
				{
					var existing = gameObject.GetComponents<IStudioObject>();
					for (var k = existing.Length - 1; k >= 0; k--)
						DestroyImmediate((Component)existing[k]);

					Component comp;
					if (template.Advanced)
					{
						comp = AssignCustomComponent(gameObject, template.Type);
						if (comp == null)
						{
							Debug.LogError($"Type not found in generated assembly for {template.TemplateType.ToString()} {template.Name}");
							continue;
						}
					}
					else
					{
						comp = gameObject.AddComponent<BaseStudioObject>();
					}
					
					if (comp is IStudioObject studioObject)
					{
						studioObject.StudioObjectType = template.StudioObjectType;
						studioObject.Name = template.Name;
						studioObject.Description = template.Description;
						studioObject.IsNSFW = template.IsNSFW;
						studioObject.Icon = template.Icon;
						studioObject.Tags = template.Tags;
						studioObject.FKData = template.FKData;
					}
				}
				else if (template.TemplateType == ETemplateType.ModdedScene)
				{
					var path = AssetDatabase.GetAssetOrScenePath((SceneAsset)Prefabs[i]);
					var currentScene = SceneManager.GetActiveScene();
					
					var loaded = currentScene.path == path;
					var moddedScene = loaded ? currentScene : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

					gameObject = moddedScene.GetRootGameObjects()[0];

					var existing = gameObject.GetComponents<ModdedScene>();
					for (var k = existing.Length - 1; k >= 0; k--)
						DestroyImmediate(existing[k]);

					var moddedSceneComp = gameObject.AddComponent<ModdedScene>();
					moddedSceneComp.Name = template.Name;
					moddedSceneComp.Description = template.Description;
					moddedSceneComp.IsNSFW = template.IsNSFW;
					moddedSceneComp.Icon = template.Icon;
					moddedSceneComp.Tags = template.Tags;
					moddedSceneComp.UsageFlags = template.ModdedSceneUsageFlags;
					moddedSceneComp.LargeBackground = template.LargeBackground;

					EditorSceneManager.SaveScene(moddedScene);
					
					if (!loaded)
					{
						EditorSceneManager.CloseScene(moddedScene, true);
						SceneManager.SetActiveScene(currentScene);
					}
				}
				else if (template.TemplateType == ETemplateType.Animation)
				{
					var existing = gameObject.GetComponents<IAnimation>();
					for (var k = existing.Length - 1; k >= 0; k--)
						DestroyImmediate((Component)existing[k]);

					var animation = gameObject.AddComponent<BaseAnimation>();
					animation.Name = template.Name;
					animation.Description = template.Description;
					animation.IsNSFW = template.IsNSFW;
					animation.Icon = template.Icon;
					animation.Tags = template.Tags;
					animation.UsageFlags = template.AnimationUsageFlags;
					animation.FadeDuration = template.AnimationFadeDuration;
					animation.ClipContainers = template.AnimationClipContainers;
				}

				if (template.TemplateType == ETemplateType.StudioObject)
				{
					var hasLODs = gameObject.transform.Find("LOD0") != null;
					if (!hasLODs) 
						continue;
					
					var lods = new List<LOD>();

					for (var k = 0; k < gameObject.transform.childCount; k++)
					{
						var child = gameObject.transform.GetChild(k);
						if (!lodRegex.IsMatch(child.name))
							continue;

						lods.Add(new LOD(1f / (lods.Count + 2), child.GetComponentsInChildren<Renderer>()));
					}

					if (lods.Count == 0)
						continue;
					
					if (lods.Count is > 0 and < 3)
					{
						Debug.LogError($"There must be either none, or at least 3 LODs for {template.TemplateType.ToString()} {template.Name}");
						continue;
					}
					
					var lodGroup = gameObject.GetComponent<LODGroup>();
					if (lodGroup == null)
						lodGroup = gameObject.AddComponent<LODGroup>();

					lodGroup.SetLODs(lods.ToArray());
					
					var lodHolder = gameObject.GetComponent<LODHolder>();
					if (lodHolder == null)
						lodHolder = gameObject.AddComponent<LODHolder>();
					
					lodHolder.LODGroup = lodGroup;

					Debug.Log($"Created LOD Group and assigned {lods.Count} valid LODs for {template.TemplateType.ToString()} {template.Name}");
				}
				else if (template.TemplateType == ETemplateType.CharacterObject)
				{
					var targets = new List<Transform>();

					if (template.CharacterObjectType is ECharacterObjectType.Clothing)
					{
						foreach (var clothingState in template.ClothingStates)
							targets.Add(clothingState);
					}
					else
					{
						targets.Add(gameObject.transform);
					}

					foreach (var target in targets)
					{
						if (target == null)
							continue;
						
						var lods = new List<LOD>();

						for (var k = 0; k < target.childCount; k++)
						{
							var child = target.GetChild(k);
							if (!lodRegex.IsMatch(child.name))
								continue;

							lods.Add(new LOD(1f / (lods.Count + 2), child.GetComponentsInChildren<Renderer>()));
						}
					
						if (lods.Count == 0)
							continue;
						
						if (lods.Count is > 0 and < 3)
						{
							Debug.LogError($"There must be either none, or at least 3 LODs for {template.TemplateType.ToString()} {template.Name}");
							continue;
						}
						
						var stateGameObject = target.gameObject;

						var lodGroup = stateGameObject.GetComponent<LODGroup>();
						if (lodGroup == null)
							lodGroup = stateGameObject.AddComponent<LODGroup>();
						
						lodGroup.SetLODs(lods.ToArray());
					
						var lodHolder = stateGameObject.GetComponent<LODHolder>();
						if (lodHolder == null)
							lodHolder = stateGameObject.AddComponent<LODHolder>();
					
						lodHolder.LODGroup = lodGroup;
						
						Debug.Log($"Created LOD Group and assigned {lods.Count} valid LODs for {template.TemplateType.ToString()} {template.Name} target {target.name}");
					}
				}
			}

			// cache and import assembly if there's any custom components
			CacheAssembly(asmName, code);
			
			Debug.Log("Assigned components");
		}

		public void AssignRenderers()
		{
			for (var i = 0; i < Templates.Count; i++)
			{
				var template = Templates[i];
				
				var prefab = Prefabs[i];
				if (prefab == null)
				{
					Debug.LogWarning($"Failed assigning renderers for {template.Name} because it has no object assigned");
					continue;
				}

				switch (template.TemplateType)
				{
					case ETemplateType.CharacterObject:
					{
						var characterObject = ((GameObject)prefab).GetComponent<ICharacterObject>();
						characterObject.Renderers = characterObject.GetGameObject().GetComponentsInChildren<Renderer>();
						break;
					}
					case ETemplateType.StudioObject:
					{
						var studioObject = ((GameObject)prefab).GetComponent<IStudioObject>();
						studioObject.Renderers = studioObject.GetGameObject().GetComponentsInChildren<Renderer>();
						break;
					}
				}
			}
		}
		
		public bool CanBuild()
		{
			var pass = true;
			var checkAsm = true;

			if (string.IsNullOrEmpty(Manifest.Name))
			{
				pass = false;
				Debug.LogWarning("Mod name is not set");
			}
			
			if (string.IsNullOrEmpty(Manifest.Author))
			{
				pass = false;
				Debug.LogWarning("Mod author is not set");
			}
			
			if (string.IsNullOrEmpty(Manifest.Version))
			{
				pass = false;
				Debug.LogWarning("Mod version is not set");
			}
			
			if (Prefabs.Count == 0 || Templates.Count == 0)
			{
				pass = false;
				Debug.LogWarning("There are no objects or components set");
			}
			
			if (Prefabs.Count != Templates.Count)
			{
				pass = false;
				Debug.LogWarning($"Objects {Prefabs.Count} and Components {Templates.Count} count mismatch");
			}
			
			for (var i = 0; i < Templates.Count; i++)
			{
				var template = Templates[i];
				var prefab = Prefabs[i];

				AdvancedBuildCheck(template, ref checkAsm, ref pass);

				if (prefab != null)
				{
					if (template.TemplateType == ETemplateType.ModdedScene)
					{
						var path = AssetDatabase.GetAssetOrScenePath((SceneAsset)Prefabs[i]);
						var currentScene = SceneManager.GetActiveScene();
					
						var loaded = currentScene.path == path;
						var moddedScene = loaded ? currentScene : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

						var sceneComp = moddedScene.GetRootGameObjects()[0].GetComponent<ModdedScene>();
						if (sceneComp == null)
						{
							pass = false;
							Debug.LogWarning($"Component is not added for {template.Name}");
						}
						else
						{
							var anims = sceneComp.GetComponentsInChildren<BaseAnimation>(true);
							foreach (var anim in anims)
							{
								if (anim.ClipContainers.Length == 0)
								{
									pass = false;
									Debug.LogWarning($"Containers not assigned for {anim.name} in scene {template.Name}");
								}

								for (var k = 0; k < anim.ClipContainers.Length; k++)
								{
									var container = anim.ClipContainers[k];
									if (container.Clips.Length == 0)
									{
										pass = false;
										Debug.LogWarning($"No clips specified for {anim.name} container {k} in scene {template.Name}");
									}

									for (var l = 0; l < container.Clips.Length; l++)
									{
										if (container.Clips[l] == null)
										{
											pass = false;
											Debug.LogWarning($"Invalid clips found in {anim.name} container {k} in scene {template.Name}");
										}
									}
								}
							}
						}
					
						if (!loaded)
						{
							EditorSceneManager.CloseScene(moddedScene, true);
							SceneManager.SetActiveScene(currentScene);
						}
						
						continue;
					}
					
					var gameObject = (GameObject)prefab;
					
					if ((template.TemplateType == ETemplateType.CharacterObject && gameObject.GetComponent<ICharacterObject>() == null) || (template.TemplateType == ETemplateType.StudioObject && gameObject.GetComponent<IStudioObject>() == null))
					{
						pass = false;
						Debug.LogWarning($"Component is not added for {template.Name}");
					}

					if (template.TemplateType == ETemplateType.CharacterObject && template.CharacterObjectType == ECharacterObjectType.Clothing)
					{
						var clothingObject = gameObject.GetComponent<IClothing>();
						if (clothingObject != null)
						{
							var fullState = clothingObject.GetStateObject(EClothingState.Full);
							if (fullState == null)
							{
								pass = false;
								Debug.LogWarning($"Full state missing for {template.Name}");
							}
							else if (fullState.GetComponentInChildren<SkinnedMeshRenderer>() == null)
							{
								pass = false;
								Debug.LogWarning($"Skinned mesh renderer missing for full state of {template.Name}. Make sure your mesh has an Armature");
							}

							var halfState = clothingObject.GetStateObject(EClothingState.Half);
							if (halfState == null)
							{
								Debug.LogWarning($"Optional half state missing for {template.Name}");
							}
							else if (halfState.GetComponentInChildren<SkinnedMeshRenderer>() == null)
							{
								pass = false;
								Debug.LogWarning($"Skinned mesh renderer missing for half state of {template.Name}. Make sure your mesh has an Armature");
							}

							if (fullState == halfState && fullState != null)
							{
								pass = false;
								Debug.LogWarning($"Full and half states of {template.Name} are the same object. Make sure they are separate objects or unassign the half state");
							}
						}
					}

					if (template.TemplateType == ETemplateType.CharacterObject && template.CharacterObjectType == ECharacterObjectType.Texture)
					{
						var texture = gameObject.GetComponent<ITexture>();
						if (texture.Texture == null)
						{
							pass = false;
							Debug.LogWarning($"Texture missing for {template.Name}");
						}

						var components = gameObject.GetComponents<Component>().ToList();
						components.Remove(gameObject.transform);
						components.Remove((Component)texture);

						if (gameObject.transform.childCount == 0 && components.Count == 0) 
							continue;
						
						pass = false;
						Debug.LogWarning($"Object for {template.Name} is not empty. Texture mods must have empty GameObjects assigned with no meshes");
					}

					if (template.TemplateType == ETemplateType.Animation)
					{
						var animation = gameObject.GetComponent<IAnimation>();
						if (animation.ClipContainers.Length == 0)
						{
							pass = false;
							Debug.LogWarning($"Containers not assigned for {template.Name}");
						}

						for (var k = 0; k < animation.ClipContainers.Length; k++)
						{
							var container = animation.ClipContainers[k];
							if (container.Clips.Length == 0)
							{
								pass = false;
								Debug.LogWarning($"No clips specified for container {k} in {template.Name}");
							}

							for (var l = 0; l < container.Clips.Length; l++)
							{
								if (container.Clips[l] == null)
								{
									pass = false;
									Debug.LogWarning($"Invalid clips found in container {k} in {template.Name}");
								}
							}
						}
					}

					var anySkinnedRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
					if (anySkinnedRenderers != null && anySkinnedRenderers.Length != 0)
					{
						var anyBones = false;
						
						foreach (var anySkinnedRenderer in anySkinnedRenderers)
						{
							if (anySkinnedRenderer.bones != null && anySkinnedRenderer.bones.Length > 0)
							{
								anyBones = true;
								break;
							}
						}

						if (anyBones)
						{
							var armature = gameObject.transform.Find("Armature");
							if (armature == null)
							{
								pass = false;
								Debug.LogWarning($"Armature for {template.Name} was not found. Skinned meshes must have an armature at the root of the object, with the name of \"Armature\"");
							}
						}
					}
				}
				else
				{
					pass = false;
					Debug.LogWarning($"Object is not set for {template.Name}");
				}
			}

			for (var i = 0; i < Prefabs.Count; i++)
			{
				for (var k = 0; k < Prefabs.Count; k++)
				{
					if (i == k || Prefabs[i] == null || Prefabs[k] == null)
						continue;

					if (Prefabs[i] != Prefabs[k]) 
						continue;
					
					pass = false;
					Debug.LogWarning($"Items {Templates[i].Name} and {Templates[k].Name} share the same object. This is not allowed");
				}
			}
			
			return pass;
		}

		public bool CanAssignComponents()
		{
			var pass = true;

			if (Prefabs.Count == 0 || Templates.Count == 0)
			{
				pass = false;
				Debug.LogWarning("There are no objects or components set");
			}
			
			if (Prefabs.Count != Templates.Count)
			{
				pass = false;
				Debug.LogWarning($"Objects {Prefabs.Count} and Components {Templates.Count} count mismatch");
			}

			for (var i = 0; i < Templates.Count; i++)
			{
				var template = Templates[i];
				
				if (Prefabs[i] == null)
				{
					pass = false;
					Debug.LogWarning($"Object is not set for {template.Name}");
				}
				
				if (string.IsNullOrEmpty(template.Name))
				{
					pass = false;
					Debug.LogWarning($"Name is not set for {template.Name}");
				}
				
				if (template.Icon == null)
				{
					pass = false;
					Debug.LogWarning($"Icon is not set for {template.Name}");
				}
				
				if (template.TemplateType == ETemplateType.ModdedScene && template.LargeBackground == null)
				{
					pass = false;
					Debug.LogWarning($"Large Background is not set for {template.Name}");
				}
				
				if (template.TemplateType == ETemplateType.ModdedScene && Prefabs[i] != null)
				{
					if (template.ModdedSceneUsageFlags == 0)
						Debug.LogWarning($"Modded Scene usage is empty for {template.Name}");

					var path = AssetDatabase.GetAssetOrScenePath((SceneAsset)Prefabs[i]);
					var currentScene = SceneManager.GetActiveScene();
					
					var loaded = currentScene.path == path;
					var moddedScene = loaded ? currentScene : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

					var objects = moddedScene.GetRootGameObjects();
					switch (objects.Length)
					{
						case 0:
						{
							pass = false;
							Debug.LogWarning($"Root object 'ModdedScene' not found for {template.Name}");
							break;
						}
						case 1:
						{
							if (objects[0].name != "ModdedScene")
							{
								pass = false;
								Debug.LogWarning($"Root object 'ModdedScene' not found for {template.Name}");
							}
							break;
						}
						case > 1:
						{
							pass = false;
							Debug.LogWarning($"Multiple root objects found for {template.Name}");
							break;
						}
					}

					if (!loaded)
					{
						EditorSceneManager.CloseScene(moddedScene, true);
						SceneManager.SetActiveScene(currentScene);
					}
				}

				if (template.TemplateType == ETemplateType.CharacterObject && template.SupportedGendersFlags == ESupportedGendersFlags.None)
					Debug.LogWarning($"Supported genders is None for {template.Name}. This might cause the item to not show up");

				if (template.TemplateType == ETemplateType.Animation && template.AnimationUsageFlags == EAnimationUsageFlags.None)
					Debug.LogWarning($"Usage flags is None for {template.Name}. This might cause the animation to not show up");

				if (template.FKData.Groups != null)
				{
					foreach (var group in template.FKData.Groups)
					{
						if (group.Transforms == null || group.Transforms.Length == 0)
						{
							pass = false;
							Debug.LogWarning($"FK Group {group.Name} for {template.Name} does not have any transforms");
							
							continue;
						}
						
						foreach (var transform in group.Transforms)
						{
							if (transform.Transform != null) 
								continue;
							
							pass = false;
							Debug.LogWarning($"FK Group {group.Name} transform {transform.Name} for {template.Name} is null");
							
							break;
						}
					}
				}

				AdvancedAssignCheck(template, ref pass);
			}

			for (var i = 0; i < Prefabs.Count; i++)
			{
				for (var k = 0; k < Prefabs.Count; k++)
				{
					if (i == k || Prefabs[i] == null || Prefabs[k] == null)
						continue;

					if (Prefabs[i] != Prefabs[k]) 
						continue;
					
					pass = false;
					Debug.LogWarning($"Items {Templates[i].Name} and {Templates[k].Name} share the same object. This is not allowed");
				}
			}

			return pass;
		}
		
		public ContentDescriptor OnContentDescriptorCreate(Component component, ContentDescriptor contentDescriptor)
		{
			try
			{
				switch (component)
				{
					case IStudioObject studioObject:
						contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("studioobjecttype", studioObject.StudioObjectType)).ToArray();
						contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("nsfw", studioObject.IsNSFW)).ToArray();
						break;
					case ICharacterObject characterObject:
						switch (characterObject)
						{
							case IClothing clothing:
								contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("clothingtype", clothing.ClothingType)).ToArray();
								break;
							case IHair hair:
								contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("hairtype", hair.HairType)).ToArray();
								break;
							case IAccessory accessory:
								contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("accessorytype", accessory.AccessoryType)).ToArray();
								break;
							case ITexture texture:
								contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("texturetype", texture.TextureType)).ToArray();
								break;
						} 
						contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("supportedgenders", characterObject.SupportedGendersFlags)).ToArray();
						contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("nsfw", characterObject.IsNSFW)).ToArray();
						break;
					case ModdedScene moddedScene:
						contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("moddedscene", moddedScene.UsageFlags)).ToArray();
						contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("nsfw", moddedScene.IsNSFW)).ToArray();
						contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("largebackground", moddedScene.LargeBackground)).ToArray();
						break;
					case IAnimation animation:
						
						var clips = new EClipUsageFlags[animation.ClipContainers.Length];
					
						for (var i = 0; i < clips.Length; i++)
							clips[i] = animation.ClipContainers[i].ClipUsageFlags;

						contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("animation", animation.UsageFlags)).ToArray();
						contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("nsfw", animation.IsNSFW)).ToArray();
						contentDescriptor.OptionalData = contentDescriptor.OptionalData.Append(new KeyValue("clips", clips)).ToArray();
						break;
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed setting up ContentDescriptor {e}");
			}
			
			return contentDescriptor;
		}

		private void addUniqueIDs(List<Object> objects)
		{
			var id = 0;
			foreach (var go in objects)
			{
				foreach (var transform in ((GameObject)go).GetComponentsInChildren<Transform>(true))
				{
					transform.name = transform.name + uniqueIDFormat + id;
					id++;
				}

				id = 0;
			}
		}

		private void removeUniqueIDs(List<Object> objects)
		{
			foreach (var go in objects.Where(go => go != null))
			{
				removeUniqueID(go);
			}
		}
		
		private void removeUniqueID(Object obj)
		{
			foreach (var transform in ((GameObject)obj).GetComponentsInChildren<Transform>(true))
			{
				var index = transform.name.LastIndexOf(uniqueIDFormat, StringComparison.InvariantCultureIgnoreCase);
				if (index == -1)
					continue;

				transform.name = transform.name[..index];
			}
		}
		
		private void verticalList(List<Object> list, ETemplateType type, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(labelTitle + itemsCount(Templates.Count(t => t.TemplateType == type)));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
			{
				if (Templates.Count < 254)
				{
					list.Add(new Object());
					Templates.Add(new Template { TemplateType = type });

					CurrentTemplate = -1;
				}
				else
				{
					Debug.LogWarning("More than 254 modded objects in a single mod is not supported");
				}
			}
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
			{
				for (var i = Templates.Count - 1; i >= 0; i--)
				{
					if (Templates[i].TemplateType != type) 
						continue;
					
					list.RemoveAt(i);
					Templates.RemoveAt(i);
				}

				CurrentTemplate = -1;
			}
			GUILayout.EndHorizontal();

			var templates = Templates.Where(t => t.TemplateType == type).ToArray();
			
			for (var i = 0; i < templates.Length; i++)
			{
				var current = Templates.FindIndex(t => t == templates[i]);

				GUILayout.BeginHorizontal();
				
				if (type == ETemplateType.ModdedScene)
					list[current] = EditorGUILayout.ObjectField("", list[current], typeof(SceneAsset), false, GUILayout.Width(250));
				else
					list[current] = EditorGUILayout.ObjectField("", list[current], typeof(GameObject), true, GUILayout.Width(250));

				if (list[current] != null && type != ETemplateType.ModdedScene)
				{
					var go = (GameObject)list[current];
					if (go.scene != SceneManager.GetActiveScene())
						list[current] = null;
				}
				
				if (GUILayout.Button((CurrentTemplate == current ? "*" : "") + "" + Templates[current].Name))
					CurrentTemplate = current;
				
				if (GUILayout.Button("/\\", GUILayout.Width(25)))
				{
					if (i == 0)
						return;

					var target = Templates.FindIndex(t => t == templates[i - 1]);
					
					(Templates[current], Templates[target]) = (Templates[target], Templates[current]);
					(list[current], list[target]) = (list[target], list[current]);

					if (CurrentTemplate == current)
						CurrentTemplate = target;
					else if (CurrentTemplate == target)
						CurrentTemplate = current;
				}
				
				if (GUILayout.Button("\\/", GUILayout.Width(25)))
				{
					if (i == templates.Length - 1)
						return;
					
					var target = Templates.FindIndex(t => t == templates[i + 1]);
					
					(Templates[current], Templates[target]) = (Templates[target], Templates[current]);
					(list[current], list[target]) = (list[target], list[current]);
					
					if (CurrentTemplate == current)
						CurrentTemplate = target;
					else if (CurrentTemplate == target)
						CurrentTemplate = current;
				}
				
				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					list.RemoveAt(i);
					Templates.Remove(templates[i]);
					
					CurrentTemplate = -1;
					return;
				}
				GUILayout.EndHorizontal();
			}
		}
		
		private void verticalList(List<string> list, string labelTitle)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(labelTitle + itemsCount(list.Count));
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				list.Add("");
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
				list.Clear();
			GUILayout.EndHorizontal();
			
			for (var i = 0; i < list.Count; i++)
			{
				GUILayout.BeginHorizontal();
				list[i] = EditorGUILayout.TextField("", list[i]);
				if (GUILayout.Button("-", GUILayout.Width(25)))
					list.RemoveAt(i);
				GUILayout.EndHorizontal();
			}
		}
		
		private void logBuildModObject(ModObject modObject)
		{
            Debug.Log($"-----------BEGIN MODOBJECT {modObject.Name}------------");
            Debug.Log($"{modObject.Name} contains {modObject.Children.Count} children and {modObject.Components.Count} components.");

			if(modObject.Components.Count > 0)
			{
                Debug.Log($"-----------BEGIN COMPONENTS {modObject.Name}------------");
                foreach (var comp in modObject.Components)
                {
                    Debug.Log($"{comp.Type.Item1} contains {comp.ChildrenLinks.Count} links to other components, {comp.TypeLinks.Count} links to custom types, {comp.AssetLinks.Count} links to assets and {comp.ArrayLinks.Count} links to arrays.");

					if(comp.TypeLinks.Count > 0)
					{
                        Debug.Log($"-----------BEGIN DUMP TYPELINKS {modObject.Name}------------");
                        foreach (KeyValue kv in comp.TypeLinks)
                        {
                            if (kv.Value is IAssetPipelineType arr)
                            {
                                Debug.Log($"Key {kv.Key}, Value {arr.GetType()}");
                            }
                        }
                        Debug.Log($"-----------END DUMP TYPELINKS {modObject.Name}------------");
                    }

                    if (comp.AssetLinks.Count > 0)
                    {
                        Debug.Log($"-----------BEGIN DUMP ASSETLINKS {modObject.Name}------------");
                        foreach (KeyValue kv in comp.AssetLinks)
                        {
                            if (kv.Value is Asset arr)
                            {
                                Debug.Log($"Key {kv.Key}, Value {arr.GetType()}");
                            }
                        }
                        Debug.Log($"-----------END DUMP ASSETLINKS {modObject.Name}------------");
                    }

                    if (comp.ChildrenLinks.Count > 0)
                    {
                        Debug.Log($"-----------BEGIN DUMP CHILDRENLINKS {modObject.Name}------------");
                        foreach (KeyValue kv in comp.ChildrenLinks)
                        {
                            if (kv.Value is Array arr)
                            {
                                Debug.Log($"Key {kv.Key}, Value {arr.GetValue(0)} {arr.GetValue(1)}");
                            }
                        }
                        Debug.Log($"-----------END DUMP CHILDRENLINKS {modObject.Name}------------");
                    }

                    if (comp.ArrayLinks.Count > 0)
                    {
                        Debug.Log($"-----------BEGIN DUMP ARRAYLINKS {modObject.Name}------------");
                        foreach (KeyValue kv in comp.ArrayLinks)
                        {
							if(kv.Value is Array arr)
							{
                                Debug.Log($"Key {kv.Key}, Value {arr.GetValue(0)} {arr.GetValue(1)}");
                            }
                        }
                        Debug.Log($"-----------END DUMP ARRAYLINKS {modObject.Name}------------");
                    }
                }
                Debug.Log($"-----------END COMPONENTS {modObject.Name}------------");
            }
            
			if(modObject.Children.Count > 0)
			{
                Debug.Log($"-----------BEGIN CHILDREN OF {modObject.Name}------------");
                foreach (var child in modObject.Children)
                {
                    logBuildModObject(child);
                }
                Debug.Log($"-----------END CHILDREN OF {modObject.Name}------------");
            }
            Debug.Log($"-----------END MODOBJECT {modObject.Name}------------");
        }
	}
}