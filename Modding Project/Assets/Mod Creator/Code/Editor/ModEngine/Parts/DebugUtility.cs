using System;
using Railgun.AssetPipeline.Interfaces;
using Railgun.AssetPipeline.Models;
using Railgun.AssetPipeline.Types;
using UnityEngine;

namespace Code.Editor.ModEngine
{
	public partial class ModCreator
	{
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