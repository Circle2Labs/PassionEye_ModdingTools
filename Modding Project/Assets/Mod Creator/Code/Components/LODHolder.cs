using System;
using Railgun.AssetPipeline.Attributes;
using UnityEngine;

namespace Code.Components
{
	/// <summary>
	/// Component attached to objects that have a LOD Group
	/// Needed for the LOD data to be properly serialized and deserialized by ModEngine since Unity does not expose any LOD array property
	/// </summary>
	public class LODHolder : MonoBehaviour
	{
		[NonSerialized][APLIgnore]
		private LODGroup lodGroup;
		
		[APLIgnore]
		public LODGroup LODGroup
		{
			get
			{
				if (lodGroup != null) 
					return lodGroup;
				
				lodGroup = gameObject.GetComponent<LODGroup>();
					
				if (lodGroup == null)
					lodGroup = gameObject.AddComponent<LODGroup>();

				return lodGroup;
			}
			set => lodGroup = value;
		}

		public LOD[] LODs
		{
			get => LODGroup.GetLODs();
			set => LODGroup.SetLODs(value);
		}
	}
}