using System;
using Code.Frameworks.Character.Enums;
using Code.Frameworks.Character.Flags;

namespace Code.Frameworks.Character.Structs
{
	[Serializable]
	public struct SBlendShape
	{
		public string Title;
		public string[] Blendshapes;
		public EFaceBlendShapeCategory FaceCategory;
		public EBodyBlendShapeCategory BodyCategory;
		public bool FutaExclusive;
		public bool IsNSFW;
		public int[] NSFWRange;
		public int[] SFWRange;

		public SBlendShape Copy()
		{
			var copy = new SBlendShape();
			copy.Title = Title;
			
			copy.Blendshapes = new string[Blendshapes.Length];
			Array.Copy(Blendshapes, copy.Blendshapes, Blendshapes.Length);
			
			copy.FaceCategory = FaceCategory;
			copy.BodyCategory = BodyCategory;
			
			copy.FutaExclusive = FutaExclusive;
			copy.IsNSFW = IsNSFW;

			copy.NSFWRange = new int[NSFWRange.Length];
			Array.Copy(NSFWRange, copy.NSFWRange, NSFWRange.Length);
			
			copy.SFWRange = new int[SFWRange.Length];
			Array.Copy(SFWRange, copy.SFWRange, SFWRange.Length);
			
			return copy;
		}
	}
}