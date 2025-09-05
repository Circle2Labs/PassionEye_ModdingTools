using System;
using System.Collections.Generic;
using System.Linq;
using Code.Frameworks.Animation.Enums;
using Code.EditorScripts.ModCreator;
using Code.Frameworks.Animation.Structs;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.ForwardKinematics.Structs;
using Code.Frameworks.ModdedScenes.Flags;
using Code.Frameworks.PhysicsSimulation;
using Code.Frameworks.Studio.Enums;
using UnityEditor;
using UnityEngine;
using Code.Frameworks.Character.Structs;
using Code.Frameworks.ClippingFix.Enums;
using Code.Frameworks.InteractionSystem.Interactions.Base.H.Enums;
using Tuple = Code.Tools.Tuple;

namespace Code.Editor.ModEngine
{
    public partial class ModCreator 
	{
		[SerializeField]
		public bool[] BasicFolds = new bool[4];
		
		// todo: figure out how to localize enum flags field

		private EPreset selectedPhysicsPreset;
		
		private Vector2 basicScrollPosition;

		private bool bodyPartsFold;
		private bool accessoryParentsFold;
		private bool blendshapesFold;
		private bool blendshapePresetsFold;
		
		private bool blendshapeOffsetsFold;
		private bool physicsSimulationFold;
		private bool forwardKinematicsFold;
		
		public void DrawBasic()
		{
			if (Templates == null || Templates.Count == 0)
				return;
			
			var template = Templates[CurrentTemplate];
			
			basicScrollPosition = GUILayout.BeginScrollView(basicScrollPosition);
			
			if (template.TemplateType == ETemplateType.CharacterObject)
			{
				EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_COMPCONF"), EditorStyles.boldLabel);
				template.CharacterObjectType = (ECharacterObjectType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_TYPE")}*", template.CharacterObjectType, "MODCREATOR_BASIC_TYPE_");

				switch (template.CharacterObjectType)
				{
					case ECharacterObjectType.Hair:
						template.HairType = (EHairType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_HAIRSLOT")}*", template.HairType, "MODCREATOR_BASIC_HAIRSLOT_");
						
                        GUILayout.Space(2);

						// todo: hide until you can actually reparent hair
						/*if(template.Simulation.BoneAssignmentMode == EBoneAssignmentMode.AttachOnly)
						{
                            template.Reparentable = EditorGUILayout.ToggleLeft($"{GetLocalizedString("MODCREATOR_BASIC_REPARENTABLE")}*", template.Reparentable);
                            if (template.Reparentable)
                            {
                                template.DefaultParentIdx = EditorGUILayout.Popup($"{GetLocalizedString("MODCREATOR_BASIC_PARENT")}*", template.DefaultParentIdx, defaultParents);
                                template.DefaultParent = defaultParents[template.DefaultParentIdx];
                            }
                        }*/
						template.Reparentable = false;

                        GUILayout.Space(5);

						physicsSimulation(template);
						break;
					case ECharacterObjectType.Clothing:
						template.ClothingType = (EClothingType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_CLOTHSLOT")}*", template.ClothingType, "MODCREATOR_BASIC_CLOTHSLOT_");

						GUILayout.Space(10);
						
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSTATE"), EditorStyles.boldLabel);
						
						for (var i = 0; i < template.ClothingStates.Length; i++)
						{
							var stateEnum = (EClothingState)i;
							if (stateEnum == EClothingState.Off)
								continue;

							var state = $"{GetLocalizedString($"MODCREATOR_BASIC_CLOTHSTATE_{stateEnum.ToString().ToUpper()}")} state{(stateEnum == EClothingState.Full ? "*" : "")}";
							var obj = (Transform)EditorGUILayout.ObjectField(state, template.ClothingStates[i], typeof(Transform), true);
							
							template.ClothingStates[i] = obj;
						}

						GUILayout.Space(10);

						clippingFix(template);
						
						if (!IsStandalone)
						{
							GUILayout.Space(5);

							blendshapeOffsets(template);
						}

                        GUILayout.Space(2);

						// todo: hide until you can actually reparent and offset clothing
                        /*if (template.Simulation.BoneAssignmentMode == EBoneAssignmentMode.AttachOnly)
                        {
                            template.Reparentable = EditorGUILayout.ToggleLeft($"{GetLocalizedString("MODCREATOR_BASIC_REPARENTABLE")}*", template.Reparentable);
                            if (template.Reparentable)
                            {
                                template.DefaultParentIdx = EditorGUILayout.Popup($"{GetLocalizedString("MODCREATOR_BASIC_PARENT")}*", template.DefaultParentIdx, defaultParents);
                                template.DefaultParent = defaultParents[template.DefaultParentIdx];
                            }
                        }*/
						template.Reparentable = false;

                        GUILayout.Space(5);
						
						physicsSimulation(template);
						break;
					case ECharacterObjectType.Accessory:
						template.AccessoryType = (EAccessoryType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_ACCSLOT")}*", template.AccessoryType, "MODCREATOR_BASIC_ACCSLOT_");
						
						GUILayout.Space(2);

						template.Reparentable = EditorGUILayout.ToggleLeft($"{GetLocalizedString("MODCREATOR_BASIC_REPARENTABLE")}*", template.Reparentable);
						if (template.Reparentable)
						{
							var accessoryParents = new List<string>();
			
							var compatibleBaseMeshes = template.CompatibleBaseMeshes;
							if (compatibleBaseMeshes != null && compatibleBaseMeshes.Length > 0)
							{
								var baseMesh = compatibleBaseMeshes[0];
								accessoryParents = GetAccessoryParents(new Tuple<string, byte>(baseMesh.GUID, baseMesh.ID));
							}
							
							template.DefaultParentIdx = EditorGUILayout.Popup($"{GetLocalizedString("MODCREATOR_BASIC_PARENT")}*", template.DefaultParentIdx, accessoryParents.ToArray());
							
							if (template.DefaultParentIdx > accessoryParents.Count - 1 || template.DefaultParentIdx < 0)
								template.DefaultParent = "";
							else
								template.DefaultParent = accessoryParents[template.DefaultParentIdx];
						}
						
						GUILayout.Space(5);
						
						physicsSimulation(template);

						if (template.Simulation.BoneAssignmentMode == EBoneAssignmentMode.Full)
							template.Reparentable = false;
						break;
					case ECharacterObjectType.Texture:
						template.TextureType = (ETextureType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_TEXTYPE")}*", template.TextureType, "MODCREATOR_BASIC_TEXTYPE_");

						if (template.TextureType is ETextureType.BodyOverlay or ETextureType.Nipples)
						{
							template.OverlayTarget = EOverlayTarget.Body;
							template.OverlayMode = EOverlayMode.FullTexture;
							template.IsOverlay = true;
						}
						else if (template.TextureType is ETextureType.FaceOverlay or ETextureType.Lips or ETextureType.Blush or ETextureType.Nose)
						{
							template.OverlayTarget = EOverlayTarget.Face;
							template.OverlayMode = EOverlayMode.FullTexture;
							template.IsOverlay = true;
						}
						else
						{
							template.IsOverlay = false;
						}

						if (template.IsOverlay)
							template.OverlayColor = EditorGUILayout.ColorField($"{GetLocalizedString("MODCREATOR_BASIC_OVERLAYCOLOR")}*", template.OverlayColor);
						
						GUILayout.BeginHorizontal();
						template.Texture = (Texture2D)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_TEX")}*", template.Texture, typeof(Texture2D), false, GUILayout.Width(128 + 150), GUILayout.Height(128));
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
						break;
					case ECharacterObjectType.BaseMesh:
						template.Avatar = (Avatar)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_AVATAR")}*", template.Avatar, typeof(Avatar), false);
						template.SupportedGendersFlags = (ESupportedGendersFlags)EditorGUILayout.EnumFlagsField($"{GetLocalizedString("MODCREATOR_BASIC_GENDERS")}*", template.SupportedGendersFlags);
						
						GUILayout.Space(5);

						if (template.TextureMaterialMap == null || template.TextureMaterialMap.Count != 2)
							template.TextureMaterialMap = new List<Tuple.SerializableTuple<ETextureType, Renderer, int>> { new (ETextureType.FaceSkin, null, 0), new (ETextureType.BodySkin, null, 0) };

						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_FACE"));
						var faceTextureMap = template.TextureMaterialMap[0];
						
						GUILayout.BeginHorizontal();
						faceTextureMap.Item2 = (Renderer)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_RENDERER")}*", faceTextureMap.Item2, typeof(Renderer), true);
						faceTextureMap.Item3 = EditorGUILayout.IntField($"{GetLocalizedString("MODCREATOR_BASIC_SUBMESH")}*", faceTextureMap.Item3, GUILayout.Width(200));
						GUILayout.EndHorizontal();
						
