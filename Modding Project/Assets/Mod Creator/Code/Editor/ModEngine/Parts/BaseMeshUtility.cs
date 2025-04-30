using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.Frameworks.Character.CharacterObjects;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Interfaces;
using Code.Frameworks.ClippingFix.Enums;
using Code.Frameworks.RayTracing;
using Code.Tools;
using Railgun.AssetPipeline.Models;
using Railgun.ModEngine.Models;
using Railgun.ModEngine.Providers;
using Railgun.ModEngine.Repositories;
using Railgun.ModEngine.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using Tuple = Code.Tools.Tuple;

namespace Code.Editor.ModEngine
{
	public partial class ModCreator
	{
		[SerializeField]
		private List<BaseMeshData> baseMeshes = new ();

		[SerializeField]
		public GameObject BaseMeshHolder;
		
		/// <summary>
		/// Grabs available base meshes and initializes their GUID references
		/// </summary>
		public void InitializeBaseMeshes()
		{
			UnloadBaseMeshes();
			
			var modManager = ServiceProvider.Obtain<IModManager>();
			var modRegistry = ServiceProvider.Obtain<IModRegistry>();
			var metadataRepository = ServiceProvider.Obtain<IMetadataRepository>();
			
			modManager.Init(IsStandalone ? "Assets/Mod Creator/BaseMeshes/" : "mods").GetAwaiter().GetResult();

			if (!IsStandalone)
			{
				var builtinBaseMeshes = Directory.GetFiles("Assets/Code/Editor/ModEngine/BaseMeshes/", "*.rgm");
				
				foreach (var builtinBaseMesh in builtinBaseMeshes)
					modManager.InitSingle(builtinBaseMesh, "mods");
			}
			
			modManager.Preload().GetAwaiter().GetResult();

			var metadatas = metadataRepository.GetWithOptionalDataKey("basemesh").Result.ToList();
			for (var i = 0; i < metadatas.Count; i++)
			{
				var metadata = metadatas[i];
				
				var externalId = metadataRepository.GetExternalId(metadata).GetAwaiter().GetResult();
				var internalId = modRegistry.GetInternalId(externalId).GetAwaiter().GetResult();

				var guid = internalId.Item1;
				var index = internalId.Item2;
				
				if (guid.Contains("PR-builtin"))
					index = 0;
				
				var data = new BaseMeshData();
				data.GUID = new Tuple.SerializableTuple<string, byte>(guid, index);
				data.ExternalId = externalId;
				
				baseMeshes.Add(data);
			}
		}

		/// <summary>
		/// Unloads all initialized base meshes and destroys their instances
		/// </summary>
		public void UnloadBaseMeshes()
		{
			var modLoader = ServiceProvider.Obtain<IModLoader>();
			var modRegistry = ServiceProvider.Obtain<IModRegistry>();

			for (var i = 0; i < baseMeshes.Count; i++)
			{
				var baseMesh = baseMeshes[i];
				if (baseMesh.Instance != null)
				{
					Debug.Log($"Destroying instance of base mesh {baseMesh.GUID.Item1}.{baseMesh.GUID.Item2}");
					DestroyImmediate(baseMesh.Instance.GetGameObject());
				}
			}

			baseMeshes.Clear();

			var mods = modRegistry.Mods().GetAwaiter().GetResult().ToArray();
			for (var i = mods.Length - 1; i >= 0; i--)
			{
				try
				{
					modLoader.UnloadMod(mods[i].Value);
				}
				catch (Exception e)
				{
					Debug.LogError($"Failed unloading base mesh mod {mods[i].Key}, {e}");
				}
			}
			
			DestroyImmediate(BaseMeshHolder);
			BaseMeshHolder = null;

			var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			for (var i = 0; i < rootObjects.Length; i++)
			{
				var rootObject = rootObjects[i];
				if (rootObject.name != "_Template")
					continue;

				DestroyImmediate(rootObject);
				break;
			}
		}
		
