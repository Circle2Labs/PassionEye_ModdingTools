using System;
using System.Collections.Generic;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;
using Tuple = Code.Tools.Tuple;

namespace Code.Frameworks.Character.Structs
{
	[Serializable]
	public struct SBlendshapePreset
	{
		public string Name;
		
		public List<Tuple.SerializableTuple<string, float>> Blendshapes;
		
		public List<EFaceBlendShapeCategory> FaceCategories;
		public List<EBodyBlendShapeCategory> BodyCategories;
		
		public ESupportedGendersFlags SupportedGenders;
		
		public bool FutaExclusive;
		public bool IsNSFW;
	}
}