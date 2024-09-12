using System;

namespace Code.Frameworks.Character.Structs
{
	[Serializable]
	public struct SBlendshapeOffset
	{
		public string Name;
		public float Offset;

		public SBlendshapeOffset(string name, float offset)
		{
			Name = name;
			Offset = offset;
		}
	}
}