		/// <summary>
		/// Returns accessory parents of given base mesh guid
		/// Partially loaded base meshes get fully loaded to grab the data
		/// </summary>
		public List<string> GetAccessoryParents(Tuple<string, byte> guid)
		{
			for (var i = 0; i < baseMeshes.Count; i++)
			{
				var baseMesh = baseMeshes[i];
				if (baseMesh.GUID.Item1 != guid.Item1 || baseMesh.GUID.Item2 != guid.Item2)
					continue;
				
				if (baseMesh.AccessoryParents == null)
					loadBaseMesh(baseMesh);

				return baseMesh.AccessoryParents;
			}
			
			return new List<string>();
		}
		
		/// <summary>
		/// Returns body blendshapes of given base mesh guid
		/// Partially loaded base meshes get fully loaded to grab the data
		/// </summary>
		public List<string> GetBodyBlendshapes(Tuple<string, byte> guid)
		{
			for (var i = 0; i < baseMeshes.Count; i++)
			{
				var baseMesh = baseMeshes[i];
				if (baseMesh.GUID.Item1 != guid.Item1 || baseMesh.GUID.Item2 != guid.Item2)
					continue;
				
				if (baseMesh.BodyBlendshapes == null)
					loadBaseMesh(baseMesh);

				return baseMesh.BodyBlendshapes;
			}
			
			return new List<string>();
		}

		/// <summary>
		/// Returns instance of given base mesh guid
		/// Partially loaded base meshes get fully loaded to grab the data
		/// </summary>
		public IBaseMesh GetBaseMeshInstance(Tuple<string, byte> guid)
		{
			for (var i = 0; i < baseMeshes.Count; i++)
			{
				var baseMesh = baseMeshes[i];
				if (baseMesh.GUID.Item1 != guid.Item1 || baseMesh.GUID.Item2 != guid.Item2)
					continue;
				
				if (baseMesh.Instance == null)
					loadBaseMesh(baseMesh);

				return baseMesh.Instance;
			}

			return null;
		}

		/// <summary>
		/// Returns body rays of given base mesh guid
		/// Partially loaded base meshes get fully loaded to grab the data
		/// </summary>
		public Dictionary<ERaysResolution, Frameworks.RayTracing.Ray[]> GetBaseMeshRays(Tuple<string, byte> guid)
		{
			for (var i = 0; i < baseMeshes.Count; i++)
			{
				var baseMesh = baseMeshes[i];
				if (baseMesh.GUID.Item1 != guid.Item1 || baseMesh.GUID.Item2 != guid.Item2)
					continue;
				
				if (baseMesh.Rays != null)
					return baseMesh.Rays;

				if (baseMesh.Instance == null)
					loadBaseMesh(baseMesh);
				
				if (baseMesh.Instance.RaysIDs == null)
					return null;
				
				var modRegistry = ServiceProvider.Obtain<IModRegistry>();

				var dict = new Dictionary<ERaysResolution, Frameworks.RayTracing.Ray[]>();
				var resolutions = Enum.GetValues(typeof(ERaysResolution));
				
				for (var k = 0; k < resolutions.Length; k++)
				{
					var resolution = (ERaysResolution)resolutions.GetValue(k);
					
					try
					{
						foreach (var tuple in baseMesh.Instance.RaysIDs)
						{
							if (tuple.Item1 != resolution)
								continue;

							var externalId = modRegistry.GetExternalId(guid.Item1, tuple.Item2).GetAwaiter().GetResult();
				        
							var asset = modRegistry.GetAsset(externalId).GetAwaiter().GetResult();
							if (asset == null || asset is not ArbitraryData data)
							{
								Debug.LogError($"Failed grabbing clippingfixrays for {guid.Item1}.{guid.Item2}-{resolution.ToString()} via internalID {tuple.Item2}");
								continue;
							}

							if (data.Data == null)
							{
								Debug.LogError($"ArbitraryData for clippingfixrays {guid.Item1}.{guid.Item2}-{resolution.ToString()} via internalID {tuple.Item2} is null");
								continue;
							}

							dict[resolution] = data.Data.ToRaysPtr();
						}
					}
					catch (Exception e)
					{
						Debug.LogError($"Failed grabbing clippingfixrays for {guid.Item1}.{guid.Item2}-{resolution.ToString()} {e}");
					}
				}
				
				baseMesh.Rays = dict;
				return dict;
			}

			return null;
		}
		
