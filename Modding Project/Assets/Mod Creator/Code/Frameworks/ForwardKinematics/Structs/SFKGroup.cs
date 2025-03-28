using System;
using Code.Frameworks.Character.Flags;

namespace Code.Frameworks.ForwardKinematics.Structs
{
	[Serializable]
	public struct SFKGroup
	{
		public string Name;
		
		public SFKTransform[] Transforms;
		
		public bool FutaExclusive;

		public static SFKGroup Default()
		{
			return new SFKGroup
			{
				Name = "New Group",
				FutaExclusive = false,
				Transforms = Array.Empty<SFKTransform>()
			};
		}
	}
}