						template.TextureMaterialMap[0] = faceTextureMap;
						
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_BODY"));
						var bodyTextureMap = template.TextureMaterialMap[1];
						
						GUILayout.BeginHorizontal();
						bodyTextureMap.Item2 = (Renderer)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_RENDERER")}*", bodyTextureMap.Item2, typeof(Renderer), true);
						bodyTextureMap.Item3 = EditorGUILayout.IntField($"{GetLocalizedString("MODCREATOR_BASIC_SUBMESH")}*", bodyTextureMap.Item3, GUILayout.Width(200));
						GUILayout.EndHorizontal();

						template.TextureMaterialMap[1] = bodyTextureMap;
						
						GUILayout.Space(5);
						
						var eyeData = template.EyeData;
						eyeData.BlinkBlendShapeLeft = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BASIC_EYEDATA_BLINKSHAPE_LEFT")}*", eyeData.BlinkBlendShapeLeft);
						eyeData.BlinkBlendShapeRight = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BASIC_EYEDATA_BLINKSHAPE_RIGHT")}*", eyeData.BlinkBlendShapeRight);
						template.EyeData = eyeData;
						
						GUILayout.Space(5);
						
						var mouthData = template.MouthData;
						mouthData.OpenBlendShape = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BASIC_MOUTHDATA_OPENSHAPE")}*", mouthData.OpenBlendShape);
						template.MouthData = mouthData;
						
						GUILayout.Space(5);
						
						template.POV = (Transform)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_POVTR")}*", template.POV, typeof(Transform), true);
						template.FaceTransform = (Transform)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_FACETR")}*", template.FaceTransform, typeof(Transform), true);

						GUILayout.Space(5);
						
						template.Cock = (Transform)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_COCKBONE")}*", template.Cock, typeof(Transform), true);
						template.BodyRootBone = (Transform)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_BODYROOTBONE")}*", template.BodyRootBone, typeof(Transform), true);
						template.HeadRootBone = (Transform)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_HEADROOTBONE")}*", template.HeadRootBone, typeof(Transform), true);
						template.PrivatesRootBone = (Transform)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_PRIVATESROOTBONE")}*", template.PrivatesRootBone, typeof(Transform), true);

						GUILayout.Space(5);

						template.Eyes ??= new List<Transform>();
						template.Breasts ??= new List<Transform>();
						template.Buttocks ??= new List<Transform>();
						
						template.MergedEyes = (Transform)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_MERGEDEYES")}*", template.MergedEyes, typeof(Transform), true);
						template.InvertMergedEyes = EditorGUILayout.ToggleLeft($"{GetLocalizedString("MODCREATOR_BASIC_INVERTMERGEDEYES")}", template.InvertMergedEyes);
						
						verticalList(template.Eyes, $"{GetLocalizedString("MODCREATOR_BASIC_EYES")}*");
						verticalList(template.Breasts, GetLocalizedString("MODCREATOR_BASIC_BREASTS"));
						verticalList(template.Buttocks, GetLocalizedString("MODCREATOR_BASIC_BUTTOCKS"));
						
						template.BlendshapeRenderers ??= new List<SkinnedMeshRenderer>();
						template.SFWColliders ??= new List<Collider>();

						verticalList(template.SFWColliders, $"{GetLocalizedString("MODCREATOR_BASIC_SFWCOLLIDERS")}*");
						verticalList(template.BlendshapeRenderers, $"{GetLocalizedString("MODCREATOR_BASIC_SHAPERENDERERS")}*");
						
						GUILayout.BeginHorizontal();
						
						GUI.enabled = Prefabs[Templates.IndexOf(template)] != null;
						var autofillBlendshapeRenderers = GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_AUTOFILL"));
						GUI.enabled = true;
						
						if (autofillBlendshapeRenderers)
						{
							template.BlendshapeRenderers.Clear();

							var prefab = (GameObject)Prefabs[Templates.IndexOf(template)];
							var renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
							
							foreach (var rend in renderers)
								template.BlendshapeRenderers.Add(rend);
						}
						
						if (GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_REMOVEINVALID")))
						{
							for (var i = template.BlendshapeRenderers.Count - 1; i >= 0; i--)
							{
								var rend = template.BlendshapeRenderers[i];
								if (rend == null)
								{
									template.BlendshapeRenderers.RemoveAt(i);
									Debug.Log($"Removed index {i}");
									continue;
								}

								if (!rend.name.StartsWith("LOD") || rend.name.EndsWith("0"))
									continue;

								template.BlendshapeRenderers.RemoveAt(i);
								Debug.Log($"Removed {rend.name}");
							}
						}
						
						GUILayout.EndHorizontal();
						
						bodyParts(template);
						accessoryParents(template);
						blendshapes(template);
						blendshapePresets(template);
						
						forwardKinematics(template);

						if (!IsStandalone && GUILayout.Button("Read from selected"))
						{
							var selected = Selection.activeGameObject;
							if (selected != null && selected.TryGetComponent<IBaseMesh>(out var baseMesh))
							{
								template.TemplateType = ETemplateType.CharacterObject;
								template.CharacterObjectType = ECharacterObjectType.BaseMesh;
								template.Icon = baseMesh.Icon;
								template.Name = baseMesh.Name;
								template.Description = baseMesh.Description;
								template.Tags = baseMesh.Tags;
								template.IsNSFW = baseMesh.IsNSFW;
								
								template.Simulation = baseMesh.Simulation;
								template.FKData = baseMesh.FKData;
								template.CompatibleBaseMeshes = baseMesh.CompatibleBaseMeshes;
								
								template.Avatar = baseMesh.Avatar;
								template.SupportedGendersFlags = baseMesh.SupportedGendersFlags;
								template.BlendshapePresets = baseMesh.BlendshapePresets.ToList();
								template.AccessoryParents = baseMesh.AccessoryParents;
								template.TextureMaterialMap = baseMesh.TextureMaterialMap;
								template.Blendshapes = baseMesh.Blendshapes;
								template.BodyParts = baseMesh.BodyParts;
								template.BlendshapeRenderers = baseMesh.BlendshapeRenderers;
								template.SFWColliders = baseMesh.SFWColliders;
								template.POV = baseMesh.POV;
								template.FaceTransform = baseMesh.FaceTransform;
								template.EyeData = baseMesh.EyeData;
								template.MouthData = baseMesh.MouthData;
								template.Cock = baseMesh.Cock;
								template.BodyRootBone = baseMesh.BodyRootBone;
								template.HeadRootBone = baseMesh.HeadRootBone;
								template.PrivatesRootBone = baseMesh.PrivatesRootBone;
								template.MergedEyes = baseMesh.MergedEyes;
								template.InvertMergedEyes = baseMesh.InvertMergedEyes;
								template.Eyes = baseMesh.Eyes.ToList();
								template.Breasts = baseMesh.Breasts.ToList();
								template.Buttocks = baseMesh.Buttocks.ToList();
								template.EyeControl = baseMesh.EyeControl;
								template.ExpressionControl = baseMesh.ExpressionControl;
								template.PoseControl = baseMesh.PoseControl;
							}
							else
							{
								Debug.LogWarning("No IBaseMesh selected");
							}
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				GUILayout.Space(5);

				if (template.CompatibleBaseMeshes == null)
				{
					var availableBaseMeshes = GetBaseMeshes();
					template.CompatibleBaseMeshes = availableBaseMeshes.Count == 0 ? Array.Empty<SCompatibleBaseMesh>() : new [] { new SCompatibleBaseMesh(availableBaseMeshes[0]) };
				}
				
				template.CompatibleBaseMeshes = verticalList(template.CompatibleBaseMeshes, $"{GetLocalizedString("MODCREATOR_BASIC_COMPATIBLEBASEMESHES")}*");

				GUILayout.Space(10);
			}
			else if (template.TemplateType == ETemplateType.StudioObject)
			{
				EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_COMPCONF"), EditorStyles.boldLabel);
				template.StudioObjectType = (EStudioObjectType)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_STUDIOSLOT")}*", template.StudioObjectType, "MODCREATOR_BASIC_STUDIOSLOT_");
				
				GUILayout.Space(5);
						
				forwardKinematics(template);
				
				GUILayout.Space(10);
			}
			else if (template.TemplateType == ETemplateType.ModdedScene)
			{
				EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_COMPCONF"), EditorStyles.boldLabel);
				template.ModdedSceneUsageFlags = (EModdedSceneUsageFlags)EditorGUILayout.EnumFlagsField($"{GetLocalizedString("MODCREATOR_BASIC_USAGE")}*", template.ModdedSceneUsageFlags);
				
				EditorGUILayout.LabelField($"{GetLocalizedString("MODCREATOR_BASIC_LARGEBACKGROUND")}*");
				template.LargeBackground = (UnityEngine.Sprite)EditorGUILayout.ObjectField(template.LargeBackground, typeof(UnityEngine.Sprite), true, GUILayout.Height(240), GUILayout.Width(426.6f));
				
				GUILayout.Space(10);
			}
			else if (template.TemplateType == ETemplateType.Animation)
			{
				EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_COMPCONF"), EditorStyles.boldLabel);
				template.AnimationUsageFlags = (EAnimationUsageFlags)EditorGUILayout.EnumFlagsField($"{GetLocalizedString("MODCREATOR_BASIC_USAGE")}*", template.AnimationUsageFlags);
				template.AnimationFadeDuration = EditorGUILayout.FloatField($"{GetLocalizedString("MODCREATOR_BASIC_FADEDURATION")}*", template.AnimationFadeDuration);
				
				GUILayout.Space(5);
				
				template.AnimationClipContainers ??= Array.Empty<SClipContainer>();
				template.AnimationClipContainers = verticalList(template.AnimationClipContainers, GetLocalizedString("MODCREATOR_BASIC_CONTAINERS"));
				
				if (template.AnimationUsageFlags.HasFlag(EAnimationUsageFlags.HScene))
				{
					GUILayout.Space(5);
					
					template.HAnimationType = (EHAnimationType)EditorGUILayout.EnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_HTYPE")}*", template.HAnimationType);
					template.HAnimationSupportedClimaxTypes = (ESupportedClimaxTypesFlags)EditorGUILayout.EnumFlagsField($"{GetLocalizedString("MODCREATOR_BASIC_HSUPPORTEDCLIMAX")}*", template.HAnimationSupportedClimaxTypes);
					
					GUILayout.Space(5);
					
					template.HAnimationArouseActive = EditorGUILayout.Toggle($"{GetLocalizedString("MODCREATOR_BASIC_HAROUSEACTIVE")}*", template.HAnimationArouseActive);
					template.HAnimationArousePassive = EditorGUILayout.Toggle($"{GetLocalizedString("MODCREATOR_BASIC_HAROUSEPASSIVE")}*", template.HAnimationArousePassive);
					
					GUILayout.Space(5);
					
					template.HAnimationArousalMultiplier = EditorGUILayout.FloatField($"{GetLocalizedString("MODCREATOR_BASIC_HAROUSALMULT")}*", template.HAnimationArousalMultiplier);
					
					GUILayout.Space(5);
					
					template.HAnimationCameraPositionOffset = EditorGUILayout.Vector3Field(GetLocalizedString("MODCREATOR_BASIC_HCAMERAPOSITIONOFFSET"), template.HAnimationCameraPositionOffset);
					template.HAnimationCameraAnglesOffset = EditorGUILayout.Vector3Field(GetLocalizedString("MODCREATOR_BASIC_HCAMERAANGLESOFFSET"), template.HAnimationCameraAnglesOffset);
					template.HAnimationCameraDistance = EditorGUILayout.FloatField(GetLocalizedString("MODCREATOR_BASIC_HCAMERADISTANCE"), template.HAnimationCameraDistance);

					GUILayout.Space(5);

					if (template.HAnimationRaycastDown == null || template.HAnimationRaycastDown.Length != template.AnimationClipContainers.Length)
						Array.Resize(ref template.HAnimationRaycastDown, template.AnimationClipContainers.Length);
					
					template.HAnimationRaycastDown = verticalList(template.HAnimationRaycastDown, $"{GetLocalizedString("MODCREATOR_BASIC_CLIPS_HRAYCASTDOWN")}*", true);

					GUILayout.Space(5);

					if (template.HAnimationIdleClips == null || template.HAnimationIdleClips.Length != template.AnimationClipContainers.Length)
						Array.Resize(ref template.HAnimationIdleClips, template.AnimationClipContainers.Length);
					
					template.HAnimationIdleClips = verticalList(template.HAnimationIdleClips, $"{GetLocalizedString("MODCREATOR_BASIC_CLIPS_HIDLE")}*", true);

					GUILayout.Space(5);
					
					if (template.HAnimationNonClimaxClips == null || template.HAnimationNonClimaxClips.Length != template.AnimationClipContainers.Length)
						Array.Resize(ref template.HAnimationNonClimaxClips, template.AnimationClipContainers.Length);
					
					template.HAnimationNonClimaxClips = verticalList(template.HAnimationNonClimaxClips, $"{GetLocalizedString("MODCREATOR_BASIC_CLIPS_HNONCLIMAX")}", true);

					GUILayout.Space(5);
					
					var climaxTypes = new List<EClimaxType>();
					
					foreach (Enum supportedClimaxType in Enum.GetValues(typeof(ESupportedClimaxTypesFlags)))
					{
						if (!template.HAnimationSupportedClimaxTypes.HasFlag(supportedClimaxType))
							continue;

						var typeInt = Convert.ToInt32(supportedClimaxType);
						
						// None is always set, skip it
						if (typeInt == 0)
							continue;
					
						climaxTypes.Add((EClimaxType)typeInt);
					}

					if (template.HAnimationClimaxClips == null || template.HAnimationClimaxClips.Length != climaxTypes.Count)
						Array.Resize(ref template.HAnimationClimaxClips, climaxTypes.Count);

					for (var i = 0; i < climaxTypes.Count; i++)
					{
						var tuple = template.HAnimationClimaxClips[i];
						
						if (tuple.Item2 == null || tuple.Item2.Length != template.AnimationClipContainers.Length)
							Array.Resize(ref tuple.Item2, template.AnimationClipContainers.Length);
						
						var climaxType = climaxTypes[i];
						
						tuple.Item1 = climaxType;
						tuple.Item2 = verticalList(tuple.Item2, $"{GetLocalizedString($"MODCREATOR_BASIC_CLIPS_HCLIMAX_{((EClimaxType)i).ToString().ToUpper()}")}*");
						template.HAnimationClimaxClips[i] = tuple;
					}
				}
				
				GUILayout.Space(10);
			}

			EditorGUILayout.LabelField(
				template.TemplateType == ETemplateType.ModdedScene
					? GetLocalizedString("MODCREATOR_BASIC_SCENECONF")
					: GetLocalizedString("MODCREATOR_BASIC_ITEMCONF"), EditorStyles.boldLabel);

			template.Tags ??= Array.Empty<string>();
			
			GUILayout.BeginHorizontal();
			
			GUILayout.BeginVertical();
			
			template.Name = EditorGUILayout.TextField($"{GetLocalizedString("MODCREATOR_BASIC_NAME")}*", template.Name);
			template.Description = EditorGUILayout.TextField(GetLocalizedString("MODCREATOR_BASIC_DESC"), template.Description);
			
			GUILayout.EndVertical();

			EditorGUIUtility.labelWidth = 35;
			template.Icon = (UnityEngine.Sprite)EditorGUILayout.ObjectField($"{GetLocalizedString("MODCREATOR_BASIC_ICON")}*", template.Icon, typeof(UnityEngine.Sprite), true, GUILayout.Width(95));
			EditorGUIUtility.labelWidth = 0;
			
			GUILayout.EndHorizontal();
			
			GUILayout.Space(10);
			
			template.Tags = verticalList(template.Tags, GetLocalizedString("MODCREATOR_BASIC_TAGS"), -1, true);
			
			GUILayout.Space(10);

			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_NSFWCAT"), EditorStyles.boldLabel);

			// custom base meshes are bound to have some sort of compatibility issues, for safety stick to the default ones marked as sfw
			if (template.TemplateType is ETemplateType.CharacterObject && template.CharacterObjectType is ECharacterObjectType.BaseMesh)
			{
				template.IsNSFW = EditorGUILayout.ToggleLeft($"{GetLocalizedString("MODCREATOR_BASIC_ISNSFW")}*", true);
			}
			else
			{
				template.IsNSFW = EditorGUILayout.ToggleLeft($"{GetLocalizedString("MODCREATOR_BASIC_ISNSFW")}*", template.IsNSFW);
			}
			
			GUILayout.Space(5);
			
			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_NSFWWARN1"));
			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_NSFWWARN2"));
			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_NSFWWARN3"));

			GUILayout.EndScrollView();
	
			GUILayout.FlexibleSpace();
			
			DrawAdvancedToggle(template);
			
			GUILayout.BeginHorizontal();

			if (GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_COPY")))
				copyBuffer = template.Copy();

			if (GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_PASTE")))
			{
				if (template.TemplateType != copyBuffer.TemplateType)
					return;
				
				Templates[CurrentTemplate] = copyBuffer.Copy();
			}

			GUILayout.EndHorizontal();
		}
		
		private void bodyParts(Template template)
		{
			GUILayout.Space(5);

			template.BodyParts ??= new List<SBodyPart>();

			// unity can you be more hacky than this?
			var style = EditorStyles.foldout;
			var previousStyle = style.fontStyle;
			
			style.fontStyle = FontStyle.Bold;
			bodyPartsFold = EditorGUILayout.Foldout(bodyPartsFold, $"{GetLocalizedString("MODCREATOR_BASIC_BODYPARTS")}*", true);
			style.fontStyle = previousStyle;

			if (!bodyPartsFold) 
				return;

			verticalList(template.BodyParts, "List");
		}
		
		private void accessoryParents(Template template)
		{
			GUILayout.Space(5);

			template.AccessoryParents ??= new List<Tuple.SerializableTuple<Transform, string>>();

			// unity can you be more hacky than this?
			var style = EditorStyles.foldout;
			var previousStyle = style.fontStyle;
			
			style.fontStyle = FontStyle.Bold;
			accessoryParentsFold = EditorGUILayout.Foldout(accessoryParentsFold, $"{GetLocalizedString("MODCREATOR_BASIC_ACCESSORYPARENTS")}*", true);
			style.fontStyle = previousStyle;

			if (!accessoryParentsFold) 
				return;

			verticalList(template.AccessoryParents, "List");

			GUILayout.BeginHorizontal();

			GUI.enabled = template.BodyRootBone != null;
			var autofillAccessoryParents = GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_AUTOFILL"));
			GUI.enabled = true;
						
			if (autofillAccessoryParents)
			{
				template.AccessoryParents.Clear();
							
				var transforms = template.BodyRootBone.GetComponentsInChildren<Transform>(true);
				foreach (var transform in transforms)
					template.AccessoryParents.Add(new Tuple.SerializableTuple<Transform, string>(transform, transform.name));
			}
						
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_REMOVEINVALID")))
			{
				for (var i = template.AccessoryParents.Count - 1; i >= 0; i--)
				{
					var tuple = template.AccessoryParents[i];
					if (tuple.Item2.EndsWith(".C") || tuple.Item2 == "SFWCollider" || tuple.Item2.StartsWith("PHY_") || tuple.Item1 == template.FaceTransform || tuple.Item1 == template.POV || tuple.Item1 == null)
					{
						template.AccessoryParents.RemoveAt(i);
						Debug.Log($"Removed {tuple.Item2}");
					}
				}
			}
						
			GUILayout.EndHorizontal();
		}

		private void blendshapes(Template template)
		{
			GUILayout.Space(5);

			template.Blendshapes ??= new List<SBlendShape>();

			// unity can you be more hacky than this?
			var style = EditorStyles.foldout;
			var previousStyle = style.fontStyle;
			
			style.fontStyle = FontStyle.Bold;
			blendshapesFold = EditorGUILayout.Foldout(blendshapesFold, $"{GetLocalizedString("MODCREATOR_BASIC_BLENDSHAPES")}*", true);
			style.fontStyle = previousStyle;

			if (!blendshapesFold) 
				return;

			verticalList(template.Blendshapes, "List");

			GUILayout.BeginHorizontal();
			
			GUI.enabled = template.BlendshapeRenderers != null && template.BlendshapeRenderers.Count > 0;
			var autofillBlendShapes = GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_AUTOFILL"));
			var autofillBlendShapesNew = GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_AUTOFILLNEW"));
			GUI.enabled = true;
			
			if (autofillBlendShapes || autofillBlendShapesNew)
			{
				if (!autofillBlendShapesNew)
					template.Blendshapes.Clear();
				
				foreach (var meshRenderer in template.BlendshapeRenderers)
				{
					var sharedMesh = meshRenderer.sharedMesh;

					for (var i = 0; i < sharedMesh.blendShapeCount; i++)
					{
						var thisName = sharedMesh.GetBlendShapeName(i);
						var nextName = i < sharedMesh.blendShapeCount - 1 ? sharedMesh.GetBlendShapeName(i + 1) : "";
			
						if (containsBlendshape(template, thisName) || containsBlendshape(template, nextName)) 
							continue;
			
						if (thisName.Contains("-ve") && nextName.Contains("+ve"))
						{
							var blendshapeTitle = thisName.Replace("_", " ")[..(thisName.Length - 4)];
							template.Blendshapes.Add(new SBlendShape { Blendshapes = new [] {thisName, nextName}, Title = blendshapeTitle});
							i++;
						}
						else
						{
							template.Blendshapes.Add(new SBlendShape { Blendshapes = new [] {thisName}, Title = thisName.Replace("_", " ")});
						}

						Debug.Log($"Added {thisName} {nextName}");
					}
				}
			}

			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();

			if (GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_DEFAULTRANGES")))
			{
				for (var i = 0; i < template.Blendshapes.Count; i++)
				{
					var blendShape = template.Blendshapes[i];
					blendShape.SFWRange = new[] { -100, 100 };
					blendShape.NSFWRange = new[] { -100, 100 };

					template.Blendshapes[i] = blendShape;
				}
			}
			
			GUILayout.EndHorizontal();
		}
		
		private void blendshapePresets(Template template)
		{
			GUILayout.Space(5);

			template.BlendshapePresets ??= new List<SBlendshapePreset>();

			// unity can you be more hacky than this?
			var style = EditorStyles.foldout;
			var previousStyle = style.fontStyle;
			
			style.fontStyle = FontStyle.Bold;
			blendshapePresetsFold = EditorGUILayout.Foldout(blendshapePresetsFold, GetLocalizedString("MODCREATOR_BASIC_BLENDSHAPEPRESETS"), true);
			style.fontStyle = previousStyle;

			if (!blendshapePresetsFold) 
				return;

			verticalList(template.BlendshapePresets, "List");
		}
		
		private void blendshapeOffsets(Template template)
		{
			GUILayout.Space(5);

			template.BlendshapeOffsets ??= Array.Empty<SBlendshapeOffset>();
			var tempList = template.BlendshapeOffsets.ToList();

			// unity can you be more hacky than this?
			var style = EditorStyles.foldout;
			var previousStyle = style.fontStyle;
			
			style.fontStyle = FontStyle.Bold;
			blendshapeOffsetsFold = EditorGUILayout.Foldout(blendshapeOffsetsFold, GetLocalizedString("MODCREATOR_BASIC_BLENDOFF_TITLE"), true);
			style.fontStyle = previousStyle;

			if (!blendshapeOffsetsFold) 
				return;

			var bodyBlendshapes = new List<string>();
			
			var compatibleBaseMeshes = template.CompatibleBaseMeshes;
			if (compatibleBaseMeshes != null && compatibleBaseMeshes.Length > 0)
			{
				var baseMesh = compatibleBaseMeshes[0];
				bodyBlendshapes = GetBodyBlendshapes(new Tuple<string, byte>(baseMesh.GUID, baseMesh.ID));
			}
			
			foreach (var blendshape in bodyBlendshapes)
			{
				var contains = false;
				foreach (var offset in tempList)
				{
					if (offset.Name != blendshape) 
						continue;
					
					contains = true;
					break;
				}

				if (!contains)
					tempList.Add(new SBlendshapeOffset(blendshape, 0f));
			}
			
			GUILayout.BeginHorizontal();
			GUILayout.Label($"List{itemsCount(bodyBlendshapes.Count)}");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(GetLocalizedString("MODCREATOR_RESET"), GUILayout.Width(50)))
			{
				tempList.Clear();
				
				foreach (var blendshape in bodyBlendshapes)
					tempList.Add(new SBlendshapeOffset(blendshape, 0f));
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginVertical(GetBackgroundStyle(new Color(0.3f, 0.3f, 0.3f)));

			for (var i = 0; i < tempList.Count; i++)
			{
				var offset = tempList[i];
				
				if (!bodyBlendshapes.Contains(offset.Name))
					continue;
				
				tempList[i] = new SBlendshapeOffset(offset.Name, EditorGUILayout.Slider(offset.Name, offset.Offset, -100f, 100f));
			}

			GUILayout.EndVertical();
			
			template.BlendshapeOffsets = tempList.ToArray();
		}
		
		private void physicsSimulation(Template template)
		{
			GUILayout.Space(5);

			var simulation = template.Simulation;
			var simulationDataTempArr = template.Simulation.SimulationData ?? new SimulationData[0];
            var simulationDataTempList = simulationDataTempArr.ToList();

			// unity can you be more hacky than this?
			var style = EditorStyles.foldout;
			var previousStyle = style.fontStyle;
			
			style.fontStyle = FontStyle.Bold;
			physicsSimulationFold = EditorGUILayout.Foldout(physicsSimulationFold, GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_TITLE"), true);
			style.fontStyle = previousStyle;

			if (physicsSimulationFold)
			{
                GUILayout.BeginVertical();
                simulation.BoneAssignmentMode = (EBoneAssignmentMode)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_BONEALIGNMENTMODE"), simulation.BoneAssignmentMode, "MODCREATOR_BASIC_CLOTHSIM_BONEALIGNMENTMODE_");
				GUILayout.EndVertical();
			}

            if (physicsSimulationFold)
            {
                GUILayout.Space(2);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Entries{itemsCount(simulationDataTempList.Count)}");
                GUILayout.FlexibleSpace();
                
                EditorGUIUtility.labelWidth = 55;
                selectedPhysicsPreset = (EPreset)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_PRESET"), selectedPhysicsPreset, "MODCREATOR_BASIC_CLOTHSIM_PRESET_", GUILayout.Width(200));
                EditorGUIUtility.labelWidth = 0;

				if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
				{
					SimulationData sData = SimulationData.Create(selectedPhysicsPreset);
					sData.Preset = selectedPhysicsPreset;
					simulationDataTempList.Add(sData);
                }
				
				GUI.enabled = copySimulationData != null;

				if (GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_PASTE"), GUILayout.Width(50)) && copySimulationData != null)
					simulationDataTempList.Add(copySimulationData.Value.Copy());

				GUI.enabled = true;
                    
                if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
                    simulationDataTempList.Clear();
                GUILayout.EndHorizontal();

            }

            if (physicsSimulationFold)
			{
				GUILayout.Space(2);
				
				for (var i = 0; i < simulationDataTempList.Count; i++)
				{
					GUILayout.BeginVertical(GetBackgroundStyle(new Color(0.3f, 0.3f, 0.3f)));
					
					var simulationData = simulationDataTempList[i];

					GUILayout.BeginHorizontal();
					
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_GENERAL"), EditorStyles.boldLabel);
					
					GUILayout.FlexibleSpace();
					
					if (GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_COPY")))
						copySimulationData = simulationData.Copy();
					
					GUILayout.EndHorizontal();
                    
					simulationData.Name = EditorGUILayout.TextField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_NAME"), simulationData.Name);
					
					GUILayout.BeginHorizontal();
                    simulationData.Enabled = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ENABLED"), simulationData.Enabled);
					if (GUILayout.Button("-", GUILayout.Width(25)))
					{
                        simulationDataTempList.RemoveAt(i);
						break;
					}
					GUILayout.EndHorizontal();

                    simulationData.AnimationPoseRatio = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ANIMPOSERATIO"), simulationData.AnimationPoseRatio, 0f, 1f);
                    
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_COLLISION"), EditorStyles.boldLabel);

                    simulationData.CollisionMode = (ECollisionMode)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_COLLISIONMODE"), simulationData.CollisionMode, "MODCREATOR_BASIC_CLOTHSIM_COLLISIONMODE_");
                    simulationData.Radius = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_RADIUS"), simulationData.Radius, 0f, 1f);
                    simulationData.RadiusCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_RADIUSCURVE"), simulationData.RadiusCurve, Color.green, new Rect(0, 0, 1, 1));
                    simulationData.Friction = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_FRICTION"), simulationData.Friction, 0f, 1f);

					GUILayout.Space(10);
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ADVCOLLISION"), EditorStyles.boldLabel);

                    simulationData.MaxDistanceRadius = EditorGUILayout.FloatField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_MAXDISTRADIUS"), simulationData.MaxDistanceRadius);
                    simulationData.MaxDistanceRadiusCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_MAXDISTRADIUSCURVE"), simulationData.MaxDistanceRadiusCurve, Color.green, new Rect(0, 0, 1, 1));
                    simulationData.BackstopNormalAlignment = dragTransformNameSingle(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_BACKSTOPNORMALALIGN"), simulationData.BackstopNormalAlignment, Prefabs[Templates.IndexOf(template)]);
                    simulationData.BackstopDistance = EditorGUILayout.FloatField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_BACKSTOPDIST"), simulationData.BackstopDistance);
                    simulationData.BackstopDistanceCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_BACKSTOPDISTCURVE"), simulationData.BackstopDistanceCurve, Color.green, new Rect(0, 0, 1, 1));
                    simulationData.BackstopRadius = EditorGUILayout.FloatField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_BACKSTOPRADIUS"), simulationData.BackstopRadius);
                    simulationData.BackstopStiffness = EditorGUILayout.FloatField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_BACKSTOPSTIFFNESS"), simulationData.BackstopStiffness);

					GUILayout.Space(10);
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_FORCE"), EditorStyles.boldLabel);

                    simulationData.Gravity = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_GRAVITY"), simulationData.Gravity, 0f, 10f);
                    simulationData.Damping = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_DAMPING"), simulationData.Damping, 0f, 1f);
                    simulationData.DampingCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_DAMPINGCURVE"), simulationData.DampingCurve, Color.green, new Rect(0, 0, 1, 1));

					GUILayout.Space(10);
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ANGLE"), EditorStyles.boldLabel);

                    simulationData.Stiffness = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_STIFFNESS"), simulationData.Stiffness, 0f, 1f);
                    simulationData.StiffnessCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_STIFFNESSCURVE"), simulationData.StiffnessCurve, Color.green, new Rect(0, 0, 1, 1));
                    simulationData.VelocityAttenuation = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_VELATTEN"), simulationData.VelocityAttenuation, 0f, 1f);
                    simulationData.AngleLimit = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ANGLIMIT"), simulationData.AngleLimit, 0f, 180f);
                    simulationData.AngleLimitCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ANGLIMITCURVE"), simulationData.AngleLimitCurve, Color.green, new Rect(0, 0, 1, 1));
                    simulationData.AngleLimitStiffness = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ANGLIMITSTIFFNESS"), simulationData.AngleLimitStiffness, 0f, 1f);

					GUILayout.Space(10);
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SHAPE"), EditorStyles.boldLabel);

                    simulationData.Rigidness = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_RIGIDNESS"), simulationData.Rigidness, 0f, 1f);
                    simulationData.RigidnessCurve = EditorGUILayout.CurveField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_RIGIDCURVE"), simulationData.RigidnessCurve, Color.green, new Rect(0, 0, 1, 1));
                    simulationData.Tether = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_TETHER"), simulationData.Tether, 0f, 1f);
					
					GUILayout.Space(10);
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_INERTIA"), EditorStyles.boldLabel);

                    simulationData.WorldInertia = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_WORLDINERT"), simulationData.WorldInertia, 0f, 1f);
                    simulationData.LocalInertia = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_LOCALINERT"), simulationData.LocalInertia, 0f, 1f);
					
					GUILayout.Space(10);
					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SIMULATION"), EditorStyles.boldLabel);

                    simulationData.SimulationType = (ESimulationType)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SIMULATIONTYPE"), simulationData.SimulationType, "MODCREATOR_BASIC_CLOTHSIM_SIMULATIONTYPE_");

					if (simulationData.SimulationType is ESimulationType.BoneCloth or ESimulationType.MeshCloth)
					{
                        simulationData.SkinningBones ??= Array.Empty<string>();

						GUILayout.Space(2);
						var skinningBonesList = simulationData.SkinningBones.ToList();
						verticalDragTransformNameList(skinningBonesList, GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SKINNINGBONES"), Prefabs[Templates.IndexOf(template)]);
						GUILayout.Space(8);

                        simulationData.SkinningBones = skinningBonesList.ToArray();
					}
					
					if (simulationData.SimulationType is ESimulationType.BoneCloth or ESimulationType.BoneSpring)
					{
                        simulationData.RootBones ??= Array.Empty<string>();

						GUILayout.Space(2);
						var rootBonesList = simulationData.RootBones.ToList();
						verticalDragTransformNameList(rootBonesList, GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_ROOTBONES"), Prefabs[Templates.IndexOf(template)]);
						GUILayout.Space(2);

                        simulationData.RootBones = rootBonesList.ToArray();
					}
					
					if (simulationData.SimulationType is ESimulationType.MeshCloth)
					{
                        simulationData.Reduction = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_REDUCTION"), simulationData.Reduction, 0f, 0.2f);
                        simulationData.PaintMap = (Texture2D)EditorGUILayout.ObjectField(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_PAINTMAP"), simulationData.PaintMap, typeof(Texture2D), false);
					}

					if (simulationData.SimulationType == ESimulationType.BoneCloth)
                        simulationData.ConnectionMode = (EConnectionMode)LocalizedEnumPopup(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_CONNECTIONMODE"), simulationData.ConnectionMode, "MODCREATOR_BASIC_CLOTHSIM_CONNECTIONMODE_");

					if (simulationData.SimulationType is ESimulationType.BoneSpring)
					{
                        simulationData.SpringStrength = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SPRINGSTR"), simulationData.SpringStrength, 0f, 0.2f);
                        simulationData.SpringDistance = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SPRINGDIST"), simulationData.SpringDistance, 0f, 0.5f);
                        simulationData.SpringNormalDistanceRatio = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SPRINGNORMALDISTRATIO"), simulationData.SpringNormalDistanceRatio, 0f, 1f);
                        simulationData.SpringNoise = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLOTHSIM_SPRINGNOISE"), simulationData.SpringNoise, 0f, 1f);
					}
					
					GUILayout.EndVertical();
					
					if (i != simulationDataTempList.Count - 1)
						EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    simulationDataTempList[i] = simulationData;
				}
			}

            simulation.SimulationData = simulationDataTempList.ToArray();
            template.Simulation = simulation;
		}
		
		private void forwardKinematics(Template template)
		{
			GUILayout.Space(5);

			var fkData = template.FKData;
			fkData.Groups ??= Array.Empty<SFKGroup>();
			
			var tempList = fkData.Groups.ToList();

			// unity can you be more hacky than this?
			var style = EditorStyles.foldout;
			var previousStyle = style.fontStyle;
			
			style.fontStyle = FontStyle.Bold;
			forwardKinematicsFold = EditorGUILayout.Foldout(forwardKinematicsFold, GetLocalizedString("MODCREATOR_BASIC_FK_TITLE"), true);
			style.fontStyle = previousStyle;

			if (forwardKinematicsFold)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label($"List{itemsCount(tempList.Count)}");
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
					tempList.Add(SFKGroup.Default());
				if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
					tempList.Clear();
				GUILayout.EndHorizontal();
			}
			
			if (forwardKinematicsFold)
			{
				GUILayout.Space(2);
				
				for (var i = 0; i < tempList.Count; i++)
				{
					GUILayout.BeginVertical(GetBackgroundStyle(new Color(0.3f, 0.3f, 0.3f)));
					
					var fkGroup = tempList[i];

					EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_FK_GENERAL"), EditorStyles.boldLabel);
					
					GUILayout.BeginHorizontal();
					fkGroup.Name = EditorGUILayout.TextField(GetLocalizedString("MODCREATOR_BASIC_FK_GROUPNAME"), fkGroup.Name);
					if (GUILayout.Button("-", GUILayout.Width(25)))
					{
						tempList.RemoveAt(i);
						break;
					}
					GUILayout.EndHorizontal();

					if (template.TemplateType == ETemplateType.CharacterObject && template.CharacterObjectType == ECharacterObjectType.BaseMesh)
					{
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_FK_CHARASETTINGS"), EditorStyles.boldLabel);
						fkGroup.FutaExclusive = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_FK_FUTAEXCLUSIVE"), fkGroup.FutaExclusive);
					}

					fkGroup.Transforms ??= Array.Empty<SFKTransform>();

					GUILayout.Space(2);
					var transformsList = fkGroup.Transforms.ToList();

					GUILayout.BeginHorizontal();
					GUILayout.Label($"List{itemsCount(transformsList.Count)}");
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(GetLocalizedString("MODCREATOR_ADD"), GUILayout.Width(50)))
						transformsList.Add(SFKTransform.Default());
					if (GUILayout.Button(GetLocalizedString("MODCREATOR_CLEAR"), GUILayout.Width(50)))
						transformsList.Clear();
					GUILayout.EndHorizontal();

					var previousIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel = 1;
					
					for (var k = 0; k < transformsList.Count; k++)
					{
						GUILayout.BeginVertical(GetBackgroundStyle(new Color(0.35f, 0.35f, 0.35f)));
						
						var fkTransform = transformsList[k];
						
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_FK_GENERAL"), EditorStyles.boldLabel);
					
						GUILayout.BeginHorizontal();
						fkTransform.Name = EditorGUILayout.TextField(GetLocalizedString("MODCREATOR_BASIC_FK_TRANSFORMNAME"), fkTransform.Name);
						if (GUILayout.Button("-", GUILayout.Width(25)))
						{
							transformsList.RemoveAt(k);
							break;
						}
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						
						fkTransform.Transform = (Transform)EditorGUILayout.ObjectField(GetLocalizedString("MODCREATOR_BASIC_FK_TRANSFORM"), fkTransform.Transform, typeof(Transform), true);

						GUI.enabled = fkTransform.Transform != null;
						
						if (GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_FK_READRESTINGANG"), GUILayout.Width(175)))
							fkTransform.RestingAngle = fkTransform.Transform.localRotation;
						
						GUI.enabled = true;
						
						GUILayout.EndHorizontal();
						
						var angVec = new Vector4
						{
							x = fkTransform.RestingAngle.x,
							y = fkTransform.RestingAngle.y,
							z = fkTransform.RestingAngle.z,
							w = fkTransform.RestingAngle.w
						};
						
						angVec = EditorGUILayout.Vector4Field(GetLocalizedString("MODCREATOR_BASIC_FK_RESTINGANGLE"), angVec);
						fkTransform.RestingAngle = new Quaternion(angVec.x, angVec.y, angVec.z, angVec.w);

						fkTransform.MovableTargetScale = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_FK_MOVABLETARGSCA"), fkTransform.MovableTargetScale, 0.1f, 2f);
						
						GUILayout.Space(10);
						EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_FK_MIRRORING"), EditorStyles.boldLabel);

						fkTransform.MirrorTransform = (Transform)EditorGUILayout.ObjectField(GetLocalizedString("MODCREATOR_BASIC_FK_MIRRORTRANSFORM"), fkTransform.MirrorTransform, typeof(Transform), true);
						
						GUILayout.BeginHorizontal();
						fkTransform.MirrorInvertAxis.Item1 = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_FK_MIRRORX"), fkTransform.MirrorInvertAxis.Item1);
						fkTransform.MirrorInvertAxis.Item2 = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_FK_MIRRORY"), fkTransform.MirrorInvertAxis.Item2);
						fkTransform.MirrorInvertAxis.Item3 = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_FK_MIRRORZ"), fkTransform.MirrorInvertAxis.Item3);
						GUILayout.EndHorizontal();
						
						GUILayout.EndVertical();
					
						if (k != transformsList.Count - 1)
							EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
						
						transformsList[k] = fkTransform;
					}
					
					EditorGUI.indentLevel = previousIndent;

					GUILayout.Space(8);

					fkGroup.Transforms = transformsList.ToArray();
					
					GUILayout.EndVertical();
					
					if (i != tempList.Count - 1)
						EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
					
					tempList[i] = fkGroup;
				}
			}

			fkData.Groups = tempList.ToArray();
			template.FKData = fkData;
		}

		private void clippingFix(Template template)
		{
			EditorGUILayout.LabelField(GetLocalizedString("MODCREATOR_BASIC_CLIPPINGFIX"), EditorStyles.boldLabel);

			var previousClippingFix = template.UseClippingFix;
			var previousClippingDistance = template.ClippingDistance;
			var previousBodyRaysResolution = BodyRaysResolution;
			
			template.UseClippingFix = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_USECLIPPINGFIX"), template.UseClippingFix);
			EditorGUIUtility.fieldWidth = 60;
			template.ClippingDistance = EditorGUILayout.Slider(GetLocalizedString("MODCREATOR_BASIC_CLIPPINGDISTANCE"), template.ClippingDistance, 0.00001f, 0.01f);
			EditorGUIUtility.fieldWidth = 0;
			
			GUILayout.BeginHorizontal();
			HidePreviewRenderers = EditorGUILayout.ToggleLeft(GetLocalizedString("MODCREATOR_BASIC_HIDEPREVIEWRENDERERS"), HidePreviewRenderers);
			bodyRaysResolution = (ERaysResolution)LocalizedEnumPopup($"{GetLocalizedString("MODCREATOR_BASIC_RAYSRESOLUTION")}*", bodyRaysResolution, "MODCREATOR_BASIC_RAYSRESOLUTION_");
			GUILayout.EndHorizontal();
			
			GUILayout.Space(2);
			
			GUILayout.BeginHorizontal();
			for (var i = 0; i < template.ClothingStates.Length; i++)
			{
				var stateEnum = (EClothingState)i;
				if (stateEnum == EClothingState.Off)
					continue;

				var previewingThis = template == previewingTemplate && stateEnum == previewingState;

				GUI.enabled = template.ClothingStates[i] != null;
				var startPreviewState = GUILayout.Button(GetLocalizedString($"MODCREATOR_BASIC_PREVIEW_STATE_{stateEnum.ToString().ToUpper()}") + (previewingThis ? "*" : ""));
				GUI.enabled = true;

				if (!startPreviewState)
					continue;

				stopPreview();
				var clothingTransform = ((GameObject)Prefabs[Templates.IndexOf(template)]).transform;
				startPreview(template, stateEnum, clothingTransform);
			}

			GUILayout.Space(5);

			GUI.enabled = previewingTemplate != null;
			var stopPreviewState = GUILayout.Button(GetLocalizedString("MODCREATOR_BASIC_PREVIEW_STOP"), GUILayout.Width(125));
			GUI.enabled = true;

			if (stopPreviewState)
				stopPreview();
			
			GUILayout.EndHorizontal();

			if (template.UseClippingFix != previousClippingFix || template.ClippingDistance != previousClippingDistance || BodyRaysResolution != previousBodyRaysResolution)
				refreshPreview(previousClippingDistance, previousBodyRaysResolution);
		}
	}
}