		/// <summary>
		/// Returns GUIDs of all initialized base meshes
		/// </summary>
		public List<Tuple<string, byte>> GetBaseMeshes()
		{
			var list = new List<Tuple<string, byte>>();

			for (var i = 0; i < baseMeshes.Count; i++)
				list.Add(new Tuple<string, byte>(baseMeshes[i].GUID.Item1, baseMeshes[i].GUID.Item2));
			
			return list;
		}

		/// <summary>
		/// Fully loads the given base mesh data, creating its instance 
		/// </summary>
		private void loadBaseMesh(BaseMeshData baseMesh)
		{
			// Don't load it again if it's already there
			if (baseMesh.Instance != null)
				return;

			// Create an empty object to store base mesh instances
			if (BaseMeshHolder == null)
				BaseMeshHolder = new GameObject("BaseMeshHolder (Cache)");
			
			var metadataRepository = ServiceProvider.Obtain<IMetadataRepository>();

			var metadata = metadataRepository.Get(baseMesh.ExternalId).GetAwaiter().GetResult();
			if (metadata == null)
			{
				Debug.LogError($"Failed loading base mesh {baseMesh.GUID.Item1}.{baseMesh.GUID.Item2}, possibly broken mod");
				return;
			}
			
			var gameObjectRepository = ServiceProvider.Obtain<IGameObjectRepository>();

			var go = gameObjectRepository.Get(metadata.Value).GetAwaiter().GetResult();
			go.transform.SetParent(BaseMeshHolder.transform);

			var instance = go.GetComponent<IBaseMesh>();
			if (instance == null)
			{
				Debug.LogError($"Couldn't find base mesh instance {baseMesh.GUID.Item1}.{baseMesh.GUID.Item2}, possibly broken mod");
				return;
			}

			// Cache accessory parents
			baseMesh.AccessoryParents = new List<string>();
			
			for (var i = 0; i < instance.AccessoryParents.Count; i++)
			{
				var accessoryParent = instance.AccessoryParents[i];
				if (accessoryParent.Item1 == null)
					continue;
				
				baseMesh.AccessoryParents.Add(accessoryParent.Item1.name);
			}

			// Cache body blendshapes
			baseMesh.BodyBlendshapes = new List<string>();

			for (var i = 0; i < instance.Blendshapes.Count; i++)
			{
				var blendshape = instance.Blendshapes[i];
				if (blendshape.Blendshapes == null || blendshape.BodyCategory == EBodyBlendShapeCategory.None)
					continue;

				for (var k = 0; k < blendshape.Blendshapes.Length; k++)
					baseMesh.BodyBlendshapes.AddUnique(blendshape.Blendshapes[k]);
			}
			
			// Cache base mesh instance
			baseMesh.Instance = (BaseBaseMesh)instance;
		}
		
		[Serializable]
		public class BaseMeshData
		{
			[SerializeField]
			public Tuple.SerializableTuple<string, byte> GUID;

			[SerializeField]
			public int ExternalId;
			
			[SerializeField]
			public List<string> AccessoryParents;
			[SerializeField]
			public List<string> BodyBlendshapes;
			
			[SerializeField]
			public BaseBaseMesh Instance;
			
			public Dictionary<ERaysResolution, Frameworks.RayTracing.Ray[]> Rays;
		}
	}
}