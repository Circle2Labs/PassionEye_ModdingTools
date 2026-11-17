using System;
using System.Collections.Generic;
using UnityEngine;
using Tuple = Code.Tools.Tuple;

namespace Code.Frameworks.Shared.Structs
{
	[Serializable]
	public struct SVariant
	{
		[SerializeField]
		public string Name;
		
		// This is set to null after being copied to metadata. Use ContentRepository.GetVariantSprite to retrieve it
		[SerializeField]
		public Sprite Icon;
		
		[SerializeField]
		public List<Tuple.SerializableTuple<Transform, bool>> Transforms;

		public SVariant Clone()
		{
			var variant = new SVariant();
			variant.Name = Name;
			variant.Icon = Icon;
			variant.Transforms = new List<Tuple.SerializableTuple<Transform, bool>>();

			for (var i = 0; i < Transforms.Count; i++)
				variant.Transforms.Add(new Tuple.SerializableTuple<Transform, bool>(Transforms[i].Item1, Transforms[i].Item2));
			
			return variant;
		}
	}
}